// like-handler.js - Simple script to handle like button clicks

document.addEventListener('DOMContentLoaded', function() {
    console.log('Like handler initialized');
    
    // Disable any existing like button handlers from inventory-detail.js to avoid duplicates
    // We'll do this by setting a flag that our dedicated handler has taken control
    window.likeHandlerActive = true;
    
    // Log all clicks for debugging
    document.addEventListener('click', function(e) {
        console.log('Clicked element:', e.target);
        console.log('Is or contains like button:', e.target.classList.contains('like-btn') || e.target.closest('.like-btn'));
    });
    
    // Create a hidden form for like submissions
    const likeForm = document.createElement('form');
    likeForm.id = 'like-form';
    likeForm.method = 'post';
    likeForm.style.display = 'none';
    
    // Add the CSRF token if available
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = token.value;
        likeForm.appendChild(tokenInput);
    }
    
    document.body.appendChild(likeForm);
    
    // Handle clicks on like buttons
    document.addEventListener('click', function(e) {
        // Check if the click was on a like button or its child
        const likeBtn = e.target.classList.contains('like-btn') 
            ? e.target 
            : e.target.closest('.like-btn');
            
        if (!likeBtn) return;
        
        // Stop propagation to prevent other handlers from firing
        e.stopPropagation();
        e.preventDefault();
        
        console.log('Like button clicked', likeBtn);
        
        // Get authentication state and log it for debugging
        const authState = likeBtn.getAttribute('data-authenticated');
        console.log('Auth state:', authState, 'Type:', typeof authState);
        
        // Check if user is authenticated - compare as string without type checking
        // Only show the alert if explicitly false (not if null, undefined, etc.)
        if (authState === 'false') {
            console.log('User is not authenticated, showing alert');
            ToastUtility.warning('You must be logged in to like items.');
            return;
        } else {
            console.log('User is authenticated, continuing with like action');
        }
        
        const itemId = likeBtn.getAttribute('data-item-id');
        if (!itemId) {
            console.error('No item ID found on like button');
            return;
        }
        
        console.log('Processing like for item:', itemId);
        
        // Use the form to submit the request
        likeForm.action = `/Item/ToggleLike/${itemId}`;
        
        // Use the Fetch API to submit the form and get a JSON response
        fetch(likeForm.action, {
            method: 'POST',
            body: new FormData(likeForm),
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Like response:', data);
            
            if (data.success) {
                // Update the heart icon
                const icon = likeBtn.querySelector('i');
                if (icon) {
                    if (data.isLiked) {
                        icon.classList.remove('bi-heart');
                        icon.classList.add('bi-heart-fill');
                    } else {
                        icon.classList.remove('bi-heart-fill');
                        icon.classList.add('bi-heart');
                    }
                }
                
                // Update the tooltip
                likeBtn.setAttribute('data-bs-original-title', data.isLiked ? 'Unlike' : 'Like');
                
                // Reinitialize tooltip
                if (window.bootstrap && bootstrap.Tooltip) {
                    const tooltip = bootstrap.Tooltip.getInstance(likeBtn);
                    if (tooltip) {
                        tooltip.dispose();
                    }
                    new bootstrap.Tooltip(likeBtn);
                }
                
                // Update the like count
                if (data.likesCount !== undefined) {
                    const likesCount = document.querySelector(`span.likes-count[data-item-id="${itemId}"]`);
                    if (likesCount) {
                        likesCount.textContent = data.likesCount;
                    }
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            ToastUtility.error('Failed to process like. Please try again.');
        });
    });
});
