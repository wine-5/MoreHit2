namespace MoreHit.Attack
{
    /// <summary>
    /// プレイヤーの溜め攻撃（強化された遠距離攻撃）
    /// 【役割】
    /// - 強化された弾丸の発射処理を実行（ProjectileAttackBaseから継承）
    /// - チャージエフェクトの管理はPlayerChargeEffectManagerが担当
    /// - このクラスは攻撃処理のみに専念
    /// </summary>
    public class ChargedAttack : ProjectileAttackBase
    {
        // ProjectileAttackBaseのExecute()メソッドで弾丸発射を実行
    }
}