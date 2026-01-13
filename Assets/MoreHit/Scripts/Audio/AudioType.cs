namespace MoreHit.Audio
{
    /// <summary>
    /// SEの種類
    /// </summary>
    public enum SeType
    {
        None,
        Button,
        Charge,
        Projectile,
        NormalAttack,
        Jump,
        BossDefeat,
        TakeDamage,
        EnemyDefeat,
    }

    /// <summary>
    /// BGMの種類
    /// </summary>
    public enum BgmType
    {
        None,
        Title,
        InGame,
        GameClear,
        GameOver,
    }
}
