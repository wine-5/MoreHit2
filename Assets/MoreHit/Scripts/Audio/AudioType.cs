namespace MoreHit.Audio
{
    /// <summary>
    /// SEの種類
    /// </summary>
    public enum SeType
    {
        None,
        SE_Button,
        SE_Charge,
        SE_Projectile,
        SE_NormalAttack,
        SE_Jump,
        SE_BossDefeat,
        SE_TakeDamage,
        SE_EnemyDefeat,
    }

    /// <summary>
    /// BGMの種類
    /// </summary>
    public enum BgmType
    {
        None,
        BGM_Title,
        BGM_InGame,
        BGM_GameClear,
        BGM_GameOver,
    }
}
