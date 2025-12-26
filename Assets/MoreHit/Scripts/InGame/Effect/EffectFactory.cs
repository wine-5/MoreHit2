using UnityEngine;
using MoreHit.Effect;
using MoreHit.Pool;
using System.Collections.Generic;
using System.Linq;

namespace MoreHit
{
    /// <summary>
    /// エフェクト生成を一元管理するFactory
    /// </summary>
    public class EffectFactory : Singleton<EffectFactory>
    {
        [Header("Effect Data")]
        [SerializeField] private EffectDataSO effectDataCollection;
        
        private ObjectPool objectPool;
        private Dictionary<EffectType, EffectData> effectDataDictionary;
        
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Awake()
        {
            base.Awake();
            
            // エフェクト用ObjectPoolを取得
            FindEffectObjectPool();
            
            // エフェクトデータを辞書化
            InitializeEffectDataDictionary();
        }
        
        private void FindEffectObjectPool()
        {
            var allObjectPools = FindObjectsByType<ObjectPool>(FindObjectsSortMode.None);
            
            // エフェクト用ObjectPoolを優先的に検索
            foreach (var pool in allObjectPools)
            {
                if (pool.IsEffectPool())
                {
                    objectPool = pool;
                    return;
                }
            }
            
            // エフェクト用が見つからない場合は最初のものを使用
            objectPool = FindFirstObjectByType<ObjectPool>();
            if (objectPool == null)
            {
                Debug.LogError("❌ EffectFactory: ObjectPool が見つかりません！プールなしでは動作できません");
                return;
            }
        }
        
        private void InitializeEffectDataDictionary()
        {
            effectDataDictionary = new Dictionary<EffectType, EffectData>();
            
            if (effectDataCollection == null)
            {
                Debug.LogError("❌ EffectFactory: EffectDataSOが設定されていません！");
                return;
            }
            
            var allEffects = effectDataCollection.GetAllEffects();
            
            foreach (var data in allEffects)
            {
                if (data != null && data.effectPrefab != null)
                {
                    effectDataDictionary[data.effectType] = data;
                }
            }
        }
        
        /// <summary>
        /// エフェクトを生成
        /// </summary>
        /// <param name="effectType">生成するエフェクトの種類</param>
        /// <param name="position">生成位置</param>
        /// <returns>生成されたエフェクトオブジェクト</returns>
        public GameObject CreateEffect(EffectType effectType, Vector3 position)
        {
            if (!effectDataDictionary.TryGetValue(effectType, out EffectData data))
            {
                Debug.LogError($"❌ EffectFactory: EffectType '{effectType}' のデータが見つかりません！");
                Debug.LogError($"❌ 利用可能なエフェクトタイプ: {string.Join(", ", effectDataDictionary.Keys)}");
                return null;
            }
            
            if (objectPool == null)
            {
                Debug.LogError("❌ EffectFactory: ObjectPool が利用できません！エフェクト生成を中止します");
                return null;
            }
            
            if (data.effectPrefab == null)
            {
                Debug.LogError($"❌ EffectFactory: EffectType '{effectType}' のプレハブがnullです！");
                return null;
            }
            
            // プールからエフェクトオブジェクトを取得
            var result = objectPool.GetObject(data.effectPrefab, position, Quaternion.identity);
            
            return result;
        }
        
        /// <summary>
        /// エフェクトをプールに返却
        /// </summary>
        /// <param name="effectObject">返却するエフェクトオブジェクト</param>
        public void ReturnEffect(GameObject effectObject)
        {
            if (effectObject == null) return;
            
            if (objectPool == null)
            {
                Debug.LogError("EffectFactory: ObjectPool が利用できません！エフェクトを直接破棄します");
                Destroy(effectObject);
                return;
            }
            
            objectPool.ReturnObject(effectObject);
        }
        
        /// <summary>
        /// 一定時間後にエフェクトをプールに返却
        /// </summary>
        /// <param name="effectObject">返却するエフェクトオブジェクト</param>
        /// <param name="delay">返却までの時間</param>
        public void ReturnEffectDelayed(GameObject effectObject, float delay)
        {
            if (effectObject != null)
                StartCoroutine(ReturnEffectAfterDelay(effectObject, delay));
        }
        
        private System.Collections.IEnumerator ReturnEffectAfterDelay(GameObject effectObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (effectObject != null)
                ReturnEffect(effectObject);
        }
        
        /// <summary>
        /// エフェクトデータコレクションを設定
        /// </summary>
        /// <param name="dataCollection">設定するエフェクトデータコレクション</param>
        public void SetEffectDataCollection(EffectDataSO dataCollection)
        {
            effectDataCollection = dataCollection;
            InitializeEffectDataDictionary();
        }
        
        /// <summary>
        /// 利用可能なエフェクトタイプの一覧を取得
        /// </summary>
        /// <returns>利用可能なエフェクトタイプの配列</returns>
        public EffectType[] GetAvailableEffectTypes()
        {
            if (effectDataDictionary == null) return new EffectType[0];
            
            EffectType[] types = new EffectType[effectDataDictionary.Count];
            effectDataDictionary.Keys.CopyTo(types, 0);
            return types;
        }
        
        /// <summary>
        /// 指定したエフェクトタイプが利用可能かチェック
        /// </summary>
        /// <param name="effectType">チェックするエフェクトタイプ</param>
        /// <returns>利用可能な場合はtrue</returns>
        public bool IsEffectAvailable(EffectType effectType)
        {
            return effectDataDictionary != null && effectDataDictionary.ContainsKey(effectType);
        }
    }
}
