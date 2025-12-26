using UnityEngine;

namespace MoreHit.Attack
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "MoreHit/AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("基本パラメータ")]
        [SerializeField, Min(0)] private int damage = 10;
        [SerializeField, Min(0)] private float range = 1.5f;
        [SerializeField] private Vector2 hitboxSize = new Vector2(1f, 1f);
        [SerializeField] private string[] targetTags = { "Enemy" };

        [Header("ストックシステム")]
        [SerializeField, Min(0)] private int stockAmount = 1;

        public int Damage => damage;
        public float Range => range;
        public Vector2 HitboxSize => hitboxSize;
        public string[] TargetTags => targetTags;
        public int StockAmount => stockAmount;
    }
}