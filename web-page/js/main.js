// ===== DOMコンテンツ読み込み完了イベント =====
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

// ===== アプリケーション初期化 =====
function initializeApp() {
    handleLoading();
    setupNavigation();
    setupScrollEffects();
    setupAnimations();
    setupBackToTop();
    setupHamburgerMenu();
    setupSmoothScrolling();
}
