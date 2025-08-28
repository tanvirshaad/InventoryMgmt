/**
 * Item Form Handler - Manages custom fields display and validation
 */

$(document).ready(function() {
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Set up numeric field validations
    setupNumericValidation();
    
    // Handle custom field interactions
    setupCustomFieldsInteractions();
    
    // Setup auto-generate ID functionality
    setupAutoGenerateId();
    
    console.log("Item form initialized");
});

/**
 * Set up validation for numeric fields based on configurations
 */
function setupNumericValidation() {
    // Get numeric field configurations
    $('.numeric-field').each(function() {
        const fieldId = $(this).data('field-id');
        const isInteger = $(this).data('is-integer');
        const minValue = $(this).data('min-value');
        const maxValue = $(this).data('max-value');
        
        // Get the input element
        const input = $(this).find('input');
        
        // Set input attributes based on configuration
        if (isInteger) {
            input.attr('step', '1');
            input.on('change', function() {
                // Force integer value
                if (this.value) {
                    this.value = Math.round(this.value);
                }
            });
        } else {
            input.attr('step', '0.01');
        }
        
        if (minValue !== null && minValue !== undefined) {
            input.attr('min', minValue);
        }
        
        if (maxValue !== null && maxValue !== undefined) {
            input.attr('max', maxValue);
        }
        
        // Add validation on blur
        input.on('blur', function() {
            validateNumericInput($(this), isInteger, minValue, maxValue);
        });
    });
}

/**
 * Validate a numeric input against constraints
 */
function validateNumericInput(input, isInteger, minValue, maxValue) {
    const value = input.val();
    if (!value) return; // Skip validation for empty values
    
    let numericValue = parseFloat(value);
    let hasError = false;
    let errorMessage = '';
    
    // Check if it's an integer if required
    if (isInteger && !Number.isInteger(numericValue)) {
        numericValue = Math.round(numericValue);
        input.val(numericValue);
        errorMessage = 'Value must be an integer and has been rounded';
        hasError = true;
    }
    
    // Check minimum value
    if (minValue !== null && minValue !== undefined && numericValue < minValue) {
        input.val(minValue);
        errorMessage = `Value cannot be less than ${minValue}`;
        hasError = true;
    }
    
    // Check maximum value
    if (maxValue !== null && maxValue !== undefined && numericValue > maxValue) {
        input.val(maxValue);
        errorMessage = `Value cannot be greater than ${maxValue}`;
        hasError = true;
    }
    
    // Show or clear validation message
    const feedbackElement = input.siblings('.invalid-feedback');
    if (hasError) {
        input.addClass('is-invalid');
        if (feedbackElement.length) {
            feedbackElement.text(errorMessage);
        } else {
            input.after(`<div class="invalid-feedback">${errorMessage}</div>`);
        }
    } else {
        input.removeClass('is-invalid');
        feedbackElement.remove();
    }
}

/**
 * Set up text field validation
 */
function setupTextFieldValidation() {
    // Validate text fields for max length
    $('.text-field').each(function() {
        const input = $(this).find('input, textarea');
        const maxLength = $(this).data('max-length');
        
        if (maxLength) {
            input.attr('maxlength', maxLength);
            
            // Add character count indicator
            const counterId = `char-counter-${Math.random().toString(36).substring(2, 10)}`;
            
            if ($(this).find('.char-counter').length === 0) {
                input.after(`<small class="char-counter text-muted" id="${counterId}">0/${maxLength}</small>`);
                
                // Update character count on input
                input.on('input', function() {
                    const count = $(this).val().length;
                    $(`#${counterId}`).text(`${count}/${maxLength}`);
                    
                    // Highlight counter when approaching limit
                    if (count > maxLength * 0.9) {
                        $(`#${counterId}`).addClass('text-warning');
                    } else {
                        $(`#${counterId}`).removeClass('text-warning');
                    }
                });
                
                // Trigger initial count
                input.trigger('input');
            }
        }
    });
}

/**
 * Set up interactions for custom fields
 */
function setupCustomFieldsInteractions() {
    // Toggle sections if needed
    $('.custom-fields-toggle').on('click', function() {
        const target = $($(this).data('bs-target'));
        target.slideToggle();
        
        // Toggle icon
        const icon = $(this).find('i.bi');
        if (icon.hasClass('bi-chevron-down')) {
            icon.removeClass('bi-chevron-down').addClass('bi-chevron-up');
        } else {
            icon.removeClass('bi-chevron-up').addClass('bi-chevron-down');
        }
    });
    
    // Setup text field validation
    setupTextFieldValidation();
}

/**
 * Set up auto-generate ID functionality
 */
function setupAutoGenerateId() {
    $('#autoGenerateIdBtn').on('click', function() {
        const btn = $(this);
        const inventoryId = $('input[name="InventoryId"]').val();
        const customIdInput = $('#CustomId');
        
        if (!inventoryId) {
            console.error("Inventory ID not found");
            return;
        }
        
        // Show loading state
        btn.prop('disabled', true);
        btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Generating...');
        
        // Call API to generate ID
        $.ajax({
            url: `/Item/GenerateItemCustomId?inventoryId=${inventoryId}`,
            type: 'GET',
            success: function(response) {
                if (response && response.success) {
                    customIdInput.val(response.customId);
                } else {
                    alert('Error generating ID: ' + (response.message || 'Unknown error'));
                }
            },
            error: function(xhr, status, error) {
                console.error('Error generating ID:', error);
                alert('Error generating ID. Please try again.');
            },
            complete: function() {
                // Reset button state
                btn.prop('disabled', false);
                btn.html('<i class="bi bi-magic"></i> Auto-generate ID');
            }
        });
    });
}
