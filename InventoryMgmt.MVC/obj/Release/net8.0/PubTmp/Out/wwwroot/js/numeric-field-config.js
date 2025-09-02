// numeric-field-config.js
$(document).ready(function() {
    // Function to set up field configuration panels
    function initNumericFieldConfig() {
        $('.numeric-field-config').each(function() {
            const fieldId = $(this).data('field-id');
            const isInteger = $(this).find('.is-integer').is(':checked');
            
            // Toggle step input visibility based on integer toggle
            toggleStepVisibility(fieldId, isInteger);
            
            // Handle integer toggle change
            $(this).find('.is-integer').on('change', function() {
                const isInteger = $(this).is(':checked');
                toggleStepVisibility(fieldId, isInteger);
                updateFieldPreview(fieldId);
            });
            
            // Handle other input changes
            $(this).find('input').on('change', function() {
                updateFieldPreview(fieldId);
            });
            
            // Initialize preview
            updateFieldPreview(fieldId);
        });
    }
    
    // Toggle step visibility based on integer setting
    function toggleStepVisibility(fieldId, isInteger) {
        const stepContainer = $(`.numeric-field-config[data-field-id="${fieldId}"] .step-container`);
        if (isInteger) {
            stepContainer.hide();
        } else {
            stepContainer.show();
        }
    }
    
    // Update the field preview
    function updateFieldPreview(fieldId) {
        const container = $(`.numeric-field-config[data-field-id="${fieldId}"]`);
        const isInteger = container.find('.is-integer').is(':checked');
        const minValue = container.find('.min-value').val();
        const maxValue = container.find('.max-value').val();
        const stepValue = isInteger ? 1 : container.find('.step-value').val();
        
        const preview = container.find('.field-preview');
        
        // Update the input attributes for the preview
        preview.find('input')
            .attr('type', 'number')
            .attr('step', stepValue)
            .prop('required', false);
            
        if (minValue) {
            preview.find('input').attr('min', minValue);
        } else {
            preview.find('input').removeAttr('min');
        }
        
        if (maxValue) {
            preview.find('input').attr('max', maxValue);
        } else {
            preview.find('input').removeAttr('max');
        }
        
        // Update the HTML representation
        let htmlAttrs = `type="number" step="${stepValue}"`;
        if (minValue) htmlAttrs += ` min="${minValue}"`;
        if (maxValue) htmlAttrs += ` max="${maxValue}"`;
        
        container.find('.html-representation').text(`<input ${htmlAttrs}>`);
    }
    
    // Initialize when document is ready
    initNumericFieldConfig();
    
    // Save configuration
    $('#save-field-config').on('click', function() {
        const fields = [];
        
        $('.field-config-panel').each(function() {
            const fieldId = $(this).data('field-id');
            const fieldType = $(this).data('field-type');
            
            if (fieldType === 'numeric') {
                const container = $(this).find('.numeric-field-config');
                const isInteger = container.find('.is-integer').is(':checked');
                const minValue = container.find('.min-value').val();
                const maxValue = container.find('.max-value').val();
                const stepValue = container.find('.step-value').val();
                const displayFormat = container.find('.display-format').val();
                
                fields.push({
                    id: fieldId,
                    type: fieldType,
                    name: $(this).find('.field-name').val(),
                    description: $(this).find('.field-description').val(),
                    showInTable: $(this).find('.show-in-table').is(':checked'),
                    order: parseInt($(this).find('.field-order').val()) || 0,
                    numericConfig: {
                        isInteger: isInteger,
                        minValue: minValue ? parseFloat(minValue) : null,
                        maxValue: maxValue ? parseFloat(maxValue) : null,
                        stepValue: stepValue ? parseFloat(stepValue) : (isInteger ? 1 : 0.01),
                        displayFormat: displayFormat
                    }
                });
            } else {
                fields.push({
                    id: fieldId,
                    type: fieldType,
                    name: $(this).find('.field-name').val(),
                    description: $(this).find('.field-description').val(),
                    showInTable: $(this).find('.show-in-table').is(':checked'),
                    order: parseInt($(this).find('.field-order').val()) || 0
                });
            }
        });
        
        // Save fields via AJAX
        $.ajax({
            url: '/Inventory/SaveCustomFields',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                inventoryId: $('#inventory-id').val(),
                fields: fields
            }),
            success: function(response) {
                if (response.success) {
                    showToast('Fields configuration saved successfully', 'success');
                    setTimeout(() => window.location.reload(), 1000);
                } else {
                    showToast('Failed to save fields configuration', 'error');
                }
            },
            error: function() {
                showToast('An error occurred while saving fields configuration', 'error');
            }
        });
    });
    
    // Simple toast notification
    function showToast(message, type) {
        const toast = $(`<div class="toast ${type}-toast">${message}</div>`);
        $('body').append(toast);
        setTimeout(() => toast.addClass('show'), 10);
        setTimeout(() => {
            toast.removeClass('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }
});
