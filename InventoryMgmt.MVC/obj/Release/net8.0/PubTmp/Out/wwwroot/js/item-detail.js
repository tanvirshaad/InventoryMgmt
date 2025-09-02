// item-detail.js - Handles item detail functionality

$(document).ready(function() {
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Handle clicks on the heart icons inside like buttons
    $(document).on('click', '.like-btn i.bi', function(e) {
        e.stopPropagation();
        // Trigger click on parent button
        $(this).parent().trigger('click');
    });
    
    // Like button functionality
    $('.like-btn').on('click', function(e) {
        // Stop event propagation
        e.stopPropagation();
        e.preventDefault();
        
        console.log('Like button clicked in item detail view');
        
        // Check if user is authenticated
        if ($(this).data('authenticated') !== 'true') {
            alert('You must be logged in to like items.');
            return;
        }
        
        const itemId = $(this).data('item-id');
        const $likeBtn = $(this);
        const $icon = $likeBtn.find('i');
        const $likesCount = $likeBtn.find('.likes-count');
        
        $.ajax({
            url: `/Item/ToggleLike/${itemId}`,
            type: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            success: function(result) {
                if (result.success) {
                    if (result.isLiked) {
                        $icon.removeClass('bi-heart').addClass('bi-heart-fill');
                        $likeBtn.attr('data-bs-original-title', 'Unlike');
                    } else {
                        $icon.removeClass('bi-heart-fill').addClass('bi-heart');
                        $likeBtn.attr('data-bs-original-title', 'Like');
                    }
                    
                    // Update tooltip
                    var tooltip = bootstrap.Tooltip.getInstance($likeBtn[0]);
                    if (tooltip) {
                        tooltip.dispose();
                    }
                    new bootstrap.Tooltip($likeBtn[0]);
                    
                    // Update like count if it's returned in the response
                    if (result.likesCount !== undefined) {
                        $likesCount.text(result.likesCount);
                    }
                }
            },
            error: function(error) {
                console.error('Error toggling like:', error);
                if (error.status === 401) {
                    alert('You must be logged in to like items.');
                } else {
                    console.log('Response:', error.responseJSON);
                    alert('Failed to toggle like. Please try again.');
                }
            }
        });
    });
    
    // Initialize comment functionality if the hub exists
    if (typeof signalR !== 'undefined') {
        const itemId = $('#item-id').val();
        
        // Connect to SignalR hub
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/commentHub")
            .build();
            
        // Load existing comments
        loadComments(itemId);
        
        // Send comment
        $('#send-comment').on('click', function() {
            const commentText = $('#new-comment').val().trim();
            if (commentText) {
                connection.invoke("AddComment", itemId, commentText)
                    .then(function() {
                        $('#new-comment').val('');
                    })
                    .catch(function(err) {
                        console.error(err);
                    });
            }
        });
        
        // Listen for new comments
        connection.on("ReceiveComment", function(comment) {
            if (comment.itemId === itemId) {
                addCommentToList(comment);
            }
        });
        
        // Start the connection
        connection.start()
            .catch(function(err) {
                console.error(err);
            });
    }
    
    // Function to load existing comments
    function loadComments(itemId) {
        $.ajax({
            url: `/Comment/GetByItem/${itemId}`,
            type: 'GET',
            success: function(comments) {
                const $container = $('#comments-container');
                $container.empty();
                
                if (comments.length === 0) {
                    $container.append('<p class="text-muted text-center py-3">No comments yet. Be the first to comment!</p>');
                } else {
                    comments.forEach(function(comment) {
                        addCommentToList(comment);
                    });
                }
            }
        });
    }
    
    // Function to add a comment to the list
    function addCommentToList(comment) {
        const $container = $('#comments-container');
        const $commentEl = $(
            `<div class="comment-item mb-3">
                <div class="d-flex">
                    <div class="flex-shrink-0">
                        <div class="avatar">
                            <span class="avatar-text">${comment.userName.charAt(0).toUpperCase()}</span>
                        </div>
                    </div>
                    <div class="flex-grow-1 ms-3">
                        <div class="comment-content">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <div class="fw-bold">${comment.userName}</div>
                                <small class="text-muted">${new Date(comment.createdAt).toLocaleString()}</small>
                            </div>
                            <p class="mb-0">${comment.content}</p>
                        </div>
                    </div>
                </div>
            </div>`
        );
        
        $container.append($commentEl);
        $commentEl.hide().fadeIn(300);
        
        // Scroll to the bottom if there are many comments
        $container.scrollTop($container[0].scrollHeight);
    }
});
