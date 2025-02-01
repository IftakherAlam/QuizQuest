document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("adminSidebar");
    const footer = document.querySelector(".admin-footer");

    sidebar.addEventListener("mouseenter", function () {
        sidebar.classList.add("expanded");
        footer.classList.add("expanded");
    });

    sidebar.addEventListener("mouseleave", function () {
        sidebar.classList.remove("expanded");
        footer.classList.remove("expanded");
    });
});
