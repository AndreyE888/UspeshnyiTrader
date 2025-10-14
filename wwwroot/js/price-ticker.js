// Price Ticker functionality for real-time price updates

class PriceTicker {
    constructor() {
        this.instruments = new Map();
        this.updateInterval = 5000; // 5 seconds
        this.isRunning = false;
        this.tickerElement = null;
    }

    // Initialize price ticker
    init(containerSelector = '#priceTicker') {
        this.tickerElement = $(containerSelector);
        if (this.tickerElement.length === 0) {
            console.warn('Price ticker container not found');
            return;
        }

        this.loadInstruments();
        this.start();

        console.log('Price ticker initialized');
    }

    // Load available instruments
    async loadInstruments() {
        try {
            const response = await $.get('/Instruments/GetAll');
            this.instruments.clear();

            response.forEach(instrument => {
                this.instruments.set(instrument.id, {
                    id: instrument.id,
                    symbol: instrument.symbol,
                    name: instrument.name,
                    currentPrice: instrument.currentPrice,
                    lastUpdate: instrument.lastPriceUpdate,
                    change: 0,
                    changePercent: 0
                });
            });

            this.renderTicker();
        } catch (error) {
            console.error('Failed to load instruments:', error);
        }
    }

    // Start price updates
    start() {
        if (this.isRunning) return;

        this.isRunning = true;
        this.updatePrices();

        // Set up periodic updates
        this.intervalId = setInterval(() => {
            this.updatePrices();
        }, this.updateInterval);
    }

    // Stop price updates
    stop() {
        this.isRunning = false;
        if (this.intervalId) {
            clearInterval(this.intervalId);
        }
    }

    // Update all instrument prices
    async updatePrices() {
        try {
            const prices = await $.get('/Instruments/GetPrices');

            // Update instrument prices and calculate changes
            for (const [instrumentId, newPrice] of Object.entries(prices)) {
                const id = parseInt(instrumentId);
                const instrument = this.instruments.get(id);

                if (instrument) {
                    const oldPrice = instrument.currentPrice;
                    instrument.currentPrice = newPrice;

                    // Calculate change
                    if (oldPrice && oldPrice !== newPrice) {
                        instrument.change = newPrice - oldPrice;
                        instrument.changePercent = (instrument.change / oldPrice) * 100;
                    }

                    instrument.lastUpdate = new Date();
                }
            }

            this.renderTicker();
            this.dispatchPriceUpdateEvent(prices);

        } catch (error) {
            console.error('Failed to update prices:', error);
        }
    }

    // Render the ticker display
    renderTicker() {
        if (!this.tickerElement) return;

        let tickerHTML = '<div class="ticker-content">';

        this.instruments.forEach(instrument => {
            const changeClass = instrument.change >= 0 ? 'price-up' : 'price-down';
            const changeIcon = instrument.change >= 0 ? '▲' : '▼';
            const changeSign = instrument.change >= 0 ? '+' : '';

            tickerHTML += `
                <div class="ticker-item" data-instrument-id="${instrument.id}">
                    <span class="ticker-symbol">${instrument.symbol}</span>
                    <span class="ticker-price ${changeClass}">
                        $${instrument.currentPrice.toFixed(4)}
                    </span>
                    <span class="ticker-change ${changeClass}">
                        ${changeIcon} ${changeSign}${instrument.change.toFixed(4)} 
                        (${changeSign}${Math.abs(instrument.changePercent).toFixed(2)}%)
                    </span>
                </div>
            `;
        });

        tickerHTML += '</div>';
        this.tickerElement.html(tickerHTML);

        // Add animation for price changes
        this.animatePriceChanges();
    }

    // Animate price changes
    animatePriceChanges() {
        this.tickerElement.find('.ticker-item').each(function() {
            const $item = $(this);
            $item.addClass('price-update');

            setTimeout(() => {
                $item.removeClass('price-update');
            }, 1000);
        });
    }

    // Dispatch custom event for price updates
    dispatchPriceUpdateEvent(prices) {
        const event = new CustomEvent('priceUpdate', {
            detail: {
                prices: prices,
                timestamp: new Date(),
                instruments: Array.from(this.instruments.values())
            }
        });

        window.dispatchEvent(event);
    }

    // Get current price for specific instrument
    getPrice(instrumentId) {
        const instrument = this.instruments.get(instrumentId);
        return instrument ? instrument.currentPrice : null;
    }

