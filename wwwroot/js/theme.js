document.addEventListener("DOMContentLoaded", function () {
    let theme = localStorage.getItem("theme") || "light";
    document.body.setAttribute("data-bs-theme", theme);

    document.getElementById("themeToggle")?.addEventListener("click", function () {
        theme = (theme === "light") ? "dark" : "light";
        document.body.setAttribute("data-bs-theme", theme);
        localStorage.setItem("theme", theme);
    });
});
