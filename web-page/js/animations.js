// ===== Animations Setup =====
// アニメーション遅延の間隔（秒）
const ANIMATION_DELAY_INTERVAL = 0.1;

function setupAnimations() {
    // ページロード時のアニメーション設定
    setupFadeInAnimations();
}

// ===== Setup Fade In Animations =====
function setupFadeInAnimations() {
    // フェードインアニメーション対象要素を指定
    const elements = document.querySelectorAll(
        '.feature-card, .screenshot-item, .about-text, .download-text'
    );
    
    elements.forEach((element, index) => {
        element.classList.add('fade-in');
        element.style.animationDelay = `${index * ANIMATION_DELAY_INTERVAL}s`;
    });
}
