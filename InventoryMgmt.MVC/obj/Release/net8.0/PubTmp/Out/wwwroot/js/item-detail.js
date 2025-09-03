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
        
        // Check if user is authenticated
        if ($(this).data('authenticated') !== 'true') {
            ToastUtility.warning('You must be logged in to like items', {
                header: 'Authentication Required',
                action: {
                    text: 'Login',
                    url: '/Auth/Login'
                }
            });
            return;
        }
        
        const itemId = $(this).data('item-id');
        // Use our ItemHandler utility
        ItemHandler.toggleLike(itemId, this);
    });
    
    // Add duplicate button handler
    $('.duplicate-item').on('click', function() {
        const itemId = $(this).data('item-id');
        ItemHandler.duplicateItem(itemId);
    });
    
    // Add delete button handler for the modal
    $('#deleteModal .btn-danger').on('click', function(e) {
        e.preventDefault();
        const itemId = $(this).closest('form').attr('action').split('/').pop();
        ItemHandler.deleteItem(itemId);
        $('#deleteModal').modal('hide');
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
