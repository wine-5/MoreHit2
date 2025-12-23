# 攻撃システム設計書 - 改訂版

## 概要

ハイブリッド型アーキテクチャを採用し、各クラスの責任を明確に分離した設計。
AttackExecutorで攻撃判定を統一管理し、専門クラスに処理を委譲することで、拡張性と保守性を向上。

**特徴:**
- Player→Enemy、Enemy→Player 両方向で同じ仕組みを使用
- 各クラスの責任が明確で単一責任原則に準拠
- 疎結合で拡張性が高い
- プーリングによるパフォーマンス最適化

---

## クラス一覧と責任

### コアシステム

| クラス名 | タイプ | 責任 |
|---------|--------|------|
| **AttackData** | ScriptableObject | 攻撃パラメータの定義（ダメージ、範囲、エフェクトなど） |
| **AttackContext** | データクラス | 攻撃実行時の文脈情報（攻撃者、位置、方向） |
| **AttackResult** | データクラス | 攻撃結果の情報（ヒット数、対象リスト） |
| **AttackExecutor** | Singleton | 攻撃判定の統括・各処理への委譲 |
| **HitDetector** | クラス | 物理判定の実行、コライダー検出 |
| **HitProcessor** | クラス | ヒット時の処理適用（ダメージ、ノックバック等） |

### 攻撃インターフェースと実装

| クラス名 | タイプ | 責任 |
|---------|--------|------|
| **IAttack** | Interface | 攻撃クラスの共通仕様 |
| **AttackManager** | MonoBehaviour | プレイヤーの全攻撃を管理、攻撃切り替え |
| **NormalAttack** | MonoBehaviour | 通常攻撃の実装（3段コンボ、近接攻撃） |
| **RangedAttack** | MonoBehaviour | 射撃攻撃の実装（遠距離、弾生成） |
| **RushAttack** | MonoBehaviour | 突進攻撃の実装（移動+攻撃） |
| **EnemyAttack** | MonoBehaviour | 敵の攻撃実装 |

### ダメージ・ヘルスシステム

| クラス名 | タイプ | 責任 |
|---------|--------|------|
| **IDamageable** | Interface | ダメージを受ける能力 ✅実装済み |
| **IStockable** | Interface | ストックを蓄積する能力 |
| **PlayerHealth** | MonoBehaviour | プレイヤーのHP管理 ✅実装済み |
| **EnemyHealth** | MonoBehaviour | 敵のHP管理 |
| **InvincibilityController** | クラス | 無敵時間の管理 |

### エフェクトシステム

| クラス名 | タイプ | 責任 |
|---------|--------|------|
| **EffectType** | Enum | エフェクトの種類定義 |
| **EffectData** | ScriptableObject | エフェクトのパラメータ定義 |
| **EffectFactory** | Singleton | エフェクトの生成・管理 |
| **EffectPool** | クラス | エフェクトのオブジェクトプーリング |
| **AutoReturnToPool** | MonoBehaviour | エフェクトの自動返却 |

---

## クラス間の依存関係図

```
PlayerInputManager
       ↓
PlayerController
       ↓
AttackManager ────────────┐
       ↓                  │
   IAttack (Interface)    │
       ↑                  │
       ├─ NormalAttack    │
       ├─ RangedAttack    │
       └─ RushAttack      │
              ↓           │
        AttackContext     │
              ↓           ↓
         AttackExecutor ←─┘
              ↓
    ┌─────────┼─────────┐
    ↓         ↓         ↓
HitDetector  HitProcessor  EffectFactory
    ↓         ↓              ↓
    └─────→ Result      EffectPool
              ↓
    ┌─────────┴─────────┐
    ↓                   ↓
IDamageable         IStockable
    ↑                   ↑
    ├─ PlayerHealth     ├─ EnemyHealth
    │  (+ Invincibility)│  (+ Invincibility)
    └─ EnemyHealth      └─ (implements both)


EnemyAI
   ↓
EnemyAttack (IAttack)
   ↓
AttackExecutor
   ↓
PlayerHealth (IDamageable)
```

---

## 詳細なクラス設計

### 1. AttackData (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "AttackData", menuName = "MoreHit/AttackData")]
public class AttackData : ScriptableObject
{
    [Header("基本パラメータ")]
    [Tooltip("ダメージ量")]
    public float Damage = 10f;
    
    [Tooltip("攻撃範囲")]
    public float Range = 1.5f;
    
    [Tooltip("ノックバック強度")]
    public float Knockback = 5f;
    
    [Header("判定")]
    [Tooltip("判定の位置オフセット")]
    public Vector2 HitboxOffset;
    
    [Tooltip("判定のサイズ")]
    public Vector2 HitboxSize = new Vector2(1f, 1f);
    
    [Tooltip("攻撃対象レイヤー")]
    public LayerMask TargetLayers;
    
    [Header("タイミング")]
    [Tooltip("攻撃モーション時間")]
    public float AttackDuration = 0.5f;
    
