using UnityEngine;

namespace MoreHit.Attack
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "MoreHit/AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("基本パラメータ")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float range = 1.5f;
        [SerializeField] private Vector2 hitboxSize = new Vector2(1f, 1f);
        [SerializeField] private string[] targetTags = new string[] { "Enemy" };
        
        [Header("ストックシステム")]
        [SerializeField] private int stockAmount = 1;
        
        [Header("エフェクト")]
        [SerializeField] private GameObject hitEffectPrefab;
        
        // プロパティで公開
        public float Damage => damage;
        public float Range => range;
        public Vector2 HitboxSize => hitboxSize;
        public string[] TargetTags => targetTags;
        public int StockAmount => stockAmount;
        public GameObject HitEffectPrefab => hitEffectPrefab;
    }
}