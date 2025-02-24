document.addEventListener("DOMContentLoaded", function () {
    var templateId = @Model.Id;

    // Initialize SignalR connection
    var connection = new signalR.HubConnectionBuilder().withUrl("/commentHub").build();

    connection.on("ReceiveComment", function (receivedTemplateId, userName, text) {
        if (receivedTemplateId == templateId) {
            var newComment = `<div class="card mb-2">
                <div class="card-body">
                    <strong>${userName}</strong> - <span class="text-muted">Just now</span>
                    <p>${text}</p>
                </div>
            </div>`;
            document.getElementById("comments").innerHTML += newComment;
        }
    });

    connection.on("ReceiveLikeUpdate", function (receivedTemplateId, likeCount) {
        if (receivedTemplateId == templateId) {
            document.getElementById("likeCount").textContent = likeCount;
        }
    });

    connection.start().catch(err => console.error(err.toString()));

    // Post Comment
    document.getElementById("commentForm")?.addEventListener("submit", function (event) {
        event.preventDefault();
        let commentText = document.getElementById("commentText").value;

        fetch('/Comment/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ templateId: templateId, text: commentText })
        }).then(response => {
            if (response.ok) {
                document.getElementById("commentText").value = "";
            }
        });
    });

    // Toggle Like
    document.getElementById("likeButton")?.addEventListener("click", function () {
        fetch(`/Like/ToggleLike?templateId=${templateId}`, { method: 'POST' })
            .then(response => response.json())
            .then(data => {
                document.getElementById("likeCount").textContent = data.likes;
            });
    });
});
