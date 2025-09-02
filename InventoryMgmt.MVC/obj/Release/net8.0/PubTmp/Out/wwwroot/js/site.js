// Theme management
document.addEventListener('DOMContentLoaded', function () {
    initializeTheme();
    initializeLanguage();
    initializeContextMenus();
    initializeSelectionToolbar();
    initializeTooltips();
    initializeAutoSave();
});

// Theme switching
function initializeTheme() {
    const themeToggle = document.getElementById('theme-toggle');
    const currentTheme = localStorage.getItem('theme') || 'light';

    // Set initial theme
    document.documentElement.setAttribute('data-theme', currentTheme);
    updateThemeToggle(currentTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', function (e) {
            e.preventDefault();
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateThemeToggle(newTheme);

            // Save to server
            saveThemePreference(newTheme);
        });
    }
}

function updateThemeToggle(theme) {
    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) {
        if (theme === 'dark') {
            themeToggle.innerHTML = '<i class="bi bi-sun-fill me-2"></i>Light Mode';
        } else {
            themeToggle.innerHTML = '<i class="bi bi-moon-fill me-2"></i>Dark Mode';
        }
    }
}

function saveThemePreference(theme) {
    fetch('/Account/UpdateTheme', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({ theme: theme })
    }).catch(err => console.log('Theme preference not saved:', err));
}

// Language switching
function initializeLanguage() {
    const languageToggle = document.getElementById('language-toggle');
    const currentLanguage = localStorage.getItem('language') || 'en';

    updateLanguageToggle(currentLanguage);

    if (languageToggle) {
        languageToggle.addEventListener('click', function (e) {
            e.preventDefault();
            const newLanguage = currentLanguage === 'en' ? 'es' : 'en';
            localStorage.setItem('language', newLanguage);

            // Reload page with new language
            const url = new URL(window.location);
            url.searchParams.set('culture', newLanguage);
            window.location.href = url.toString();
        });
    }
}

function updateLanguageToggle(language) {
    const languageToggle = document.getElementById('language-toggle');
    if (languageToggle) {
        if (language === 'es') {
            languageToggle.innerHTML = '<i class="bi bi-translate me-2"></i>English';
        } else {
            languageToggle.innerHTML = '<i class="bi bi-translate me-2"></i>Español';
        }
    }
}

// Context menus for table rows
function initializeContextMenus() {
    let contextMenu = null;

    // Right-click on table rows
    document.querySelectorAll('tbody tr[data-item-id], tbody tr[data-inventory-id]').forEach(row => {
        row.addEventListener('contextmenu', function (e) {
            e.preventDefault();

            const itemId = this.dataset.itemId;
            const inventoryId = this.dataset.inventoryId;
            const canEdit = this.dataset.canEdit === 'true';

            showContextMenu(e.clientX, e.clientY, itemId, inventoryId, canEdit);
        });
    });

    // Close context menu when clicking elsewhere
    document.addEventListener('click', function () {
        if (contextMenu) {
            contextMenu.remove();
            contextMenu = null;
        }
    });

    function showContextMenu(x, y, itemId, inventoryId, canEdit) {
        // Remove existing menu
        if (contextMenu) {
            contextMenu.remove();
        }

        // Create new menu
        contextMenu = document.createElement('div');
        contextMenu.className = 'context-menu fade-in';
        contextMenu.style.left = x + 'px';
        contextMenu.style.top = y + 'px';

        let menuItems = [];

        if (itemId) {
            menuItems.push(`<button class="context-menu-item" onclick="viewItem(${itemId})"><i class="bi bi-eye me-2"></i>View</button>`);
            if (canEdit) {
                menuItems.push(`<button class="context-menu-item" onclick="editItem(${itemId})"><i class="bi bi-pencil me-2"></i>Edit</button>`);
                menuItems.push(`<button class="context-menu-item" onclick="duplicateItem(${itemId})"><i class="bi bi-files me-2"></i>Duplicate</button>`);
                menuItems.push(`<hr class="my-1">`);
                menuItems.push(`<button class="context-menu-item text-danger" onclick="deleteItem(${itemId})"><i class="bi bi-trash me-2"></i>Delete</button>`);
            }
        } else if (inventoryId) {
            menuItems.push(`<button class="context-menu-item" onclick="viewInventory(${inventoryId})"><i class="bi bi-eye me-2"></i>View</button>`);
            if (canEdit) {
                menuItems.push(`<button class="context-menu-item" onclick="editInventory(${inventoryId})"><i class="bi bi-pencil me-2"></i>Edit</button>`);
                menuItems.push(`<hr class="my-1">`);
                menuItems.push(`<button class="context-menu-item text-danger" onclick="deleteInventory(${inventoryId})"><i class="bi bi-trash me-2"></i>Delete</button>`);
            }
        }

        contextMenu.innerHTML = menuItems.join('');
        document.body.appendChild(contextMenu);

        // Adjust position if menu goes off-screen
        const rect = contextMenu.getBoundingClientRect();
        if (rect.right > window.innerWidth) {
            contextMenu.style.left = (x - rect.width) + 'px';
        }
        if (rect.bottom > window.innerHeight) {
            contextMenu.style.top = (y - rect.height) + 'px';
        }
    }
}

