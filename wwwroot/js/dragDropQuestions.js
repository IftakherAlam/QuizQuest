document.addEventListener("DOMContentLoaded", function () {
    let questionsList = document.getElementById("questionsList");

    if (questionsList) {
        new Sortable(questionsList, {
            animation: 150,
            onEnd: function (evt) {
                updateOrder();
            }
        });
    }

    function updateOrder() {
        let orderData = [];
        document.querySelectorAll("#questionsList .question-item").forEach((el, index) => {
            orderData.push({ id: el.dataset.id, order: index });
        });

        fetch("/Template/UpdateQuestionOrder", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(orderData)
        });
    }
});
