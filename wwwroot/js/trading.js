// Trading platform functionality

let selectedInstrument = null;
let currentPrice = 0;
let priceUpdateInterval = null;

// Initialize trading platform
function initializeTrading() {
    loadInstruments();
    updateUserBalance();
    loadActiveTrades();
    loadRecentTrades();

    // Set up event listeners
    $('#instrumentSelect').change(onInstrumentSelect);
    $('#amount').on('input', onAmountChange);
    $('#tradeForm').submit(placeTrade);

    // Start price updates
    startPriceUpdates();

    // Update trades every 10 seconds
    setInterval(loadActiveTrades, 10000);
    setInterval(loadRecentTrades, 15000);
    setInterval(updateUserBalance, 30000);
}

// Load available instruments
function loadInstruments() {
    $.get('/Instruments/GetAll', function(instruments) {
        const select = $('#instrumentSelect');
        select.empty().append('<option value="">Select instrument...</option>');

        instruments.forEach(instrument => {
            select.append(`<option value="${instrument.id}" data-symbol="${instrument.symbol}">
                ${instrument.symbol} - ${instrument.name}
            </option>`);
        });
    }).fail(function() {
        showAlert('Failed to load instruments', 'danger');
    });
}

// Handle instrument selection
function onInstrumentSelect() {
    const instrumentId = $(this).val();
    const symbol = $(this).find('option:selected').data('symbol');

    if (instrumentId) {
        selectedInstrument = parseInt(instrumentId);
        $('#selectedInstrument').text(`Trading: ${symbol}`);
        startInstrumentPriceUpdates(instrumentId);
    } else {
        selectedInstrument = null;
        $('#selectedInstrument').text('Select an instrument to start trading');
        $('#livePrice').text('$0.0000');
        $('#priceChange').text('Loading...');
        stopPriceUpdates();
    }
}

// Start price updates for selected instrument
function startInstrumentPriceUpdates(instrumentId) {
    stopPriceUpdates();

    // Immediate price fetch
    updateInstrumentPrice(instrumentId);

    // Set up interval for updates
    priceUpdateInterval = setInterval(() => {
        updateInstrumentPrice(instrumentId);
    }, 2000);
}

// Stop price updates
function stopPriceUpdates() {
    if (priceUpdateInterval) {
        clearInterval(priceUpdateInterval);
        priceUpdateInterval = null;
    }
}

// Update instrument price
function updateInstrumentPrice(instrumentId) {
    $.get(`/Instruments/GetPrice?instrumentId=${instrumentId}`, function(data) {
        if (data.price !== undefined) {
            const oldPrice = currentPrice;
            currentPrice = data.price;

            // Update displays
            $('#currentPrice').text(formatCurrency(currentPrice));
            $('#livePrice').text(formatCurrency(currentPrice));

            // Show price change
            if (oldPrice > 0) {
                const change = currentPrice - oldPrice;
                const changePercent = (change / oldPrice) * 100;
                const changeText = `${change >= 0 ? '+' : ''}${change.toFixed(4)} (${changePercent.toFixed(2)}%)`;

                $('#priceChange').html(`
                    <span class="${change >= 0 ? 'price-up' : 'price-down'}">
                        ${change >= 0 ? '▲' : '▼'} ${changeText}
                    </span>
                `);

                // Add pulse animation
                $('#livePrice').addClass('price-update');
                setTimeout(() => {
                    $('#livePrice').removeClass('price-update');
                }, 500);
            }
        }
    });
}

// Handle amount change
function onAmountChange() {
    const amount = parseFloat($(this).val()) || 0;
    updatePayoutDisplay(amount);
}

// Place a new trade
function placeTrade(e) {
    e.preventDefault();

    if (!selectedInstrument) {
        showAlert('Please select an instrument', 'warning');
        return;
    }

    const amount = parseFloat($('#amount').val());
    const direction = $('input[name="direction"]:checked').val();
    const duration = parseInt($('#duration').val());

    // Validation
    if (amount < 1 || amount > 1000) {
        showAlert('Amount must be between $1 and $1000', 'warning');
        return;
    }

    const placeTradeBtn = $('#placeTradeBtn');
    setButtonLoading(placeTradeBtn, true);

    $.post('/Trading/OpenTrade', {
        instrumentId: selectedInstrument,
        direction: direction,
        amount: amount,
        duration: duration
    }, function(response) {
        if (response.success) {
            showAlert(`Trade placed successfully! Trade ID: ${response.tradeId}`, 'success');
            updateUserBalance();
            loadActiveTrades();
            loadRecentTrades();
            $('#tradeForm')[0].reset();
            $('#amount').val(10); // Reset to default amount
            updatePayoutDisplay(10);
        } else {
            showAlert(response.message, 'danger');
        }
    }).fail(function() {
        showAlert('Failed to place trade', 'danger');
    }).always(function() {
        setButtonLoading(placeTradeBtn, false);
    });
}

