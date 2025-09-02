// inventory-detail.js - Handles selection and actions for inventory items

// Debug event handler to log all clicks on the page
$(document).on('click', function(e) {
    console.log('Document click event target:', e.target);
    console.log('Target classList:', e.target.classList);
    console.log('Target is like button:', $(e.target).is('.like-btn') || $(e.target).closest('.like-btn').length > 0);
    console.log('------');
});

// Initialize the inventory page
function initializeInventoryPage(inventoryId) {
    console.log("Initializing inventory page with ID:", inventoryId);
    
    // Handle tab switching
    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        const tabId = $(e.target).attr('id');
        console.log("Tab activated:", tabId);
        
        // If we're switching to the fields tab, ensure custom fields are loaded
        if (tabId === 'fields-tab') {
            // The custom fields manager will handle this
        }
        
        // Load custom ID elements when switching to custom ID tab
        if (tabId === 'customid-tab') {
            console.log("Loading custom ID elements from tab event");
            loadCustomIdElements(inventoryId);
        }
        
        // Load comments when switching to chat tab
        if (tabId === 'chat-tab') {
            loadComments(inventoryId);
        }
    });
    
    // Load custom ID elements if that tab is active initially
    if ($('#customid-tab').hasClass('active')) {
        console.log("Custom ID tab is active on page load, loading elements");
        loadCustomIdElements(inventoryId);
    }
    
    // Also check if the custom ID content panel is active
    if ($('#customid').hasClass('show active')) {
        console.log("Custom ID content panel is active on page load, loading elements");
        loadCustomIdElements(inventoryId);
    }
    
    // Load comments if chat tab is active
    if ($('#chat-tab').hasClass('active')) {
        loadComments(inventoryId);
    }
}

