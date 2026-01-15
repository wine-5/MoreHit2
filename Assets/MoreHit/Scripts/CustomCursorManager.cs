using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CustomCursorManager : Singleton<CustomCursorManager>
{
    
    private const int CURSOR_SORTING_ORDER = 999;
    
    
    [SerializeField] private Canvas cursorCanvas;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    
    protected override bool UseDontDestroyOnLoad => true;
    
    protected override void Awake()
    {
        base.Awake();
        
        // システムカーソルを非表示
        Cursor.visible = false;
        
        // カーソルImageがクリック判定を遮断しないように設定
        if (cursorImage != null)
            cursorImage.raycastTarget = false;
        
        // Canvasの設定
        if (cursorCanvas != null)
            cursorCanvas.sortingOrder = CURSOR_SORTING_ORDER;
    }
    
    private void Update()
    {
        if (cursorImage != null && Mouse.current != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            cursorImage.transform.position = mousePosition + hotspot;
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Cursor.visible = true;
    }
    
    public void SetCursorSprite(Sprite sprite)
    {
        if (cursorImage != null)
            cursorImage.sprite = sprite;
    }
}