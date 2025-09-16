/**
 * Support Ticket Functionality
 * Handles creation and submission of support tickets through the modal interface
 */
const SupportTicket = (function() {
    // DOM elements
    const helpButton = document.getElementById('helpButton');
    const createSupportTicketLink = document.getElementById('createSupportTicketLink');
    const supportTicketModal = document.getElementById('supportTicketModal');
    const supportTicketForm = document.getElementById('supportTicketForm');
    const submitSupportTicketButton = document.getElementById('submitSupportTicket');
    const sourceUrlInput = document.getElementById('sourceUrl');
    const inventoryIdInput = document.getElementById('inventoryId');
    
    // Get CSRF token from meta tag
    const csrfToken = document.querySelector('meta[name="__RequestVerificationToken"]').content;
    
    /**
     * Initialize support ticket functionality
     */
    function init() {
        // Attach event listeners
        if (helpButton) {
            helpButton.addEventListener('click', openSupportTicketModal);
        }
        
        if (createSupportTicketLink) {
            createSupportTicketLink.addEventListener('click', function(e) {
                e.preventDefault();
                openSupportTicketModal();
            });
        }
        
        if (submitSupportTicketButton) {
            submitSupportTicketButton.addEventListener('click', submitSupportTicket);
        }
    }
    
    /**
     * Opens the support ticket modal and pre-populates URL information
     */
    function openSupportTicketModal() {
        // Get current URL to associate with the ticket
        sourceUrlInput.value = window.location.href;
        
        // Try to get inventory ID from the current URL if applicable
        const urlParams = new URLSearchParams(window.location.search);
        const idParam = urlParams.get('id');
        
        // Check if we're on an inventory page (multiple patterns)
        const path = window.location.pathname.toLowerCase();
        const isInventoryPage = path.includes('/inventory/details/') || 
                               path.includes('/inventory/edit/') || 
                               (path.includes('/inventory/') && idParam);
        
        if (isInventoryPage && idParam) {
            inventoryIdInput.value = idParam;
        } else {
            // Check if there's an inventory ID in the URL path (like /Inventory/Details/5)
            const pathSegments = window.location.pathname.split('/');
            const inventoryIndex = pathSegments.findIndex(segment => 
                segment.toLowerCase() === 'inventory' || segment.toLowerCase() === 'details'
            );
            
            if (inventoryIndex !== -1 && inventoryIndex < pathSegments.length - 1) {
                const possibleId = pathSegments[inventoryIndex + 1];
                if (possibleId && !isNaN(possibleId)) {
                    inventoryIdInput.value = possibleId;
                } else {
                    inventoryIdInput.value = '';
                }
            } else {
                inventoryIdInput.value = '';
            }
        }
        
        // Show the modal
        const modal = new bootstrap.Modal(supportTicketModal);
        modal.show();
    }
    
    /**
     * Submits the support ticket form data to the API
     */
    async function submitSupportTicket() {
        try {
            // Get form data
            const formData = {
                summary: document.getElementById('summary').value,
                priority: document.getElementById('priority').value,
                additionalInfo: document.getElementById('additionalInfo').value,
                sourceUrl: sourceUrlInput.value,
                inventoryId: inventoryIdInput.value || null
            };
            
            // Validate required fields
            if (!formData.summary || !formData.priority) {
                ToastUtility.show('Please fill in all required fields.', 'warning');
                return;
            }
            
            // Disable submit button during API call
            submitSupportTicketButton.disabled = true;
            submitSupportTicketButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Submitting...';
            
            // Submit to API
            const response = await fetch('/api/SupportTicket', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': csrfToken
                },
                body: JSON.stringify(formData)
            });
            
            const result = await response.json();
            
            // Re-enable submit button
            submitSupportTicketButton.disabled = false;
            submitSupportTicketButton.innerHTML = '<i class="bi bi-send me-2"></i>Submit Ticket';
            
            if (response.ok && result.success) {
                // Close the modal
                bootstrap.Modal.getInstance(supportTicketModal).hide();
                
                // Reset form
                supportTicketForm.reset();
                
                // Show success message
                ToastUtility.show('Your support ticket has been successfully submitted!', 'success');
            } else {
                // Show error message
                ToastUtility.show(result.message || 'Failed to submit support ticket. Please try again.', 'danger');
            }
        } catch (error) {
            console.error('Error submitting support ticket:', error);
            
            // Re-enable submit button
            submitSupportTicketButton.disabled = false;
            submitSupportTicketButton.innerHTML = '<i class="bi bi-send me-2"></i>Submit Ticket';
            
            // Show error message
            ToastUtility.show('An unexpected error occurred. Please try again later.', 'danger');
        }
    }
    
    // Return public methods
    return {
        init: init
    };
})();

// Initialize support ticket functionality when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    SupportTicket.init();
});