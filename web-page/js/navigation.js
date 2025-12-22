// ===== Navigation Setup =====
function setupNavigation() {
    const navbar = document.getElementById('navbar');
    const navLinks = document.querySelectorAll('.nav-link');
    
    // ナビゲーションバーのスクロール効果
    window.addEventListener('scroll', () => {
        if (window.scrollY > 100) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });
    
    // ナビゲーションリンククリック処理
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            
            if (target) {
                const offsetTop = target.offsetTop - 70;
                window.scrollTo({
                    top: offsetTop,
                    behavior: 'smooth'
                });
            }
            
            // アクティブリンク更新
            navLinks.forEach(l => l.classList.remove('active'));
            this.classList.add('active');
            
            // モバイルメニューが開いていれば閉じる
            const navMenu = document.getElementById('nav-menu');
            if (navMenu.classList.contains('active')) {
                navMenu.classList.remove('active');
                updateHamburgerIcon(false);
            }
        });
    });
    
    // スクロール時のアクティブナビゲーション更新
    window.addEventListener('scroll', updateActiveNavLink);
}

// ===== Update Active Navigation Link =====
function updateActiveNavLink() {
    const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.nav-link');
    const scrollPos = window.scrollY + 100;
    
    sections.forEach(section => {
        const top = section.offsetTop;
        const height = section.offsetHeight;
        const id = section.getAttribute('id');
        
        if (scrollPos >= top && scrollPos < top + height) {
            navLinks.forEach(link => {
                link.classList.remove('active');
                if (link.getAttribute('href') === `#${id}`) {
                    link.classList.add('active');
                }
            });
        }
    });
}
