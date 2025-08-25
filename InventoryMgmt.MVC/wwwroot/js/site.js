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
    initializeDragAndDrop();
    initializeAutoComplete();

    // Update custom ID preview on input
    const customIdInput = document.getElementById('custom-id-format');
    if (customIdInput) {
        customIdInput.addEventListener('input', updateCustomIdPreview);
        updateCustomIdPreview(); // Initial preview
    }
});

// Initialize inventory page functionality
function initializeInventoryPage(inventoryId) {
    initializeCustomIdConfiguration();
    initializeCustomFields();
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
    const addTextBtn = document.getElementById('add-text-field');
    const addNumericBtn = document.getElementById('add-numeric-field');
    const addBooleanBtn = document.getElementById('add-boolean-field');
    
    if (addTextBtn) addTextBtn.addEventListener('click', () => addCustomField('text'));
    if (addNumericBtn) addNumericBtn.addEventListener('click', () => addCustomField('numeric'));
    if (addBooleanBtn) addBooleanBtn.addEventListener('click', () => addCustomField('boolean'));
}

function addCustomField(type) {
    const container = document.getElementById('custom-fields-list');
    const fieldId = 'field-' + Date.now();
    
    const fieldHtml = `
        <div class="custom-field field-item" draggable="true" data-field-id="${fieldId}" data-field-type="${type}">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="drag-handle">
                        <i class="bi bi-grip-vertical"></i>
                    </div>
                </div>
                <div class="col-md-3">
                    <input type="text" class="form-control field-name" placeholder="Field name" data-field-id="${fieldId}">
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
    
    container.insertAdjacentHTML('beforeend', fieldHtml);
    initializeFieldEventListeners(fieldId);
}

function initializeFieldEventListeners(fieldId) {
    const removeBtn = document.querySelector(`[data-field-id="${fieldId}"].remove-field`);
    
    if (removeBtn) {
        removeBtn.addEventListener('click', function() {
            removeCustomField(fieldId);
        });
    }
}

function removeCustomField(fieldId) {
    const field = document.querySelector(`[data-field-id="${fieldId}"]`);
    if (field) {
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
    
    fetch('/Inventory/SaveCustomIdConfiguration', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ elements: elements })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast('Custom ID configuration saved', 'success');
        } else {
            showToast('Error saving configuration', 'error');
        }
    })
    .catch(err => {
        console.error('Error saving custom ID configuration:', err);
        showToast('Error saving configuration', 'error');
    });
}

function saveCustomFields() {
    const fields = getCustomFields();
    
    fetch('/Inventory/SaveCustomFields', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ fields: fields })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast('Custom fields saved', 'success');
        } else {
            showToast('Error saving fields', 'error');
        }
    })
    .catch(err => {
        console.error('Error saving custom fields:', err);
        showToast('Error saving fields', 'error');
    });
}

function getCustomFields() {
    const fields = [];
    const fieldDivs = document.querySelectorAll('.custom-field');
    
    fieldDivs.forEach((div, index) => {
        const fieldId = div.getAttribute('data-field-id');
        const fieldType = div.getAttribute('data-field-type');
        const nameInput = div.querySelector('.field-name');
        const descriptionInput = div.querySelector('.field-description');
        const showInTableCheckbox = div.querySelector('.show-in-table');
        
        if (nameInput) {
            fields.push({
                id: fieldId,
                type: fieldType,
                name: nameInput.value,
                description: descriptionInput ? descriptionInput.value : '',
                showInTable: showInTableCheckbox ? showInTableCheckbox.checked : false,
                order: index
            });
        }
    });
    
    return fields;
}

// Load data functions
function loadCustomIdElements() {
    const inventoryId = getInventoryIdFromUrl();
    if (!inventoryId) return;
    
    fetch(`/Inventory/GetCustomIdElements/${inventoryId}`)
        .then(response => response.json())
        .then(data => {
            const container = document.getElementById('custom-id-elements');
            if (container && data.elements) {
                container.innerHTML = '';
                data.elements.forEach(element => {
                    addCustomIdElementFromData(element);
                });
                updateCustomIdPreview();
            }
        })
        .catch(err => {
            console.error('Error loading custom ID elements:', err);
        });
}

function loadCustomFields() {
    const inventoryId = getInventoryIdFromUrl();
    if (!inventoryId) return;
    
    fetch(`/Inventory/GetCustomFields/${inventoryId}`)
        .then(response => response.json())
        .then(data => {
            const container = document.getElementById('custom-fields-list');
            if (container && data.fields) {
                container.innerHTML = '';
                data.fields.forEach(field => {
                    addCustomFieldFromData(field);
                });
            }
        })
        .catch(err => {
            console.error('Error loading custom fields:', err);
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
    const idIndex = urlParts.indexOf('Details') + 1;
    return urlParts[idIndex];
}

function addCustomIdElementFromData(element) {
    // Implementation for adding element from server data
}

function addCustomFieldFromData(field) {
    // Implementation for adding field from server data
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