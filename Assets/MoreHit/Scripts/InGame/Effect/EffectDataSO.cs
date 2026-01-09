using UnityEngine;

namespace MoreHit.Effect
{
    /// <summary>
    /// エフェクトデータ
    /// </summary>
    [System.Serializable]
    public class EffectData
    {
        [SerializeField] private EffectType effectType;
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private float duration = 1.0f;
        
        public EffectType EffectType => effectType;
        public GameObject EffectPrefab => effectPrefab;
        public float Duration => duration;
    }

    /// <summary>
    /// エフェクトデータを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EffectData", menuName = "MoreHit/Effect/EffectData")]
    public class EffectDataSO : ScriptableObject
    {
        [Header("エフェクトリスト")]
        [SerializeField] private EffectData[] effectDataList;
        
        public EffectData[] EffectDataList => effectDataList;
    }
}