// Selection toolbar for bulk operations
function initializeSelectionToolbar() {
    let selectedItems = new Set();
    const toolbar = document.querySelector('.selection-toolbar');

    // Handle checkbox changes
    document.querySelectorAll('.item-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function () {
            const itemId = this.value;
            const row = this.closest('tr');

            if (this.checked) {
                selectedItems.add(itemId);
                row.classList.add('selected-row');
            } else {
                selectedItems.delete(itemId);
                row.classList.remove('selected-row');
            }

            updateSelectionToolbar();
        });
    });

    // Handle select all checkbox
    const selectAllCheckbox = document.getElementById('select-all');
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function () {
            const checkboxes = document.querySelectorAll('.item-checkbox');
            checkboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
                checkbox.dispatchEvent(new Event('change'));
            });
        });
    }

    function updateSelectionToolbar() {
        if (!toolbar) return;

        const selectedCount = selectedItems.size;
        const countElement = toolbar.querySelector('.selected-count');

        if (selectedCount > 0) {
            toolbar.classList.remove('hide');
            toolbar.classList.add('show');
            if (countElement) {
                countElement.textContent = selectedCount;
            }
        } else {
            toolbar.classList.remove('show');
            toolbar.classList.add('hide');
        }

        // Update select all checkbox state
        if (selectAllCheckbox) {
            const totalCheckboxes = document.querySelectorAll('.item-checkbox').length;
            selectAllCheckbox.indeterminate = selectedCount > 0 && selectedCount < totalCheckboxes;
            selectAllCheckbox.checked = selectedCount === totalCheckboxes && totalCheckboxes > 0;
        }
    }

    // Bulk delete
    window.bulkDelete = function () {
        if (selectedItems.size === 0) return;

        if (confirm(`Are you sure you want to delete ${selectedItems.size} selected items?`)) {
            const itemIds = Array.from(selectedItems);

            fetch('/Item/BulkDelete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ itemIds: itemIds })
            }).then(response => {
                if (response.ok) {
                    // Remove deleted rows with animation
                    itemIds.forEach(itemId => {
                        const checkbox = document.querySelector(`.item-checkbox[value="${itemId}"]`);
                        const row = checkbox?.closest('tr');
                        if (row) {
                            row.style.transition = 'all 0.3s ease';
                            row.style.opacity = '0';
                            row.style.transform = 'translateX(-100%)';
                            setTimeout(() => row.remove(), 300);
                        }
                    });

                    selectedItems.clear();
                    updateSelectionToolbar();
                    showToast('Items deleted successfully', 'success');
                } else {
                    showToast('Error deleting items', 'error');
                }
            }).catch(err => {
                console.error('Error:', err);
                showToast('Error deleting items', 'error');
            });
        }
    };
}

// Initialize tooltips
function initializeTooltips() {
    const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltips.forEach(tooltip => {
        new bootstrap.Tooltip(tooltip);
    });
}

// Auto-save functionality
function initializeAutoSave() {
    let autoSaveTimer;
    let hasChanges = false;

    // Track form changes
    const forms = document.querySelectorAll('form[data-auto-save]');
    forms.forEach(form => {
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('input', function () {
                hasChanges = true;
                clearTimeout(autoSaveTimer);
                autoSaveTimer = setTimeout(() => {
                    if (hasChanges) {
                        autoSave(form);
                    }
                }, 7000); // Auto-save after 7 seconds of inactivity
            });
        });
    });

    function autoSave(form) {
        const formData = new FormData(form);
        const url = form.dataset.autoSaveUrl || form.action;

        fetch(url, {
            method: 'POST',
            body: formData
        }).then(response => {
            if (response.ok) {
                hasChanges = false;
                showToast('Changes saved automatically', 'info', 2000);
            }
        }).catch(err => {
            console.error('Auto-save failed:', err);
        });
    }
}

// Utility functions
function showToast(message, type = 'info', duration = 3000) {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-bg-${type} border-0 fade-in`;
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" aria-label="Close"></button>
        </div>
    `;

    toastContainer.appendChild(toast);

    // Auto-remove toast
    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    }, duration);

    // Handle manual close
    toast.querySelector('.btn-close').addEventListener('click', () => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    });
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1055';
    document.body.appendChild(container);
    return container;
}

// Context menu actions
window.viewItem = function (itemId) {
    window.location.href = `/Item/Details/${itemId}`;
};

window.editItem = function (itemId) {
    window.location.href = `/Item/Edit/${itemId}`;
};

window.deleteItem = function (itemId) {
    if (confirm('Are you sure you want to delete this item?')) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = `/Item/Delete/${itemId}`;

        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        form.appendChild(tokenInput);

        document.body.appendChild(form);
        form.submit();
    }
};

window.duplicateItem = function (itemId) {
    fetch(`/Item/Duplicate/${itemId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        }
    }).then(response => {
        if (response.ok) {
            return response.json();
        }
        throw new Error('Failed to duplicate item');
    }).then(data => {
        showToast('Item duplicated successfully', 'success');
        if (data.newItemId) {
            setTimeout(() => {
                window.location.href = `/Item/Edit/${data.newItemId}`;
            }, 1000);
        }
    }).catch(err => {
        console.error('Error:', err);
        showToast('Error duplicating item', 'error');
    });
};

window.viewInventory = function (inventoryId) {
    window.location.href = `/Inventory/Details/${inventoryId}`;
};

window.editInventory = function (inventoryId) {
    window.location.href = `/Inventory/Edit/${inventoryId}`;
};

window.deleteInventory = function (inventoryId) {
    if (confirm('Are you sure you want to delete this inventory? This will also delete all items in it.')) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = `/Inventory/Delete/${inventoryId}`;

        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        form.appendChild(tokenInput);

        document.body.appendChild(form);
        form.submit();
    }
};

// Like functionality
window.toggleLike = function (itemId, button) {
    fetch(`/Item/ToggleLike/${itemId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        }
    }).then(response => response.json()).then(data => {
        const icon = button.querySelector('i');
        const countElement = button.querySelector('.likes-count');

        if (data.liked) {
            button.classList.add('liked');
            icon.className = 'bi bi-heart-fill';
        } else {
            button.classList.remove('liked');
            icon.className = 'bi bi-heart';
        }

        if (countElement) {
            countElement.textContent = data.likesCount;
        }

        // Add animation
        button.style.transform = 'scale(1.2)';
        setTimeout(() => {
            button.style.transform = 'scale(1)';
        }, 200);
    }).catch(err => {
        console.error('Error toggling like:', err);
        showToast('Error updating like status', 'error');
    });
};

