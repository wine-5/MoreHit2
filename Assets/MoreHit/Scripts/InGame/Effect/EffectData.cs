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
        public EffectType effectType;
        public GameObject effectPrefab;
        public float duration = 2f;

        public EffectData(EffectType type, GameObject prefab, float dur)
        {
            effectType = type;
            effectPrefab = prefab;
            duration = dur;
        }
    }

    /// <summary>
    /// 静的エフェクトデータストア - WebGL対応
    /// </summary>
    public static class EffectDataStore
    {
        // エフェクトプレハブのパス定義
        private const string EFFECT_PATH = "Effect/";
        
        /// <summary>
        /// エフェクトデータを取得
        /// </summary>
        public static EffectData GetEffectData(EffectType effectType)
        {
            GameObject prefab = LoadEffectPrefab(effectType);
            float duration = GetEffectDuration(effectType);
            
            return new EffectData(effectType, prefab, duration);
        }

        /// <summary>
        /// エフェクトプレハブを読み込み
        /// </summary>
        private static GameObject LoadEffectPrefab(EffectType effectType)
        {
            string prefabName = GetEffectPrefabName(effectType);
            GameObject prefab = Resources.Load<GameObject>(EFFECT_PATH + prefabName);
            
            if (prefab == null)
            {
                Debug.LogWarning($"[EffectDataStore] プレハブが見つかりません: {EFFECT_PATH + prefabName}");
            }
            
            return prefab;
        }

        /// <summary>
        /// エフェクトタイプに対応するプレハブ名を取得
        /// </summary>
        private static string GetEffectPrefabName(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.HitEffect:
                    return "HitEffect";
                case EffectType.ChargeAttackEffect:
                    return "ChargeAttackEffect";
                case EffectType.FullStockEffect:
                    return "FullStockEffect";
                default:
                    Debug.LogWarning($"[EffectDataStore] 未知のEffectType: {effectType}");
                    return "HitEffect"; // デフォルト
            }
        }

        /// <summary>
        /// エフェクトタイプに対応する継続時間を取得
        /// </summary>
        private static float GetEffectDuration(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.HitEffect:
                    return 1.0f;
                case EffectType.ChargeAttackEffect:
                    return 2.0f;
                case EffectType.FullStockEffect:
                    return 3.0f;
                default:
                    return 2.0f; // デフォルト
            }
        }

        /// <summary>
        /// 利用可能な全てのEffectTypeを取得
        /// </summary>
        public static EffectType[] GetAllEffectTypes()
        {
            return new EffectType[]
            {
                EffectType.HitEffect,
                EffectType.ChargeAttackEffect,
                EffectType.FullStockEffect
            };
        }

        /// <summary>
        /// 指定したエフェクトタイプが利用可能かチェック
        /// </summary>
        public static bool IsEffectAvailable(EffectType effectType)
        {
            GameObject prefab = LoadEffectPrefab(effectType);
            return prefab != null;
        }
    }
}
