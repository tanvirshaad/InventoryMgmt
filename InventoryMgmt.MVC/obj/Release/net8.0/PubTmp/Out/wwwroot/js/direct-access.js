// direct-access.js - A direct implementation of the access control functionality

document.addEventListener('DOMContentLoaded', function() {
    // Get references to the elements with error checking
    const addUserBtn = document.getElementById('add-user-btn');
    const userInput = document.getElementById('add-user-input');
    const statusArea = document.getElementById('access-status');
    const autocompleteResults = document.getElementById('user-autocomplete-results');
    
    if (!autocompleteResults) {
        console.error('user-autocomplete-results element not found!');
    }
    
    // Current sort state
    let currentSortField = 'name'; // Default sort by name
    let usersList = []; // Store the users list for sorting
    
    // Load the list of users with access
    const accessUsersList = document.getElementById('access-users-list');
    if (accessUsersList) {
        loadAccessUsers();
    }
    
    // Initialize sort buttons - using event delegation for reliability
    document.addEventListener('click', function(e) {
        if (e.target && e.target.classList.contains('sort-users-btn') || 
            (e.target.parentElement && e.target.parentElement.classList.contains('sort-users-btn'))) {
            
            // Get the button element
            const button = e.target.classList.contains('sort-users-btn') ? 
                           e.target : e.target.parentElement;
            
            // Log the sort button clicked
            console.log("Sort button clicked:", button.getAttribute('data-sort'));
            
            // Remove active class from all buttons
            document.querySelectorAll('.sort-users-btn').forEach(btn => {
                btn.classList.remove('active');
            });
            
            // Add active class to clicked button
            button.classList.add('active');
            
            // Update sort field
            currentSortField = button.getAttribute('data-sort');
            
            // Resort the user list
            sortAndDisplayUsers();
        }
    });
    
    // Implement autocomplete functionality
    if (userInput && autocompleteResults) {
        // Store all users once loaded
        let allUsersLoaded = false;
        
        // Debounce function for input
        let typingTimer;
        const doneTypingInterval = 300;
        
        userInput.addEventListener('input', function() {
            const query = this.value.trim();
            
            // Clear any existing timer
            clearTimeout(typingTimer);
            
            // Show autocomplete and filter results
            if (allUsersLoaded) {
                autocompleteResults.style.display = 'block';
                filterUserResults(query);
            } else {
                // Load all users on first input
                fetchUserSuggestions('', true);
                allUsersLoaded = true;
                // Still filter by the current query
                setTimeout(() => {
                    filterUserResults(query);
                }, 300);
            }
        });
        
        // Handle focus events - load all users on focus
        userInput.addEventListener('focus', function() {
            if (!allUsersLoaded) {
                fetchUserSuggestions('', true); // Load all users
                allUsersLoaded = true;
            } else {
                autocompleteResults.style.display = 'block';
            }
        });
        
        // Close autocomplete on click outside
        document.addEventListener('click', function(e) {
            if (e.target !== userInput && e.target !== autocompleteResults && !autocompleteResults.contains(e.target)) {
                autocompleteResults.style.display = 'none';
            }
        });
        
        // Prevent closing when clicking inside the autocomplete dropdown
        autocompleteResults.addEventListener('click', function(e) {
            e.stopPropagation();
        });
        
        // Trigger load of all users immediately when the page loads
        setTimeout(() => {
            fetchUserSuggestions('', true);
            allUsersLoaded = true;
        }, 500); // Small delay to ensure DOM is fully ready
    } else {
        console.error('Cannot set up autocomplete - missing elements:', {
            userInput: !!userInput,
            autocompleteResults: !!autocompleteResults
        });
        
        // Try to find the element again after a delay
        setTimeout(() => {
            const delayedAutocompleteResults = document.getElementById('user-autocomplete-results');
            const delayedUserInput = document.getElementById('add-user-input');
            
            if (delayedAutocompleteResults && delayedUserInput) {
                setupAutocompleteDelayed(delayedUserInput, delayedAutocompleteResults);
            }
        }, 1000);
    }
    
    if (addUserBtn && userInput) {
        // Add click event listener
        addUserBtn.addEventListener('click', function(e) {
            e.preventDefault();
            
            const userEmail = userInput.value.trim();
            
            if (!userEmail) {
                showMessage('Please enter a user email', 'danger');
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
    
    });
    
    // Check for TempData messages on page load
    checkTempDataMessages();
    
    /**
     * Helper function to get inventory ID from URL if not available globally
     */
    function getInventoryIdFromUrl() {
        const urlMatch = window.location.pathname.match(/\/Inventory\/Details\/(\d+)/);
        return urlMatch ? parseInt(urlMatch[1]) : null;
    }
    
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
            
            // Store the users for sorting
            window.usersList = data.users;
            
            // Sort and display the users
            sortAndDisplayUsers();
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
