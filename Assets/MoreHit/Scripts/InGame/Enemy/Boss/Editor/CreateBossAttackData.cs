using UnityEngine;
using UnityEditor;

namespace MoreHit.Enemy.Editor
{
    /// <summary>
    /// BossAttackDataSOを自動作成するエディタツール
    /// </summary>
    public class CreateBossAttackData
    {
        [MenuItem("MoreHit/Boss/Create Default BossAttackData")]
        public static void CreateDefaultBossAttackData()
        {
            BossAttackDataSO data = ScriptableObject.CreateInstance<BossAttackDataSO>();
            
            // 近接コンボ攻撃
            data.meleeCombo = new BossAttackPatternData
            {
                damage = 15f,
                attackRange = 300f,
                attackDuration = 1f,
                animationDuration = 1.5f,
                projectileCount = 8,
                projectileInterval = 0.1f,
                projectileSpeed = 0f
            };
            
            // 火球攻撃
            data.fireBall = new BossAttackPatternData
            {
                damage = 10f,
                attackRange = 3000f,
                attackDuration = 2f,
                animationDuration = 2.5f,
                projectileCount = 5,
                projectileInterval = 0.3f,
                projectileSpeed = 10f
            };
            
            // 地面叩きつけ攻撃
            data.groundSlam = new BossAttackPatternData
            {
                damage = 20f,
                attackRange = 500f,
                attackDuration = 1.5f,
                animationDuration = 2f,
                projectileCount = 3,
                projectileInterval = 0.5f,
                projectileSpeed = 0f
            };
            
            // AI設定
            data.baseAttackCooldown = 2f;
            data.meleeAttackRange = 300f;
            data.rangedAttackRange = 3000f;
            data.groundSlamHPThreshold = 0.5f;
            
            string path = "Assets/MoreHit/Data/Boss/DefaultBossAttackData.asset";
            string directory = System.IO.Path.GetDirectoryName(path);
            
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = data;
            
            Debug.Log($"[CreateBossAttackData] BossAttackDataSOを作成しました: {path}");
        }
    }
}