    [Tooltip("クールタイム")]
    public float Cooldown = 0.3f;
    
    [Header("ストックシステム")]
    [Tooltip("蓄積するストック量")]
    public int StockAmount = 1;
    
    [Header("エフェクト")]
    [Tooltip("ヒットエフェクトの種類")]
    public EffectType HitEffectType = EffectType.Hit;
    
    [Tooltip("攻撃エフェクトの種類")]
    public EffectType AttackEffectType = EffectType.Slash;
    
    [Header("連続ヒット防止")]
    [Tooltip("同じ対象への連続ヒット防止時間")]
    public float HitCooldownPerTarget = 0.5f;
    
    private void OnValidate()
    {
        if (Damage < 0) Damage = 0;
        if (Range < 0) Range = 0;
        if (HitboxSize.x <= 0 || HitboxSize.y <= 0)
        {
            Debug.LogWarning($"{name}: HitboxSize must be positive");
            HitboxSize = new Vector2(
                Mathf.Max(0.1f, HitboxSize.x),
                Mathf.Max(0.1f, HitboxSize.y)
            );
        }
        if (AttackDuration < 0) AttackDuration = 0;
        if (Cooldown < 0) Cooldown = 0;
    }
}
```

### 2. AttackContext (データクラス)

```csharp
/// <summary>
/// 攻撃実行時の文脈情報
/// </summary>
public class AttackContext
{
    /// <summary>攻撃データ</summary>
    public AttackData Data { get; private set; }
    
    /// <summary>攻撃者のGameObject</summary>
    public GameObject Attacker { get; private set; }
    
    /// <summary>攻撃の原点位置</summary>
    public Vector3 Origin { get; private set; }
    
    /// <summary>攻撃の方向</summary>
    public Vector2 Direction { get; private set; }
    
    /// <summary>攻撃のタイムスタンプ</summary>
    public float Timestamp { get; private set; }
    
    public AttackContext(AttackData data, GameObject attacker, Vector3 origin, Vector2 direction)
    {
        Data = data;
        Attacker = attacker;
        Origin = origin;
        Direction = direction.normalized;
        Timestamp = Time.time;
    }
}
```

### 3. AttackResult (データクラス)

```csharp
/// <summary>
/// 攻撃結果の情報
/// </summary>
public class AttackResult
{
    /// <summary>攻撃が成功したか（1体以上にヒット）</summary>
    public bool Success => HitTargets.Count > 0;
    
    /// <summary>ヒットした対象のリスト</summary>
    public List<GameObject> HitTargets { get; private set; }
    
    /// <summary>ヒット数</summary>
    public int HitCount => HitTargets.Count;
    
    public AttackResult()
    {
        HitTargets = new List<GameObject>();
    }
    
    public void AddHit(GameObject target)
    {
        if (!HitTargets.Contains(target))
        {
            HitTargets.Add(target);
        }
    }
}
```

### 4. HitDetector (クラス)

```csharp
/// <summary>
/// 攻撃判定の物理検出を担当
/// </summary>
public class HitDetector
{
    /// <summary>
    /// 指定範囲内のコライダーを検出
    /// </summary>
    public Collider2D[] DetectHits(AttackContext context)
    {
        Vector3 hitPosition = context.Origin + (Vector3)(context.Direction * context.Data.HitboxOffset.x);
        hitPosition += Vector3.up * context.Data.HitboxOffset.y;
        
        float angle = Mathf.Atan2(context.Direction.y, context.Direction.x) * Mathf.Rad2Deg;
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitPosition,
            context.Data.HitboxSize,
            angle,
            context.Data.TargetLayers
        );
        
        // 攻撃者自身を除外
        return hits.Where(h => h.gameObject != context.Attacker).ToArray();
    }
    
    /// <summary>
    /// デバッグ用：判定範囲を可視化
    /// </summary>
    public void DrawDebugBox(AttackContext context, Color color, float duration = 0.5f)
    {
#if UNITY_EDITOR
        Vector3 hitPosition = context.Origin + (Vector3)(context.Direction * context.Data.HitboxOffset.x);
        hitPosition += Vector3.up * context.Data.HitboxOffset.y;
        
        Vector3 size = context.Data.HitboxSize;
        Vector3 halfSize = size * 0.5f;
        
        Vector3[] corners = new Vector3[4]
        {
            hitPosition + new Vector3(-halfSize.x, -halfSize.y, 0),
            hitPosition + new Vector3(halfSize.x, -halfSize.y, 0),
            hitPosition + new Vector3(halfSize.x, halfSize.y, 0),
            hitPosition + new Vector3(-halfSize.x, halfSize.y, 0)
        };
        
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(corners[i], corners[(i + 1) % 4], color, duration);
        }
#endif
    }
}
```

### 5. HitProcessor (クラス)

```csharp
/// <summary>
/// ヒット時の各種処理を適用
/// </summary>
public class HitProcessor
{
    private Dictionary<GameObject, float> lastHitTimes = new Dictionary<GameObject, float>();
    