// Load active trades
function loadActiveTrades() {
    $.get('/Trading/GetActiveTrades', function(data) {
        const container = $('#activeTrades');

        if (!data.trades || data.trades.length === 0) {
            container.html('<div class="text-center text-muted py-4">No active trades</div>');
            return;
        }

        let html = '';
        data.trades.forEach(trade => {
            const timeRemaining = getTimeRemaining(trade.closeTime);
            const progressPercent = calculateProgressPercent(trade.openTime, trade.closeTime);

            html += `
                <div class="trade-item trade-active mb-2">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <strong>${trade.instrument.symbol}</strong>
                            <span class="ms-2">${formatTradeDirection(trade.direction)}</span>
                            <br>
                            <small class="text-muted">Amount: ${formatCurrency(trade.amount)}</small>
                        </div>
                        <div class="text-end">
                            <div class="countdown countdown-timer" data-end-time="${trade.closeTime}">
                                ${timeRemaining}
                            </div>
                            <small class="text-muted">Open: ${formatCurrency(trade.openPrice)}</small>
                        </div>
                    </div>
                    <div class="trade-progress mt-2">
                        <div class="trade-progress-bar" style="width: ${progressPercent}%"></div>
                    </div>
                </div>
            `;
        });

        container.html(html);
        updateCountdowns();
    });
}

// Load recent trades
function loadRecentTrades() {
    $.get('/Trading/GetUserTrades', function(data) {
        const container = $('#recentTrades');

        if (!data.trades || data.trades.length === 0) {
            container.html('<div class="text-center text-muted py-4">No recent trades</div>');
            return;
        }

        // Get last 5 trades
        const recentTrades = data.trades.slice(0, 5);

        let html = '';
        recentTrades.forEach(trade => {
            const statusClass = trade.status.toLowerCase();
            const payoutText = trade.payout ? formatCurrency(trade.payout) : '-';
            const resultClass = trade.status === 'Won' ? 'text-success' :
                trade.status === 'Lost' ? 'text-danger' : 'text-muted';

            html += `
                <div class="trade-history-item">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <strong>${trade.instrumentSymbol}</strong>
                            <span class="ms-2">${formatTradeDirection(trade.direction)}</span>
                            <br>
                            <small class="text-muted">
                                ${formatDateTime(trade.openTime)} | 
                                Amount: ${formatCurrency(trade.amount)}
                            </small>
                        </div>
                        <div class="text-end">
                            <div class="${resultClass} fw-bold">
                                ${formatTradeStatus(trade.status)}
                            </div>
                            <small class="text-muted">Payout: ${payoutText}</small>
                        </div>
                    </div>
                </div>
            `;
        });

        container.html(html);
    });
}

// Calculate progress percentage for active trade
function calculateProgressPercent(openTime, closeTime) {
    const now = new Date().getTime();
    const start = new Date(openTime).getTime();
    const end = new Date(closeTime).getTime();

    if (now >= end) return 100;
    if (now <= start) return 0;

    const total = end - start;
    const elapsed = now - start;
    return (elapsed / total) * 100;
}

// Start general price updates
function startPriceUpdates() {
    // Simulate price changes every 5 seconds for demo
    setInterval(() => {
        if (selectedInstrument) {
            // In real app, this would come from server
            // For demo, we'll simulate small price changes
            const change = (Math.random() - 0.5) * 0.001 * currentPrice;
            currentPrice += change;
            currentPrice = Math.max(currentPrice, 0.0001); // Ensure positive price

            $('#currentPrice').text(formatCurrency(currentPrice));
            $('#livePrice').text(formatCurrency(currentPrice));
        }
    }, 5000);
}

// Initialize when document is ready
$(document).ready(function() {
    // Check if we're on the trading page
    if ($('#tradeForm').length > 0) {
        initializeTrading();
    }
});

// Clean up on page unload
$(window).on('beforeunload', function() {
    stopPriceUpdates();
});