    // Get all current prices
    getAllPrices() {
        const prices = {};
        this.instruments.forEach((instrument, id) => {
            prices[id] = instrument.currentPrice;
        });
        return prices;
    }

    // Subscribe to price updates
    onPriceUpdate(callback) {
        window.addEventListener('priceUpdate', callback);
    }

    // Unsubscribe from price updates
    offPriceUpdate(callback) {
        window.removeEventListener('priceUpdate', callback);
    }

    // Simulate price change (for testing/demo)
    async simulatePriceChange() {
        try {
            await $.post('/Instruments/SimulatePriceChange');
            await this.updatePrices(); // Refresh prices immediately
        } catch (error) {
            console.error('Failed to simulate price change:', error);
        }
    }

    // Set update interval
    setUpdateInterval(intervalMs) {
        this.updateInterval = intervalMs;
        this.stop();
        this.start();
    }

    // Get instrument info
    getInstrument(instrumentId) {
        return this.instruments.get(instrumentId);
    }

    // Get all instruments
    getAllInstruments() {
        return Array.from(this.instruments.values());
    }
}

// Global price ticker instance
const priceTicker = new PriceTicker();

// Initialize when document is ready
$(document).ready(function() {
    // Auto-initialize if ticker element exists
    if ($('#priceTicker').length > 0) {
        priceTicker.init();
    }

    // Also initialize if navigation ticker exists
    if ($('.navbar').length > 0) {
        initializeNavigationTicker();
    }
});

// Navigation ticker functionality
function initializeNavigationTicker() {
    function updateNavigationTicker() {
        $.get('/Instruments/GetPrices', function(prices) {
            // Update main instruments in navigation
            const instruments = {
                1: 'ticker-eurusd',
                2: 'ticker-gbpusd',
                4: 'ticker-btcusd',
                5: 'ticker-ethusd',
                6: 'ticker-xauusd'
            };

            for (const [id, elementId] of Object.entries(instruments)) {
                const price = prices[id];
                if (price && $(`#${elementId}`).length) {
                    const oldPrice = parseFloat($(`#${elementId}`).text().replace('$', '')) || price;
                    const change = price - oldPrice;
                    const changeClass = change >= 0 ? 'price-up' : 'price-down';

                    $(`#${elementId}`)
                        .text('$' + price.toFixed(id >= 4 ? 2 : 4))
                        .removeClass('price-up price-down')
                        .addClass(changeClass)
                        .addClass('price-update');

                    setTimeout(() => {
                        $(`#${elementId}`).removeClass('price-update');
                    }, 1000);
                }
            }
        });
    }

    // Start navigation ticker updates
    setInterval(updateNavigationTicker, 5000);
    updateNavigationTicker(); // Initial update
}

// CSS for price ticker (dynamically add if not present)
if (!$('#priceTickerStyles').length) {
    $('head').append(`
        <style id="priceTickerStyles">
            .ticker-content {
                display: flex;
                gap: 2rem;
                align-items: center;
                padding: 0.5rem 1rem;
                background: linear-gradient(90deg, #f8f9fa, #e9ecef);
                border-radius: 0.375rem;
                overflow-x: auto;
            }
            
            .ticker-item {
                display: flex;
                align-items: center;
                gap: 0.5rem;
                white-space: nowrap;
                padding: 0.25rem 0.5rem;
                background: white;
                border-radius: 0.25rem;
                box-shadow: 0 1px 3px rgba(0,0,0,0.1);
            }
            
            .ticker-symbol {
                font-weight: bold;
                color: #2c3e50;
            }
            
            .ticker-price {
                font-weight: bold;
                font-family: 'Courier New', monospace;
            }
            
            .ticker-change {
                font-size: 0.875rem;
                font-family: 'Courier New', monospace;
            }
            
            .price-up {
                color: #28a745;
            }
            
            .price-down {
                color: #dc3545;
            }
            
            .price-update {
                animation: pricePulse 0.5s ease-in-out;
            }
            
            @keyframes pricePulse {
                0% { transform: scale(1); }
                50% { transform: scale(1.05); }
                100% { transform: scale(1); }
            }
            
            /* Navigation ticker styles */
            .navbar .ticker-item {
                background: transparent;
                box-shadow: none;
                padding: 0;
            }
            
            .navbar .ticker-price {
                font-size: 0.875rem;
            }
        </style>
    `);
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { PriceTicker, priceTicker };
} else {
    window.PriceTicker = PriceTicker;
    window.priceTicker = priceTicker;
}