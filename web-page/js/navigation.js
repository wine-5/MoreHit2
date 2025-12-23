// ===== ナビゲーション設定 =====
// ナビゲーションバーのスクロール効果を適用する閾値（ピクセル）
const NAV_SCROLL_THRESHOLD = 100;

function setupNavigation() {
    const navbar = document.getElementById('navbar');
    const navLinks = document.querySelectorAll('.nav-link');
    
    // Navbarの高さを動的に計算
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
        link.addEventListener('click', handleNavLinkClick);
    });
    
    // スクロール時のアクティブナビゲーション更新
    window.addEventListener('scroll', updateActiveNavLink);
    
    // ページ内リンク（#から始まるリンク）のスムーススクロール
    setupSmoothScrolling();
}

// ===== ナビゲーションリンククリック処理 =====
function handleNavLinkClick(e) {
    e.preventDefault();
    const target = document.querySelector(this.getAttribute('href'));
    
    if (target) {
        const navbar = document.getElementById('navbar');
        const navbarHeight = navbar.offsetHeight;
        const offsetTop = target.offsetTop - navbarHeight;
        window.scrollTo({
            top: offsetTop,
            behavior: 'smooth'
        });
    }
    
    // アクティブリンク更新
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(l => l.classList.remove('active'));
    this.classList.add('active');
    
    // モバイルメニューが開いていれば閉じる
    const navMenu = document.getElementById('nav-menu');
    if (navMenu.classList.contains('active')) {
        navMenu.classList.remove('active');
    }
}

// ===== アクティブナビゲーションリンク更新 =====
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

// ===== スムーススクロール設定 =====
function setupSmoothScrolling() {
    const navbar = document.getElementById('navbar');
    
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            
            // #のみの場合はスキップ
            if (href === '#') return;
            
            const target = document.querySelector(href);
            
            if (target) {
                e.preventDefault();
                
                // Navbarの高さを動的に取得してオフセットを計算
                const navbarHeight = navbar.offsetHeight;
                const offsetTop = target.offsetTop - navbarHeight;
                window.scrollTo({
                    top: offsetTop,
                    behavior: 'smooth'
                });
            }
        });
    });
}
