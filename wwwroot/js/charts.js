// Charting functionality for trading platform
class TradingCharts {
    constructor() {
        this.charts = new Map();
        this.defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                intersect: false,
                mode: 'index'
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                tooltip: {
                    enabled: true,
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'minute'
                    },
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: false,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        };
    }

    // Initialize candle chart
    initCandleChart(canvasId, instrumentId, timeframe = '5m') {
        const ctx = document.getElementById(canvasId).getContext('2d');

        // Destroy existing chart if it exists
        if (this.charts.has(canvasId)) {
            this.charts.get(canvasId).destroy();
        }

        const chart = new Chart(ctx, {
            type: 'candlestick',
            data: {
                datasets: [{
                    label: `${this.getInstrumentSymbol(instrumentId)} Price`,
                    data: [],
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1,
                    color: {
                        up: 'rgba(40, 167, 69, 1)',
                        down: 'rgba(220, 53, 69, 1)',
                        unchanged: 'rgba(108, 117, 125, 1)'
                    }
                }]
            },
            options: {
                ...this.defaultOptions,
                plugins: {
                    ...this.defaultOptions.plugins,
                    title: {
                        display: true,
                        text: `${this.getInstrumentSymbol(instrumentId)} - ${timeframe} Chart`
                    }
                }
            }
        });

        this.charts.set(canvasId, chart);
        this.loadCandleData(instrumentId, timeframe, canvasId);

        return chart;
    }

    // Load candle data from API
    async loadCandleData(instrumentId, timeframe, canvasId) {
        try {
            // This would call your API to get candle data
            // For now, we'll generate demo data
            const candles = await this.generateDemoCandles(50);
            this.updateCandleChart(canvasId, candles);
        } catch (error) {
            console.error('Failed to load candle data:', error);
        }
    }

    // Update candle chart with new data
    updateCandleChart(canvasId, candles) {
        const chart = this.charts.get(canvasId);
        if (!chart) return;

        const candleData = candles.map(candle => ({
            x: new Date(candle.time),
            o: candle.open,
            h: candle.high,
            l: candle.low,
            c: candle.close,
            v: candle.volume
        }));

        chart.data.datasets[0].data = candleData;
        chart.update('none');
    }

    // Initialize line chart for price history
    initLineChart(canvasId, instrumentId, period = '1h') {
        const ctx = document.getElementById(canvasId).getContext('2d');

        if (this.charts.has(canvasId)) {
            this.charts.get(canvasId).destroy();
        }

        const chart = new Chart(ctx, {
            type: 'line',
            data: {
                datasets: [{
                    label: 'Price',
                    data: [],
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                ...this.defaultOptions,
                plugins: {
                    ...this.defaultOptions.plugins,
                    title: {
                        display: true,
                        text: `Price History - ${period}`
                    }
                }
            }
        });

        this.charts.set(canvasId, chart);
        this.loadLineData(instrumentId, period, canvasId);

        return chart;
    }

    // Load line chart data
    async loadLineData(instrumentId, period, canvasId) {
        try {
            const prices = await this.generateDemoPrices(30);
            this.updateLineChart(canvasId, prices);
        } catch (error) {
            console.error('Failed to load line data:', error);
        }
    }

    // Update line chart
    updateLineChart(canvasId, prices) {
        const chart = this.charts.get(canvasId);
        if (!chart) return;

        const lineData = prices.map((price, index) => ({
            x: new Date(Date.now() - (prices.length - index) * 60000),
            y: price
        }));

        chart.data.datasets[0].data = lineData;
        chart.update('none');
    }

    // Initialize performance chart for user trades
    initPerformanceChart(canvasId) {
        const ctx = document.getElementById(canvasId).getContext('2d');

        if (this.charts.has(canvasId)) {
            this.charts.get(canvasId).destroy();
        }

        const chart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Won', 'Lost', 'Active'],
                datasets: [{
                    label: 'Trades',
                    data: [0, 0, 0],
                    backgroundColor: [
                        'rgba(40, 167, 69, 0.8)',
                        'rgba(220, 53, 69, 0.8)',
                        'rgba(255, 193, 7, 0.8)'
                    ],
                    borderColor: [
                        'rgba(40, 167, 69, 1)',
                        'rgba(220, 53, 69, 1)',
                        'rgba(255, 193, 7, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                ...this.defaultOptions,
                indexAxis: 'y',
                plugins: {
                    ...this.defaultOptions.plugins,
                    title: {
                        display: true,
                        text: 'Trade Performance'
                    }
                }
            }
        });

        this.charts.set(canvasId, chart);
        this.loadPerformanceData(canvasId);

        return chart;
    }

    // Load performance data
    async loadPerformanceChart(canvasId) {
        try {
            const stats = await $.get('/History/GetStats');
            this.updatePerformanceChart(canvasId, stats);
        } catch (error) {
            console.error('Failed to load performance data:', error);
        }
    }

    // Update performance chart
    updatePerformanceChart(canvasId, stats) {
        const chart = this.charts.get(canvasId);
        if (!chart) return;

        chart.data.datasets[0].data = [
            stats.wonTrades || 0,
            stats.lostTrades || 0,
            stats.activeTrades || 0
        ];

        chart.update();
    }

    // Initialize profit/loss chart
    initPnLChart(canvasId) {
        const ctx = document.getElementById(canvasId).getContext('2d');

        if (this.charts.has(canvasId)) {
            this.charts.get(canvasId).destroy();
        }

        const chart = new Chart(ctx, {
            type: 'line',
            data: {
                datasets: [{
                    label: 'Profit/Loss',
                    data: [],
                    borderColor: 'rgba(40, 167, 69, 1)',
                    backgroundColor: 'rgba(40, 167, 69, 0.1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                ...this.defaultOptions,
                plugins: {
                    ...this.defaultOptions.plugins,
                    title: {
                        display: true,
                        text: 'Profit/Loss Over Time'
                    }
                }
            }
        });

        this.charts.set(canvasId, chart);
        this.loadPnLData(canvasId);

        return chart;
    }

    // Generate demo candle data
    generateDemoCandles(count) {
        return new Promise((resolve) => {
            const candles = [];
            let price = 100 + Math.random() * 10; // Start between 100-110

            for (let i = 0; i < count; i++) {
                const change = (Math.random() - 0.5) * 2; // -1 to +1
                const open = price;
                const close = price + change;
                const high = Math.max(open, close) + Math.random() * 0.5;
                const low = Math.min(open, close) - Math.random() * 0.5;
                const volume = Math.random() * 1000 + 100;

                candles.push({
                    time: new Date(Date.now() - (count - i) * 300000), // 5 min intervals
                    open: open,
                    high: high,
                    low: low,
                    close: close,
                    volume: volume
                });

                price = close;
            }

            resolve(candles);
        });
    }

    // Generate demo price data
    generateDemoPrices(count) {
        return new Promise((resolve) => {
            const prices = [];
            let price = 100 + Math.random() * 10;

            for (let i = 0; i < count; i++) {
                const change = (Math.random() - 0.5) * 0.5; // Smaller changes
                price += change;
                prices.push(price);
            }

            resolve(prices);
        });
    }

    // Get instrument symbol (would normally call API)
    getInstrumentSymbol(instrumentId) {
        const symbols = {
            1: 'EUR/USD',
            2: 'GBP/USD',
            3: 'USD/JPY',
            4: 'BTC/USD',
            5: 'ETH/USD',
            6: 'XAU/USD'
        };

        return symbols[instrumentId] || 'Unknown';
    }

    // Add real-time price update to chart
    addRealTimePrice(chartId, newPrice) {
        const chart = this.charts.get(chartId);
        if (!chart || chart.config.type !== 'line') return;

        const now = new Date();
        const data = chart.data.datasets[0].data;

        // Add new point
        data.push({
            x: now,
            y: newPrice
        });

        // Keep only last 50 points
        if (data.length > 50) {
            data.shift();
        }

        chart.update('none');
    }

    // Destroy a chart
    destroyChart(canvasId) {
        if (this.charts.has(canvasId)) {
            this.charts.get(canvasId).destroy();
            this.charts.delete(canvasId);
        }
    }

    // Destroy all charts
    destroyAllCharts() {
        this.charts.forEach((chart, canvasId) => {
            chart.destroy();
        });
        this.charts.clear();
    }

    // Export chart as image
    exportChartAsImage(canvasId, filename = 'chart.png') {
        const chart = this.charts.get(canvasId);
        if (!chart) return;

        const link = document.createElement('a');
        link.download = filename;
        link.href = chart.toBase64Image();
        link.click();
    }

    // Get chart data
    getChartData(canvasId) {
        const chart = this.charts.get(canvasId);
        return chart ? chart.data : null;
    }
}

// Global chart instance
const tradingCharts = new TradingCharts();

// Initialize when document is ready
$(document).ready(function() {
    // Auto-initialize charts if canvas elements exist
    $('[data-chart-type]').each(function() {
        const $canvas = $(this);
        const chartType = $canvas.data('chart-type');
        const instrumentId = $canvas.data('instrument-id');
        const timeframe = $canvas.data('timeframe') || '5m';

        switch (chartType) {
            case 'candle':
                tradingCharts.initCandleChart($canvas.attr('id'), instrumentId, timeframe);
                break;
            case 'line':
                tradingCharts.initLineChart($canvas.attr('id'), instrumentId, timeframe);
                break;
            case 'performance':
                tradingCharts.initPerformanceChart($canvas.attr('id'));
                break;
            case 'pnl':
                tradingCharts.initPnLChart($canvas.attr('id'));
                break;
        }
    });
});

// Add Chart.js candlestick controller if not already registered
if (Chart.controllers.candlestick === undefined) {
    Chart.register({
        id: 'candlestick',
        controller: {
            // Basic candlestick implementation
            // In production, you'd want a proper candlestick controller
        }
    });
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { TradingCharts, tradingCharts };
} else {
    window.TradingCharts = TradingCharts;
    window.tradingCharts = tradingCharts;
}