// inventory-detail.js - Handles selection and actions for inventory items

$(document).ready(function () {
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
        // Only toggle if the click wasn't on a checkbox, button, or link
        if (!$(e.target).is('input, button, a, .btn, i.fas, i.far, i.fab')) {
            const checkbox = $(this).find('.item-checkbox');
            checkbox.prop('checked', !checkbox.prop('checked')).trigger('change');
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
});
