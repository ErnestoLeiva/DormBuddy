// Get references to DOM elements
const container = document.querySelector('.account-forms-container');
const registerBtn = document.getElementById('register');
const loginBtn = document.getElementById('login');
const mobileToggleBtns = document.querySelectorAll('#mobileToggle');

// Event listener for desktop register button
registerBtn.addEventListener('click', () => {
    container.classList.add("active");
});

// Event listener for desktop login button
loginBtn.addEventListener('click', () => {
    container.classList.remove("active");
});

// Event listeners for mobile toggle buttons
mobileToggleBtns.forEach(btn => {
    btn.addEventListener('click', (e) => {
        e.preventDefault();
        container.classList.toggle("active");
    });
});

// Function to check screen size and adjust layout
function checkScreenSize() {
    if (window.innerWidth <= 768) {
        // Reset to sign-in view on mobile
        container.classList.remove("active");
    }
}

// Check screen size on load and resize
window.addEventListener('load', checkScreenSize);
window.addEventListener('resize', checkScreenSize);