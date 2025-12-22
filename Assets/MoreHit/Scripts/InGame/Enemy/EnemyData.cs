using UnityEngine;

namespace MoreHit.Enemy
{
    [System.Serializable]
    public class EnemyData
    {
        [Header("基本情報")]
        [SerializeField] private string enemyName;
        
        [Header("ステータス")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float attackPower = 10f;
        [SerializeField] private float moveSpeed = 3f;
        
        [Header("ストックシステム")]
        [SerializeField] private int stockCount = 1; // 何ストックで倒せるか

        // プロパティ
        public string EnemyName => enemyName;
        public float MaxHP => maxHP;
        public float AttackPower => attackPower;
        public float MoveSpeed => moveSpeed;
        public int StockCount => stockCount;
    }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "MoreHit/Enemy/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("敵リスト")]
        [SerializeField] private EnemyData[] enemyDataList;

        public EnemyData[] EnemyDataList => enemyDataList;
    }
}