    /// <summary>
    /// ヒット対象に全ての処理を適用
    /// </summary>
    public void ProcessHit(GameObject target, AttackContext context)
    {
        // 連続ヒット防止チェック
        if (IsInHitCooldown(target, context))
        {
            return;
        }
        
        ApplyDamage(target, context);
        ApplyKnockback(target, context);
        ApplyStock(target, context);
        
        // ヒット時間を記録
        RecordHitTime(target);
    }
    
    /// <summary>
    /// ダメージを適用
    /// </summary>
    private void ApplyDamage(GameObject target, AttackContext context)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(context.Data.Damage);
        }
    }
    
    /// <summary>
    /// ノックバックを適用
    /// </summary>
    private void ApplyKnockback(GameObject target, AttackContext context)
    {
        var rb = target.GetComponent<Rigidbody2D>();
        if (rb != null && context.Data.Knockback > 0)
        {
            Vector2 knockbackForce = context.Direction * context.Data.Knockback;
            rb.AddForce(knockbackForce, ForceMode2D.Impulse);
        }
    }
    
    /// <summary>
    /// ストックを蓄積
    /// </summary>
    private void ApplyStock(GameObject target, AttackContext context)
    {
        var stockable = target.GetComponent<IStockable>();
        if (stockable != null && context.Data.StockAmount > 0)
        {
            stockable.AddStock(context.Data.StockAmount);
        }
    }
    
    /// <summary>
    /// 連続ヒット防止チェック
    /// </summary>
    private bool IsInHitCooldown(GameObject target, AttackContext context)
    {
        if (lastHitTimes.TryGetValue(target, out float lastHitTime))
        {
            float timeSinceLastHit = Time.time - lastHitTime;
            return timeSinceLastHit < context.Data.HitCooldownPerTarget;
        }
        return false;
    }
    
    /// <summary>
    /// ヒット時間を記録
    /// </summary>
    private void RecordHitTime(GameObject target)
    {
        lastHitTimes[target] = Time.time;
    }
    
    /// <summary>
    /// 古いヒット記録をクリア
    /// </summary>
    public void CleanupOldRecords(float maxAge = 5f)
    {
        float currentTime = Time.time;
        var expiredKeys = lastHitTimes
            .Where(kvp => currentTime - kvp.Value > maxAge)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in expiredKeys)
        {
            lastHitTimes.Remove(key);
        }
    }
}
```

### 6. AttackExecutor (Singleton)

```csharp
/// <summary>
/// 攻撃実行の統括管理・各処理への委譲
/// </summary>
public class AttackExecutor : Singleton<AttackExecutor>
{
    private HitDetector hitDetector;
    private HitProcessor hitProcessor;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color debugColor = Color.red;
    
    protected override void Awake()
    {
        base.Awake();
        hitDetector = new HitDetector();
        hitProcessor = new HitProcessor();
    }
    
    private void Update()
    {
        // 古いヒット記録を定期的にクリーンアップ
        hitProcessor.CleanupOldRecords();
    }
    
    /// <summary>
    /// 攻撃を実行
    /// </summary>
    public AttackResult ExecuteAttack(AttackContext context)
    {
        var result = new AttackResult();
        
        // 1. 判定検出
        Collider2D[] hits = hitDetector.DetectHits(context);
        
        // デバッグ表示
        if (showDebugGizmos)
        {
            hitDetector.DrawDebugBox(context, debugColor);
        }
        
        // 2. 各ヒット対象に処理を適用
        foreach (var hit in hits)
        {
            hitProcessor.ProcessHit(hit.gameObject, context);
            result.AddHit(hit.gameObject);
        }
        
        // 3. エフェクト生成
        if (result.Success)
        {
            SpawnEffects(context, hits);
        }
        
        return result;
    }
    
    /// <summary>
    /// エフェクトを生成
    /// </summary>
    private void SpawnEffects(AttackContext context, Collider2D[] hits)
    {
        // 攻撃エフェクト
        if (context.Data.AttackEffectType != EffectType.None)
        {
            Vector3 effectPosition = context.Origin + (Vector3)(context.Direction * context.Data.HitboxOffset.x);
            EffectFactory.I.CreateEffect(
                context.Data.AttackEffectType,
                effectPosition,
                context.Direction
            );
        }
        
        // ヒットエフェクト（各ヒット対象に）
        if (context.Data.HitEffectType != EffectType.None)
        {
            foreach (var hit in hits)
            {
                EffectFactory.I.CreateEffect(
                    context.Data.HitEffectType,
                    hit.transform.position,
                    context.Direction
                );
            }
        }
    }
}
```

### 7. IAttack (Interface)

```csharp
/// <summary>
/// 攻撃クラスの共通インターフェース
/// </summary>
public interface IAttack
{
    /// <summary>
    /// 攻撃を実行できるか（クールタイム・条件チェック）
    /// </summary>
    bool CanExecute();
    
