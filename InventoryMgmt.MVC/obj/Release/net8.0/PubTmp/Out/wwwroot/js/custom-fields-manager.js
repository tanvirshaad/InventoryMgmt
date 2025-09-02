/**
 * Custom Fields Manager - Handles custom fields configuration for inventory
 * This script provides functionality to add, edit, and save custom field configurations
 */

// Global variables to store state
let customFields = [];
let currentInventoryId = null;

/**
 * Initialize the custom fields manager
 * @param {number} inventoryId - The ID of the current inventory
 */
function initCustomFieldsManager(inventoryId) {
    console.log("Initializing custom fields manager for inventory ID:", inventoryId);
    currentInventoryId = inventoryId;
    
    // Set up event handlers
    setupEventHandlers();
    
    // Load current fields
    loadCustomFields();
    
    // Enable debug panel toggle
    $('#toggle-debug').on('click', function() {
        $('#debug-content').toggle();
    });
    
    // Debug panel buttons
    $('#debug-show-ids').on('click', function() {
        showFieldIds();
    });
    
    $('#debug-clear-fields').on('click', function() {
        clearAllFields();
    });
    
    $('#debug-log-fields').on('click', function() {
        console.log("Current fields:", customFields);
    });
    
    $('#debug-raw-db').on('click', function() {
        checkRawDbValues();
    });
}

/**
 * Set up event handlers for buttons
 */
function setupEventHandlers() {
    // First, remove any existing event handlers to prevent duplication
    $('#add-text-field').off('click');
    $('#add-numeric-field').off('click');
    $('#add-boolean-field').off('click');
    $('#save-fields-button').off('click');
    $('#reload-fields-button').off('click');
    $('#clear-all-fields-button').off('click');
    
    // Now add our event handlers with namespacing to avoid conflicts
    $('#add-text-field').on('click.customFieldsManager', function(e) {
        console.log("Text field button clicked (customFieldsManager)");
        e.preventDefault();
        e.stopPropagation(); // Stop event propagation
        addCustomField('text');
        return false; // Prevent default and stop propagation
    });
    
    // Button for adding a numeric field
    $('#add-numeric-field').on('click.customFieldsManager', function(e) {
        console.log("Numeric field button clicked (customFieldsManager)");
        e.preventDefault();
        e.stopPropagation(); // Stop event propagation
        addCustomField('numeric');
        return false; // Prevent default and stop propagation
    });
    
    // Button for adding a boolean field
    $('#add-boolean-field').on('click.customFieldsManager', function(e) {
        console.log("Boolean field button clicked (customFieldsManager)");
        e.preventDefault();
        e.stopPropagation(); // Stop event propagation
        addCustomField('boolean');
        return false; // Prevent default and stop propagation
    });
    
    // Save button
    $('#save-fields-button').on('click.customFieldsManager', function(e) {
        console.log("Save fields button clicked (customFieldsManager)");
        e.preventDefault();
        e.stopPropagation(); // Stop event propagation
        saveCustomFields();
        return false; // Prevent default and stop propagation
    });
    
    // Reload button
    $('#reload-fields-button').on('click.customFieldsManager', function(e) {
        console.log("Reload fields button clicked (customFieldsManager)");
        e.preventDefault();
        e.stopPropagation(); // Stop event propagation
        loadCustomFields();
        return false; // Prevent default and stop propagation
    });
    
    // Clear all button
    $('#clear-all-fields-button').on('click.customFieldsManager', function(e) {
        console.log("Clear all fields button clicked (customFieldsManager)");
        e.preventDefault();
        e.stopPropagation(); // Stop event propagation
        if (confirm('Are you sure you want to clear all custom fields? This cannot be undone.')) {
            clearAllFields();
        }
        return false; // Prevent default and stop propagation
    });
}

/**
 * Load custom fields from the server
 */
