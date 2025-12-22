// ===== Loading Screen Handler =====
function handleLoading() {
    const loadingScreen = document.getElementById('loading-screen');
    
    // ローディング画面を1500ms後に非表示化
    setTimeout(() => {
        loadingScreen.classList.add('hidden');
        // アニメーション完了後、DOMから削除
        setTimeout(() => {
            if (loadingScreen.parentNode) {
                loadingScreen.parentNode.removeChild(loadingScreen);
            }
        }, 500);
    }, 1500);
}
