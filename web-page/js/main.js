// ===== DOM Content Loaded Event =====
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

// ===== Initialize Application =====
function initializeApp() {
    handleLoading();
    setupNavigation();
    setupScrollEffects();
    setupAnimations();
    setupBackToTop();
    setupHamburgerMenu();
    setupSmoothScrolling();
}