// Drag and drop for field reordering
function initializeDragAndDrop() {
    const fieldsList = document.getElementById('custom-fields-list');
    if (!fieldsList) return;

    let draggedElement = null;

    fieldsList.addEventListener('dragstart', function (e) {
        if (e.target.classList.contains('field-item')) {
            draggedElement = e.target;
            e.target.classList.add('dragging');
        }
    });

    fieldsList.addEventListener('dragend', function (e) {
        if (e.target.classList.contains('field-item')) {
            e.target.classList.remove('dragging');
            draggedElement = null;
        }
    });

    fieldsList.addEventListener('dragover', function (e) {
        e.preventDefault();
        const afterElement = getDragAfterElement(fieldsList, e.clientY);
        if (draggedElement) {
            if (afterElement == null) {
                fieldsList.appendChild(draggedElement);
            } else {
                fieldsList.insertBefore(draggedElement, afterElement);
            }
        }
    });

    function getDragAfterElement(container, y) {
        const draggableElements = [...container.querySelectorAll('.field-item:not(.dragging)')];

        return draggableElements.reduce((closest, child) => {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;

            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: child };
            } else {
                return closest;
            }
        }, { offset: Number.NEGATIVE_INFINITY }).element;
    }
}

// Custom ID format preview
function updateCustomIdPreview() {
    const formatInput = document.getElementById('custom-id-format');
    const previewElement = document.getElementById('custom-id-preview');

    if (!formatInput || !previewElement) return;

    const format = formatInput.value || '{SEQUENCE}';

    fetch(`/Inventory/GenerateCustomIdPreview?format=${encodeURIComponent(format)}`)
        .then(response => response.json())
        .then(data => {
            previewElement.textContent = data.preview || 'Invalid format';
        })
        .catch(err => {
            console.error('Error generating preview:', err);
            previewElement.textContent = 'Error generating preview';
        });
}

// Auto-complete functionality
function initializeAutoComplete() {
    const inputs = document.querySelectorAll('[data-autocomplete]');

    inputs.forEach(input => {
        let dropdown = null;
        let timeout = null;

        input.addEventListener('input', function () {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                const query = this.value.trim();
                const source = this.dataset.autocomplete;

                if (query.length >= 2) {
                    fetchSuggestions(source, query, this);
                } else {
                    hideDropdown();
                }
            }, 300);
        });

        input.addEventListener('blur', function () {
            setTimeout(hideDropdown, 200); // Delay to allow clicking on dropdown items
        });

        function fetchSuggestions(source, query, inputElement) {
            fetch(`/${source}?query=${encodeURIComponent(query)}`)
                .then(response => response.json())
                .then(data => showDropdown(data, inputElement))
                .catch(err => console.error('Auto-complete error:', err));
        }

        function showDropdown(suggestions, inputElement) {
            hideDropdown();

            if (suggestions.length === 0) return;

            dropdown = document.createElement('div');
            dropdown.className = 'autocomplete-dropdown';

            suggestions.forEach(suggestion => {
                const item = document.createElement('div');
                item.className = 'autocomplete-item';
                item.textContent = suggestion.text || suggestion;
                item.addEventListener('click', function () {
                    inputElement.value = suggestion.text || suggestion;
                    if (suggestion.value) {
                        inputElement.setAttribute('data-value', suggestion.value);
                    }
                    hideDropdown();
                    inputElement.dispatchEvent(new Event('change'));
                });
                dropdown.appendChild(item);
            });

            const inputRect = inputElement.getBoundingClientRect();
            dropdown.style.position = 'absolute';
            dropdown.style.top = (inputRect.bottom + window.scrollY) + 'px';
            dropdown.style.left = inputRect.left + 'px';
            dropdown.style.width = inputRect.width + 'px';

            document.body.appendChild(dropdown);
        }

        function hideDropdown() {
            if (dropdown) {
                dropdown.remove();
                dropdown = null;
            }
        }
    });
}

// Initialize all components when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log("DOM ready in site.js");
    initializeDragAndDrop();
    initializeAutoComplete();

    // Update custom ID preview on input
    const customIdInput = document.getElementById('custom-id-format');
    if (customIdInput) {
        customIdInput.addEventListener('input', updateCustomIdPreview);
        updateCustomIdPreview(); // Initial preview
    }
    
    // Direct event handlers for custom field buttons as a backup approach
    $(document).ready(function() {
        console.log("jQuery document ready in site.js");
        console.log("Checking for field buttons directly in document.ready");
        console.log("Text button exists:", $('#add-text-field').length);
        console.log("Numeric button exists:", $('#add-numeric-field').length);
        console.log("Boolean button exists:", $('#add-boolean-field').length);
        
        // Set direct click handlers
        $('#add-text-field').on('click', function() {
            console.log("Text field button clicked (from document.ready)");
            addCustomField('text');
            return false;
        });
        
        $('#add-numeric-field').on('click', function() {
            console.log("Numeric field button clicked (from document.ready)");
            addCustomField('numeric');
            return false;
        });
        
        $('#add-boolean-field').on('click', function() {
            console.log("Boolean field button clicked (from document.ready)");
            addCustomField('boolean');
            return false;
        });
    });
});

// Initialize inventory page functionality
function initializeInventoryPage(inventoryId) {
    console.log("Initializing inventory page with ID:", inventoryId);
    initializeCustomIdConfiguration();
    console.log("About to initialize custom fields");
    initializeCustomFields();
    console.log("Custom fields initialized");
    initializeChat(inventoryId);
    initializeItemSelection();
    initializeAutoSave();
    loadCustomIdElements();
    loadCustomFields();
    loadComments(inventoryId);
    loadAccessUsers(inventoryId);
}

// Custom ID Configuration
function initializeCustomIdConfiguration() {
    const addElementBtn = document.getElementById('add-custom-id-element');
    if (addElementBtn) {
        addElementBtn.addEventListener('click', addCustomIdElement);
    }

    // Initialize drag and drop for custom ID elements
    const elementsContainer = document.getElementById('custom-id-elements');
    if (elementsContainer) {
        initializeDragAndDropForElements(elementsContainer);
    }
}

