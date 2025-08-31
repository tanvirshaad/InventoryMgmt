// direct-access.js - A direct implementation of the access control functionality

document.addEventListener('DOMContentLoaded', function() {
    console.log('Direct access script loaded');
    
    // Get references to the elements
    const addUserBtn = document.getElementById('add-user-btn');
    const userInput = document.getElementById('add-user-input');
    const statusArea = document.getElementById('access-status');
    
    // Load the list of users with access
    const accessUsersList = document.getElementById('access-users-list');
    if (accessUsersList) {
        loadAccessUsers();
    }
    
    if (addUserBtn && userInput) {
        console.log('Found add user button and input field');
        
        // Add click event listener
        addUserBtn.addEventListener('click', function(e) {
            e.preventDefault();
            
            const userEmail = userInput.value.trim();
            console.log('Email entered:', userEmail);
            
            if (!userEmail) {
                showMessage('Please enter a user email', 'danger');
                return;
            }
            
            // Validate email format (basic validation)
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(userEmail)) {
                showMessage('Please enter a valid email address', 'danger');
                return;
            }
            
            // Extract inventory ID from URL
            let inventoryId = null;
            const urlMatch = window.location.pathname.match(/\/Inventory\/Details\/(\d+)/);
            if (urlMatch && urlMatch[1]) {
                inventoryId = urlMatch[1];
            } else {
                // Try to get from the page
                inventoryId = document.querySelector('input[name="InventoryId"]')?.value;
            }
            
            if (!inventoryId) {
                // Try one more fallback
                const idFromUI = document.querySelector('.inventory-id-value')?.textContent;
                if (idFromUI) {
                    inventoryId = idFromUI.trim();
                } else {
                    console.log('Could not find inventory ID, using fallback');
                }
            }
            
            if (!inventoryId) {
                showMessage('Could not determine inventory ID. Please try again or contact support.', 'danger');
                return;
            }
            
            console.log('Adding user', userEmail, 'to inventory', inventoryId);
            
            // Show loading message
            showMessage('Processing... Adding user to inventory', 'info');
            
            // Use simple GET endpoint with query parameters
            window.location.href = `/Access/AddUserSimple?inventoryId=${inventoryId}&email=${encodeURIComponent(userEmail)}`;
        });
        
        // Add enter key support for the input
        userInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                addUserBtn.click();
            }
        });
    }
    
    // Check for TempData messages on page load
    checkTempDataMessages();
    
    // Handle remove buttons
    document.addEventListener('click', function(e) {
        if (e.target && (e.target.classList.contains('remove-access') || e.target.closest('.remove-access'))) {
            e.preventDefault();
            
            // Get the button or its parent if the click was on an icon
            const button = e.target.classList.contains('remove-access') ? e.target : e.target.closest('.remove-access');
            const userId = button.getAttribute('data-user-id');
            
            // Get inventory ID from URL
            let inventoryId = null;
            const urlMatch = window.location.pathname.match(/\/Inventory\/Details\/(\d+)/);
            if (urlMatch && urlMatch[1]) {
                inventoryId = urlMatch[1];
            } else {
                // Try to get from the page
                inventoryId = document.querySelector('input[name="InventoryId"]')?.value;
            }
            
            if (!inventoryId) {
                showMessage('Could not determine inventory ID. Please refresh the page and try again.', 'danger');
                return;
            }
            
            if (confirm('Are you sure you want to remove this user from the inventory?')) {
                // Show loading message
                showMessage('Processing... Removing user from inventory', 'info');
                
                // Create a form for POST request with anti-forgery token
                const form = document.createElement('form');
                form.method = 'POST';
                form.action = '/Access/RemoveUserAccess';
                form.style.display = 'none';
                
                // Add inventory ID input
                const inventoryIdInput = document.createElement('input');
                inventoryIdInput.type = 'hidden';
                inventoryIdInput.name = 'inventoryId';
                inventoryIdInput.value = inventoryId;
                form.appendChild(inventoryIdInput);
                
                // Add user ID input
                const userIdInput = document.createElement('input');
                userIdInput.type = 'hidden';
                userIdInput.name = 'userId';
                userIdInput.value = userId;
                form.appendChild(userIdInput);
                
                // Add anti-forgery token
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenInput) {
                    const token = tokenInput.cloneNode(true);
                    form.appendChild(token);
                }
                
                // Add form to body and submit
                document.body.appendChild(form);
                form.submit();
            }
        }
    });
});

/**
 * Shows a message in the status area
 * @param {string} message - The message to display
 * @param {string} type - The type of message (success, danger, info, warning)
 */
