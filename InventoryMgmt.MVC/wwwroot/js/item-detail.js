// item-detail.js - Handles item detail functionality

$(document).ready(function() {
    // Like button functionality
    $('.like-btn').on('click', function() {
        const itemId = $(this).data('item-id');
        const $likeBtn = $(this);
        const $icon = $likeBtn.find('i');
        const $likesCount = $likeBtn.find('.likes-count');
        
        $.ajax({
            url: `/Item/ToggleLike/${itemId}`,
            type: 'POST',
            success: function(result) {
                if (result.isLiked) {
                    $icon.removeClass('bi-heart').addClass('bi-heart-fill');
                    $likesCount.text(parseInt($likesCount.text()) + 1);
                } else {
                    $icon.removeClass('bi-heart-fill').addClass('bi-heart');
                    $likesCount.text(Math.max(0, parseInt($likesCount.text()) - 1));
                }
            },
            error: function(error) {
                console.error('Error toggling like:', error);
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