function addCustomIdElement() {
    const container = document.getElementById('custom-id-elements');
    const elementId = 'element-' + Date.now();
    
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
                        <option value="fixed">Fixed</option>
                        <option value="20-bit random">20-bit random</option>
                        <option value="32-bit random">32-bit random</option>
                        <option value="6-digit random">6-digit random</option>
                        <option value="9-digit random">9-digit random</option>
                        <option value="guid">GUID</option>
                        <option value="date/time">Date/time</option>
                        <option value="sequence">Sequence</option>
                    </select>
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control element-value" placeholder="Format or text" data-element-id="${elementId}">
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
    updateCustomIdPreview();
    initializeElementEventListeners(elementId);
}

function initializeElementEventListeners(elementId) {
    const typeSelect = document.querySelector(`[data-element-id="${elementId}"].element-type`);
    const valueInput = document.querySelector(`[data-element-id="${elementId}"].element-value`);
    const removeBtn = document.querySelector(`[data-element-id="${elementId}"].remove-element`);
    const descriptionDiv = document.querySelector(`[data-element-id="${elementId}"] .element-description`);
    
    if (typeSelect) {
        typeSelect.addEventListener('change', function() {
            updateElementDescription(elementId);
            updateCustomIdPreview();
        });
    }
    
    if (valueInput) {
        valueInput.addEventListener('input', updateCustomIdPreview);
    }
    
    if (removeBtn) {
        removeBtn.addEventListener('click', function() {
            removeCustomIdElement(elementId);
        });
    }
    
    updateElementDescription(elementId);
}

function updateElementDescription(elementId) {
    const typeSelect = document.querySelector(`[data-element-id="${elementId}"].element-type`);
    const descriptionDiv = document.querySelector(`[data-element-id="${elementId}"] .element-description`);
    
    if (!typeSelect || !descriptionDiv) return;
    
    const descriptions = {
        'fixed': 'A piece of unchanging text. E.g., you can use Unicode emoji.',
        '20-bit random': 'A random value. E.g., you can format it as a six-digit decimal (D6) or 5-digit hex (X5).',
        '32-bit random': 'A random value. E.g., you can format it as a six-digit decimal (D6) or 5-digit hex (X5).',
        '6-digit random': 'A random value. E.g., you can format it as a six-digit decimal (D6) or 5-digit hex (X5).',
        '9-digit random': 'A random value. E.g., you can format it as a six-digit decimal (D6) or 5-digit hex (X5).',
        'guid': 'A globally unique identifier.',
        'date/time': 'An item creation date and time. E.g., you can use an abbreviated day of the week (ddd).',
        'sequence': 'A sequential index. E.g., you can format it with leading zeros (D4) or without them (D).'
    };
    
    descriptionDiv.textContent = descriptions[typeSelect.value] || '';
}

function removeCustomIdElement(elementId) {
    const element = document.querySelector(`[data-element-id="${elementId}"]`);
    if (element) {
        element.remove();
        updateCustomIdPreview();
    }
}

function updateCustomIdPreview() {
    const elements = getCustomIdElements();
    const previewElement = document.getElementById('custom-id-preview');
    
    if (!previewElement) return;
    
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
        previewElement.textContent = data.preview || 'Invalid format';
    })
    .catch(err => {
        console.error('Error generating preview:', err);
        previewElement.textContent = 'Error generating preview';
    });
}

function getCustomIdElements() {
    const elements = [];
    const elementDivs = document.querySelectorAll('.custom-id-element');
    
    elementDivs.forEach((div, index) => {
        const elementId = div.getAttribute('data-element-id');
        const typeSelect = div.querySelector('.element-type');
        const valueInput = div.querySelector('.element-value');
        
        if (typeSelect && valueInput) {
            elements.push({
                id: elementId,
                type: typeSelect.value,
                value: valueInput.value,
                order: index
            });
        }
    });
    
    return elements;
}

