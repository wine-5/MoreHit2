using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using MoreHit.Audio;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        //拡大するときのアニメーションのためのコルーチン
        StartScaleAnimation(initialScale * scaleFactor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaleAnimation(initialScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.I != null)
        {
            AudioManager.I.PlaySE(SeType.Button);
        }
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
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
            // アニメーションの進行度 (0.0 ～ 1.0)
            // 毎フレームコルーチンによって毎フレーム更新されることでサイズが変わる
            float t = elapsedTime / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
        currentCoroutine = null;
    }
}
