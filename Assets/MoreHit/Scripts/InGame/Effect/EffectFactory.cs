using UnityEngine;
using MoreHit.Effect;
using MoreHit.Pool;
using System.Collections.Generic;

namespace MoreHit
{
    /// <summary>
    /// エフェクト生成を一元管理するFactory
    /// </summary>
    public class EffectFactory : Singleton<EffectFactory>
    {
        [Header("エフェクトデータ")]
        [SerializeField] private EffectDataSO effectDataSO;
        
        private ObjectPool objectPool;
        private Dictionary<EffectType, EffectData> effectDataDictionary;
        
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Awake()
        {
            base.Awake();
            
            // エフェクト用ObjectPoolを取得
            FindEffectObjectPool();
            
            // ScriptableObjectからエフェクトデータを初期化
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
            
            if (effectDataSO == null || effectDataSO.EffectDataList == null)
            {
                Debug.LogWarning("EffectFactory: エフェクトデータが設定されていません");
                return;
            }
            
            foreach (var effectData in effectDataSO.EffectDataList)
            {
                if (effectData != null && effectData.EffectPrefab != null)
                {
                    effectDataDictionary[effectData.EffectType] = effectData;
                }
                else if (effectData != null)
                {
                    Debug.LogWarning($"EffectFactory: {effectData.EffectType} のプレハブが設定されていません");
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
            if (effectDataDictionary == null || effectDataDictionary.Count == 0)
            {
                Debug.LogError("EffectFactory: エフェクトデータ辞書が初期化されていません");
                InitializeEffectDataDictionary();
                if (effectDataDictionary == null || effectDataDictionary.Count == 0)
                {
                    return null;
                }
            }
            
            if (!effectDataDictionary.TryGetValue(effectType, out EffectData data))
            {
                Debug.LogError($"EffectFactory: EffectType '{effectType}' のデータが見つかりません");
                return null;
            }
            
            if (objectPool == null)
            {
                Debug.LogError("EffectFactory: ObjectPool が利用できません");
                return null;
            }
            
            if (data.EffectPrefab == null)
            {
                Debug.LogError($"EffectFactory: EffectType '{effectType}' のプレハブがnullです");
                return null;
            }
            
            // プールからエフェクトオブジェクトを取得
            var result = objectPool.GetObject(data.EffectPrefab, position, Quaternion.identity);
            
            // スケールをプレハブの元の値にリセット
            if (result != null)
            {
                result.transform.localScale = data.EffectPrefab.transform.localScale;
            }
            
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
        
        /// <summary>
        /// 指定したエフェクトタイプの継続時間を取得
        /// </summary>
        /// <param name="effectType">取得するエフェクトタイプ</param>
        /// <returns>継続時間、エフェクトが見つからない場合は0f</returns>
        public float GetEffectDuration(EffectType effectType)
        {
            if (effectDataDictionary.TryGetValue(effectType, out EffectData data))
            {
                return data.Duration;
            }
            
            Debug.LogWarning($"EffectFactory: EffectType '{effectType}' のデータが見つかりません");
            return 0f;
        }
    }
}