// Custom Fields Management
function initializeCustomFields() {
    // Using jQuery instead of vanilla JavaScript to ensure consistent handling
    console.log("Inside initializeCustomFields function");
    const addTextBtn = $('#add-text-field');
    const addNumericBtn = $('#add-numeric-field');
    const addBooleanBtn = $('#add-boolean-field');
    const saveFieldsBtn = $('#save-fields-button');
    const reloadFieldsBtn = $('#reload-fields-button');
    
    console.log("Text button found:", addTextBtn.length > 0);
    console.log("Numeric button found:", addNumericBtn.length > 0);
    console.log("Boolean button found:", addBooleanBtn.length > 0);
    console.log("Save button found:", saveFieldsBtn.length > 0);
    console.log("Reload button found:", reloadFieldsBtn.length > 0);
    
    if (addTextBtn.length) {
        console.log("Adding click handler to text button");
        addTextBtn.on('click', function() {
            console.log("Text button clicked");
            addCustomField('text');
        });
    }
    
    if (addNumericBtn.length) {
        console.log("Adding click handler to numeric button");
        addNumericBtn.on('click', function() {
            console.log("Numeric button clicked");
            addCustomField('numeric');
        });
    }
    
    if (addBooleanBtn.length) {
        console.log("Adding click handler to boolean button");
        addBooleanBtn.on('click', function() {
            console.log("Boolean button clicked");
            addCustomField('boolean');
        });
    }
    
    // Save button handler
    if (saveFieldsBtn.length) {
        console.log("Adding click handler to save button");
        saveFieldsBtn.on('click', function() {
            console.log("Save button clicked");
            saveCustomFields();
        });
    }
    
    // Reload button handler
    if (reloadFieldsBtn.length) {
        console.log("Adding click handler to reload button");
        reloadFieldsBtn.on('click', function() {
            console.log("Reload button clicked");
            const container = document.getElementById('custom-fields-list');
            if (container) {
                container.innerHTML = '<div class="alert alert-info">Loading fields...</div>';
            }
            loadCustomFields();
        });
    }
    
    // Clear all fields button handler
    const clearAllFieldsBtn = $('#clear-all-fields-button');
    if (clearAllFieldsBtn.length) {
        console.log("Adding click handler to clear all fields button");
        clearAllFieldsBtn.on('click', function() {
            if (confirm("Are you sure you want to clear ALL custom fields? This action cannot be undone once saved.")) {
                console.log("Clear all fields confirmed");
                
                const inventoryId = getInventoryIdFromUrl();
                if (!inventoryId) {
                    console.error("Could not determine inventory ID from URL");
                    return;
                }
                
                // Get the anti-forgery token
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                const headers = {
                    'Content-Type': 'application/json',
                };
                
                // Add the token if it exists
                if (token) {
                    headers['RequestVerificationToken'] = token;
                }
                
                // Send empty fields array to clear all fields
                fetch('/Inventory/SaveCustomFields', {
                    method: 'POST',
                    headers: headers,
                    body: JSON.stringify({ inventoryId: parseInt(inventoryId), fields: [] })
                })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Server returned ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success) {
                        // Clear the UI
                        const container = document.getElementById('custom-fields-list');
                        if (container) {
                            container.innerHTML = '';
                        }
                        
                        showToast('All custom fields cleared successfully', 'success');
                    } else {
                        console.error("Error clearing custom fields:", data.error);
                        showToast(`Error clearing fields: ${data.error || 'Unknown error'}`, 'error');
                    }
                })
                .catch(err => {
                    console.error('Error clearing custom fields:', err);
                    showToast(`Error clearing fields: ${err.message}`, 'error');
                });
            }
        });
    }
    
    // Debug panel functionality
    const toggleDebugBtn = $('#toggle-debug');
    const showIdsBtn = $('#debug-show-ids');
    const clearFieldsBtn = $('#debug-clear-fields');
    const logFieldsBtn = $('#debug-log-fields');
    
    if (toggleDebugBtn.length) {
        toggleDebugBtn.on('click', function() {
            $('#debug-content').toggle();
        });
    }
    
    if (showIdsBtn.length) {
        showIdsBtn.on('click', function() {
            $('.field-debug').toggle();
        });
    }
    
    if (clearFieldsBtn.length) {
        clearFieldsBtn.on('click', function() {
            if (confirm('Are you sure you want to clear all fields? This cannot be undone.')) {
                $('#custom-fields-list').empty();
                saveCustomFields();
            }
        });
    }
    
    if (logFieldsBtn.length) {
        logFieldsBtn.on('click', function() {
            const fields = getCustomFields();
            console.log('Current custom fields:', fields);
            
            const debugOutput = JSON.stringify(fields, null, 2);
            $('#debug-current-fields').html(`<pre>${debugOutput}</pre>`);
        });
    }
    
    // Debug Raw DB Values button
    const debugRawDbBtn = $('#debug-raw-db');
    if (debugRawDbBtn.length) {
        debugRawDbBtn.on('click', function() {
            const inventoryId = getInventoryIdFromUrl();
            if (!inventoryId) {
                console.error("Could not determine inventory ID from URL");
                return;
            }
            
            fetch(`/Inventory/DebugCustomFields/${inventoryId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Failed to load raw DB data: ${response.status} ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log("Raw DB data:", data);
                    
                    const debugOutput = JSON.stringify(data.customFields, null, 2);
                    $('#debug-current-fields').html(`<pre>RAW DB VALUES (Last updated: ${new Date(data.lastUpdated).toLocaleString()}):\n${debugOutput}</pre>`);
                })
                .catch(err => {
                    console.error('Error loading raw DB data:', err);
                    showToast('Error loading raw DB data', 'error');
                });
        });
    }
}

function addCustomField(type) {
    console.log("Adding custom field of type:", type);
    const container = $('#custom-fields-list');
    console.log("Container found:", container.length > 0);
    
    if (!container.length) {
        console.error("Custom fields container not found!");
        return;
    }
    
    // Generate a proper field ID that matches the server pattern
    const existingFields = container.find('.custom-field[data-field-type="' + type + '"]');
    const fieldNumber = existingFields.length + 1;
    const fieldId = `${type}-field-${fieldNumber}`;
    
    const defaultName = `${type.charAt(0).toUpperCase() + type.slice(1)} Field ${fieldNumber}`;
    
    console.log(`Generated field ID: ${fieldId} for type ${type}`);
    
    const fieldHtml = `
        <div class="custom-field field-item" draggable="true" data-field-id="${fieldId}" data-field-type="${type}">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="drag-handle">
                        <i class="bi bi-grip-vertical"></i>
                    </div>
                </div>
                <div class="col-md-3">
                    <input type="text" class="form-control field-name" placeholder="Field name" data-field-id="${fieldId}" value="${defaultName}">
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control field-description" placeholder="Description (optional)" data-field-id="${fieldId}">
                </div>
                <div class="col-md-2">
                    <div class="form-check">
                        <input class="form-check-input show-in-table" type="checkbox" data-field-id="${fieldId}">
                        <label class="form-check-label">Show in table</label>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="d-flex gap-1">
                        <span class="badge bg-secondary field-type-badge">${type}</span>
                        <button class="btn btn-sm btn-outline-danger remove-field" data-field-id="${fieldId}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    console.log("About to append HTML to container");
    container.append(fieldHtml);
    console.log("HTML appended");
    initializeFieldEventListeners(fieldId);
    console.log("Event listeners initialized");
    
    // Show toast to remind user to save
    showToast('Field added. Remember to save your changes!', 'info');
}

function initializeFieldEventListeners(fieldId) {
    const removeBtn = $(`[data-field-id="${fieldId}"].remove-field`);
    
    if (removeBtn.length) {
        removeBtn.on('click', function() {
            removeCustomField(fieldId);
        });
    }
}

function removeCustomField(fieldId) {
    const field = $(`[data-field-id="${fieldId}"]`);
    if (field.length) {
        field.remove();
    }
}

// Chat/Discussion functionality
function initializeChat(inventoryId) {
    const sendBtn = document.getElementById('send-comment');
    const commentInput = document.getElementById('new-comment');
    
    if (sendBtn && commentInput) {
        sendBtn.addEventListener('click', function() {
            sendComment(inventoryId, commentInput.value);
            commentInput.value = '';
        });
        
        commentInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendComment(inventoryId, commentInput.value);
                commentInput.value = '';
            }
        });
    }
}

