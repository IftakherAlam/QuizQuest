document.addEventListener("DOMContentLoaded", function () {
    let userInput = document.getElementById("userSearch");
    let suggestionsList = document.getElementById("userSuggestions");

    if (!userInput) return;

    userInput.addEventListener("input", function () {
        let query = userInput.value.trim();
        if (query.length < 2) {
            suggestionsList.innerHTML = "";
            return;
        }

        fetch(`/User/SearchUsers?query=${query}`)
            .then(response => response.json())
            .then(users => {
                suggestionsList.innerHTML = "";
                users.forEach(user => {
                    let item = document.createElement("li");
                    item.classList.add("list-group-item");
                    item.innerText = `${user.name} (${user.email})`;
                    item.addEventListener("click", () => addUser(user));
                    suggestionsList.appendChild(item);
                });
            });
    });

    function addUser(user) {
        let container = document.createElement("div");
        container.innerHTML = `<span>${user.name} (${user.email})</span> 
            <input type="hidden" name="allowedUsers" value="${user.email}" /> 
            <button type="button" class="btn btn-sm btn-danger" onclick="this.parentNode.remove();">Remove</button>`;
        document.getElementById("allowedUsersDiv").appendChild(container);
    }
});
