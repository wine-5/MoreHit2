using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 1.5倍の指定
    [SerializeField] private float scaleFactor = 1.5f;
    [SerializeField] private float animationDuration = 0.1f;

    private Vector3 initialScale;
    private Coroutine currentCoroutine;

    void Awake()
    {
        initialScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //ヌルっとしたアニメーションのためのコルーチン
        StartScaleAnimation(initialScale * scaleFactor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaleAnimation(initialScale);
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        // 特定のコルーチンのみを停止させて干渉を防ぐ
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ScaleAnimationRoutine(targetScale));
    }

    private IEnumerator ScaleAnimationRoutine(Vector3 targetScale)
    {
        float elapsedTime = 0;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            // アニメーションの進捗率 (0.0 ～ 1.0)
            // tがコルーチンによって毎フレーム増えることでサイズが変わる
            float t = elapsedTime / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
        currentCoroutine = null;
    }
}