function sendComment(inventoryId, content) {
    if (!content.trim()) return;
    
    fetch('/Inventory/AddComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            inventoryId: inventoryId,
            content: content
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            loadComments(inventoryId);
        } else {
            showToast('Error sending comment', 'error');
        }
    })
    .catch(err => {
        console.error('Error sending comment:', err);
        showToast('Error sending comment', 'error');
    });
}

function loadComments(inventoryId) {
    fetch(`/Inventory/GetComments/${inventoryId}`)
        .then(response => response.json())
        .then(data => {
            const container = document.getElementById('comments-container');
            if (container) {
                container.innerHTML = data.comments.map(comment => `
                    <div class="comment mb-3">
                        <div class="d-flex justify-content-between">
                            <strong>${comment.userName}</strong>
                            <small class="text-muted">${new Date(comment.createdAt).toLocaleString()}</small>
                        </div>
                        <div class="comment-content">${comment.content}</div>
                    </div>
                `).join('');
            }
        })
        .catch(err => {
            console.error('Error loading comments:', err);
        });
}

// Item selection and bulk actions
function initializeItemSelection() {
    const selectAllCheckbox = document.getElementById('selectAll');
    const itemCheckboxes = document.querySelectorAll('.item-checkbox');
    const selectionToolbar = document.getElementById('selectionToolbar');
    
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            itemCheckboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
            updateSelectionToolbar();
        });
    }
    
    itemCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', updateSelectionToolbar);
    });
    
    // Bulk action buttons
    const bulkEditBtn = document.getElementById('bulk-edit-btn');
    const bulkDeleteBtn = document.getElementById('bulk-delete-btn');
    const clearSelectionBtn = document.getElementById('clear-selection-btn');
    
    if (bulkEditBtn) bulkEditBtn.addEventListener('click', bulkEditItems);
    if (bulkDeleteBtn) bulkDeleteBtn.addEventListener('click', bulkDeleteItems);
    if (clearSelectionBtn) clearSelectionBtn.addEventListener('click', clearSelection);
}

function updateSelectionToolbar() {
    const selectedItems = document.querySelectorAll('.item-checkbox:checked');
    const toolbar = document.getElementById('selectionToolbar');
    
    if (selectedItems.length > 0) {
        toolbar.classList.remove('hide');
        toolbar.classList.add('show');
    } else {
        toolbar.classList.remove('show');
        toolbar.classList.add('hide');
    }
}

function clearSelection() {
    document.querySelectorAll('.item-checkbox').forEach(checkbox => {
        checkbox.checked = false;
    });
    document.getElementById('selectAll').checked = false;
    updateSelectionToolbar();
}

function bulkEditItems() {
    const selectedItems = Array.from(document.querySelectorAll('.item-checkbox:checked')).map(cb => cb.value);
    if (selectedItems.length > 0) {
        // Implement bulk edit functionality
        showToast(`Editing ${selectedItems.length} items`, 'info');
    }
}

function bulkDeleteItems() {
    const selectedItems = Array.from(document.querySelectorAll('.item-checkbox:checked')).map(cb => cb.value);
    if (selectedItems.length > 0) {
        if (confirm(`Are you sure you want to delete ${selectedItems.length} items?`)) {
            // Implement bulk delete functionality
            showToast(`Deleting ${selectedItems.length} items`, 'info');
        }
    }
}

// Auto-save functionality
function initializeAutoSave() {
    let saveTimeout;
    const saveDelay = 8000; // 8 seconds
    
    // Auto-save custom ID configuration
    const customIdContainer = document.getElementById('custom-id-elements');
    if (customIdContainer) {
        customIdContainer.addEventListener('input', function() {
            clearTimeout(saveTimeout);
            saveTimeout = setTimeout(saveCustomIdConfiguration, saveDelay);
        });
    }
    
    // Auto-save custom fields
    const customFieldsContainer = document.getElementById('custom-fields-list');
    if (customFieldsContainer) {
        customFieldsContainer.addEventListener('input', function() {
            clearTimeout(saveTimeout);
            saveTimeout = setTimeout(saveCustomFields, saveDelay);
        });
    }
}

