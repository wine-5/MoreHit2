// ===== Loading Screen Handler =====
// ローディング画面の表示時間（ミリ秒）
const LOADING_DURATION = 1500;
// ローディング画面のアニメーション完了後の削除時間（ミリ秒）
const LOADING_REMOVE_DELAY = 500;

function handleLoading() {
    const loadingScreen = document.getElementById('loading-screen');
    
    // ローディング画面を指定時間後に非表示化
    setTimeout(() => {
        loadingScreen.classList.add('hidden');
        // アニメーション完了後、DOMから削除
        setTimeout(() => {
            if (loadingScreen.parentNode) {
                loadingScreen.parentNode.removeChild(loadingScreen);
            }
        }, LOADING_REMOVE_DELAY);
    }, LOADING_DURATION);
}
