using UnityEngine;
using MoreHit.Effect;
using MoreHit.Pool;
using System.Collections.Generic;

namespace MoreHit
{
    /// <summary>
    /// エフェクト生成を一元管理するFactory - Static Data Pattern
    /// </summary>
    public class EffectFactory : Singleton<EffectFactory>
    {
        private ObjectPool objectPool;
        private Dictionary<EffectType, EffectData> effectDataDictionary;
        
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Awake()
        {
            base.Awake();
            
            // エフェクト用ObjectPoolを取得
            FindEffectObjectPool();
            
            // 静的データストアからエフェクトデータを初期化
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
            
            Debug.Log("🔄 EffectFactory: 静的データストアからエフェクトデータを初期化中...");
            
            // 静的データストアから全てのエフェクトタイプを取得
            EffectType[] allEffectTypes = EffectDataStore.GetAllEffectTypes();
            
            foreach (var effectType in allEffectTypes)
            {
                EffectData data = EffectDataStore.GetEffectData(effectType);
                
                if (data != null && data.effectPrefab != null)
                {
                    effectDataDictionary[effectType] = data;
                    Debug.Log($"✅ EffectFactory: {effectType} エフェクトを登録しました");
                }
                else if (data != null)
                {
                    Debug.LogWarning($"⚠️ EffectFactory: {effectType} のプレハブが設定されていません");
                }
                else
                {
                    Debug.LogError($"❌ EffectFactory: {effectType} のデータ取得に失敗しました");
                }
            }
            
            Debug.Log($"✅ EffectFactory: 合計{effectDataDictionary.Count}個のエフェクトを登録しました");
        }
        
        /// <summary>
        /// エフェクトを生成
        /// </summary>
        /// <param name="effectType">生成するエフェクトの種類</param>
        /// <param name="position">生成位置</param>
        /// <returns>生成されたエフェクトオブジェクト</returns>
        public GameObject CreateEffect(EffectType effectType, Vector3 position)
        {
            // エフェクトデータ辞書がnullまたは空の場合
            if (effectDataDictionary == null || effectDataDictionary.Count == 0)
            {
                Debug.LogError("❌ EffectFactory: エフェクトデータ辞書が初期化されていません！InitializeEffectDataDictionary()を実行してください");
                InitializeEffectDataDictionary(); // 再初期化を試行
                if (effectDataDictionary == null || effectDataDictionary.Count == 0)
                {
                    return null;
                }
            }
            
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
            
            // スケールをプレハブの元の値にリセット
            if (result != null)
            {
                result.transform.localScale = data.effectPrefab.transform.localScale;
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
                return data.duration;
            }
            
            Debug.LogWarning($"⚠️ EffectFactory: EffectType '{effectType}' のデータが見つかりません！継続時間0を返します");
            return 0f;
        }
    }
}
