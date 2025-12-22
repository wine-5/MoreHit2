using UnityEngine;

namespace MoreHit.Enemy
{
    [System.Serializable]
    public class EnemyData
    {
        [Header("基本情報")]
        [SerializeField] private EnemyType enemyType = EnemyType.Zako;
        
        [Header("ステータス")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float attackPower = 10f;
        [SerializeField] private float moveSpeed = 3f;
        
        [Header("ストックシステム")]
        [SerializeField] private int stockCount = 0;//現在のストック数をカウント
        [SerializeField] private int needstock = 1; //必要ストック数
        [SerializeField] private bool isSmash = false;               //現在ぶっ飛ばされた状態か

        public EnemyType EnemyType => enemyType;
        public float MaxHP => maxHP;
        public float AttackPower => attackPower;
        public float MoveSpeed => moveSpeed;
        public int StockCount => stockCount;

        public int Needstock => needstock;
        public bool IsSmash => isSmash;
    }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "MoreHit/Enemy/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("敵リスト")]
        [SerializeField] private EnemyData[] enemyDataList;

        public EnemyData[] EnemyDataList => enemyDataList;
    }
}