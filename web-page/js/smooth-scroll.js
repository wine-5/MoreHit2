// ===== Smooth Scrolling Setup =====
function setupSmoothScrolling() {
    // Navbarの高さを動的に計算
    const getNavbarHeight = () => {
        const navbar = document.getElementById('navbar');
        return navbar ? navbar.offsetHeight : 0;
    };
    
    // ページ内リンククリック時のスムーススクロール
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            
            // #のみの場合はスキップ
            if (href === '#') return;
            
            const target = document.querySelector(href);
            
            if (target) {
                e.preventDefault();
                
                // Navbarの高さを動的に取得してオフセットを計算
                const navbarHeight = getNavbarHeight();
                const offsetTop = target.offsetTop - navbarHeight;
                window.scrollTo({
                    top: offsetTop,
                    behavior: 'smooth'
                });
            }
        });
    });
}