function loadCustomFields() {
    if (!currentInventoryId) {
        console.error("No inventory ID set");
        return;
    }
    
    console.log("Loading custom fields for inventory ID:", currentInventoryId);
    
    // Clear the current fields list
    $('#custom-fields-list').html('<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>');
    
    $.ajax({
        url: `/Inventory/GetCustomFields?id=${currentInventoryId}`,
        type: 'GET',
        success: function(response) {
            console.log("Loaded fields:", response);
            
            // Store the fields
            customFields = response.fields || [];
            
            // Update the debug panel
            updateDebugPanel();
            
            // Render the fields
            renderCustomFields();
        },
        error: function(xhr, status, error) {
            console.error("Error loading custom fields:", error);
            $('#custom-fields-list').html(`
                <div class="alert alert-danger">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>
                    Error loading custom fields. Please try again.
                </div>
            `);
        }
    });
}

// Flag to prevent multiple simultaneous field additions
let isAddingField = false;

/**
 * Add a new custom field
 * @param {string} type - The type of field to add (text, numeric, boolean)
 */
function addCustomField(type) {
    // Prevent multiple fields being added at once by rapid clicking
    if (isAddingField) {
        console.log("Already adding a field, ignoring duplicate request");
        return;
    }
    
    isAddingField = true;
    console.log("Adding new field of type:", type);
    
    // Find the next available slot for this field type
    let fieldNumber = getNextAvailableFieldNumber(type);
    
    if (fieldNumber > 3) {
        alert(`You can only have a maximum of 3 ${type} fields.`);
        isAddingField = false;
        return;
    }
    
    // Check if a field with this ID already exists
    const existingField = customFields.find(f => f.id === `${type}-field-${fieldNumber}`);
    if (existingField) {
        console.log("Field already exists with ID:", `${type}-field-${fieldNumber}`);
        isAddingField = false;
        return;
    }
    
    // Create a field configuration object
    let newField = {
        type: type,
        id: `${type}-field-${fieldNumber}`,
        name: `New ${type} field`,
        description: '',
        showInTable: false,
        order: customFields.length
    };
    
    // Add numeric configuration if needed
    if (type === 'numeric') {
        newField.numericConfig = {
            isInteger: false,
            minValue: null,
            maxValue: null
        };
    }
    
    // Add the field to the array
    customFields.push(newField);
    
    // Update the UI
    renderCustomFields();
    
    // Update debug panel
    updateDebugPanel();
    
    console.log("New field added:", newField);
    
    // Reset the flag after a delay to prevent rapid multiple additions
    setTimeout(() => {
        isAddingField = false;
    }, 300);
}

/**
 * Get the next available field number for a type
 * @param {string} type - The field type
 * @returns {number} - The next available number (1-3)
 */
function getNextAvailableFieldNumber(type) {
    // Create an array of used field numbers
    let usedNumbers = customFields
        .filter(field => field.type === type)
        .map(field => parseInt(field.id.split('-').pop()));
    
    // Find the first unused number between 1 and 3
    for (let i = 1; i <= 3; i++) {
        if (!usedNumbers.includes(i)) {
            return i;
        }
    }
    
    // If all slots are used, return 4 (which will trigger an error)
    return 4;
}

/**
 * Render the custom fields in the UI
 */
