// ===== Scroll Effects Setup =====
// スクロールイベントのスロットル時間（ミリ秒）
const SCROLL_THROTTLE_DELAY = 100;

function setupScrollEffects() {
    // スクロール時のアニメーション設定
    window.addEventListener('scroll', throttle(handleScrollAnimations, SCROLL_THROTTLE_DELAY));
}

// ===== Handle Scroll Animations =====
function handleScrollAnimations() {
    const elements = document.querySelectorAll('.fade-in');
    
    elements.forEach(element => {
        const elementTop = element.getBoundingClientRect().top;
        const elementBottom = element.getBoundingClientRect().bottom;
        
        // ビューポート内にある場合、アニメーションを適用
        if (elementTop < window.innerHeight && elementBottom > 0) {
            element.classList.add('appear');
        }
    });
}

// ===== Throttle Function =====
function throttle(func, delay) {
    let lastCall = 0;
    return function(...args) {
        const now = Date.now();
        if (now - lastCall >= delay) {
            lastCall = now;
            return func(...args);
        }
    };
}
