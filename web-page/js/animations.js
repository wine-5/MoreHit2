// ===== Animations Setup =====
function setupAnimations() {
    // ページロード時のアニメーション設定
    setupFadeInAnimations();
    setupScrollAnimations();
}

// ===== Setup Fade In Animations =====
function setupFadeInAnimations() {
    // フェードインアニメーション対象要素を指定
    const elements = document.querySelectorAll(
        '.feature-card, .screenshot-item, .about-text, .download-text'
    );
    
    elements.forEach((element, index) => {
        element.classList.add('fade-in');
        element.style.animationDelay = `${index * 0.1}s`;
    });
}
