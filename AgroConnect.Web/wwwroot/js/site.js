// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
<script>
    document.addEventListener("DOMContentLoaded", function () {
        // Düyməyə kliklədikdə animasiya effekti
        const btnStart = document.getElementById("btnStart");
    if(btnStart) {
        btnStart.addEventListener("click", function () {
            this.style.transform = "scale(0.95)";
        });
        }

    // Kartların üzərinə gəldikdə ikonları yüngül fırlatmaq üçün JS interaktivliyi
    const cards = document.querySelectorAll('.agro-card');
        cards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            const icon = this.querySelector('.icon-wrapper i');
            if (icon) {
                icon.style.transition = "transform 0.4s ease";
                icon.style.transform = "rotate(15deg) scale(1.1)";
            }
        });
    card.addEventListener('mouseleave', function() {
                const icon = this.querySelector('.icon-wrapper i');
    if(icon) {
        icon.style.transform = "rotate(0deg) scale(1)";
                }
            });
        });
    });
</script>
<script>
    document.addEventListener("DOMContentLoaded", function () {
        const toggler = document.querySelector('.custom-toggler');
        const collapseMenu = document.querySelector('#navbarAgro');

        if (toggler && collapseMenu) {
            // Menyu linkinə kliklədikdə mobil menyunun avtomatik bağlanması
            const navLinks = collapseMenu.querySelectorAll('.nav-link:not(.dropdown-toggle)');
            navLinks.forEach(link => {
                link.addEventListener('click', () => {
                    if (window.innerWidth <= 576 && collapseMenu.classList.contains('show')) {
                        toggler.click();
                    }
                });
            });

            // Ekran böyüdükdə açıq qalan mobil menyu qalıqlarını təmizləmək
            window.addEventListener('resize', () => {
                if (window.innerWidth > 576 && collapseMenu.classList.contains('show')) {
                    collapseMenu.classList.remove('show');
                    toggler.setAttribute('aria-expanded', 'false');
                }
            });
        }
    });
</script>