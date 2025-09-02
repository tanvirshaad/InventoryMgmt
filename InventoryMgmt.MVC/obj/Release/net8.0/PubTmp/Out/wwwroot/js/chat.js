// Chat functionality for inventory discussions

// Connect to the SignalR hub and set up event handlers
let chatConnection;

function initializeChat(inventoryId) {
    if (typeof signalR !== 'undefined') {
        console.log(`Initializing chat for inventory ID: ${inventoryId}`);
        
        // Create the connection
        chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/commentHub")
            .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000]) // Retry with backoff
            .configureLogging(signalR.LogLevel.Information) // Enable more detailed logging
            .build();
            
        // Set up event handler for receiving comments before starting connection
        chatConnection.on("CommentAdded", (receivedInventoryId) => {
            console.log("Comment added notification received for inventory:", receivedInventoryId);
            // Only refresh comments if it's for our current inventory
            if (receivedInventoryId == inventoryId) {
                console.log("Updating comments due to SignalR notification");
                loadComments(inventoryId);
            }
        });
        
        // Also handle legacy ReceiveComment event for backward compatibility
        chatConnection.on("ReceiveComment", function(data) {
            console.log("ReceiveComment event received:", data);
            loadComments(inventoryId);
        });
        
        // Connection state change handlers for debugging
        chatConnection.onreconnecting(error => {
            console.log("SignalR reconnecting due to error:", error);
            showToast("Chat connection lost. Reconnecting...", "warning");
        });

        chatConnection.onreconnected(connectionId => {
            console.log("SignalR reconnected with ID:", connectionId);
            showToast("Chat connection restored!", "success");
            // Rejoin the group after reconnection
            chatConnection.invoke("JoinInventoryGroup", inventoryId).catch(err => {
                console.error("Error rejoining inventory group after reconnection:", err);
            });
        });
        
        // Start connection and join inventory group
        chatConnection.start()
            .then(() => {
                console.log("Connected to comment hub");
                return chatConnection.invoke("JoinInventoryGroup", inventoryId);
            })
            .then(() => {
                console.log(`Successfully joined inventory_${inventoryId} group`);
                // Load initial comments only after joining group
                loadComments(inventoryId);
            })
            .catch(err => {
                console.error("Error during SignalR connection setup:", err);
                // Even if SignalR fails, still try to load comments via regular HTTP
                loadComments(inventoryId);
            });
        
        // Set up send comment handler
        $('#send-comment').on('click', () => {
            const comment = $('#new-comment').val().trim();
            if (comment) {
                sendComment(inventoryId, comment);
            }
        });
        
        // Also add enter key press handler for the comment textarea
        $('#new-comment').on('keypress', function(e) {
            // Check if Enter was pressed without the Shift key
            if (e.which === 13 && !e.shiftKey) {
                e.preventDefault();
                const comment = $(this).val().trim();
                if (comment) {
                    sendComment(inventoryId, comment);
                }
            }
        });
        
        // Add Markdown reference toggle
        $('#markdown-help-toggle').on('click', function(e) {
            e.preventDefault();
            $('#markdown-help').slideToggle();
        });
    } else {
        console.error("SignalR is not available");
    }
}

// Send a new comment to the server
function sendComment(inventoryId, content) {
    if (!content.trim()) return;
    
    // Disable the send button and show loading state
    const sendButton = $('#send-comment');
    const originalHtml = sendButton.html();
    sendButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');
    
    console.log(`Sending comment to inventory ${inventoryId}...`);
    
    // Check if SignalR connection is active and use that if possible
    if (chatConnection && chatConnection.state === signalR.HubConnectionState.Connected) {
        console.log("Using SignalR to send comment");
        
        chatConnection.invoke("SendComment", inventoryId, content)
            .then(() => {
                console.log("Comment sent successfully through SignalR");
                $('#new-comment').val(''); // Clear the input field
            })
            .catch(err => {
                console.error("Error sending comment through SignalR:", err);
                // Fall back to regular HTTP POST if SignalR fails
                sendCommentViaHttp(inventoryId, content);
            })
            .finally(() => {
                // Re-enable the send button
                sendButton.prop('disabled', false).html(originalHtml);
            });
    } else {
        console.log("SignalR not connected, using HTTP to send comment");
        // Use regular HTTP POST as fallback
        sendCommentViaHttp(inventoryId, content, sendButton, originalHtml);
    }
}

