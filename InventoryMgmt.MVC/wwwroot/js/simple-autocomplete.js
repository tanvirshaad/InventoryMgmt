// Simple autocomplete implementation for inventory access control
(function() {
    'use strict';
    
    let allUsers = [];
    let isInitialized = false;
    
    function initAutocomplete() {
        const userInput = document.getElementById('add-user-input');
        const autocompleteResults = document.getElementById('user-autocomplete-results');
        
        if (!userInput || !autocompleteResults) {
            return false;
        }
        
        // Load all users immediately
        loadAllUsers().then(() => {
            // Input event
            userInput.addEventListener('input', function(e) {
                filterAndShowResults(e.target.value);
            });
            
            // Focus event
            userInput.addEventListener('focus', function() {
                filterAndShowResults('');
            });
            
            // Click outside to close
            document.addEventListener('click', function(e) {
                if (!userInput.contains(e.target) && !autocompleteResults.contains(e.target)) {
                    autocompleteResults.style.display = 'none';
                }
            });
            
            isInitialized = true;
        });
        
        return true;
    }
    
    async function loadAllUsers() {
        try {
            // Get inventory ID from global variable or URL
            const inventoryId = window.currentInventoryId || getInventoryIdFromUrl();
            const searchUrl = inventoryId ? 
                `/Access/SearchUsers?term=&inventoryId=${inventoryId}` : 
                '/Access/SearchUsers?term=';
            
            const response = await fetch(searchUrl);
            const users = await response.json();
            allUsers = users;
            return users;
        } catch (error) {
            console.error('Error loading users:', error);
            allUsers = [];
            return [];
        }
    }
    
    function getInventoryIdFromUrl() {
        const urlMatch = window.location.pathname.match(/\/Inventory\/Details\/(\d+)/);
        return urlMatch ? parseInt(urlMatch[1]) : null;
    }
    
    function filterAndShowResults(query) {
        const autocompleteResults = document.getElementById('user-autocomplete-results');
        if (!autocompleteResults) return;
        
        const filteredUsers = allUsers.filter(user => {
            const searchText = (user.name + ' ' + user.email).toLowerCase();
            return searchText.includes(query.toLowerCase());
        });
        
        if (filteredUsers.length === 0) {
            autocompleteResults.innerHTML = '<div class="p-2 text-muted small">No users found</div>';
        } else {
            let html = '';
            filteredUsers.forEach(user => {
                html += `
                    <div class="user-item d-flex justify-content-between align-items-center p-2" 
                         data-email="${user.email}" data-user-id="${user.id}"
                         style="cursor: pointer; border-bottom: 1px solid #f0f0f0;">
                        <div>
                            <div>${user.name}</div>
                            <small class="text-muted">${user.email}</small>
                        </div>
                    </div>
                `;
            });
            autocompleteResults.innerHTML = html;
            
            // Add click handlers
            autocompleteResults.querySelectorAll('.user-item').forEach(item => {
                item.addEventListener('click', function() {
                    const email = this.getAttribute('data-email');
                    const userInput = document.getElementById('add-user-input');
                    if (userInput) {
                        userInput.value = email;
                    }
                    autocompleteResults.style.display = 'none';
                });
            });
        }
        
        autocompleteResults.style.display = 'block';
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAutocomplete);
    } else {
        // DOM already loaded
        initAutocomplete();
    }
    
    // Also try after a delay
    setTimeout(initAutocomplete, 1000);
    
})();
