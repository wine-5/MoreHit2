// ===== Back to Top Button Setup =====
// トップへ戻るボタンを表示する閾値（ピクセル）
const BACK_TO_TOP_THRESHOLD = 300;

function setupBackToTop() {
    const backToTopBtn = document.getElementById('back-to-top');
    
    // スクロール時のボタン表示/非表示
    window.addEventListener('scroll', () => {
        if (window.scrollY > BACK_TO_TOP_THRESHOLD) {
            backToTopBtn.classList.add('show');
        } else {
            backToTopBtn.classList.remove('show');
        }
    });
    
    // ボタンクリック処理
    backToTopBtn.addEventListener('click', () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}