function saveCustomIdConfiguration() {
    const elements = getCustomIdElements();
    const inventoryId = getInventoryIdFromUrl();
    
    if (!inventoryId || isNaN(inventoryId) || inventoryId <= 0) {
        console.error("Invalid inventory ID:", inventoryId);
        showToast('Error: Invalid inventory ID', 'error');
        return;
    }
    
    console.log("Saving custom ID configuration for inventory ID:", inventoryId);
    console.log("Elements to save:", elements);
    
    // Get the anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const headers = {
        'Content-Type': 'application/json',
    };
    
    // Add the token if it exists
    if (token) {
        headers['RequestVerificationToken'] = token;
    }
    
    fetch('/Inventory/SaveCustomIdConfiguration', {
        method: 'POST',
        headers: headers,
        body: JSON.stringify({ inventoryId: parseInt(inventoryId), elements: elements })
    })
    .then(response => {
        console.log("SaveCustomIdConfiguration response status:", response.status);
        if (!response.ok) {
            throw new Error(`Server returned ${response.status}: ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        console.log("SaveCustomIdConfiguration response data:", data);
        if (data.success) {
            showToast('Custom ID configuration saved', 'success');
        } else {
            console.error("Error saving custom ID configuration:", data.error);
            showToast(`Error saving configuration: ${data.error || 'Unknown error'}`, 'error');
        }
    })
    .catch(err => {
        console.error('Error saving custom ID configuration:', err);
        showToast(`Error saving configuration: ${err.message}`, 'error');
    });
}

function saveCustomFields() {
    const fields = getCustomFields();
    const inventoryId = getInventoryIdFromUrl();
    
    if (!inventoryId || isNaN(inventoryId) || inventoryId <= 0) {
        console.error("Invalid inventory ID:", inventoryId);
        showToast('Error: Invalid inventory ID', 'error');
        return;
    }
    
    console.log("Saving custom fields for inventory ID:", inventoryId);
    console.log("Fields to save:", fields);
    
    // Show save indicator
    const saveButton = document.getElementById('save-fields-button');
    const originalText = saveButton ? saveButton.innerHTML : '';
    if (saveButton) {
        saveButton.innerHTML = '<i class="bi bi-clock me-2"></i>Saving...';
        saveButton.disabled = true;
    }
    
    // Get the anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const headers = {
        'Content-Type': 'application/json',
    };
    
    // Add the token if it exists
    if (token) {
        headers['RequestVerificationToken'] = token;
    }
    
    fetch('/Inventory/SaveCustomFields', {
        method: 'POST',
        headers: headers,
        body: JSON.stringify({ inventoryId: parseInt(inventoryId), fields: fields })
    })
    .then(response => {
        console.log("SaveCustomFields response status:", response.status);
        if (!response.ok) {
            throw new Error(`Server returned ${response.status}: ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        console.log("SaveCustomFields response data:", data);
        if (data.success) {
            showToast(`Custom fields saved successfully (${fields.length} fields)`, 'success');
            
            // Update debug panel
            const debugOutput = JSON.stringify(fields, null, 2);
            $('#debug-current-fields').html(`<pre>Saved fields:\n${debugOutput}</pre>`);
        } else {
            console.error("Error saving custom fields:", data.error);
            showToast(`Error saving fields: ${data.error || 'Unknown error'}`, 'error');
        }
    })
    .catch(err => {
        console.error('Error saving custom fields:', err);
        showToast(`Error saving fields: ${err.message}`, 'error');
    })
    .finally(() => {
        // Restore save button
        if (saveButton) {
            saveButton.innerHTML = originalText;
            saveButton.disabled = false;
        }
    });
}

function getCustomFields() {
    const fields = [];
    const fieldDivs = document.querySelectorAll('.custom-field');
    
    console.log(`Found ${fieldDivs.length} custom field elements`);
    
    fieldDivs.forEach((div, index) => {
        const fieldId = div.getAttribute('data-field-id');
        const fieldType = div.getAttribute('data-field-type');
        const nameInput = div.querySelector('.field-name');
        const descriptionInput = div.querySelector('.field-description');
        const showInTableCheckbox = div.querySelector('.show-in-table');
        
        // Skip fields without a name
        if (nameInput && nameInput.value && nameInput.value.trim() !== '') {
            const fieldData = {
                id: fieldId,
                type: fieldType,
                name: nameInput.value.trim(),
                description: descriptionInput && descriptionInput.value ? descriptionInput.value.trim() : '',
                showInTable: showInTableCheckbox ? showInTableCheckbox.checked : false,
                order: index
            };
            
            console.log(`Adding field: ${fieldData.type} - ${fieldData.name} (${fieldData.id})`);
            fields.push(fieldData);
        } else {
            console.log(`Skipping empty field at index ${index}`);
        }
    });
    
    console.log(`Total of ${fields.length} fields to save`);
    return fields;
}

// Load data functions
function loadCustomIdElements() {
    const inventoryId = getInventoryIdFromUrl();
    if (!inventoryId) {
        console.error("Could not determine inventory ID from URL");
        return;
    }
    
    console.log(`Loading custom ID elements for inventory ID: ${inventoryId}`);
    
    fetch(`/Inventory/GetCustomIdElements/${inventoryId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Failed to load custom ID elements: ${response.status} ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            console.log("Custom ID elements loaded:", data);
            const container = document.getElementById('custom-id-elements');
            if (container && data.elements) {
                container.innerHTML = '';
                data.elements.forEach(element => {
                    addCustomIdElementFromData(element);
                });
                updateCustomIdPreview();
                
                // If elements were loaded, we should save them immediately to ensure format is correct
                saveCustomIdConfiguration();
            } else {
                console.warn("Custom ID elements container not found or no elements in response");
            }
        })
        .catch(err => {
            console.error('Error loading custom ID elements:', err);
            showToast('Error loading custom ID configuration', 'error');
        });
}

function loadCustomFields() {
    const inventoryId = getInventoryIdFromUrl();
    if (!inventoryId) {
        console.error("Could not determine inventory ID from URL");
        return;
    }
    
    console.log(`Loading custom fields for inventory ID: ${inventoryId}`);
    
    fetch(`/Inventory/GetCustomFields/${inventoryId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Failed to load custom fields: ${response.status} ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            console.log("Custom fields loaded:", data);
            const container = document.getElementById('custom-fields-list');
            if (container && data.fields) {
                container.innerHTML = '';
                // Clear any debug notification
                if (document.getElementById('debug-notification')) {
                    document.getElementById('debug-notification').remove();
                }
                
                // Add a debug notification with the returned data
                if (data.debug) {
                    const debugElement = document.createElement('div');
                    debugElement.id = 'debug-notification';
                    debugElement.className = 'alert alert-info small';
                    debugElement.innerHTML = `Loaded ${data.fields.length} fields for inventory #${data.debug.inventoryId}`;
                    container.parentNode.insertBefore(debugElement, container);
                    
                    // Auto-hide after 5 seconds
                    setTimeout(() => {
                        if (document.getElementById('debug-notification')) {
                            document.getElementById('debug-notification').remove();
                        }
                    }, 5000);
                }
                
                // Process the fields
                data.fields.forEach(field => {
                    console.log("Processing field:", field);
                    addCustomFieldFromData(field);
                });
                
                // Update the debug panel
                const debugOutput = JSON.stringify(data.fields, null, 2);
                $('#debug-current-fields').html(`<pre>${debugOutput}</pre>`);
                
                console.log(`Successfully loaded ${data.fields.length} custom fields`);
            } else {
                console.warn("Custom fields container not found or no fields in response");
            }
        })
        .catch(err => {
            console.error('Error loading custom fields:', err);
            showToast('Error loading custom fields', 'error');
        });
}

function loadAccessUsers(inventoryId) {
    fetch(`/Inventory/GetAccessUsers/${inventoryId}`)
        .then(response => response.json())
        .then(data => {
            const container = document.getElementById('access-users-list');
            if (container && data.users) {
                container.innerHTML = data.users.map(user => `
                    <div class="access-user-item d-flex justify-content-between align-items-center p-2 border rounded mb-2">
                        <div>
                            <strong>${user.firstName} ${user.lastName}</strong>
                            <br><small class="text-muted">${user.email}</small>
                        </div>
                        <button class="btn btn-sm btn-outline-danger remove-access" data-user-id="${user.id}">
                            <i class="bi bi-x-circle"></i>
                        </button>
                    </div>
                `).join('');
            }
        })
        .catch(err => {
            console.error('Error loading access users:', err);
        });
}

// Utility functions
function getInventoryIdFromUrl() {
    const urlParts = window.location.pathname.split('/');
    console.log("URL parts:", urlParts);
    
    // Try to find 'Details' in the path
    const idIndex = urlParts.indexOf('Details') + 1;
    
    if (idIndex > 0 && idIndex < urlParts.length) {
        console.log(`Found inventory ID ${urlParts[idIndex]} at index ${idIndex} (after 'Details')`);
        return urlParts[idIndex];
    }
    
    // If we couldn't find it after 'Details', try to look for a number in the URL
    for (let i = 0; i < urlParts.length; i++) {
        if (!isNaN(urlParts[i]) && urlParts[i] !== '') {
            console.log(`Found inventory ID ${urlParts[i]} at index ${i} (numeric part in URL)`);
            return urlParts[i];
        }
    }
    
    // If we still can't find it, look for a hidden input with inventory ID
    const hiddenInventoryInput = document.querySelector('input[name="InventoryId"]');
    if (hiddenInventoryInput) {
        console.log(`Found inventory ID ${hiddenInventoryInput.value} in hidden input`);
        return hiddenInventoryInput.value;
    }
    
    // Look for it in the model value
    const modelIdElement = document.querySelector('[data-inventory-id]');
    if (modelIdElement) {
        console.log(`Found inventory ID ${modelIdElement.dataset.inventoryId} in data attribute`);
        return modelIdElement.dataset.inventoryId;
    }
    
    console.error("Could not determine inventory ID from URL or DOM");
    return null;
}

function addCustomIdElementFromData(element) {
    console.log("Adding custom ID element from data:", element);
    const container = document.getElementById('custom-id-elements');
    
    if (!container) {
        console.error("Custom ID elements container not found");
        return;
    }
    
    const elementId = element.id || 'element-' + Date.now();
    
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
                        <option value="fixed" ${element.type === 'fixed' ? 'selected' : ''}>Fixed</option>
                        <option value="20-bit random" ${element.type === '20-bit random' ? 'selected' : ''}>20-bit random</option>
                        <option value="32-bit random" ${element.type === '32-bit random' ? 'selected' : ''}>32-bit random</option>
                        <option value="6-digit random" ${element.type === '6-digit random' ? 'selected' : ''}>6-digit random</option>
                        <option value="9-digit random" ${element.type === '9-digit random' ? 'selected' : ''}>9-digit random</option>
                        <option value="guid" ${element.type === 'guid' ? 'selected' : ''}>GUID</option>
                        <option value="date/time" ${element.type === 'date/time' ? 'selected' : ''}>Date/time</option>
                        <option value="sequence" ${element.type === 'sequence' ? 'selected' : ''}>Sequence</option>
                    </select>
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control element-value" placeholder="Format or text" data-element-id="${elementId}" value="${element.value || ''}">
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
}

function addCustomFieldFromData(field) {
    console.log("Adding custom field from data:", field);
    const container = $('#custom-fields-list');
    
    if (!container.length) {
        console.error("Custom fields container not found");
        return;
    }
    
    const fieldId = field.id || 'field-' + Date.now();
    const fieldType = field.type || 'text';
    
    // Check if a field with this ID already exists - if so, remove it first to avoid duplicates
    const existingField = document.querySelector(`.custom-field[data-field-id="${fieldId}"]`);
    if (existingField) {
        console.log(`Field with ID ${fieldId} already exists, removing it first`);
        existingField.remove();
    }
    
    // Add special styling for fields loaded from server
    const loadedClass = field.id && field.id.includes('-field-') ? 'loaded-field' : '';
    
    const fieldHtml = `
        <div class="custom-field field-item ${loadedClass}" draggable="true" data-field-id="${fieldId}" data-field-type="${fieldType}">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="drag-handle">
                        <i class="bi bi-grip-vertical"></i>
                    </div>
                </div>
                <div class="col-md-3">
                    <input type="text" class="form-control field-name" placeholder="Field name" data-field-id="${fieldId}" value="${field.name || ''}">
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control field-description" placeholder="Description (optional)" data-field-id="${fieldId}" value="${field.description || ''}">
                </div>
                <div class="col-md-2">
                    <div class="form-check">
                        <input class="form-check-input show-in-table" type="checkbox" data-field-id="${fieldId}" ${field.showInTable ? 'checked' : ''}>
                        <label class="form-check-label">Show in table</label>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="d-flex gap-1">
                        <span class="badge bg-secondary field-type-badge">${fieldType}</span>
                        <button class="btn btn-sm btn-outline-danger remove-field" data-field-id="${fieldId}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
            <div class="field-debug text-muted small" style="display: none;">
                ID: ${fieldId}, Type: ${fieldType}, Order: ${field.order || 0}
            </div>
        </div>
    `;
    
    container.append(fieldHtml);
    initializeFieldEventListeners(fieldId);
    
    // Flash the field to indicate it was added
    const addedField = document.querySelector(`.custom-field[data-field-id="${fieldId}"]`);
    if (addedField) {
        addedField.classList.add('field-flash');
        setTimeout(() => {
            addedField.classList.remove('field-flash');
        }, 1000);
    }
}

// Export functions
function exportData(format) {
    const inventoryId = getInventoryIdFromUrl();
    if (!inventoryId) return;
    
    window.open(`/Inventory/ExportData/${inventoryId}?format=${format}`, '_blank');
}

function exportSettings(format) {
    const inventoryId = getInventoryIdFromUrl();
    if (!inventoryId) return;
    
    window.open(`/Inventory/ExportSettings/${inventoryId}?format=${format}`, '_blank');
}