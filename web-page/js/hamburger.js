// ===== Hamburger Menu Setup =====
function setupHamburgerMenu() {
    const hamburger = document.getElementById('hamburger');
    const navMenu = document.getElementById('nav-menu');
    
    hamburger.addEventListener('click', function() {
        navMenu.classList.toggle('active');
        updateHamburgerIcon(navMenu.classList.contains('active'));
    });
    
    // ナビゲーションリンククリック時にメニューを閉じる
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', function() {
            navMenu.classList.remove('active');
            updateHamburgerIcon(false);
        });
    });
}

// ===== Update Hamburger Icon =====
function updateHamburgerIcon(isActive) {
    const hamburger = document.getElementById('hamburger');
    
    if (isActive) {
        hamburger.style.transform = 'rotate(90deg)';
    } else {
        hamburger.style.transform = 'rotate(0deg)';
    }
}