// Fallback HTTP method to send comments
function sendCommentViaHttp(inventoryId, content, sendButton, originalHtml) {
    fetch('/Inventory/AddComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        body: JSON.stringify({
            inventoryId: inventoryId,
            content: content
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            console.log("Comment sent successfully via HTTP");
            // Clear the input field
            $('#new-comment').val('');
            
            // Manually reload comments since we might have missed the SignalR notification
            loadComments(inventoryId);
        } else {
            console.error("Error response from server:", data);
            showToast(data.message || 'Error sending comment', 'error');
        }
    })
    .catch(err => {
        console.error('Error sending comment via HTTP:', err);
        showToast('Error sending comment', 'error');
    })
    .finally(() => {
        // Re-enable the send button if it was passed
        if (sendButton && originalHtml) {
            sendButton.prop('disabled', false).html(originalHtml);
        }
    });
}

// Load comments for the specified inventory
function loadComments(inventoryId) {
    console.log(`Loading comments for inventory ID: ${inventoryId}`);
    const commentsContainer = $('#comments-container');
    
    // Show loading state
    commentsContainer.html('<div class="text-center py-3"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
    
    fetch(`/Inventory/GetComments?id=${inventoryId}`)
        .then(response => response.json())
        .then(data => {
            console.log(`Loaded ${data.comments?.length || 0} comments`);
            renderComments(commentsContainer, data.comments || []);
        })
        .catch(err => {
            console.error('Error loading comments:', err);
            commentsContainer.html('<div class="alert alert-danger">Failed to load comments. Please try refreshing the page.</div>');
        });
}

// Render comments in the specified container
function renderComments(container, comments) {
    // Clear the container
    container.empty();
    
    if (comments.length === 0) {
        container.html('<div class="text-center py-4 text-muted"><i class="bi bi-chat-dots display-4"></i><p class="mt-2">No comments yet. Start the conversation!</p></div>');
        return;
    }
    
    // Sort comments by creation date (oldest first)
    comments.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
    
    // Initialize the marked library for markdown rendering if available
    const markdownRenderer = typeof marked !== 'undefined' ? marked : null;
    
    // Add each comment
    comments.forEach(comment => {
        // Format the date
        const commentDate = new Date(comment.createdAt);
        const formattedDate = commentDate.toLocaleString();
        
        // Get initials for avatar
        const initials = comment.userName.split(' ')
            .map(name => name.charAt(0))
            .join('')
            .toUpperCase()
            .substring(0, 2);
        
        // Generate a consistent color based on the user's name
        const colorIndex = comment.userName.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0) % avatarColors.length;
        const avatarColor = avatarColors[colorIndex];
        
        // Render content as markdown if possible, otherwise escape HTML
        let renderedContent;
        if (markdownRenderer) {
            try {
                renderedContent = markdownRenderer.parse(comment.content);
            } catch (e) {
                console.error("Markdown rendering error:", e);
                renderedContent = escapeHtml(comment.content);
            }
        } else {
            renderedContent = `<p>${escapeHtml(comment.content)}</p>`;
        }
        
        // Create the comment element
        const commentEl = $(`
            <div class="comment mb-4">
                <div class="d-flex">
                    <div class="flex-shrink-0">
                        <div class="avatar" style="background-color: ${avatarColor}">
                            <span>${initials}</span>
                        </div>
                    </div>
                    <div class="flex-grow-1 ms-3">
                        <div class="d-flex justify-content-between align-items-center mb-1">
                            <strong class="text-primary">${escapeHtml(comment.userName)}</strong>
                            <small class="text-muted">${formattedDate}</small>
                        </div>
                        <div class="comment-content markdown-body">${renderedContent}</div>
                    </div>
                </div>
            </div>
        `);
        
        container.append(commentEl);
    });
    
    // Scroll to bottom of container
    container.scrollTop(container.prop('scrollHeight'));
}

// Colors for user avatars
const avatarColors = [
    '#007bff', '#6610f2', '#6f42c1', '#e83e8c', '#dc3545',
    '#fd7e14', '#ffc107', '#28a745', '#20c997', '#17a2b8'
];

// Helper function to escape HTML
function escapeHtml(text) {
    if (!text) return '';
    return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;")
        .replace(/\n/g, "<br>");
}

// Helper function to show toast notifications
function showToast(message, type = 'info') {
    if (typeof Toastify === 'function') {
        Toastify({
            text: message,
            duration: 3000,
            close: true,
            gravity: "top",
            position: "right",
            backgroundColor: type === 'error' ? "#dc3545" : 
                            type === 'success' ? "#28a745" : "#007bff"
        }).showToast();
    } else {
        alert(message);
    }
}

// Clean up when leaving the page
function cleanupChat(inventoryId) {
    if (chatConnection && chatConnection.state === signalR.HubConnectionState.Connected) {
        chatConnection.invoke("LeaveInventoryGroup", inventoryId).catch(err => {
            console.error("Error leaving inventory group:", err);
        });
        
        chatConnection.stop();
    }
}
