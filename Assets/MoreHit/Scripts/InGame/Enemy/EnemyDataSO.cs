using UnityEngine;

namespace MoreHit.Enemy
{
    /// <summary>
    /// 敵データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "MoreHit/Enemy/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("敵設定")]
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private int maxHP = 100;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private int stockCount = 1;
        [SerializeField] private int needStock = 5;
        
        public EnemyType EnemyType => enemyType;
        public int MaxHP => maxHP;
        public float MoveSpeed => moveSpeed;
        public int StockCount => stockCount;
        public int NeedStock => needStock;
    }
}