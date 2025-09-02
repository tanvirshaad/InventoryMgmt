/**
 * Item Handler - Handles AJAX requests for item operations with toast notifications
 */

const ItemHandler = {
    /**
     * Toggle item like status
     * @param {number} itemId - The ID of the item
     * @param {HTMLElement} button - The like button element
     */
    toggleLike: function(itemId, button) {
        // Get the anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        fetch(`/Item/ToggleLike/${itemId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: `id=${itemId}&__RequestVerificationToken=${token}`
        })
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    // User not authenticated
                    ToastUtility.error('You must be logged in to like items');
                    return Promise.reject('Not authenticated');
                } else if (response.status === 403) {
                    // User doesn't have permission
                    ToastUtility.error('You don\'t have permission to like this item');
                    return Promise.reject('Forbidden');
                } else if (response.status === 404) {
                    // Item not found
                    ToastUtility.error('Item not found');
                    return Promise.reject('Not found');
                }
                return Promise.reject('Server error');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Update the UI
                const likeIcon = button.querySelector('i');
                const likeCount = button.closest('.like-container').querySelector('.like-count');
                
                if (data.isLiked) {
                    likeIcon.classList.remove('bi-heart');
                    likeIcon.classList.add('bi-heart-fill');
                    ToastUtility.success('Item liked');
                } else {
                    likeIcon.classList.remove('bi-heart-fill');
                    likeIcon.classList.add('bi-heart');
                    ToastUtility.info('Item unliked');
                }
                
                // Update the like count
                if (likeCount) {
                    likeCount.textContent = data.likesCount;
                }
            } else {
                ToastUtility.error(data.message || 'Failed to update like status');
            }
        })
        .catch(error => {
            console.error('Error toggling like:', error);
            // If not already handled above
            if (error !== 'Not authenticated' && error !== 'Forbidden' && error !== 'Not found') {
                ToastUtility.error('Error updating like status');
            }
        });
    },
    
    /**
     * Duplicate an item
     * @param {number} itemId - The ID of the item to duplicate
     */
    duplicateItem: function(itemId) {
        // Get the anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        ToastUtility.info('Duplicating item...', { autoHide: false, id: 'duplicate-toast' });
        
        fetch(`/Item/Duplicate/${itemId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            }
        })
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    // User not authenticated
                    ToastUtility.error('You must be logged in to duplicate items');
                    return Promise.reject('Not authenticated');
                } else if (response.status === 403) {
                    // User doesn't have permission
                    ToastUtility.error('You don\'t have permission to duplicate this item');
                    return Promise.reject('Forbidden');
                } else if (response.status === 404) {
                    // Item not found
                    ToastUtility.error('Item not found');
                    return Promise.reject('Not found');
                }
                return Promise.reject('Server error');
            }
            return response.json();
        })
        .then(data => {
            // Hide the "Duplicating" toast
            ToastUtility.hide('duplicate-toast');
            
            if (data.success) {
                ToastUtility.success('Item duplicated successfully');
                // Reload the page after a short delay
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                ToastUtility.error(data.message || 'Failed to duplicate item');
            }
        })
        .catch(error => {
            console.error('Error duplicating item:', error);
            // Hide the "Duplicating" toast
            ToastUtility.hide('duplicate-toast');
            
            // If not already handled above
            if (error !== 'Not authenticated' && error !== 'Forbidden' && error !== 'Not found') {
                ToastUtility.error('Error duplicating item');
            }
        });
    },
    
    /**
     * Delete an item
     * @param {number} itemId - The ID of the item to delete
     * @param {boolean} isAjax - Whether to handle this as an AJAX request
     */
    deleteItem: function(itemId, isAjax = true) {
        // Get confirmation from user
        if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
            return;
        }
        
        // Get the anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        ToastUtility.info('Deleting item...', { autoHide: false, id: 'delete-toast' });
        
        fetch(`/Item/Delete/${itemId}?isAjax=${isAjax}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            }
        })
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    // User not authenticated
                    ToastUtility.error('You must be logged in to delete items');
                    return Promise.reject('Not authenticated');
                } else if (response.status === 403) {
                    // User doesn't have permission
                    ToastUtility.error('You don\'t have permission to delete this item');
                    return Promise.reject('Forbidden');
                } else if (response.status === 404) {
                    // Item not found
                    ToastUtility.error('Item not found');
                    return Promise.reject('Not found');
                }
                return Promise.reject('Server error');
            }
            return response.json();
        })
        .then(data => {
            // Hide the "Deleting" toast
            ToastUtility.hide('delete-toast');
            
            if (data.success) {
                ToastUtility.success('Item deleted successfully');
                // Redirect to the inventory details page
                setTimeout(() => {
                    window.location.href = `/Inventory/Details/${data.inventoryId}`;
                }, 1000);
            } else {
                ToastUtility.error(data.message || 'Failed to delete item');
            }
        })
        .catch(error => {
            console.error('Error deleting item:', error);
            // Hide the "Deleting" toast
            ToastUtility.hide('delete-toast');
            
            // If not already handled above
            if (error !== 'Not authenticated' && error !== 'Forbidden' && error !== 'Not found') {
                ToastUtility.error('Error deleting item');
            }
        });
    },
    
    /**
     * Delete multiple items
     * @param {Array<number>} itemIds - The IDs of the items to delete
     */
    deleteMultipleItems: function(itemIds) {
        if (!itemIds || itemIds.length === 0) {
            ToastUtility.warning('No items selected for deletion');
            return;
        }
        
        // Get confirmation from user
        if (!confirm(`Are you sure you want to delete ${itemIds.length} item(s)? This action cannot be undone.`)) {
            return;
        }
        
        // Get the anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        ToastUtility.info(`Deleting ${itemIds.length} item(s)...`, { autoHide: false, id: 'delete-multi-toast' });
        
        fetch('/Item/DeleteMultiple', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(itemIds)
        })
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    // User not authenticated
                    ToastUtility.error('You must be logged in to delete items');
                    return Promise.reject('Not authenticated');
                } else if (response.status === 403) {
                    // User doesn't have permission
                    ToastUtility.error('You don\'t have permission to delete these items');
                    return Promise.reject('Forbidden');
                }
                return Promise.reject('Server error');
            }
            return response.json();
        })
        .then(data => {
            // Hide the "Deleting" toast
            ToastUtility.hide('delete-multi-toast');
            
            if (data.success) {
                ToastUtility.success(data.message || 'Items deleted successfully');
                // Reload the page after a short delay
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                ToastUtility.error(data.message || 'Failed to delete items');
            }
        })
        .catch(error => {
            console.error('Error deleting items:', error);
            // Hide the "Deleting" toast
            ToastUtility.hide('delete-multi-toast');
            
            // If not already handled above
            if (error !== 'Not authenticated' && error !== 'Forbidden') {
                ToastUtility.error('Error deleting items');
            }
        });
    },
    
    /**
     * Generate a custom ID for a new item
     * @param {number} inventoryId - The ID of the inventory
     * @param {HTMLElement} targetElement - The input element to update with the generated ID
     */
    generateCustomId: function(inventoryId, targetElement) {
        fetch(`/Item/GenerateItemCustomId?inventoryId=${inventoryId}`)
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    // User not authenticated
                    ToastUtility.error('You must be logged in to generate custom IDs');
                    return Promise.reject('Not authenticated');
                } else if (response.status === 403) {
                    // User doesn't have permission
                    ToastUtility.error('You don\'t have permission to generate custom IDs for this inventory');
                    return Promise.reject('Forbidden');
                } else if (response.status === 404) {
                    // Inventory not found
                    ToastUtility.error('Inventory not found');
                    return Promise.reject('Not found');
                }
                return Promise.reject('Server error');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Update the target element with the generated ID
                if (targetElement) {
                    targetElement.value = data.customId;
                    // Trigger change event to update validation
                    const event = new Event('change', { bubbles: true });
                    targetElement.dispatchEvent(event);
                }
            } else {
                ToastUtility.error(data.message || 'Failed to generate custom ID');
            }
        })
        .catch(error => {
            console.error('Error generating custom ID:', error);
            // If not already handled above
            if (error !== 'Not authenticated' && error !== 'Forbidden' && error !== 'Not found') {
                ToastUtility.error('Error generating custom ID');
            }
        });
    }
};
