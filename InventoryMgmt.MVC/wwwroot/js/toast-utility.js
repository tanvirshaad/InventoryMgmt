/**
 * Toast Utility - A modern replacement for JavaScript alerts
 * 
 * This utility creates Bootstrap toasts for showing notifications
 * instead of using standard browser alerts.
 */

const ToastUtility = {
    /**
     * Container element for toasts
     * @type {HTMLElement}
     */
    toastContainer: null,
    
    /**
     * Configuration options for toasts
     */
    config: {
        position: 'bottom-end', // top-start, top-center, top-end, middle-start, middle-center, middle-end, bottom-start, bottom-center, bottom-end
        maxToasts: 4, // Maximum number of toasts to show at once
        newestOnTop: true // Whether to show newest toasts on top
    },

    /**
     * Initialize the toast container
     */
    init: function(options) {
        // Apply custom options if provided
        if (options) {
            this.config = {...this.config, ...options};
        }
        
        // Check if container already exists
        if (document.getElementById('toast-container')) {
            this.toastContainer = document.getElementById('toast-container');
            return;
        }

        // Create toast container
        this.toastContainer = document.createElement('div');
        this.toastContainer.id = 'toast-container';
        
        // Set position based on config
        const positionClass = this.getPositionClass(this.config.position);
        this.toastContainer.className = `toast-container position-fixed ${positionClass} p-3`;
        this.toastContainer.style.zIndex = '1080';
        document.body.appendChild(this.toastContainer);
    },
    
    /**
     * Get the appropriate CSS class for the toast position
     * @param {string} position - The position string
     * @returns {string} The position CSS class
     */
    getPositionClass: function(position) {
        const posMap = {
            'top-start': 'top-0 start-0',
            'top-center': 'top-0 start-50 translate-middle-x',
            'top-end': 'top-0 end-0',
            'middle-start': 'top-50 start-0 translate-middle-y',
            'middle-center': 'top-50 start-50 translate-middle',
            'middle-end': 'top-50 end-0 translate-middle-y',
            'bottom-start': 'bottom-0 start-0',
            'bottom-center': 'bottom-0 start-50 translate-middle-x',
            'bottom-end': 'bottom-0 end-0'
        };
        
        return posMap[position] || 'bottom-0 end-0';
    },

    /**
     * Show a toast notification
     * @param {string} message - The message to display
     * @param {string} type - The type of toast (success, error, warning, info)
     * @param {Object} options - Options for the toast
     * @param {number} options.autohideDelay - Delay in ms before the toast auto-hides (default: 5000, 0 to disable)
     * @param {string} options.title - Custom title for the toast (defaults to capitalized type)
     * @param {boolean} options.closeButton - Whether to show a close button (default: true)
     */
    show: function(message, type = 'info', options = {}) {
        // Initialize if not already done
        if (!this.toastContainer) {
            this.init();
        }
        
        // Default options
        const defaults = {
            autohideDelay: type === 'error' ? 0 : 5000,
            title: type.charAt(0).toUpperCase() + type.slice(1),
            closeButton: true,
        };
        
        // Merge default options with provided options
        const settings = {...defaults, ...options};

        // Create a unique ID for the toast
        const toastId = 'toast-' + Date.now();
        
        // Set icon and color based on type
        let icon, bgClass;
        switch (type) {
            case 'success':
                icon = 'bi-check-circle-fill';
                bgClass = 'bg-success text-white';
                break;
            case 'error':
                icon = 'bi-exclamation-triangle-fill';
                bgClass = 'bg-danger text-white';
                break;
            case 'warning':
                icon = 'bi-exclamation-circle-fill';
                bgClass = 'bg-warning';
                break;
            case 'info':
            default:
                icon = 'bi-info-circle-fill';
                bgClass = 'bg-info text-white';
                break;
        }

        // Check if we need to remove older toasts because of the maximum limit
        while (this.toastContainer.children.length >= this.config.maxToasts) {
            // Remove the oldest toast if we're at the maximum
            if (this.config.newestOnTop) {
                this.toastContainer.removeChild(this.toastContainer.lastChild);
            } else {
                this.toastContainer.removeChild(this.toastContainer.firstChild);
            }
        }

        // Create the toast HTML
        const toastHtml = `
            <div id="${toastId}" class="toast toast-${type}" role="alert" aria-live="assertive" aria-atomic="true"
                 ${settings.autohideDelay > 0 ? `data-bs-delay="${settings.autohideDelay}"` : ''} 
                 data-bs-autohide="${settings.autohideDelay > 0 ? 'true' : 'false'}">
                <div class="toast-header ${bgClass}">
                    <i class="bi ${icon} me-2"></i>
                    <strong class="me-auto">${settings.title}</strong>
                    ${settings.closeButton ? `<button type="button" class="btn-close ${type !== 'warning' ? 'btn-close-white' : ''}" data-bs-dismiss="toast" aria-label="Close"></button>` : ''}
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        // Add the toast to the container (either at the beginning or end)
        if (this.config.newestOnTop) {
            this.toastContainer.insertAdjacentHTML('afterbegin', toastHtml);
        } else {
            this.toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        }

        // Get the toast element and initialize it with Bootstrap
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement);
        
        // Show the toast
        toast.show();

        // Remove the toast element after it's hidden if needed
        toastElement.addEventListener('hidden.bs.toast', function () {
            if (toastElement && toastElement.parentNode) {
                toastElement.parentNode.removeChild(toastElement);
            }
        });
        
        // Return the toast element for potential further manipulation
        return toastElement;
    },

    /**
     * Show a success toast
     * @param {string} message - The message to display
     * @param {Object|number} options - Options object or autohide delay
     */
    success: function(message, options = {}) {
        if (typeof options === 'number') {
            options = { autohideDelay: options };
        }
        return this.show(message, 'success', options);
    },

    /**
     * Show an error toast
     * @param {string} message - The message to display
     * @param {Object|number} options - Options object or autohide delay
     */
    error: function(message, options = {}) {
        if (typeof options === 'number') {
            options = { autohideDelay: options };
        }
        options.autohideDelay = options.autohideDelay || 0; // Default to no auto-hide for errors
        return this.show(message, 'error', options);
    },

    /**
     * Show a warning toast
     * @param {string} message - The message to display
     * @param {Object|number} options - Options object or autohide delay
     */
    warning: function(message, options = {}) {
        if (typeof options === 'number') {
            options = { autohideDelay: options };
        }
        options.autohideDelay = options.autohideDelay || 7000; // Default to 7 seconds for warnings
        return this.show(message, 'warning', options);
    },

    /**
     * Show an info toast
     * @param {string} message - The message to display
     * @param {Object|number} options - Options object or autohide delay
     */
    info: function(message, options = {}) {
        if (typeof options === 'number') {
            options = { autohideDelay: options };
        }
        return this.show(message, 'info', options);
    },
    
    /**
     * Analyze message text and show an appropriate toast type
     * This is helpful for converting alerts to toasts automatically
     * 
     * @param {string} message - Message to analyze and display
     * @param {Object} options - Toast options
     * @returns {HTMLElement} The toast element
     */
    smart: function(message, options = {}) {
        // Convert the message to lowercase for easier comparison
        const lowerMsg = message.toLowerCase();
        
        // Check for error indicators
        if (
            lowerMsg.includes('error') || 
            lowerMsg.includes('failed') || 
            lowerMsg.includes('invalid') || 
            lowerMsg.includes('cannot') ||
            lowerMsg.includes('could not')
        ) {
            return this.error(message, options);
        }
        
        // Check for warning indicators
        if (
            lowerMsg.includes('warning') || 
            lowerMsg.includes('caution') || 
            lowerMsg.includes('attention')
        ) {
            return this.warning(message, options);
        }
        
        // Check for success indicators
        if (
            lowerMsg.includes('success') || 
            lowerMsg.includes('successful') || 
            lowerMsg.includes('saved') ||
            lowerMsg.includes('created') ||
            lowerMsg.includes('updated') ||
            lowerMsg.includes('deleted')
        ) {
            return this.success(message, options);
        }
        
        // Default to info
        return this.info(message, options);
    },
    
    /**
     * Clear all toasts from the container
     */
    clear: function() {
        if (this.toastContainer) {
            this.toastContainer.innerHTML = '';
        }
    },
    
    /**
     * Change the position of the toast container
     * @param {string} position - New position (top-start, top-center, etc.)
     */
    setPosition: function(position) {
        if (this.toastContainer) {
            // Remove all position classes
            this.toastContainer.className = this.toastContainer.className
                .replace(/top-\d+|bottom-\d+|start-\d+|end-\d+|translate-middle(-x|-y)?/g, '')
                .trim();
            
            // Add the new position class
            const positionClass = this.getPositionClass(position);
            this.toastContainer.className += ' ' + positionClass;
            this.config.position = position;
        }
    }
};

// Initialize toast container when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    ToastUtility.init();
});

// Override the default alert function to use our toast utility instead
window.originalAlert = window.alert;
window.alert = function(message) {
    return ToastUtility.smart(message);
};

// Override the site.js showToast function to use our toast utility
if (typeof window.showToast !== 'undefined') {
    window.originalShowToast = window.showToast;
}

window.showToast = function(message, type = 'info', duration = 5000) {
    // Convert type from site.js format to ToastUtility format
    const typeMap = {
        'success': 'success',
        'error': 'error',
        'danger': 'error',
        'warning': 'warning',
        'info': 'info'
    };
    
    const toastType = typeMap[type] || 'info';
    return ToastUtility.show(message, toastType, { autohideDelay: duration });
};
