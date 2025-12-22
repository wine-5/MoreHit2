// ===== Back to Top Button Setup =====
function setupBackToTop() {
    const backToTopBtn = document.getElementById('back-to-top');
    
    // スクロール時のボタン表示/非表示
    window.addEventListener('scroll', () => {
        if (window.scrollY > 300) {
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
