// ===== Navigation Setup =====
// ナビゲーションバーのスクロール効果を適用する閾値（ピクセル）
const NAV_SCROLL_THRESHOLD = 100;

function setupNavigation() {
    const navbar = document.getElementById('navbar');
    const navLinks = document.querySelectorAll('.nav-link');
    
    // Navbarの高さを動的に計算してオフセットを決定
    const getNavbarHeight = () => navbar.offsetHeight;
    
    // ナビゲーションバーのスクロール効果
    window.addEventListener('scroll', () => {
        if (window.scrollY > NAV_SCROLL_THRESHOLD) {
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
                // Navbarの高さを動的に取得してオフセットを計算
                const navbarHeight = getNavbarHeight();
                const offsetTop = target.offsetTop - navbarHeight;
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
    const scrollPos = window.scrollY + NAV_SCROLL_THRESHOLD;
    
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
