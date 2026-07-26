// AgroConnect Admin Panel - ümumi JS

document.addEventListener('DOMContentLoaded', function () {

    // Bütün "data-confirm" atributu olan formalar üçün SweetAlert təsdiqi
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            const message = form.getAttribute('data-confirm') || 'Bu əməliyyatı etmək istədiyinizə əminsiniz?';
            const title = form.getAttribute('data-confirm-title') || 'Diqqət';
            const confirmColor = form.getAttribute('data-confirm-color') || '#dc3545';

            Swal.fire({
                title: title,
                text: message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Bəli',
                cancelButtonText: 'İmtina',
                confirmButtonColor: confirmColor,
                cancelButtonColor: '#6c757d'
            }).then(function (result) {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });

    // Aktiv sidebar linkini işıqlandır
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sidebar-nav .nav-link').forEach(function (link) {
        const href = (link.getAttribute('href') || '').toLowerCase();
        if (href && currentPath.startsWith(href) && href !== '/') {
            link.classList.add('active-link');
        }
    });
});

// Toast göstərmək üçün ümumi funksiya (Layout tərəfindən çağırılır)
function showAppToast(icon, message) {
    if (!message) return;
    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: icon,
        title: message,
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });
}