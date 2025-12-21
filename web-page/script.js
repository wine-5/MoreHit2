// ===== DOM Content Loaded Event =====
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

// ===== Initialize Application =====
function initializeApp() {
    handleLoading();
    setupNavigation();
    setupScrollEffects();
    setupAnimations();
    setupBackToTop();
    setupNewsletterForm();
    setupHamburgerMenu();
    setupSmoothScrolling();
}

// ===== Loading Screen =====
function handleLoading() {
    const loadingScreen = document.getElementById('loading-screen');
    
    // Simulate loading time
    setTimeout(() => {
        loadingScreen.classList.add('hidden');
        // Remove loading screen from DOM after animation
        setTimeout(() => {
            if (loadingScreen.parentNode) {
                loadingScreen.parentNode.removeChild(loadingScreen);
            }
        }, 500);
    }, 1500);
}

// ===== Navigation =====
function setupNavigation() {
    const navbar = document.getElementById('navbar');
    const navLinks = document.querySelectorAll('.nav-link');
    
    // Navbar scroll effect
    window.addEventListener('scroll', () => {
        if (window.scrollY > 100) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });
    
    // Active link highlighting
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
            
            // Update active link
            navLinks.forEach(l => l.classList.remove('active'));
            this.classList.add('active');
            
            // Close mobile menu if open
            const navMenu = document.getElementById('nav-menu');
            if (navMenu.classList.contains('active')) {
                navMenu.classList.remove('active');
                updateHamburgerIcon(false);
            }
        });
    });
    
    // Scroll spy for active navigation
    window.addEventListener('scroll', updateActiveNavLink);
}

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

// ===== Hamburger Menu =====
function setupHamburgerMenu() {
    const hamburger = document.getElementById('hamburger');
    const navMenu = document.getElementById('nav-menu');
    
    hamburger.addEventListener('click', () => {
        const isActive = navMenu.classList.toggle('active');
        updateHamburgerIcon(isActive);
    });
    
    // Close menu when clicking outside
    document.addEventListener('click', (e) => {
        if (!hamburger.contains(e.target) && !navMenu.contains(e.target)) {
            navMenu.classList.remove('active');
            updateHamburgerIcon(false);
        }
    });
}

function updateHamburgerIcon(isActive) {
    const hamburger = document.getElementById('hamburger');
    const bars = hamburger.querySelectorAll('.bar');
    
    if (isActive) {
        bars[0].style.transform = 'rotate(-45deg) translate(-5px, 6px)';
        bars[1].style.opacity = '0';
        bars[2].style.transform = 'rotate(45deg) translate(-5px, -6px)';
    } else {
        bars[0].style.transform = 'none';
        bars[1].style.opacity = '1';
        bars[2].style.transform = 'none';
    }
}

// ===== Scroll Effects =====
function setupScrollEffects() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver(handleIntersection, observerOptions);
    
    // Observe elements that should animate on scroll
    const animateElements = document.querySelectorAll('.feature-card, .screenshot-item, .stat, .about-text, .download-text');
    animateElements.forEach(el => {
        el.classList.add('fade-in');
        observer.observe(el);
    });
}

function handleIntersection(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('appear');
        }
    });
}

// ===== Animations =====
function setupAnimations() {
    // Animate hero elements on load
    setTimeout(() => {
        const heroElements = document.querySelectorAll('.hero-title, .hero-description, .hero-buttons, .hero-game-preview');
        heroElements.forEach((el, index) => {
            setTimeout(() => {
                el.style.opacity = '1';
                el.style.transform = 'translateY(0)';
            }, index * 200);
        });
    }, 1600);
    
    // Add hover effects to cards
    setupCardHoverEffects();
    
    // Setup parallax effect for hero background
    setupParallaxEffect();
    
    // Setup counter animation for stats
    setupCounterAnimations();
}

function setupCardHoverEffects() {
    const cards = document.querySelectorAll('.feature-card, .screenshot-item');
    
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-10px) scale(1.02)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });
}

