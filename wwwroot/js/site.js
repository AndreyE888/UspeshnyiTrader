// Global site functionality

// Initialize tooltips
$(document).ready(function() {
    $('[data-bs-toggle="tooltip"]').tooltip();
});

// Auto-dismiss alerts
$(document).ready(function() {
    setTimeout(function() {
        $('.alert').fadeTo(500, 0).slideUp(500, function() {
            $(this).remove();
        });
    }, 5000);
});

$(document).ready(function() {
    // При загрузке страницы устанавливаем ВСЕМ ценам зеленый цвет
    $('.instrument-price').each(function() {
        $(this).addClass('text-success');
        $(this).data('last-price', parseFloat($(this).text().replace(/[^\d.-]/g, '')) || 0);
    });

    // Обновляем цены каждую секунду
    setInterval(updateAllPrices, 1000);
});

// Format currency
function formatCurrency(amount) {
    return '$' + parseFloat(amount).toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 4
    });
}

// Format percentage
function formatPercentage(value) {
    return (value * 100).toFixed(1) + '%';
}

// Format date/time
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Calculate time remaining
function getTimeRemaining(endTime) {
    const now = new Date().getTime();
    const end = new Date(endTime).getTime();
    const distance = end - now;

    if (distance < 0) {
        return 'Expired';
    }

    const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((distance % (1000 * 60)) / 1000);

    return `${minutes}m ${seconds}s`;
}

// Update countdown timers
function updateCountdowns() {
    $('.countdown-timer').each(function() {
        const endTime = $(this).data('end-time');
        if (endTime) {
            $(this).text(getTimeRemaining(endTime));
        }
    });
}

// Start countdown updates
setInterval(updateCountdowns, 1000);

// AJAX error handler
$(document).ajaxError(function(event, jqxhr, settings, thrownError) {
    // Игнорируем ошибки на странице Profile
    if (window.location.pathname.includes('/Account/Profile')) {
        console.log('AJAX Error on Profile page (ignored):', settings.url);
        return; // Не показываем alert на странице профиля
    }

    console.error('AJAX Error:', thrownError);
    showAlert('An error occurred. Please try again.', 'danger');
});

// Show alert message
function showAlert(message, type = 'info') {
    const alertClass = `alert-${type}`;
    const alertHtml = `
        <div class="alert alert-trade alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $('#alertsContainer').append(alertHtml);

    // Auto-remove after 5 seconds
    setTimeout(function() {
        $(`.alert`).fadeTo(500, 0).slideUp(500, function() {
            $(this).remove();
        });
    }, 5000);
}

// Confirm dialog
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Toggle loading state for buttons
function setButtonLoading(button, isLoading) {
    const $button = $(button);
    if (isLoading) {
        $button.prop('disabled', true);
        $button.data('original-text', $button.html());
        $button.html('<span class="loading-spinner me-2"></span>Loading...');
    } else {
        $button.prop('disabled', false);
        $button.html($button.data('original-text'));
    }
}

// Debounce function for search inputs
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Get user balance
function updateUserBalance() {
    $.get('/Trading/GetUserBalance', function(data) {
        if (data.balance !== undefined) {
            $('.balance-display, #userBalance').text(formatCurrency(data.balance));
        }
    }).fail(function() {
        console.error('Failed to fetch user balance');
    });
}

// Update prices for all instruments
function updateAllPrices() {
    $.get('/Instruments/GetPrices', function(prices) {
        for (const instrumentId in prices) {
            const priceElement = $(`#price-${instrumentId}`);
            const oldPrice = parseFloat(priceElement.data('last-price')) || prices[instrumentId];
            const newPrice = prices[instrumentId];

            priceElement.data('last-price', newPrice);
            priceElement.text(formatCurrency(newPrice));

            // Определяем цвет по логике
            if (newPrice < oldPrice) {
                priceElement.removeClass('text-success').addClass('text-danger')
                    .css('color', '#dc3545'); // Красный
            } else {
                priceElement.removeClass('text-danger').addClass('text-success')
                    .css('color', '#28a745'); // Зеленый
            }
        }
    });
}

// ПРОСТО ДОБАВЛЯЕМ КРАСНЫЙ КЛАСС ТОЛЬКО КОГДА НУЖНО
// УВАЖАЕМ СУЩЕСТВУЮЩИЕ КЛАССЫ, НО ИСПРАВЛЯЕМ ЧЕРНЫЙ
// СОХРАНЯЕМ ЛОГИКУ ЦВЕТОВ И УБИРАЕМ ТОЛЬКО ЧЕРНЫЙ
setInterval(function() {
    $('.instrument-price, .price-cell, [id*="price"]').each(function() {
        const $element = $(this);
        const currentText = $element.text();
        const price = parseFloat(currentText.replace(/[^\d.-]/g, '')) || 0;
        const lastPrice = parseFloat($element.data('last-price')) || price;

        // Сохраняем текущую цену для сравнения
        $element.data('last-price', price);

        // УДАЛЯЕМ ТОЛЬКО КЛАССЫ КОТОРЫЕ ДЕЛАЮТ ЧЕРНЫЙ
        $element.removeClass('text-dark text-secondary text-muted text-black text-body');

        // ОПРЕДЕЛЯЕМ ЦВЕТ ПО ЛОГИКЕ
        if (price < lastPrice) {
            $element.removeClass('text-success').addClass('text-danger')
                .css({'color': '#dc3545', 'font-weight': 'bold'});
        } else {
            $element.removeClass('text-danger').addClass('text-success')
                .css({'color': '#28a745', 'font-weight': 'bold'});
        }
    });
}, 1000);



// Format trade direction with icon
function formatTradeDirection(direction) {
    if (direction.toLowerCase() === 'up') {
        return '<span class="text-success">▲ UP</span>';
    } else {
        return '<span class="text-danger">▼ DOWN</span>';
    }
}

// Format trade status with badge
function formatTradeStatus(status) {
    const statusClass = {
        'active': 'bg-warning',
        'won': 'bg-success',
        'lost': 'bg-danger',
        'cancelled': 'bg-secondary'
    }[status.toLowerCase()] || 'bg-secondary';

    return `<span class="badge ${statusClass}">${status.toUpperCase()}</span>`;
}

// Calculate potential payout
function calculatePayout(amount) {
    return amount * 1.8; // 80% return
}

// Update potential payout display
function updatePayoutDisplay(amount) {
    const payout = calculatePayout(amount);
    $('#potentialPayout').text(formatCurrency(payout));
}


// Initialize when document is ready
$(document).ready(function() {
    // Update user balance
    if ($('.balance-display, #userBalance').length > 0) {
        updateUserBalance();
    }

    // Update prices every 3 seconds
    if ($('[id^="price-"]').length > 0) {
        setInterval(updateAllPrices, 1000);
    }

    // Initialize any countdowns
    updateCountdowns();

    // Add alert container if it doesn't exist
    if ($('#alertsContainer').length === 0) {
        $('main').prepend('<div id="alertsContainer"></div>');
    }
});

// Export functions for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        formatCurrency,
        formatPercentage,
        formatDateTime,
        showAlert,
        setButtonLoading,
        updateUserBalance
    };
}