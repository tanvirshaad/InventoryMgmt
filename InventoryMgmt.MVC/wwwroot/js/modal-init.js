// modal-init.js - Initialize the item details modal

// This script runs as soon as it loads
console.log('modal-init.js loaded - initializing item details modal');

// Function to initialize the modal
function initializeItemDetailsModal() {
    console.log('Initializing item details modal');
    
    // Check if Bootstrap is loaded
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap is not loaded');
        return false;
    }
    
    // Check if the modal element exists
    const modalElement = document.getElementById('itemDetailsModal');
    if (!modalElement) {
        console.error('Modal element not found');
        return false;
    }
    
    console.log('Bootstrap version:', bootstrap.Tooltip.VERSION);
    console.log('Modal element found:', modalElement.id);
    
    // Create a Bootstrap modal instance
    try {
        // Store the modal instance globally so it can be accessed by other scripts
        window.itemDetailsModal = new bootstrap.Modal(modalElement);
        console.log('Modal instance created successfully');
        return true;
    } catch (error) {
        console.error('Error creating modal instance:', error);
        return false;
    }
}

// Override the item-link click handler to use our modal
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOMContentLoaded - Setting up item link handlers');
    
    // Initialize the modal
    const modalInitialized = initializeItemDetailsModal();
    console.log('Modal initialization result:', modalInitialized);
    
    // Function to handle item link clicks
    function handleItemLinkClick(e) {
        console.log('Item link clicked!');
        e.preventDefault();
        e.stopPropagation();
        
        const itemId = this.dataset.itemId;
        console.log('Item ID from link:', itemId);
        
        if (!itemId) {
            console.error('No item ID found in link');
            return;
        }
        
        console.log('Opening modal for item:', itemId);
        
        if (window.itemDetailsModal) {
            // Show the modal
            window.itemDetailsModal.show();
            
            // Load item details
            if (typeof loadItemDetailsInModal === 'function') {
                loadItemDetailsInModal(itemId);
            } else {
                console.error('loadItemDetailsInModal function not found');
                
                // Set a loading message in the modal
                const modalBody = document.getElementById('itemDetailsModalBody');
                if (modalBody) {
                    modalBody.innerHTML = '<div class="text-center"><div class="spinner-border text-primary" role="status"></div><p>Loading item details...</p></div>';
                }
                
                // Fetch item details directly
                fetch(`/Item/GetDetails/${itemId}`)
                    .then(response => response.json())
                    .then(data => {
                        console.log('Item details loaded:', data);
                        
                        // Display item details
                        if (modalBody) {
                            modalBody.innerHTML = `<div class="alert alert-success">Item ${data.name || data.customId || data.id} loaded</div>
                            <div class="mb-4">
                                <h5>Item Information</h5>
                                <p><strong>ID:</strong> ${data.customId || data.id}</p>
                                <p><strong>Created:</strong> ${new Date(data.createdAt).toLocaleString()}</p>
                            </div>`;
                            
                            // Use the clean itemData if available
                            if (data.itemData) {
                                modalBody.innerHTML += `<h5>Custom Fields</h5><ul>`;
                                
                                // Skip ID and Created as they're already shown above
                                for (const [key, value] of Object.entries(data.itemData)) {
                                    if (key !== 'ID' && key !== 'Created') {
                                        modalBody.innerHTML += `<li><strong>${key}:</strong> ${value}</li>`;
                                    }
                                }
                                
                                modalBody.innerHTML += `</ul>`;
                            }
                            // Fallback to custom fields if itemData is not available
                            else if (data.customFields && data.customFields.length > 0) {
                                const fieldsList = data.customFields.map(field => 
                                    `<li><strong>${field.name}:</strong> ${field.value !== null ? field.value : ''}</li>`
                                ).join('');
                                
                                modalBody.innerHTML += `<h5>Custom Fields</h5><ul>${fieldsList}</ul>`;
                            }
                            
                            // Show/hide edit button based on permissions
                            const editBtn = document.getElementById('editItemModalBtn');
                            if (editBtn) {
                                if (data.canEdit) {
                                    editBtn.style.display = 'inline-block';
                                    editBtn.setAttribute('data-item-id', data.id);
                                } else {
                                    editBtn.style.display = 'none';
                                }
                            }
                        }
                    })
                    .catch(error => {
                        console.error('Error loading item details:', error);
                        if (modalBody) {
                            modalBody.innerHTML = '<div class="alert alert-danger">Error loading item details</div>';
                        }
                    });
            }
        } else {
            console.error('Modal instance not available - falling back to navigation');
            window.location.href = this.href;
        }
    }
    
    // Add click handlers to all item links
    const itemLinks = document.querySelectorAll('.item-link');
    console.log('Found item links:', itemLinks.length);
    
    itemLinks.forEach(link => {
        link.addEventListener('click', handleItemLinkClick);
        console.log('Added click handler to link:', link.innerText, 'with item ID:', link.dataset.itemId);
    });
});