function renderCustomFields() {
    const container = $('#custom-fields-list');
    
    // Clear the container
    container.empty();
    
    // If no fields, show a message
    if (customFields.length === 0) {
        container.html(`
            <div class="alert alert-info mb-3">
                <i class="bi bi-info-circle me-2"></i>
                No custom fields have been configured yet. Use the buttons below to add fields.
            </div>
        `);
        return;
    }
    
    // Render each field
    customFields.forEach((field, index) => {
        let fieldHtml = `
            <div class="card mb-3 field-card" data-field-id="${field.id}" data-field-type="${field.type}">
                <div class="card-header bg-light d-flex justify-content-between align-items-center">
                    <h6 class="mb-0">
                        <i class="bi ${getFieldIcon(field.type)} me-2"></i>
                        ${field.type.charAt(0).toUpperCase() + field.type.slice(1)} Field
                    </h6>
                    <div class="btn-group">
                        <button type="button" class="btn btn-sm btn-outline-danger delete-field" data-field-id="${field.id}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label">Name</label>
                        <input type="text" class="form-control field-name" value="${field.name || ''}" placeholder="Field name">
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Description</label>
                        <input type="text" class="form-control field-description" value="${field.description || ''}" placeholder="Field description">
                    </div>
                    <div class="form-check mb-3">
                        <input class="form-check-input field-show-in-table" type="checkbox" ${field.showInTable ? 'checked' : ''}>
                        <label class="form-check-label">Show in item table</label>
                    </div>`;
        
        // Add numeric field options if needed
        if (field.type === 'numeric') {
            fieldHtml += `
                <div class="card mb-3">
                    <div class="card-header bg-light">
                        <h6 class="mb-0">Numeric Options</h6>
                    </div>
                    <div class="card-body">
                        <div class="form-check mb-3">
                            <input class="form-check-input field-is-integer" type="checkbox" ${field.numericConfig?.isInteger ? 'checked' : ''}>
                            <label class="form-check-label">Integer only</label>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Minimum Value</label>
                            <input type="number" class="form-control field-min-value" value="${field.numericConfig?.minValue !== null ? field.numericConfig.minValue : ''}">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Maximum Value</label>
                            <input type="number" class="form-control field-max-value" value="${field.numericConfig?.maxValue !== null ? field.numericConfig.maxValue : ''}">
                        </div>
                    </div>
                </div>`;
        }
        
        fieldHtml += `
                </div>
            </div>
        `;
        
        container.append(fieldHtml);
    });
    
    // Add event handlers for the field elements
    addFieldEventHandlers();
}

/**
 * Get the appropriate Bootstrap icon class for a field type
 * @param {string} type - The field type
 * @returns {string} - The icon class
 */
function getFieldIcon(type) {
    switch(type) {
        case 'text':
            return 'bi-font';
        case 'multitext':
            return 'bi-text-paragraph';
        case 'numeric':
            return 'bi-123';
        case 'boolean':
            return 'bi-toggle-on';
        case 'document':
            return 'bi-file-earmark';
        default:
            return 'bi-question';
    }
}

/**
 * Add event handlers to field elements
 */
function addFieldEventHandlers() {
    // Delete field button
    $('.delete-field').on('click', function() {
        const fieldId = $(this).data('field-id');
        deleteField(fieldId);
    });
    
    // Field name change
    $('.field-name').on('change', function() {
        const fieldId = $(this).closest('.field-card').data('field-id');
        const newName = $(this).val();
        updateFieldProperty(fieldId, 'name', newName);
    });
    
    // Field description change
    $('.field-description').on('change', function() {
        const fieldId = $(this).closest('.field-card').data('field-id');
        const newDescription = $(this).val();
        updateFieldProperty(fieldId, 'description', newDescription);
    });
    
    // Show in table checkbox
    $('.field-show-in-table').on('change', function() {
        const fieldId = $(this).closest('.field-card').data('field-id');
        const showInTable = $(this).prop('checked');
        updateFieldProperty(fieldId, 'showInTable', showInTable);
    });
    
    // Numeric field options
    $('.field-is-integer').on('change', function() {
        const fieldId = $(this).closest('.field-card').data('field-id');
        const isInteger = $(this).prop('checked');
        updateNumericProperty(fieldId, 'isInteger', isInteger);
    });
    
    $('.field-min-value').on('change', function() {
        const fieldId = $(this).closest('.field-card').data('field-id');
        const minValue = $(this).val() !== '' ? parseFloat($(this).val()) : null;
        updateNumericProperty(fieldId, 'minValue', minValue);
    });
    
    $('.field-max-value').on('change', function() {
        const fieldId = $(this).closest('.field-card').data('field-id');
        const maxValue = $(this).val() !== '' ? parseFloat($(this).val()) : null;
        updateNumericProperty(fieldId, 'maxValue', maxValue);
    });
}

/**
 * Update a property of a field
 * @param {string} fieldId - The ID of the field to update
 * @param {string} property - The property to update
 * @param {any} value - The new value
 */
function updateFieldProperty(fieldId, property, value) {
    const field = customFields.find(f => f.id === fieldId);
    if (field) {
        field[property] = value;
        console.log(`Updated field ${fieldId} ${property} to:`, value);
        updateDebugPanel();
    }
}

/**
 * Update a property of a numeric field's config
 * @param {string} fieldId - The ID of the field to update
 * @param {string} property - The property to update
 * @param {any} value - The new value
 */