// Load custom ID elements
function loadCustomIdElements(inventoryId) {
    console.log("Loading custom ID elements for inventory ID:", inventoryId);
    
    if (!inventoryId) {
        console.error("Cannot load custom ID elements: No inventory ID provided");
        return;
    }
    
    // Clear any existing content first to avoid stale data
    const container = document.getElementById('custom-id-elements');
    if (container) {
        container.innerHTML = '<div class="alert alert-info">Loading custom ID elements...</div>';
    }
    
    $.ajax({
        url: `/Inventory/GetCustomIdElements?id=${inventoryId}`,
        type: 'GET',
        dataType: 'json',
        cache: false, // Prevent caching of this request
        success: function(response) {
            console.log("Custom ID elements loaded successfully:", response);
            
            if (!response) {
                console.error("Empty response received");
                alert("Error: Empty response received from server");
                return;
            }
            
            if (!response.elements) {
                console.warn("No elements property in response");
                return;
            }
            
            console.log(`Loaded ${response.elements.length} custom ID elements`);
            
            // Check if elements are valid
            if (Array.isArray(response.elements)) {
                console.log("Elements data:", JSON.stringify(response.elements));
                
                // Process each element to ensure it's valid
                const validElements = response.elements.map(el => {
                    return {
                        id: el.id || `element-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                        type: el.type || 'fixed',
                        value: el.value || '',
                        order: el.order || 0
                    };
                });
                
                renderCustomIdElements(validElements);
            } else {
                console.error("Elements is not an array:", response.elements);
                alert("Error: Invalid elements data received from server");
            }
        },
        error: function(xhr, status, error) {
            console.error("Error loading custom ID elements:", error);
            console.error("Status:", status);
            console.error("Response:", xhr.responseText);
            alert("Error loading custom ID elements. Check console for details.");
            
            // Display error message in the container
            if (container) {
                container.innerHTML = '<div class="alert alert-danger">Failed to load custom ID elements. Please try refreshing the page.</div>';
            }
        }
    });
}

// Render custom ID elements
function renderCustomIdElements(elements) {
    console.log("Rendering custom ID elements:", elements);
    
    // Clear existing elements
    const container = document.getElementById('custom-id-elements');
    if (!container) {
        console.error("Could not find custom-id-elements container");
        alert("Error: Could not find custom-id-elements container");
        return;
    }
    
    console.log("Container found, clearing it");
    container.innerHTML = '';
    
    // If no elements, show a message
    if (!elements || elements.length === 0) {
        console.log("No custom ID elements to render");
        // Optionally show a "no elements" message
        container.innerHTML = '<div class="alert alert-info">No custom ID elements configured. Click "Add element" to create one.</div>';
        return;
    }
    
    console.log(`Found ${elements.length} elements to render`);
    
    // Sort elements by order if available
    const sortedElements = [...elements].sort((a, b) => (a.order || 0) - (b.order || 0));
    console.log("Rendering sorted elements:", sortedElements);
    
    // Add each element to the container
    sortedElements.forEach((element) => {
        // Ensure we have a valid element ID
        const elementId = element.id || 'element-' + Date.now() + '-' + Math.floor(Math.random() * 1000);
        
        console.log(`Rendering element ${elementId}: type=${element.type}, value=${element.value}`);
        
        // Fix for case-sensitivity in type comparison
        const typeValue = (element.type || '').toLowerCase();
        
        // Properly encode the value to ensure all characters are preserved
        const escapedValue = element.value
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
            
        console.log(`Element value: "${element.value}" -> escaped: "${escapedValue}"`);
            
        const elementHtml = `
            <div class="custom-id-element field-item" draggable="true" data-element-id="${elementId}">
                <div class="row align-items-center">
                    <div class="col-auto">
                        <div class="drag-handle">
                            <i class="bi bi-grip-vertical"></i>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <select class="form-select element-type" data-element-id="${elementId}">
                            <option value="fixed" ${typeValue === 'fixed' ? 'selected' : ''}>Fixed</option>
                            <option value="20-bit random" ${typeValue === '20-bit random' ? 'selected' : ''}>20-bit random</option>
                            <option value="32-bit random" ${typeValue === '32-bit random' ? 'selected' : ''}>32-bit random</option>
                            <option value="6-digit random" ${typeValue === '6-digit random' ? 'selected' : ''}>6-digit random</option>
                            <option value="9-digit random" ${typeValue === '9-digit random' ? 'selected' : ''}>9-digit random</option>
                            <option value="guid" ${typeValue === 'guid' ? 'selected' : ''}>GUID</option>
                            <option value="date/time" ${typeValue === 'date/time' ? 'selected' : ''}>Date/time</option>
                            <option value="sequence" ${typeValue === 'sequence' ? 'selected' : ''}>Sequence</option>
                        </select>
                    </div>
                    <div class="col-md-4">
                        <input type="text" class="form-control element-value" placeholder="Format or text" value="${escapedValue}" data-element-id="${elementId}">
                    </div>
                    <div class="col-md-3">
                        <div class="d-flex gap-1">
                            <button class="btn btn-sm btn-outline-info help-btn" data-bs-toggle="tooltip" title="Help">
                                <i class="bi bi-question-circle"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger remove-element" data-element-id="${elementId}">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
                <div class="element-description mt-2 text-muted small"></div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', elementHtml);
        initializeElementEventListeners(elementId);
    });
    
    // Add drag-and-drop reordering capabilities
    initializeElementDragDrop();
    
    // Update the preview after rendering
    updateCustomIdPreview();
}

// Get custom ID elements for saving
function getCustomIdElements() {
    const elements = [];
    const elementDivs = document.querySelectorAll('.custom-id-element');
    
    elementDivs.forEach((div, index) => {
        const elementId = div.getAttribute('data-element-id');
        const typeSelect = div.querySelector('.element-type');
        const valueInput = div.querySelector('.element-value');
        
        if (typeSelect && valueInput) {
            // Log raw value to debug any character issues
            const rawValue = valueInput.value;
            console.log(`Element ${elementId} raw value:`, rawValue);
            
            // For fixed elements, show exact characters for debugging
            if (typeSelect.value.toLowerCase() === 'fixed') {
                console.log(`Fixed element characters:`, Array.from(rawValue).map(c => c + ' (' + c.charCodeAt(0).toString(16) + ')').join(', '));
            }
            
            elements.push({
                id: elementId,
                type: typeSelect.value,
                value: rawValue,
                order: index
            });
        }
    });
    
    console.log("Collected custom ID elements:", elements);
    return elements;
}

// Load comments for the chat tab
function loadComments(inventoryId) {
    $.ajax({
        url: `/Inventory/GetComments?id=${inventoryId}`,
        type: 'GET',
        success: function(response) {
            console.log("Loaded comments:", response);
            renderComments(response.comments || []);
        },
        error: function(xhr, status, error) {
            console.error("Error loading comments:", error);
        }
    });
}

// Function to update custom ID preview
function updateCustomIdPreview() {
    console.log("Updating custom ID preview");
    const elements = getCustomIdElements();
    const previewElement = document.getElementById('custom-id-preview');
    
    if (!previewElement) {
        console.error("Could not find custom-id-preview element");
        return;
    }
    
    // If no elements, show placeholder
    if (elements.length === 0) {
        previewElement.textContent = '(Preview will appear here)';
        return;
    }
    
    // Show loading indicator
    previewElement.textContent = 'Generating preview...';
    
    // Debug - log the elements being sent
    console.log("Sending elements to preview:", JSON.stringify(elements));
    
    // Generate a debug representation of the elements
    const debugElementsText = elements.map(elem => {
        if (elem.type === 'fixed') {
            return `Fixed: "${elem.value}" chars: ${Array.from(elem.value).map(c => c + '(' + c.charCodeAt(0).toString(16) + ')').join('')}`;
        }
        return `${elem.type}: ${elem.value}`;
    }).join(', ');
    
    console.log("Elements debug representation:", debugElementsText);
    
    // Generate preview using the current elements
    fetch('/Inventory/GenerateAdvancedCustomIdPreview', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ elements: elements })
    })
    .then(response => response.json())
    .then(data => {
        console.log("Preview generated:", data);
        
        // Debug - inspect the raw characters
        if (data.preview) {
            console.log("Preview characters:", Array.from(data.preview).map(c => c + ' (' + c.charCodeAt(0).toString(16) + ')').join(', '));
        }
        
        // Use the innerText property to directly set the text with all special characters
        if (data.preview) {
            // Create a pre-formatted element to ensure all characters are displayed exactly as-is
            previewElement.innerHTML = `<pre style="margin: 0; font-family: inherit; display: inline;">${data.preview.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</pre>`;
            
            // Double-check for underscores - they should be preserved with the pre tag approach
            if (data.preview.includes('_')) {
                console.log("UNDERSCORE DETECTED IN PREVIEW!");
                const preElement = previewElement.querySelector('pre');
                if (!preElement.innerText.includes('_')) {
                    console.error("Underscore lost in display! Using alternative rendering...");
                    // Try an alternative approach with explicit space preservation
                    previewElement.innerHTML = '';
                    const formattedText = document.createElement('span');
                    formattedText.style.whiteSpace = 'pre';
                    formattedText.textContent = data.preview;
                    previewElement.appendChild(formattedText);
                }
            }
        } else {
            previewElement.textContent = 'Invalid format';
        }
        
        // Extra validation to confirm the preview contains the correct characters
        console.log("Preview element content:", previewElement.textContent);
        if (data.preview) {
            console.log("Content characters:", Array.from(previewElement.textContent).map(c => c + ' (' + c.charCodeAt(0).toString(16) + ')').join(', '));
            
            // Log any server-side debug information
            if (data.debugInfo) {
                console.log("Server debug info:", data.debugInfo);
            }
        }
    })
    .catch(err => {
        console.error('Error generating preview:', err);
        previewElement.textContent = 'Error generating preview';
    });
}

// Render comments
function renderComments(comments) {
    // Implementation would go here
    console.log("Comments:", comments);
}

// Initialize element event listeners
function initializeElementEventListeners(elementId) {
    console.log("Initializing event listeners for element:", elementId);
    
    const typeSelect = document.querySelector(`[data-element-id="${elementId}"].element-type`);
    const valueInput = document.querySelector(`[data-element-id="${elementId}"].element-value`);
    const removeBtn = document.querySelector(`[data-element-id="${elementId}"].remove-element`);
    const helpBtn = document.querySelector(`[data-element-id="${elementId}"].help-btn`);
    
    if (typeSelect) {
        typeSelect.addEventListener('change', function() {
            updateElementDescription(elementId);
            updateCustomIdPreview();
        });
        
        // Initialize description
        updateElementDescription(elementId);
    }
    
    if (valueInput) {
        valueInput.addEventListener('input', function() {
            updateCustomIdPreview();
        });
    }
    
    if (removeBtn) {
        removeBtn.addEventListener('click', function() {
            removeCustomIdElement(elementId);
        });
    }
    
    if (helpBtn) {
        // Initialize tooltip
        new bootstrap.Tooltip(helpBtn);
    }
}

// Update element description based on type
function updateElementDescription(elementId) {
    console.log("Updating description for element:", elementId);
    
    const typeSelect = document.querySelector(`[data-element-id="${elementId}"].element-type`);
    const descriptionDiv = document.querySelector(`[data-element-id="${elementId}"] .element-description`);
    
    if (!typeSelect || !descriptionDiv) {
        console.error("Could not find type select or description div for element:", elementId);
        return;
    }
    
    const descriptions = {
        'fixed': 'A piece of unchanging text. Supports any characters including underscores (_) and Unicode emoji.',
        '20-bit random': 'A random value. Format it as a decimal (D6) or hex (X5). You can include underscores or other characters (e.g., X5_).',
        '32-bit random': 'A random value. Format it as a decimal (D10) or hex (X8). You can include underscores or other characters (e.g., D10_).',
        '6-digit random': 'A random 6-digit number. You can include underscores or other characters after the format (e.g., D6_).',
        '9-digit random': 'A random 9-digit number. You can include underscores or other characters after the format (e.g., D9_).',
        'guid': 'A globally unique identifier. Format: N (no dashes), D (dashes), B (braces), P (parentheses). You can append underscores (e.g., N_).',
        'date/time': 'Item creation date/time. Format like: yyyy (year), MM (month), dd (day). You can include underscores (e.g., yyyy_MM).',
        'sequence': 'A sequential index. Format with leading zeros (D4) or without (D). You can include underscores (e.g., D3_).'
    };
    
    const selectedType = typeSelect.value;
    descriptionDiv.textContent = descriptions[selectedType] || '';
    
    // Update value placeholder based on type
    const valueInput = document.querySelector(`[data-element-id="${elementId}"].element-value`);
    if (valueInput) {
        switch (selectedType) {
            case 'fixed':
                valueInput.placeholder = 'Text (e.g., ABC-, 📦, ABC_123, etc.)';
                break;
            case '20-bit random':
            case '32-bit random':
                valueInput.placeholder = 'Format (e.g., X5, D6, X5_, D6_)';
                break;
            case '6-digit random':
            case '9-digit random':
                valueInput.placeholder = 'Format (e.g., D6, D6_)';
                break;
            case 'guid':
                valueInput.placeholder = 'Format (N, D, B, P, N_, D_)';
                break;
            case 'date/time':
                valueInput.placeholder = 'Format (e.g., yyyy, MM-dd, yyyy_MM)';
                break;
            case 'sequence':
                valueInput.placeholder = 'Format (e.g., D3, D, D3_)';
                break;
            default:
                valueInput.placeholder = 'Format or text';
        }
    }
}

// Function to remove a custom ID element
function removeCustomIdElement(elementId) {
    console.log("Removing element:", elementId);
    const element = document.querySelector(`.custom-id-element[data-element-id="${elementId}"]`);
    if (element) {
        element.remove();
        updateCustomIdPreview();
    }
}

// Initialize drag-and-drop functionality for custom ID elements
function initializeElementDragDrop() {
    console.log("Initializing drag-and-drop functionality");
    
    const container = document.getElementById('custom-id-elements');
    if (!container) {
        console.error("Cannot initialize drag-and-drop: container not found");
        return;
    }
    
    const elements = container.querySelectorAll('.custom-id-element');
    console.log(`Found ${elements.length} elements for drag-drop initialization`);
    
    if (elements.length === 0) {
        console.warn("No elements found to initialize drag-and-drop");
        return;
    }
    
    let draggedElement = null;
    
    elements.forEach((element, index) => {
        console.log(`Setting up drag events for element ${index + 1}:`, element.getAttribute('data-element-id'));
        
        element.addEventListener('dragstart', function(e) {
            draggedElement = this;
            setTimeout(() => {
                this.classList.add('dragging');
            }, 0);
            
            // Set data for drag operation
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/html', this.innerHTML);
            console.log("Element drag started");
        });
        
        element.addEventListener('dragend', function() {
            this.classList.remove('dragging');
            draggedElement = null;
            
            // Update the preview and element order values
            updateCustomIdPreview();
            console.log("Element drag ended, updating preview");
        });
        
        element.addEventListener('dragover', function(e) {
            e.preventDefault();
            
            if (draggedElement === this) return;
            
            const boundingRect = this.getBoundingClientRect();
            const offset = boundingRect.y + (boundingRect.height / 2);
            
            if (e.clientY - offset > 0) {
                this.parentNode.insertBefore(draggedElement, this.nextElementSibling);
            } else {
                this.parentNode.insertBefore(draggedElement, this);
            }
        });
    });
}

$(document).ready(function () {
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Selection management
    const selectionToolbar = $('#selectionToolbar');
    const selectedCountBadge = $('#selectedCount');
    const selectAllCheckbox = $('#selectAllItems');
    const itemTable = $('#itemsTable');
    const itemCheckboxes = $('.item-checkbox');

    let selectedItems = [];
    
    // Initialize the toolbar state
    updateSelectionToolbar();

    // Select all checkbox handler
    selectAllCheckbox.on('change', function() {
        const isChecked = $(this).prop('checked');
        itemCheckboxes.prop('checked', isChecked);
        
        if (isChecked) {
            // Select all items
            $('.item-checkbox:checked').each(function() {
                const itemId = $(this).data('item-id');
                if (!selectedItems.includes(itemId)) {
                    selectedItems.push(itemId);
                }
                $(this).closest('tr').addClass('selected-row');
            });
        } else {
            // Unselect all items
            selectedItems = [];
            $('.item-row').removeClass('selected-row');
        }
        
        updateSelectionToolbar();
    });
    
    // Individual checkbox handler
    itemCheckboxes.on('change', function() {
        const itemId = $(this).data('item-id');
        const isChecked = $(this).prop('checked');
        
        if (isChecked) {
            // Add item to selection
            if (!selectedItems.includes(itemId)) {
                selectedItems.push(itemId);
            }
            $(this).closest('tr').addClass('selected-row');
        } else {
            // Remove item from selection
            selectedItems = selectedItems.filter(id => id !== itemId);
            $(this).closest('tr').removeClass('selected-row');
            
            // Update "select all" checkbox
            if (selectAllCheckbox.prop('checked')) {
                selectAllCheckbox.prop('checked', false);
            }
        }
        
        updateSelectionToolbar();
    });
    
    // Row click handler (excluding checkbox and action buttons)
    $('.item-row').on('click', function(e) {
        console.log('Row clicked, target:', e.target);
        
        // Only toggle if the click wasn't on a checkbox, button, or link
        // Exclude specific elements to ensure they can handle their own clicks
        if (!$(e.target).is('input, button, a, .btn, i.bi, .heart-icon, .like-btn') && 
            !$(e.target).closest('.like-btn').length && 
            !$(e.target).closest('button').length) {
            
            console.log('Row click handler: toggling checkbox');
            const checkbox = $(this).find('.item-checkbox');
            checkbox.prop('checked', !checkbox.prop('checked')).trigger('change');
        } else {
            console.log('Row click handler: click on excluded element, not toggling checkbox');
        }
    });
    
    // Function to update the selection toolbar
    function updateSelectionToolbar() {
        if (selectedItems.length > 0) {
            selectionToolbar.addClass('show').removeClass('hide');
            selectedCountBadge.text(selectedItems.length);
            
            // Enable/disable certain actions based on selection count
            if (selectedItems.length === 1) {
                $('.single-item-action').removeClass('disabled');
            } else {
                $('.single-item-action').addClass('disabled');
            }
        } else {
            selectionToolbar.addClass('hide').removeClass('show');
        }
    }
    
    // Action button handlers
    $('#editSelectedBtn').on('click', function() {
        if (selectedItems.length === 1) {
            window.location.href = `/Item/Edit/${selectedItems[0]}`;
        }
    });
    
    $('#deleteSelectedBtn').on('click', function() {
        if (selectedItems.length > 0) {
            if (confirm(`Are you sure you want to delete ${selectedItems.length} item(s)?`)) {
                // Send delete request using AJAX
                $.ajax({
                    url: '/Item/DeleteMultiple',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(selectedItems),
                    success: function(result) {
                        // Reload the page or remove the rows
                        location.reload();
                    },
                    error: function(error) {
                        alert('Error deleting items: ' + error.responseText);
                    }
                });
            }
        }
    });
    
    // Context menu for item rows
    const contextMenu = $('<div class="context-menu" id="itemContextMenu" style="display:none;">'+
        '<button class="context-menu-item" data-action="view"><i class="fas fa-eye me-2"></i>View</button>'+
        '<button class="context-menu-item" data-action="edit"><i class="fas fa-edit me-2"></i>Edit</button>'+
        '<button class="context-menu-item" data-action="duplicate"><i class="fas fa-copy me-2"></i>Duplicate</button>'+
        '<div class="dropdown-divider"></div>'+
        '<button class="context-menu-item text-danger" data-action="delete"><i class="fas fa-trash-alt me-2"></i>Delete</button>'+
    '</div>');
    
    $('body').append(contextMenu);
    
    // Context menu event handlers
    let contextMenuTargetId = null;
    
    $('.item-row').on('contextmenu', function(e) {
        e.preventDefault();
        
        // Hide any visible context menu
        hideContextMenu();
        
        // Store the item ID
        contextMenuTargetId = $(this).find('.item-checkbox').data('item-id');
        
        // Position and show the context menu
        $('#itemContextMenu')
            .css({
                top: e.pageY + 'px',
                left: e.pageX + 'px'
            })
            .show();
            
        // Select the row when right-clicked
        const checkbox = $(this).find('.item-checkbox');
        if (!checkbox.prop('checked')) {
            checkbox.prop('checked', true).trigger('change');
        }
    });
    
    // Hide context menu when clicking elsewhere
    $(document).on('click', function(e) {
        if (!$(e.target).closest('#itemContextMenu').length) {
            hideContextMenu();
        }
    });
    
    function hideContextMenu() {
        $('#itemContextMenu').hide();
    }
    
    // Context menu actions
    $('.context-menu-item').on('click', function() {
        const action = $(this).data('action');
        
        if (contextMenuTargetId) {
            switch(action) {
                case 'view':
                    window.location.href = `/Item/Details/${contextMenuTargetId}`;
                    break;
                case 'edit':
                    window.location.href = `/Item/Edit/${contextMenuTargetId}`;
                    break;
                case 'duplicate':
                    duplicateItem(contextMenuTargetId);
                    break;
                case 'delete':
                    if (confirm('Are you sure you want to delete this item?')) {
                        deleteItem(contextMenuTargetId);
                    }
                    break;
            }
        }
        
        hideContextMenu();
    });
    
    // Helper functions for actions
    function duplicateItem(itemId) {
        $.ajax({
            url: `/Item/Duplicate/${itemId}`,
            type: 'POST',
            success: function(result) {
                if (result.success) {
                    location.reload();
                } else {
                    alert('Error duplicating item: ' + result.message);
                }
            },
            error: function(error) {
                alert('Error duplicating item: ' + error.responseText);
            }
        });
    }
    
    function deleteItem(itemId) {
        $.ajax({
            url: `/Item/Delete/${itemId}`,
            type: 'POST',
            success: function(result) {
                location.reload();
            },
            error: function(error) {
                alert('Error deleting item: ' + error.responseText);
            }
        });
    }
    
    // Additional features - Add keyboard shortcuts for common actions
    $(document).on('keydown', function(e) {
        // Only process if no input fields are focused
        if (!$(e.target).is('input, textarea, select')) {
            // Delete key to delete selected items
            if (e.keyCode === 46 && selectedItems.length > 0) { // Delete key
                $('#deleteSelectedBtn').trigger('click');
            }
            
            // Ctrl+A to select all items
            if (e.keyCode === 65 && e.ctrlKey) { // Ctrl+A
                e.preventDefault();
                selectAllCheckbox.prop('checked', !selectAllCheckbox.prop('checked')).trigger('change');
            }
            
            // Escape key to clear selection
            if (e.keyCode === 27) { // Escape
                if (selectedItems.length > 0) {
                    selectAllCheckbox.prop('checked', false).trigger('change');
                }
            }
        }
    });
    
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Handle clicks on the heart icons inside like buttons
    $(document).on('click', '.like-btn i.heart-icon, .like-btn i.bi', function(e) {
        console.log('Heart icon clicked directly');
        
        // Check if the dedicated like handler is active
        if (window.likeHandlerActive) {
            console.log('Skipping handler - dedicated like-handler.js is active');
            return;
        }
        
        e.stopPropagation();
        e.preventDefault();
        
        // Don't trigger parent click, handle the click here directly
        const $likeBtn = $(this).parent();
        handleLikeButtonClick($likeBtn, e);
    });
    
    // Like button functionality
    $(document).on('click', '.like-btn', function(e) {
        console.log('Like button clicked');
        
        // Check if the dedicated like handler is active
        if (window.likeHandlerActive) {
            console.log('Skipping handler - dedicated like-handler.js is active');
            return;
        }
        
        // Stop event propagation to prevent the row click handler from firing
        e.stopPropagation();
        e.preventDefault();
        
        handleLikeButtonClick($(this), e);
    });
    
    // Add a hidden form to the page for submitting the like action
    if ($('#like-form').length === 0) {
        $('body').append(`
            <form id="like-form" method="post" style="display: none;">
                <input type="hidden" name="__RequestVerificationToken" value="${$('input[name="__RequestVerificationToken"]').val() || ''}" />
            </form>
        `);
    }
    
    function handleLikeButtonClick($likeBtn, e) {
        console.log('Handling like button click');
        
        // Debug authentication data
        console.log('Auth state from data():', $likeBtn.data('authenticated'));
        console.log('Auth state from attr():', $likeBtn.attr('data-authenticated'));
        
        // Don't check authentication here - we're handling it in like-handler.js
        // to avoid duplicate alerts
        
        const itemId = $likeBtn.data('item-id');
        const $icon = $likeBtn.find('i');
        // Find the likes count element - might be nested or a sibling
        const $likesCount = $('span.likes-count[data-item-id="' + itemId + '"]');
        
        // Log for debugging
        console.log('Processing like for item:', itemId);
        
        // Create a simple form submission
        const form = $('#like-form');
        form.attr('action', `/Item/ToggleLike/${itemId}`);
        
        // Submit the form using fetch API to get JSON response
        fetch(form.attr('action'), {
            method: 'POST',
            body: new FormData(form[0]),
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.json())
        .then(result => {
            console.log('Like toggle success:', result);
            if (result.success) {
                if (result.isLiked) {
                    $icon.removeClass('bi-heart').addClass('bi-heart-fill');
                    $likeBtn.attr('data-bs-original-title', 'Unlike');
                } else {
                    $icon.removeClass('bi-heart-fill').addClass('bi-heart');
                    $likeBtn.attr('data-bs-original-title', 'Like');
                }
                
                // Update tooltip
                var tooltip = bootstrap.Tooltip.getInstance($likeBtn[0]);
                if (tooltip) {
                    tooltip.dispose();
                }
                new bootstrap.Tooltip($likeBtn[0]);
                
                // Update like count if it's returned in the response
                if (result.likesCount !== undefined) {
                    $likesCount.text(result.likesCount);
                }
            }
        })
        .catch(error => {
            console.error('Error toggling like:', error);
            alert('Failed to toggle like. Please try again.');
        });
    }
});