function setupParallaxEffect() {
    window.addEventListener('scroll', () => {
        const scrolled = window.pageYOffset;
        const hero = document.querySelector('.hero-background');
        if (hero) {
            hero.style.transform = `translateY(${scrolled * 0.5}px)`;
        }
    });
}

function setupCounterAnimations() {
    const stats = document.querySelectorAll('.stat-number');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
            }
        });
    });
    
    stats.forEach(stat => observer.observe(stat));
}

function animateCounter(element) {
    const target = element.textContent;
    const isInfinity = target === '∞';
    
    if (isInfinity) {
        element.style.animation = 'pulse 2s infinite';
        return;
    }
    
    const number = parseInt(target.replace('+', ''));
    const duration = 2000;
    const increment = number / (duration / 16);
    let current = 0;
    
    const timer = setInterval(() => {
        current += increment;
        if (current >= number) {
            current = number;
            clearInterval(timer);
        }
        element.textContent = Math.floor(current) + (target.includes('+') ? '+' : '');
    }, 16);
}

// ===== Back to Top Button =====
function setupBackToTop() {
    const backToTopBtn = document.getElementById('back-to-top');
    
    window.addEventListener('scroll', () => {
        if (window.scrollY > 300) {
            backToTopBtn.classList.add('show');
        } else {
            backToTopBtn.classList.remove('show');
        }
    });
    
    backToTopBtn.addEventListener('click', () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}

// ===== Newsletter Form =====
function setupNewsletterForm() {
    const newsletterForm = document.querySelector('.newsletter');
    
    // Check if newsletter form exists before proceeding
    if (!newsletterForm) {
        return;
    }
    
    const newsletterInput = newsletterForm.querySelector('input');
    const newsletterButton = newsletterForm.querySelector('button');
    
    newsletterButton.addEventListener('click', function(e) {
        e.preventDefault();
        const email = newsletterInput.value.trim();
        
        if (validateEmail(email)) {
            handleNewsletterSubmission(email);
        } else {
            showNotification('有効なメールアドレスを入力してください', 'error');
        }
    });
    
    newsletterInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            newsletterButton.click();
        }
    });
}

function validateEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

function handleNewsletterSubmission(email) {
    // Simulate API call
    const button = document.querySelector('.newsletter button');
    const input = document.querySelector('.newsletter input');
    
    if (!button || !input) {
        return;
    }
    
    const originalText = button.textContent;
    
    button.textContent = '送信中...';
    button.disabled = true;
    
    setTimeout(() => {
        showNotification('ニュースレターの登録が完了しました！', 'success');
        input.value = '';
        button.textContent = originalText;
        button.disabled = false;
    }, 1500);
}

// ===== Smooth Scrolling =====
function setupSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]');
    
    links.forEach(link => {
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
        });
    });
}

// ===== Notification System =====
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    // Add styles
    Object.assign(notification.style, {
        position: 'fixed',
        top: '20px',
        right: '20px',
        padding: '15px 20px',
        borderRadius: '10px',
        color: '#fff',
        fontWeight: '600',
        zIndex: '10000',
        opacity: '0',
        transform: 'translateX(100px)',
        transition: 'all 0.3s ease',
        maxWidth: '300px',
        boxShadow: '0 10px 30px rgba(0, 0, 0, 0.3)'
    });
    
    // Set background color based on type
    const colors = {
        success: 'linear-gradient(45deg, #4ecdc4, #45b7d1)',
        error: 'linear-gradient(45deg, #ff6b6b, #ee5a24)',
        info: 'linear-gradient(45deg, #667eea, #764ba2)'
    };
    
    notification.style.background = colors[type] || colors.info;
    
    document.body.appendChild(notification);
    
    // Animate in
    setTimeout(() => {
        notification.style.opacity = '1';
        notification.style.transform = 'translateX(0)';
    }, 100);
    
    // Auto remove
    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100px)';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 4000);
}

