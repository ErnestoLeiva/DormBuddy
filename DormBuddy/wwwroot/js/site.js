// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


document.addEventListener('DOMContentLoaded', () => {
    const sidebar = document.querySelector('.sidebar');
    const toggleButton = document.querySelector('.toggle-button');
    const icon = toggleButton.querySelector('i');

    toggleButton.addEventListener('click', () => {
        sidebar.classList.toggle('expanded');
        
        if (sidebar.classList.contains('expanded')) {
            icon.classList.remove('fa-chevron-left');
            icon.classList.add('fa-chevron-right');
        } else {
            icon.classList.remove('fa-chevron-right');
            icon.classList.add('fa-chevron-left');
        }
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const handshakeIcon = document.querySelector('.lending-header h1 i');
    if (handshakeIcon) {
        handshakeIcon.classList.add('shake-once');

        handshakeIcon.addEventListener('animationend', () => {
            handshakeIcon.classList.remove('shake-once');
        });
    }
});

document.addEventListener('DOMContentLoaded', () => {
    const videoCards = document.querySelectorAll('.dashboard-card.video-card');

    videoCards.forEach(card => {
        const video = card.querySelector('.card-video');

        card.addEventListener('mouseenter', () => {
            video.play(); 
        });

        card.addEventListener('mouseleave', () => {
            video.pause(); 
            video.currentTime = 0; 
        });
    });
});

