// Custom ID validation and formatting JavaScript

class CustomIdValidator {
    constructor(inventoryId, itemId = null) {
        this.inventoryId = inventoryId;
        this.itemId = itemId;
        this.formatInfo = null;
        this.validationTimer = null;
        
        this.init();
    }

    async init() {
        await this.loadFormatInfo();
        this.setupValidation();
    }

    async loadFormatInfo() {
        try {
            const response = await fetch(`/Item/GetCustomIdFormatInfo?inventoryId=${this.inventoryId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (response.ok) {
                this.formatInfo = await response.json();
                this.displayFormatInfo();
            }
        } catch (error) {
            console.error('Error loading format info:', error);
        }
    }

    displayFormatInfo() {
        if (!this.formatInfo || !this.formatInfo.success) return;

        const customIdInput = document.getElementById('CustomId');
        if (!customIdInput) return;

        // Look for the existing format info container or create one
        let formatInfoContainer = document.getElementById('custom-id-format-info');
        if (!formatInfoContainer) {
            // Create the format info container and place it after the input group
            formatInfoContainer = document.createElement('div');
            formatInfoContainer.id = 'custom-id-format-info';
            formatInfoContainer.className = 'form-text text-muted mt-1';
            
            // Find the input group (parent of the input) and insert after it
            const inputGroup = customIdInput.closest('.input-group');
            if (inputGroup) {
                inputGroup.parentNode.insertBefore(formatInfoContainer, inputGroup.nextSibling);
            } else {
                // Fallback: insert after the input if no input group found
                customIdInput.parentNode.insertBefore(formatInfoContainer, customIdInput.nextSibling);
            }
        }

        // Show the container and populate with format info
        formatInfoContainer.style.display = 'block';
        
        if (this.formatInfo.hasFormat) {
            formatInfoContainer.innerHTML = `
                <small>
                    <strong>Required format:</strong> ${this.formatInfo.formatExample}<br>
                    <strong>Example:</strong> ${this.formatInfo.validExample}
                </small>
            `;
        } else {
            formatInfoContainer.innerHTML = '<small>No specific format required</small>';
        }
    }

    setupValidation() {
        const customIdInput = document.getElementById('CustomId');
        if (!customIdInput) return;

        // Add validation feedback container
        let feedbackContainer = document.getElementById('custom-id-feedback');
        if (!feedbackContainer) {
            feedbackContainer = document.createElement('div');
            feedbackContainer.id = 'custom-id-feedback';
            feedbackContainer.className = 'invalid-feedback';
            customIdInput.parentNode.appendChild(feedbackContainer);
        }

        // Setup Generate button functionality
        this.setupGenerateButton();

        // Add real-time validation
        customIdInput.addEventListener('input', (e) => {
            clearTimeout(this.validationTimer);
            this.validationTimer = setTimeout(() => {
                this.validateCustomId(e.target.value);
            }, 500); // Debounce validation by 500ms
        });

        customIdInput.addEventListener('blur', (e) => {
            this.validateCustomId(e.target.value);
        });
    }

    setupGenerateButton() {
        const generateBtn = document.getElementById('generateCustomIdBtn');
        if (!generateBtn) return;

        generateBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            await this.generateCustomId();
        });
    }

    async generateCustomId() {
        const generateBtn = document.getElementById('generateCustomIdBtn');
        const customIdInput = document.getElementById('CustomId');
        
        if (!generateBtn || !customIdInput) return;

        // Show loading state
        const originalText = generateBtn.innerHTML;
        generateBtn.disabled = true;
        generateBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Generating...';

        try {
            const response = await fetch(`/Item/GenerateItemCustomId?inventoryId=${this.inventoryId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success && result.customId) {
                    customIdInput.value = result.customId;
                    // Trigger validation to show it's valid
                    await this.validateCustomId(result.customId);
                    
                    // Show success feedback briefly
                    generateBtn.innerHTML = '<i class="bi bi-check-circle"></i> Generated!';
                    generateBtn.classList.add('btn-success');
                    generateBtn.classList.remove('btn-outline-secondary');
                    
                    setTimeout(() => {
                        generateBtn.innerHTML = originalText;
                        generateBtn.classList.remove('btn-success');
                        generateBtn.classList.add('btn-outline-secondary');
                    }, 2000);
                } else {
                    throw new Error(result.message || 'Failed to generate Custom ID');
                }
            } else {
                throw new Error('Failed to generate Custom ID');
            }
        } catch (error) {
            console.error('Error generating Custom ID:', error);
            
            // Show error feedback
            generateBtn.innerHTML = '<i class="bi bi-exclamation-triangle"></i> Error';
            generateBtn.classList.add('btn-danger');
            generateBtn.classList.remove('btn-outline-secondary');
            
            setTimeout(() => {
                generateBtn.innerHTML = originalText;
                generateBtn.classList.remove('btn-danger');
                generateBtn.classList.add('btn-outline-secondary');
            }, 3000);
        } finally {
            generateBtn.disabled = false;
        }
    }

    async validateCustomId(customId) {
        const customIdInput = document.getElementById('CustomId');
        const feedbackContainer = document.getElementById('custom-id-feedback');
        
        if (!customIdInput || !feedbackContainer) return;

        // Clear previous validation state
        customIdInput.classList.remove('is-valid', 'is-invalid');
        feedbackContainer.textContent = '';

        if (!customId.trim()) {
            customIdInput.classList.add('is-invalid');
            feedbackContainer.textContent = 'Custom ID is required';
            return false;
        }

        try {
            const response = await fetch('/Item/ValidateCustomId', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    inventoryId: this.inventoryId,
                    customId: customId,
                    excludeItemId: this.itemId
                })
            });

            if (response.ok) {
                const result = await response.json();
                
                if (result.isValid) {
                    customIdInput.classList.add('is-valid');
                    feedbackContainer.textContent = '';
                } else {
                    customIdInput.classList.add('is-invalid');
                    feedbackContainer.textContent = result.message;
                }
                
                return result.isValid;
            } else {
                customIdInput.classList.add('is-invalid');
                feedbackContainer.textContent = 'Error validating custom ID';
                return false;
            }
        } catch (error) {
            console.error('Validation error:', error);
            customIdInput.classList.add('is-invalid');
            feedbackContainer.textContent = 'Error validating custom ID';
            return false;
        }
    }

    async generateNewId() {
        try {
            const response = await fetch(`/Item/GenerateItemCustomId?inventoryId=${this.inventoryId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    const customIdInput = document.getElementById('CustomId');
                    if (customIdInput) {
                        customIdInput.value = result.customId;
                        // Trigger validation
                        await this.validateCustomId(result.customId);
                    }
                    return result.customId;
                }
            }
        } catch (error) {
            console.error('Error generating custom ID:', error);
        }
        return null;
    }

    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on an item create/edit page
    const customIdInput = document.getElementById('CustomId');
    const inventoryIdInput = document.getElementById('InventoryId');
    const itemIdInput = document.getElementById('Id');
    
    if (customIdInput && inventoryIdInput) {
        const inventoryId = inventoryIdInput.value;
        const itemId = itemIdInput ? itemIdInput.value : null;
        
        if (inventoryId) {
            window.customIdValidator = new CustomIdValidator(inventoryId, itemId);
            
            // Add generate button if it doesn't exist
            const generateBtn = document.getElementById('generate-custom-id');
            if (generateBtn) {
                generateBtn.addEventListener('click', async (e) => {
                    e.preventDefault();
                    await window.customIdValidator.generateNewId();
                });
            }
        }
    }
});
