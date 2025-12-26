using UnityEngine;
using System.Collections.Generic;

namespace MoreHit.Effect
{
    /// <summary>
    /// エフェクトの基本データ
    /// </summary>
    [System.Serializable]
    public class EffectData
    {
        [Header("基本設定")]
        public EffectType effectType;
        public GameObject effectPrefab;
        
        [Header("継続時間設定")]
        [Tooltip("エフェクトの継続時間（秒）")]
        public float duration = 2f;
    }
    
    /// <summary>
    /// 複数のエフェクトデータを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Effect Data Collection", menuName = "MoreHit/Effect Data Collection")]
    public class EffectDataSO : ScriptableObject
    {
        [Header("Effect Collection")]
        [SerializeField] private List<EffectData> effectList = new List<EffectData>();
        
        /// <summary>
        /// すべてのエフェクトデータを取得
        /// </summary>
        public List<EffectData> GetAllEffects()
        {
            return new List<EffectData>(effectList);
        }
        
        /// <summary>
        /// 指定したタイプのエフェクトデータを取得
        /// </summary>
        public EffectData GetEffectByType(EffectType type)
        {
            return effectList.Find(effect => effect.effectType == type);
        }
        
        /// <summary>
        /// エフェクトデータを追加
        /// </summary>
        public void AddEffect(EffectData effectData)
        {
            if (effectData != null && !effectList.Contains(effectData))
            {
                effectList.Add(effectData);
            }
        }
        
        /// <summary>
        /// エフェクトデータを削除
        /// </summary>
        public void RemoveEffect(EffectData effectData)
        {
            effectList.Remove(effectData);
        }
        
        /// <summary>
        /// 指定したタイプのエフェクトが存在するかチェック
        /// </summary>
        public bool HasEffectType(EffectType type)
        {
            return effectList.Exists(effect => effect.effectType == type);
        }
    }
}
