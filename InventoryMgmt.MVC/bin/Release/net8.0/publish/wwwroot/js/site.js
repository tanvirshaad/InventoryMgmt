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