    /// <summary>
    /// 攻撃を実行
    /// </summary>
    void Execute();
    
    /// <summary>
    /// 攻撃をキャンセル
    /// </summary>
    void Cancel();
    
    /// <summary>
    /// 攻撃中かどうか
    /// </summary>
    bool IsAttacking { get; }
    
    /// <summary>
    /// 攻撃の優先度
    /// </summary>
    AttackPriority Priority { get; }
}

/// <summary>
/// 攻撃の優先度
/// </summary>
public enum AttackPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}
```

### 8. AttackManager (MonoBehaviour)

```csharp
/// <summary>
/// プレイヤーの攻撃を統括管理
/// </summary>
public class AttackManager : MonoBehaviour
{
    [Header("攻撃コンポーネント")]
    [SerializeField] private NormalAttack normalAttack;
    [SerializeField] private RangedAttack rangedAttack;
    [SerializeField] private RushAttack rushAttack;
    
    private IAttack currentAttack;
    
    private void Awake()
    {
        // 攻撃コンポーネントの取得
        if (normalAttack == null) normalAttack = GetComponent<NormalAttack>();
        if (rangedAttack == null) rangedAttack = GetComponent<RangedAttack>();
        if (rushAttack == null) rushAttack = GetComponent<RushAttack>();
    }
    
    /// <summary>
    /// 通常攻撃を実行
    /// </summary>
    public void ExecuteNormalAttack()
    {
        TryExecuteAttack(normalAttack);
    }
    
    /// <summary>
    /// 射撃攻撃を実行
    /// </summary>
    public void ExecuteRangedAttack()
    {
        TryExecuteAttack(rangedAttack);
    }
    
    /// <summary>
    /// 突進攻撃を実行
    /// </summary>
    public void ExecuteRushAttack()
    {
        TryExecuteAttack(rushAttack);
    }
    
    /// <summary>
    /// 攻撃実行を試みる
    /// </summary>
    private void TryExecuteAttack(IAttack attack)
    {
        if (attack == null) return;
        
        // 現在の攻撃がある場合、優先度をチェック
        if (currentAttack != null && currentAttack.IsAttacking)
        {
            // 新しい攻撃の方が優先度が高い場合はキャンセル
            if (attack.Priority > currentAttack.Priority)
            {
                currentAttack.Cancel();
            }
            else
            {
                return; // 実行しない
            }
        }
        
        // 実行可能かチェック
        if (attack.CanExecute())
        {
            currentAttack = attack;
            attack.Execute();
        }
    }
    
    /// <summary>
    /// 全ての攻撃をキャンセル
    /// </summary>
    public void CancelAllAttacks()
    {
        normalAttack?.Cancel();
        rangedAttack?.Cancel();
        rushAttack?.Cancel();
        currentAttack = null;
    }
    
    /// <summary>
    /// 攻撃中かどうか
    /// </summary>
    public bool IsAttacking => currentAttack != null && currentAttack.IsAttacking;
}
```

### 9. NormalAttack (MonoBehaviour)

```csharp
/// <summary>
/// 通常攻撃の実装（3段コンボ）
/// </summary>
public class NormalAttack : MonoBehaviour, IAttack
{
    [Header("攻撃データ")]
    [SerializeField] private AttackData[] comboAttacks; // 3段分のデータ
    
    [Header("設定")]
    [SerializeField] private float comboResetTime = 1f;
    [SerializeField] private AttackPriority priority = AttackPriority.Normal;
    
    [Header("参照")]
    [SerializeField] private Animator animator;
    
    private int comboIndex = 0;
    private float cooldownTimer = 0f;
    private float comboTimer = 0f;
    private bool isAttacking = false;
    
    public bool IsAttacking => isAttacking;
    public AttackPriority Priority => priority;
    
    private void Update()
    {
        // クールタイム更新
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // コンボタイマー更新
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }
    
    public bool CanExecute()
    {
        return !isAttacking && cooldownTimer <= 0;
    }
    
    public void Execute()
    {
        if (!CanExecute()) return;
        
        StartCoroutine(ExecuteAttackCoroutine());
    }
    
    private IEnumerator ExecuteAttackCoroutine()
    {
        isAttacking = true;
        
        // 現在のコンボデータを取得
        AttackData currentData = comboAttacks[comboIndex];
        
        // アニメーション再生
        string animationName = $"Attack{comboIndex + 1}";
        animator.Play(animationName);
        
        // 攻撃方向を取得
        Vector2 direction = PlayerDataProvider.I.IsFacingRight ? Vector2.right : Vector2.left;
        
        // AttackContext作成
        var context = new AttackContext(
            currentData,
            gameObject,
            transform.position,
            direction
        );
        
        // AttackExecutorに委譲
        AttackResult result = AttackExecutor.I.ExecuteAttack(context);
        
        // 結果に応じた処理
        if (result.Success)
        {
            // ヒット時の処理（サウンド、カメラシェイクなど）
            OnAttackHit(result);
        }
        
        // コンボ進行
        comboIndex = (comboIndex + 1) % comboAttacks.Length;
        comboTimer = comboResetTime;
        
        // クールタイム設定
        cooldownTimer = currentData.Cooldown;
        
        // 攻撃モーション終了待機
        yield return new WaitForSeconds(currentData.AttackDuration);
        
        isAttacking = false;
    }
    