function showMessage(message, type = 'info') {
    const statusArea = document.getElementById('access-status');
    if (!statusArea) {
        // If status area doesn't exist, create one
        const container = document.querySelector('.access-container') || document.querySelector('.tab-pane.active');
        
        if (container) {
            const newStatusArea = document.createElement('div');
            newStatusArea.id = 'access-status';
            newStatusArea.className = `alert alert-${type} mt-2`;
            newStatusArea.style.display = 'block';
            newStatusArea.textContent = message;
            
            // Insert at the beginning of the container
            container.insertBefore(newStatusArea, container.firstChild);
            
            // Hide after 5 seconds
            setTimeout(() => {
                newStatusArea.style.display = 'none';
            }, 5000);
        } else {
            // Fallback to alert if no container is found
            alert(message);
        }
    } else {
        // Use the existing status area
        statusArea.className = `alert alert-${type} mt-2`;
        statusArea.style.display = 'block';
        statusArea.textContent = message;
        
        // Hide after 5 seconds
        setTimeout(() => {
            statusArea.style.display = 'none';
        }, 5000);
    }
}

/**
 * Checks for TempData messages in the page and displays them
 */
function checkTempDataMessages() {
    // Look for hidden message elements that might be rendered by TempData
    const successMessage = document.getElementById('temp-success-message');
    const errorMessage = document.getElementById('temp-error-message');
    const infoMessage = document.getElementById('temp-info-message');
    const warningMessage = document.getElementById('temp-warning-message');
    
    if (successMessage && successMessage.textContent) {
        showMessage(successMessage.textContent, 'success');
    }
    
    if (errorMessage && errorMessage.textContent) {
        showMessage(errorMessage.textContent, 'danger');
    }
    
    if (infoMessage && infoMessage.textContent) {
        showMessage(infoMessage.textContent, 'info');
    }
    
    if (warningMessage && warningMessage.textContent) {
        showMessage(warningMessage.textContent, 'warning');
    }
}

/**
 * Loads the list of users with access to the inventory
 */
function loadAccessUsers() {
    // Get inventory ID from URL
    let inventoryId = null;
    const urlMatch = window.location.pathname.match(/\/Inventory\/Details\/(\d+)/);
    
    if (urlMatch && urlMatch[1]) {
        inventoryId = urlMatch[1];
    } else {
        // Try to get from the page
        inventoryId = document.querySelector('input[name="InventoryId"]')?.value;
    }
    
    if (!inventoryId) {
        showMessage('Could not determine inventory ID. Please try again or contact support.', 'danger');
        return;
    }
    
    // Create loading indicator
    const accessUsersList = document.getElementById('access-users-list');
    accessUsersList.innerHTML = `
        <div class="text-center py-3">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted mt-2">Loading users...</p>
        </div>
    `;
    
    // Fetch users with access
    fetch(`/Inventory/GetAccessUsers?id=${inventoryId}`)
        .then(response => response.json())
        .then(data => {
            console.log('Access users loaded:', data);
            
            // Check if inventory is public
            if (data.isPublic) {
                accessUsersList.innerHTML = `
                    <div class="alert alert-info">
                        <i class="bi bi-info-circle me-2"></i>
                        This inventory is public, so access control is not applicable.
                        To manage specific user access, make this inventory private in the Settings tab.
                    </div>
                `;
                return;
            }
            
            // Check if there are any users
            if (!data.users || data.users.length === 0) {
                accessUsersList.innerHTML = '<p class="text-muted">No users have access to this inventory.</p>';
                return;
            }
            
            // Render the users
            let html = '';
            data.users.forEach(user => {
                html += `
                    <div class="access-user-item d-flex justify-content-between align-items-center p-2 border rounded mb-2">
                        <div>
                            <strong>${user.firstName} ${user.lastName}</strong>
                            <br><small class="text-muted">${user.email}</small>
                        </div>
                        <div class="d-flex gap-2 align-items-center">
                        
                            <!-- Permission select dropdown (temporarily disabled)
                            <select class="form-select form-select-sm permission-select" data-user-id="${user.id}" style="width: 120px;">
                                <option value="Read">Read</option>
                                <option value="Write" selected>Write</option>
                                <option value="Manage">Manage</option>
                                <option value="FullControl">Full Control</option>
                            </select>
                            -->
                        
                            <button class="btn btn-sm btn-outline-danger remove-access" data-user-id="${user.id}">
                                <i class="bi bi-x-circle"></i>
                            </button>
                        </div>
                    </div>
                `;
            });
            
            accessUsersList.innerHTML = html;
        })
        .catch(error => {
            console.error('Error loading access users:', error);
            accessUsersList.innerHTML = `
                <div class="alert alert-danger">
                    Error loading users. Please try refreshing the page.
                </div>
            `;
        });
}