function updateNumericProperty(fieldId, property, value) {
    const field = customFields.find(f => f.id === fieldId);
    if (field && field.numericConfig) {
        field.numericConfig[property] = value;
        console.log(`Updated numeric field ${fieldId} ${property} to:`, value);
        updateDebugPanel();
    }
}

/**
 * Delete a field
 * @param {string} fieldId - The ID of the field to delete
 */
function deleteField(fieldId) {
    if (confirm('Are you sure you want to delete this field?')) {
        console.log("Deleting field:", fieldId);
        
        // Remove the field from the array
        customFields = customFields.filter(field => field.id !== fieldId);
        
        // Re-render the fields
        renderCustomFields();
        
        // Update the debug panel
        updateDebugPanel();
        
        // Save changes to server immediately to persist the deletion
        saveCustomFields();
    }
}

/**
 * Save the custom fields configuration to the server
 */
function saveCustomFields() {
    if (!currentInventoryId) {
        console.error("No inventory ID set");
        return;
    }
    
    console.log("Saving custom fields for inventory ID:", currentInventoryId);
    console.log("Fields to save:", customFields);
    
    // Make sure we have an integer inventory ID
    const inventoryId = parseInt(currentInventoryId);
    
    // Show saving indicator
    $('#save-fields-button').prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...');
    
    // Send the request
    $.ajax({
        url: '/Inventory/SaveCustomFields',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            inventoryId: inventoryId,
            fields: customFields
        }),
        success: function(response) {
            console.log("Save response:", response);
            
            // Show success message
            if (response.success) {
                alert('Fields saved successfully!');
                
                // Reload the fields to ensure we have the latest data
                loadCustomFields();
            } else {
                alert('Error saving fields: ' + (response.error || 'Unknown error'));
            }
        },
        error: function(xhr, status, error) {
            console.error("Error saving fields:", error);
            console.error("Status:", status);
            console.error("Response:", xhr.responseText);
            alert('Error saving fields. See console for details.');
        },
        complete: function() {
            // Reset the button
            $('#save-fields-button').prop('disabled', false).html('<i class="bi bi-save me-2"></i>Save Fields');
        }
    });
}

/**
 * Clear all custom fields
 */
function clearAllFields() {
    if (!currentInventoryId) {
        console.error("No inventory ID set");
        return;
    }
    
    console.log("Clearing all custom fields for inventory ID:", currentInventoryId);
    
    // Make sure we have an integer inventory ID
    const inventoryId = parseInt(currentInventoryId);
    
    // Send the request with an empty fields array
    $.ajax({
        url: '/Inventory/SaveCustomFields',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            inventoryId: inventoryId,
            fields: []
        }),
        success: function(response) {
            console.log("Clear response:", response);
            
            if (response.success) {
                alert('All fields cleared successfully!');
                
                // Clear the local array
                customFields = [];
                
                // Re-render the fields
                renderCustomFields();
                
                // Update the debug panel
                updateDebugPanel();
            } else {
                alert('Error clearing fields: ' + (response.error || 'Unknown error'));
            }
        },
        error: function(xhr, status, error) {
            console.error("Error clearing fields:", error);
            alert('Error clearing fields. See console for details.');
        }
    });
}

/**
 * Update the debug panel with current field information
 */
function updateDebugPanel() {
    const fieldsJson = JSON.stringify(customFields, null, 2);
    $('#debug-current-fields').html(`<pre>${fieldsJson}</pre>`);
}

/**
 * Show field IDs for debugging
 */
function showFieldIds() {
    alert('Field IDs: ' + customFields.map(f => f.id).join(', '));
}

/**
 * Check raw database values
 */
function checkRawDbValues() {
    if (!currentInventoryId) {
        console.error("No inventory ID set");
        return;
    }
    
    $.ajax({
        url: `/Inventory/DatabaseDebug?id=${currentInventoryId}`,
        type: 'GET',
        success: function(response) {
            console.log("Raw DB values:", response);
            $('#debug-current-fields').html(`<pre>${JSON.stringify(response, null, 2)}</pre>`);
        },
        error: function(xhr, status, error) {
            console.error("Error fetching raw DB values:", error);
        }
    });
}