    public void Cancel()
    {
        StopAllCoroutines();
        isAttacking = false;
        ResetCombo();
    }
    
    private void ResetCombo()
    {
        comboIndex = 0;
        comboTimer = 0f;
    }
    
    private void OnAttackHit(AttackResult result)
    {
        // ヒット時の演出処理
        Debug.Log($"Hit {result.HitCount} targets!");
    }
}
```

### 10. IStockable (Interface)

```csharp
/// <summary>
/// ストック蓄積可能なエンティティ
/// </summary>
public interface IStockable
{
    /// <summary>現在のストック量</summary>
    int CurrentStock { get; }
    
    /// <summary>最大ストック量</summary>
    int MaxStock { get; }
    
    /// <summary>ストックを追加</summary>
    void AddStock(int amount);
    
    /// <summary>ストックをクリア</summary>
    void ClearStock();
    
    /// <summary>ストック満タン時のイベント</summary>
    event System.Action OnStockFull;
}
```

### 11. InvincibilityController (クラス)

```csharp
/// <summary>
/// 無敵時間の管理
/// </summary>
public class InvincibilityController
{
    private float invincibilityTimer = 0f;
    private float flickerTimer = 0f;
    private SpriteRenderer spriteRenderer;
    
    public bool IsInvincible => invincibilityTimer > 0;
    
    public InvincibilityController(SpriteRenderer renderer = null)
    {
        spriteRenderer = renderer;
    }
    
    /// <summary>
    /// 無敵時間を開始
    /// </summary>
    public void StartInvincibility(float duration)
    {
        invincibilityTimer = duration;
    }
    
    /// <summary>
    /// 更新処理（毎フレーム呼ぶ）
    /// </summary>
    public void Update(float deltaTime)
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= deltaTime;
            