// ===== Game Preview Interaction =====
function setupGamePreview() {
    const gamePreview = document.querySelector('.game-placeholder');
    
    if (gamePreview) {
        gamePreview.addEventListener('click', function() {
            // This could open a modal with the actual game or redirect to play page
            showNotification('ゲームをロード中...', 'info');
            
            // Simulate game loading
            setTimeout(() => {
                showNotification('ゲームは準備中です。もうしばらくお待ちください。', 'info');
            }, 2000);
        });
        
        // Add pulse animation on hover
        gamePreview.addEventListener('mouseenter', function() {
            this.style.animation = 'pulse 1s infinite';
        });
        
        gamePreview.addEventListener('mouseleave', function() {
            this.style.animation = 'none';
        });
    }
}

// ===== Download Button Handlers =====
function setupDownloadButtons() {
    const downloadButtons = document.querySelectorAll('.download-buttons .btn');
    
    downloadButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const platform = this.textContent.trim();
            showNotification(`${platform}のダウンロードを準備中...`, 'info');
            
            // Simulate download preparation
            setTimeout(() => {
                showNotification('ダウンロードリンクをメールで送信しました！', 'success');
            }, 2000);
        });
    });
}

// ===== Keyboard Navigation =====
function setupKeyboardNavigation() {
    document.addEventListener('keydown', function(e) {
        // Escape key to close mobile menu
        if (e.key === 'Escape') {
            const navMenu = document.getElementById('nav-menu');
            if (navMenu.classList.contains('active')) {
                navMenu.classList.remove('active');
                updateHamburgerIcon(false);
            }
        }
        
        // Arrow keys for navigation (optional)
        if (e.key === 'ArrowUp' && e.ctrlKey) {
            e.preventDefault();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    });
}

// ===== Performance Optimizations =====
function optimizePerformance() {
    // Throttle scroll events
    let ticking = false;
    
    function throttledScroll() {
        if (!ticking) {
            requestAnimationFrame(() => {
                updateActiveNavLink();
                ticking = false;
            });
            ticking = true;
        }
    }
    
    window.removeEventListener('scroll', updateActiveNavLink);
    window.addEventListener('scroll', throttledScroll, { passive: true });
    
    // Lazy load images (if any are added later)
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });
        
        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }
}

// ===== Error Handling =====
window.addEventListener('error', function(e) {
    console.error('JavaScript Error:', e.error);
    showNotification('申し訳ございません。エラーが発生しました。', 'error');
});

// ===== Initialize additional features when DOM is ready =====
document.addEventListener('DOMContentLoaded', function() {
    setupGamePreview();
    setupDownloadButtons();
    setupKeyboardNavigation();
    optimizePerformance();
});

// ===== CSS Animation Keyframes (added programmatically) =====
function addCustomAnimations() {
    const style = document.createElement('style');
    style.textContent = `
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.05); }
            100% { transform: scale(1); }
        }
        
        @keyframes float {
            0%, 100% { transform: translateY(0px); }
            50% { transform: translateY(-10px); }
        }
        
        .floating {
            animation: float 3s ease-in-out infinite;
        }
    `;
    document.head.appendChild(style);
}

// Add custom animations
addCustomAnimations();

// ===== Browser Compatibility =====
function checkBrowserCompatibility() {
    const isIE = /MSIE|Trident/.test(navigator.userAgent);
    
    if (isIE) {
        showNotification('このサイトは最新のブラウザで最適に動作します。', 'info');
    }
    
    // Check for CSS Grid support
    if (!CSS.supports('display', 'grid')) {
        document.body.classList.add('no-grid');
    }
    
    // Check for Intersection Observer support
    if (!('IntersectionObserver' in window)) {
        // Fallback for older browsers
        const elements = document.querySelectorAll('.fade-in');
        elements.forEach(el => el.classList.add('appear'));
    }
}

// Check browser compatibility on load
checkBrowserCompatibility();