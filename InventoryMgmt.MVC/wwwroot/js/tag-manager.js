/**
 * Tag manager for inventory management system.
 * Handles tag autocomplete, addition, and removal.
 */
class TagManager {
    constructor(options) {
        this.inventoryId = options.inventoryId;
        this.tagInputSelector = options.tagInputSelector || '#tagInput';
        this.tagContainerSelector = options.tagContainerSelector || '#tagsContainer';
        this.addTagButtonSelector = options.addTagButtonSelector || '#addTagButton';
        
        this.tagInput = document.querySelector(this.tagInputSelector);
        this.tagContainer = document.querySelector(this.tagContainerSelector);
        this.addTagButton = document.querySelector(this.addTagButtonSelector);
        
        this.setupEventListeners();
        this.loadExistingTags();
    }
    
    /**
     * Set up event listeners for tag management
     */
    setupEventListeners() {
        // Set up autocomplete on tag input
        $(this.tagInputSelector).autocomplete({
            source: (request, response) => {
                $.ajax({
                    url: '/Inventory/SearchTags',
                    dataType: 'json',
                    data: { term: request.term },
                    success: (data) => {
                        response(data.map(tag => ({
                            label: tag.name,
                            value: tag.name,
                            id: tag.id
                        })));
                    }
                });
            },
            minLength: 1,
            select: (event, ui) => {
                event.preventDefault();
                this.tagInput.value = ui.item.value;
            }
        });
        
        // Add tag when button is clicked
        if (this.addTagButton) {
            this.addTagButton.addEventListener('click', () => this.addTag());
        }
        
        // Add tag when Enter key is pressed
        if (this.tagInput) {
            this.tagInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.addTag();
                }
            });
        }
    }
    
    /**
     * Load existing tags for the current inventory
     */
    loadExistingTags() {
        if (!this.inventoryId) return;
        
        fetch(`/Inventory/GetInventoryTags?id=${this.inventoryId}`)
            .then(response => response.json())
            .then(data => {
                if (data.tags && data.tags.length) {
                    this.tagContainer.innerHTML = '';
                    data.tags.forEach(tag => this.renderTagElement(tag));
                }
            })
            .catch(error => console.error('Error loading tags:', error));
    }
    
    /**
     * Add a new tag to the inventory
     */
    addTag() {
        const tagName = this.tagInput.value.trim();
        if (!tagName) return;
        
        // Add the tag to the server
        fetch(`/Inventory/AddTag?inventoryId=${this.inventoryId}&tagName=${encodeURIComponent(tagName)}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Refresh tags to get the newly created tag with its ID
                this.loadExistingTags();
                this.tagInput.value = '';
            }
        })
        .catch(error => console.error('Error adding tag:', error));
    }
    
    /**
     * Remove a tag from the inventory
     * @param {number} tagId - The ID of the tag to remove
     */
    removeTag(tagId) {
        fetch(`/Inventory/RemoveTag?inventoryId=${this.inventoryId}&tagId=${tagId}`, {
            method: 'POST'
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Remove the tag element from the DOM
                const tagElement = document.querySelector(`.tag-item[data-tag-id="${tagId}"]`);
                if (tagElement) {
                    tagElement.remove();
                }
            }
        })
        .catch(error => console.error('Error removing tag:', error));
    }
    
    /**
     * Render a tag element in the container
     * @param {object} tag - The tag object with id and name properties
     */
    renderTagElement(tag) {
        const tagElement = document.createElement('div');
        tagElement.className = 'tag-item';
        tagElement.dataset.tagId = tag.id;
        
        const tagText = document.createElement('span');
        tagText.textContent = tag.name;
        
        const removeButton = document.createElement('button');
        removeButton.type = 'button';
        removeButton.className = 'tag-remove';
        removeButton.textContent = '×';
        removeButton.title = 'Remove tag';
        removeButton.addEventListener('click', () => this.removeTag(tag.id));
        
        tagElement.appendChild(tagText);
        tagElement.appendChild(removeButton);
        this.tagContainer.appendChild(tagElement);
    }
}

// Initialize tag manager when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Check if we're on a page with inventory tags
    const inventoryId = document.getElementById('inventoryId');
    const tagInput = document.getElementById('tagInput');
    
    if (inventoryId && tagInput) {
        new TagManager({
            inventoryId: parseInt(inventoryId.value),
            tagInputSelector: '#tagInput',
            tagContainerSelector: '#tagsContainer',
            addTagButtonSelector: '#addTagButton'
        });
    }
});