            // 点滅演出
            if (spriteRenderer != null)
            {
                flickerTimer += deltaTime;
                if (flickerTimer >= 0.1f)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                    flickerTimer = 0f;
                }
            }
            
            // 無敵時間終了
            if (invincibilityTimer <= 0)
            {
                EndInvincibility();
            }
        }
    }
    
    /// <summary>
    /// 無敵時間を強制終了
    /// </summary>
    public void EndInvincibility()
    {
        invincibilityTimer = 0f;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
}
```

### 12. EnemyHealth (MonoBehaviour)

```csharp
/// <summary>
/// 敵のHP管理（IDamageable + IStockable実装）
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable, IStockable
{
    [Header("HP設定")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibilityDuration = 0.5f;
    
    [Header("ストック設定")]
    [SerializeField] private int maxStock = 100;
    
    [Header("参照")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private float currentHealth;
    private int currentStock = 0;
    private InvincibilityController invincibilityController;
    
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public int CurrentStock => currentStock;
    public int MaxStock => maxStock;
    
    public event System.Action OnDeath;
    public event System.Action OnStockFull;
    public event System.Action<float> OnHealthChanged;
    public event System.Action<int> OnStockChanged;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        invincibilityController = new InvincibilityController(spriteRenderer);
    }
    
    private void Update()
    {
        invincibilityController.Update(Time.deltaTime);
    }
    
    public void TakeDamage(float damage)
    {
        if (!IsAlive || invincibilityController.IsInvincible)
        {
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth);
        
        // 無敵時間開始
        invincibilityController.StartInvincibility(invincibilityDuration);
        
        // 死亡チェック
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void AddStock(int amount)
    {
        currentStock += amount;
        
        if (currentStock >= maxStock)
        {
            currentStock = maxStock;
            OnStockFull?.Invoke();
        }
        
        OnStockChanged?.Invoke(currentStock);
    }
    
    public void ClearStock()
    {
        currentStock = 0;
        OnStockChanged?.Invoke(currentStock);
    }
    
    private void Die()
    {
        OnDeath?.Invoke();
        // 死亡処理（アニメーション、オブジェクト破棄など）
        Destroy(gameObject, 0.5f);
    }
}
```

---

## エフェクトシステム設計

### 1. EffectType (Enum)

```csharp
/// <summary>
/// エフェクトの種類
/// </summary>
public enum EffectType
{
    None,
    Hit,           // 汎用ヒットエフェクト
    Slash,         // 斬撃エフェクト
    Explosion,     // 爆発エフェクト
    Shoot,         // 射撃エフェクト
    Impact,        // 衝撃エフェクト
    Blood,         // 血しぶきエフェクト
}
```

### 2. EffectData (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "EffectData", menuName = "MoreHit/EffectData")]
public class EffectData : ScriptableObject
{
    [Header("基本設定")]
    public EffectType Type;
    public GameObject Prefab;
    
    [Header("再生設定")]
    [Tooltip("エフェクトの生存時間")]
    public float Lifetime = 1f;
    
    [Tooltip("オブジェクトプーリングを使用するか")]
    public bool UsePooling = true;
    
    [Tooltip("プールに事前生成しておく数")]
    public int PrePoolCount = 5;
    
    [Tooltip("プールの最大サイズ")]
    public int MaxPoolSize = 20;
    
    private void OnValidate()
    {
        if (Prefab == null)
        {
            Debug.LogWarning($"{name}: Prefab is not assigned!");
        }
        
        if (Lifetime <= 0)
        {
            Lifetime = 1f;
        }
        
        if (PrePoolCount < 0)
        {
            PrePoolCount = 0;
        }
        
        if (MaxPoolSize < PrePoolCount)
        {
            MaxPoolSize = PrePoolCount;
        }
    }
}
```

### 3. EffectPool (クラス)

```csharp
/// <summary>
/// エフェクト専用のオブジェクトプール
/// </summary>
public class EffectPool : ObjectPool<GameObject>
{
    private GameObject prefab;
    private Transform poolParent;
    private int maxSize;
    
    public EffectPool(GameObject prefab, int initialSize = 10, int maxSize = 50)
    {
        this.prefab = prefab;
        this.maxSize = maxSize;
        
        // プール用の親オブジェクト作成
        poolParent = new GameObject($"[Pool] {prefab.name}").transform;
        
        // 事前生成
        for (int i = 0; i < initialSize; i++)
        {
            var obj = CreateNewObject();
            Return(obj);
        }
    }
    
    protected override GameObject CreateNewObject()
    {
        var obj = Object.Instantiate(prefab, poolParent);
        obj.SetActive(false);
        
        // 自動返却コンポーネントを追加
        if (!obj.TryGetComponent<AutoReturnToPool>(out var autoReturn))
        {
            autoReturn = obj.AddComponent<AutoReturnToPool>();
        }
        autoReturn.Initialize(this);
        
        return obj;
    }
    
    protected override void OnGet(GameObject obj)
    {
        obj.SetActive(true);
    }
    
    protected override void OnReturn(GameObject obj)
    {
        if (obj == null) return;
        
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }
    
    public override GameObject Get()
    {
        // プールサイズ制限チェック
        if (AvailableCount == 0 && ActiveCount >= maxSize)
        {
            Debug.LogWarning($"Pool for {prefab.name} reached max size ({maxSize})");
            return null;
        }
        
        return base.Get();
    }
    
    /// <summary>
    /// プールを完全にクリア
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        if (poolParent != null)
        {
            Object.Destroy(poolParent.gameObject);
        }
    }
}
```

### 4. AutoReturnToPool (MonoBehaviour)

```csharp
/// <summary>
/// エフェクト終了時に自動でプールに返却
/// </summary>
public class AutoReturnToPool : MonoBehaviour
{
    private EffectPool pool;
    private ParticleSystem particles;
    private float lifetime;
    private Coroutine returnCoroutine;
    
    public void Initialize(EffectPool pool)
    {
        this.pool = pool;
        particles = GetComponent<ParticleSystem>();
    }
    
    private void OnEnable()
    {
        // 既存のコルーチンを停止
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        
        // 生存時間を計算
        if (particles != null)
        {
            var main = particles.main;
            lifetime = main.duration + main.startLifetime.constantMax;
        }
        else
        {
            lifetime = 2f; // デフォルト
        }
        
        returnCoroutine = StartCoroutine(ReturnAfterDelay());
    }
    
    private void OnDisable()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }
    
    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(lifetime);
        
        if (pool != null && gameObject.activeInHierarchy)
        {
            pool.Return(gameObject);
        }
    }
    
    /// <summary>
    /// 手動で即座に返却
    /// </summary>
    public void ReturnNow()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        
        if (pool != null)
        {
            pool.Return(gameObject);
        }
    }
}
```

### 5. EffectFactory (Singleton)

```csharp
/// <summary>
/// エフェクトの生成・管理を統括
/// </summary>
public class EffectFactory : Singleton<EffectFactory>
{
    [Header("エフェクトデータ")]
    [SerializeField] private EffectData[] effectDataArray;
    
    [Header("設定")]
    [SerializeField] private Transform effectParent; // エフェクト用の親Transform
    
    private Dictionary<EffectType, EffectData> effectDataDict;
    private Dictionary<EffectType, EffectPool> pools;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeFactory();
    }
    
    private void InitializeFactory()
    {
        effectDataDict = new Dictionary<EffectType, EffectData>();
        pools = new Dictionary<EffectType, EffectPool>();
        
        // エフェクト用の親オブジェクトを作成
        if (effectParent == null)
        {
            effectParent = new GameObject("[Effects]").transform;
            effectParent.SetParent(transform);
        }
        
        // 各エフェクトデータを登録
        foreach (var data in effectDataArray)
        {
            if (data == null || data.Prefab == null)
            {
                Debug.LogWarning("EffectData or Prefab is null, skipping...");
                continue;
            }
            
            effectDataDict[data.Type] = data;
            
            // プーリング使用する場合はプールを作成
            if (data.UsePooling)
            {
                pools[data.Type] = new EffectPool(
                    data.Prefab,
                    data.PrePoolCount,
                    data.MaxPoolSize
                );
            }
        }
    }
    
    /// <summary>
    /// エフェクトを生成
    /// </summary>
    public GameObject CreateEffect(EffectType type, Vector3 position, Quaternion rotation = default)
    {
        if (!effectDataDict.TryGetValue(type, out var data))
        {
            Debug.LogWarning($"EffectData not found for type: {type}");
            return null;
        }
        
        GameObject effect;
        
        // プーリング使用
        if (data.UsePooling && pools.TryGetValue(type, out var pool))
        {
            effect = pool.Get();
            if (effect == null)
            {
                // プールから取得できなかった場合は新規生成
                effect = Instantiate(data.Prefab, position, rotation, effectParent);
                Destroy(effect, data.Lifetime);
            }
            else
            {
                effect.transform.position = position;
                effect.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            }
        }
        else
        {
            // プーリングなしの場合
            effect = Instantiate(data.Prefab, position, rotation, effectParent);
            Destroy(effect, data.Lifetime);
        }
        
        return effect;
    }
    
    /// <summary>
    /// エフェクトを生成（方向指定）
    /// </summary>
    public GameObject CreateEffect(EffectType type, Vector3 position, Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            return CreateEffect(type, position);
        }
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        return CreateEffect(type, position, rotation);
    }
    
    /// <summary>
    /// エフェクトを生成（スケール指定）
    /// </summary>
    public GameObject CreateEffect(EffectType type, Vector3 position, Vector2 direction, float scale)
    {
        var effect = CreateEffect(type, position, direction);
        if (effect != null)
        {
            effect.transform.localScale = Vector3.one * scale;
        }
        return effect;
    }
    
    /// <summary>
    /// 手動でエフェクトをプールに返却
    /// </summary>
    public void ReturnEffect(EffectType type, GameObject effect)
    {
        if (effect == null) return;
        
        if (pools.TryGetValue(type, out var pool))
        {
            pool.Return(effect);
        }
        else
        {
            Destroy(effect);
        }
    }
    
    /// <summary>
    /// 全プールをクリア（シーン遷移時など）
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }
        pools.Clear();
    }
    
    /// <summary>
    /// 特定タイプのプールをクリア
    /// </summary>
    public void ClearPool(EffectType type)
    {
        if (pools.TryGetValue(type, out var pool))
        {
            pool.Clear();
            pools.Remove(type);
        }
    }
    
    private void OnDestroy()
    {
        ClearAllPools();
    }
}
```

---

## Player → Enemy 攻撃フロー（改訂版）

```
┌─────────────────────────────────────────────────────────────┐
│ 1. 入力検出                                                   │
│ PlayerInputManager → PlayerController                        │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. 攻撃マネージャーが攻撃選択                                  │
│ AttackManager.ExecuteNormalAttack()                          │
│ - 優先度チェック                                             │
│ - CanExecute() 確認                                          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. 攻撃クラスが準備                                           │
│ NormalAttack.Execute()                                       │
│ - アニメーション再生                                          │
│ - コンボカウント更新                                          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. AttackContext 作成                                        │
│ new AttackContext(data, attacker, origin, direction)         │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. AttackExecutor に委譲                                      │
│ AttackExecutor.I.ExecuteAttack(context)                      │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. HitDetector が判定実行                                     │
│ HitDetector.DetectHits(context)                              │
│ - Physics2D.OverlapBoxAll()                                  │
│ - 攻撃者除外                                                 │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. HitProcessor が各対象に処理適用                            │
│ HitProcessor.ProcessHit(target, context)                     │
│ - 連続ヒット防止チェック                                      │
│ - ApplyDamage() → IDamageable.TakeDamage()                  │
│ - ApplyKnockback() → Rigidbody2D.AddForce()                 │
│ - ApplyStock() → IStockable.AddStock()                      │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. 敵がダメージ処理                                           │
│ EnemyHealth.TakeDamage(damage)                               │
│ - 無敵チェック (InvincibilityController)                     │
│ - HP減少                                                     │
│ - 無敵時間開始                                               │
│ - 死亡判定                                                   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 9. EffectFactory がエフェクト生成                             │
│ EffectFactory.I.CreateEffect(type, position, direction)     │
│ - プールから取得 or 新規生成                                  │
│ - 攻撃エフェクト＆ヒットエフェクト                             │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 10. 攻撃結果を返却                                            │
│ AttackResult → NormalAttack                                  │
│ - Success, HitTargets                                        │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 11. 攻撃後処理                                                │
│ NormalAttack                                                 │
│ - クールタイム設定                                           │
│ - 攻撃終了待機                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Enemy → Player 攻撃フロー（改訂版）

```
┌─────────────────────────────────────────────────────────────┐
│ 1. 敵AIが攻撃判定                                             │
│ EnemyAI → EnemyAttack.Execute()                              │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. AttackContext 作成                                        │
│ new AttackContext(enemyData, enemy, origin, direction)       │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. AttackExecutor に委譲                                      │
│ AttackExecutor.I.ExecuteAttack(context)                      │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. HitDetector が判定実行                                     │
│ HitDetector.DetectHits(context)                              │
│ - TargetLayers = Player層                                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. HitProcessor が処理適用                                    │
│ HitProcessor.ProcessHit(player, context)                     │
│ - PlayerHealth.TakeDamage()                                  │
│ - ノックバック適用                                           │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. プレイヤーがダメージ処理                                    │
│ PlayerHealth.TakeDamage(damage)                              │
│ - 無敵チェック (InvincibilityController)                     │
│ - HP減少 → 死亡判定                                          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. EffectFactory がエフェクト生成                             │
│ EffectFactory.I.CreateEffect(...)                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 責任の分離まとめ

| 責任領域 | 担当クラス | 役割 |
|---------|-----------|------|
| **入力管理** | PlayerInputManager | 入力の検出 |
| **制御** | PlayerController, EnemyAI | 入力から行動への変換 |
| **攻撃管理** | AttackManager | 攻撃の選択・優先度制御 |
| **攻撃実装** | NormalAttack等 | 個別の攻撃ロジック |
| **攻撃統括** | AttackExecutor | 判定・処理の委譲 |
| **判定検出** | HitDetector | 物理判定の実行 |
| **ヒット処理** | HitProcessor | ダメージ・ノックバック等の適用 |
| **ダメージ反応** | PlayerHealth, EnemyHealth | HP管理・死亡処理 |
| **無敵管理** | InvincibilityController | 無敵時間の制御 |
| **エフェクト生成** | EffectFactory | エフェクトの生成管理 |
| **プーリング** | EffectPool | オブジェクトの再利用 |

---

## 実装順序（推奨）

### Phase 1: コアシステム
1. **AttackData** - 攻撃パラメータ定義
2. **AttackContext, AttackResult** - データクラス
3. **HitDetector** - 判定検出
4. **HitProcessor** - ヒット処理
5. **AttackExecutor** - 統括クラス

### Phase 2: 攻撃システム
6. **IAttack** - インターフェース定義
7. **InvincibilityController** - 無敵管理
8. **AttackManager** - 攻撃管理
9. **NormalAttack** - 最初の攻撃実装

### Phase 3: エフェクトシステム
10. **EffectType, EffectData** - エフェクト定義
11. **EffectPool** - プーリング
12. **AutoReturnToPool** - 自動返却
13. **EffectFactory** - エフェクト管理

### Phase 4: 追加攻撃
14. **RangedAttack, RushAttack** - 他の攻撃
15. **EnemyAttack** - 敵の攻撃

### Phase 5: ストックシステム
16. **IStockable** - インターフェース
17. **EnemyHealth改修** - ストック実装

---

## 拡張性のポイント

### 新しい攻撃タイプの追加
1. AttackData のScriptableObjectを作成
2. IAttackを実装した新しいクラスを作成
3. AttackManagerに登録

→ 既存のコードは変更不要

### 新しいエフェクトの追加
1. EffectTypeにEnumを追加
2. EffectData のScriptableObjectを作成
3. EffectFactoryのInspectorで登録

→ コード変更は不要

### 新しいヒット時処理の追加
HitProcessor.ProcessHit() にロジック追加のみ

→ 他のクラスは影響なし

---

## デバッグ機能

### AttackExecutor のデバッグ表示
```csharp
private void OnDrawGizmos()
{
    if (!showDebugGizmos || lastContext == null) return;
    
    Gizmos.color = debugColor;
    Vector3 hitPos = lastContext.Origin + 
                     (Vector3)(lastContext.Direction * lastContext.Data.HitboxOffset.x);
    Gizmos.DrawWireCube(hitPos, lastContext.Data.HitboxSize);
}
```

### ヒット記録の可視化
HitProcessor に記録を表示する機能を追加可能

---

## パフォーマンス最適化

1. **プーリング** - 頻繁に生成されるエフェクトを再利用
2. **判定の最適化** - LayerMaskで不要な判定を除外
3. **連続ヒット防止** - 同じ対象への重複処理を削減
4. **古い記録のクリーンアップ** - メモリリークを防止

---

## まとめ

この設計により、以下を達成：

✅ **単一責任原則** - 各クラスが明確な責任を持つ
✅ **疎結合** - クラス間の依存が最小限
✅ **拡張性** - 新機能追加が容易
✅ **保守性** - コードが理解しやすく変更しやすい
✅ **パフォーマンス** - プーリングによる最適化
✅ **デバッグ性** - 問題の切り分けが容易