using UnityEngine;

namespace MoreHit.Enemy
{
    [System.Serializable]
    public class EnemyData
    {
        public EnemyType enemyType;
        public float maxHP;
        public float moveSpeed;
        public int stockCount;
        public int needstock;

        public EnemyData(EnemyType type, float hp, float speed, int stock, int needStock)
        {
            enemyType = type;
            maxHP = hp;
            moveSpeed = speed;
            stockCount = stock;
            needstock = needStock;
        }

        // プロパティ
        public EnemyType EnemyType => enemyType;
        public float MaxHP => maxHP;
        public float MoveSpeed => moveSpeed;
        public int StockCount => stockCount;
        public int Needstock => needstock;
    }

    /// <summary>
    /// 静的敵データストア - WebGL対応
    /// </summary>
    public static class EnemyDataStore
    {
        // Normal Enemy Constants
        private const float NORMAL_HP = 100f;
        private const float NORMAL_SPEED = 10f;
        private const int NORMAL_STOCK = 0;
        private const int NORMAL_NEED_STOCK = 4;

        // Middle Enemy Constants
        private const float MIDDLE_HP = 100f;
        private const float MIDDLE_SPEED = 15f;
        private const int MIDDLE_STOCK = 0;
        private const int MIDDLE_NEED_STOCK = 6;

        // Large Enemy Constants
        private const float LARGE_HP = 100f;
        private const float LARGE_SPEED = 2f;
        private const int LARGE_STOCK = 2;
        private const int LARGE_NEED_STOCK = 8;

        // Boss Enemy Constants
        private const float BOSS_HP = 200f;
        private const float BOSS_SPEED = 7f;
        private const int BOSS_STOCK = 0;
        private const int BOSS_NEED_STOCK = 20;

        public static EnemyData GetEnemyData(EnemyType enemyType)
        {
            switch (enemyType)
            {
                case EnemyType.Normal:
                    return new EnemyData(EnemyType.Normal, NORMAL_HP, NORMAL_SPEED, NORMAL_STOCK, NORMAL_NEED_STOCK);
                
                case EnemyType.Middle:
                    return new EnemyData(EnemyType.Middle, MIDDLE_HP, MIDDLE_SPEED, MIDDLE_STOCK, MIDDLE_NEED_STOCK);
                
                case EnemyType.Large:
                    return new EnemyData(EnemyType.Large, LARGE_HP, LARGE_SPEED, LARGE_STOCK, LARGE_NEED_STOCK);
                
                case EnemyType.Boss:
                    return new EnemyData(EnemyType.Boss, BOSS_HP, BOSS_SPEED, BOSS_STOCK, BOSS_NEED_STOCK);
                
                default:
                    Debug.LogWarning($"[EnemyDataStore] 未知のEnemyType: {enemyType}. Normalを返します");
                    return new EnemyData(EnemyType.Normal, NORMAL_HP, NORMAL_SPEED, NORMAL_STOCK, NORMAL_NEED_STOCK);
            }
        }

        /// <summary>
        /// 利用可能な全てのEnemyTypeを取得
        /// </summary>
        public static EnemyType[] GetAllEnemyTypes()
        {
            return new EnemyType[]
            {
                EnemyType.Normal,
                EnemyType.Middle,
                EnemyType.Large,
                EnemyType.Boss
            };
        }
    }
}