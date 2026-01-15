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
        ChargedProjectile,
        NormalAttack,
        Jump,
        BossDefeat,
        TakeDamage,
        EnemyDefeat,
        FullStock,
        Warning,
    }

    /// <summary>
    /// BGMの種類
    /// </summary>
    public enum BgmType
    {
        None,
        Title,
        InGame,
        Boss,
        GameClear,
        GameOver,
    }
}
