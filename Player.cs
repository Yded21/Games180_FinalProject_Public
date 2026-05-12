using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Games180_FinalProject_EddyZhang;

/*
Upgrades: 
General: 
• 1. Battle Instinct - no longer take knockback while attacking.
• 2. Tactical Mastery - -25% skill cooldown.
• 3. Berserker - +20% attack speed.
• 4. Master of Defense - after a successful parry, reduce the next parry cooldown by 35%.
• 5. Iron Resolve - -20% damage received while attacking.
• 6. Distant Strike - normal attacks fire a small forward projectile.
• 7. Vital Force - +20 max HP.
• 8. Sharpened Edge - +15 attack damage.
• 9. Fleet Foot - +10% movement speed.
• 10. Steady Hand - -15% dash cooldown.

Forms:: 
Knight Form (default)
• +20% max HP and gain Parry. Knight HP = 120.
• Parry behavior: blocks a melee attack during the active parry window.
• Parry Cd: 2.0s
Hero Form
• -30% attack speed, +50% attack damage, replace Parry with Master Strike.
• Master Strike: a successful parry triggers a stronger counter slash - 200% of normal attack damage; 
• cd = 7.0s.

Phantom Blade - +25% attack range
Omnistrike - Master Strike counter hits in a circular area
Heavy Blade - if the player has not attacked for 2.0s, the next normal attack deals +30% damage

Divine Form
• +100% attack range, replace Parry with Stellar Blade.
• Stellar Blade: launch a piercing slash straight forward.
• cd = 2.5s.

Divine Protection - +20% movement speed for 1.0s after using Stellar Blade
Star Explosion - Stellar Blade explodes on impact, dealing small AoE damage
Holy Reach - +25% Stellar Blade size and attack range

Demon Form
• +25% attack speed, -35% attack range, replace Parry with Blood Daggers.
• Blood Daggers: for 5.0s, lose 20% current HP on activation, each successful hit heals 10 HP, gain an additional
+15% attack speed during effect.
• cd: 15.0s.

Killing Spree - +5% attack damage per enemy killed in the current encounter, max +25%, resets
after encounter
Mayhem - -35% dash cooldown
Storm of Blood - activating Blood Daggers clears all enemy projectiles currently on screen
*/


public class Player : Entity
{
    private const int DrawWidth = 58;
    private const int DrawHeight = 78;
    private const float BaseMoveSpeed = 190f;
    private const float BaseAttackCooldown = 0.45f;
    private const float AttackActiveTime = 0.12f;
    private const float DashDuration = 0.18f;
    private const float DashIFrames = 0.12f;
    private const float ParryDuration = 0.48f;
    private const float ParryActiveStart = 0.24f;
    private const float ParryActiveEnd = 0.42f;

    private float _walkTimer;
    private float _attackCooldownTimer;
    private float _attackActiveTimer;
    private float _dashTimer;
    private float _dashInvulnerableTimer;
    private float _dashCooldownTimer;
    private float _skillCooldownTimer;
    private float _parryTimer;
    private float _bloodDaggersTimer;
    private float _divineSpeedTimer;
    private float _timeSinceAttack;
    private float _damageInvulnerableTimer;
    private Vector2 _knockbackVelocity;
    private float _knockbackTimer;

    public int CurrentHp;
    public int MaxHp;
    public int BaseDamage;
    public PlayerForm Form;
    public bool HasForm;
    public Direction Facing;
    public bool IsMoving;
    public bool IsDead;
    public bool AttackStartedThisFrame;
    public bool SkillStartedThisFrame;
    public bool DashStartedThisFrame;
    public List<string> AcquiredUpgrades;
    public int EncounterKillCount;

    public float DashCooldownMax;
    public float SkillCooldownMax;
    public float AttackRangeMultiplier;
    public float AttackCooldownMultiplier;
    public float DamageMultiplier;
    public float DamageTakenMultiplier;
    public float MoveSpeedMultiplier;

    public bool BattleInstinct;
    public bool IronResolve;
    public bool DistantStrike;
    public bool MasterDefense;
    public bool PhantomBlade;
    public bool Omnistrike;
    public bool HeavyBlade;
    public bool DivineProtection;
    public bool StarExplosion;
    public bool HolyReach;
    public bool KillingSpree;
    public bool StormOfBlood;

    public float DashCooldownRatio
    {
        get { return _dashCooldownTimer / DashCooldownMax; }
    }

    public float SkillCooldownRatio
    {
        get
        {
            if (SkillCooldownMax <= 0f)
            {
                return 0f;
            }

            return _skillCooldownTimer / SkillCooldownMax;
        }
    }

    public bool IsAttacking
    {
        get { return _attackActiveTimer > 0f; }
    }

    public bool IsBloodDaggersActive
    {
        get { return _bloodDaggersTimer > 0f; }
    }

    public bool IsDashing
    {
        get { return _dashTimer > 0f; }
    }

    public bool IsInvulnerable
    {
        get { return _dashInvulnerableTimer > 0f || _damageInvulnerableTimer > 0f; }
    }

    public bool IsParryActive
    {
        get { return _parryTimer > ParryActiveStart && _parryTimer <= ParryActiveEnd; }
    }

    public Player(Vector2 position)
    {
        Position = position;
        CollisionOffset = new Point(12, 16);
        CollisionSize = new Point(34, 50);
        Facing = Direction.Down;
        AcquiredUpgrades = new List<string>();
        ResetForNewRun();
    }

    public void ResetForNewRun()
    {
        Form = PlayerForm.Knight;
        HasForm = false;
        MaxHp = 120;
        CurrentHp = MaxHp;
        BaseDamage = 20;
        EncounterKillCount = 0;
        AcquiredUpgrades.Clear();
        RecalculateStats();
    }

    public void Update(GameTime gameTime, Input input, RoomMap room)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        AttackStartedThisFrame = false;
        SkillStartedThisFrame = false;
        DashStartedThisFrame = false;
        _timeSinceAttack += elapsed;

        UpdateTimers(elapsed);

        if (IsDead)
        {
            IsMoving = false;
            return;
        }

        Vector2 move = input.GetMoveVector();
        if (move != Vector2.Zero && !IsDashing)
        {
            Facing = VectorToDirection(move);
        }

        if (input.IsKeyPressed(Keys.LeftShift) || input.IsKeyPressed(Keys.RightShift))
        {
            TryDash();
        }

        if (input.IsKeyPressed(Keys.Space))
        {
            TryAttack();
        }

        if (input.IsKeyPressed(Keys.F))
        {
            TrySkill();
        }

        Move(elapsed, move, room);
    }

    public AttackEffect CreateAttackEffect()
    {
        int length = (int)(70f * AttackRangeMultiplier);
        int width = 60;
        Rectangle body = CollisionBounds;
        Rectangle bounds;

        if (Facing == Direction.Up)
        {
            bounds = new Rectangle(body.Center.X - width / 2, body.Top - length, width, length);
        }
        else if (Facing == Direction.Down)
        {
            bounds = new Rectangle(body.Center.X - width / 2, body.Bottom, width, length);
        }
        else if (Facing == Direction.Left)
        {
            bounds = new Rectangle(body.Left - length, body.Center.Y - width / 2, length, width);
        }
        else
        {
            bounds = new Rectangle(body.Right, body.Center.Y - width / 2, length, width);
        }

        int damage = GetAttackDamage();
        return new AttackEffect(AssetManager.PlayerSlashes[Form], bounds, Facing, damage, true, false, AttackActiveTime);
    }

    public Projectile CreateDistantStrikeProjectile()
    {
        Vector2 dir = DirectionToVector(Facing);
        Vector2 start = Center - new Vector2(16f, 16f) + dir * 28f;
        return new Projectile(AssetManager.PlayerProjectiles[Form], start, dir * 330f, BaseDamage / 2, true, false);
    }

    public Projectile CreateSkillProjectile()
    {
        Vector2 dir = DirectionToVector(Facing);
        Vector2 start = Center - new Vector2(18f, 18f) + dir * 28f;
        int damage = GetAttackDamage();
        float speed = 420f;
        int drawSize = 32;
        if (HolyReach)
        {
            speed *= 1.25f;
            drawSize = 40;
        }
        return new Projectile(AssetManager.PlayerProjectiles[Form], start, dir * speed, damage, true, true, drawSize);
    }

    public AttackEffect CreateDivineSkillSlash()
    {
        int length = 150;
        int width = 90;
        if (HolyReach)
        {
            length = (int)(length * 1.25f);
            width = (int)(width * 1.25f);
        }

        Rectangle body = CollisionBounds;
        Rectangle bounds;

        if (Facing == Direction.Up)
        {
            bounds = new Rectangle(body.Center.X - width / 2, body.Top - length, width, length);
        }
        else if (Facing == Direction.Down)
        {
            bounds = new Rectangle(body.Center.X - width / 2, body.Bottom, width, length);
        }
        else if (Facing == Direction.Left)
        {
            bounds = new Rectangle(body.Left - length, body.Center.Y - width / 2, length, width);
        }
        else
        {
            bounds = new Rectangle(body.Right, body.Center.Y - width / 2, length, width);
        }

        AttackEffect slash = new AttackEffect(AssetManager.PlayerSlashes[PlayerForm.Divine], bounds, Facing, GetAttackDamage(), true, false, 0.45f);
        slash.Velocity = DirectionToVector(Facing) * 420f;
        slash.StopOnWall = true;
        slash.ExplodesOnHit = true;
        return slash;
    }

    public AttackEffect CreateMasterStrike()
    {
        int damage = (int)(GetAttackDamage() * 2.0f);
        Rectangle bounds;

        if (Omnistrike)
        {
            int size = 180;
            bounds = new Rectangle((int)Center.X - size / 2, (int)Center.Y - size / 2, size, size);
            AttackEffect circular = new AttackEffect(AssetManager.PlayerSlashes[Form], bounds, Facing, damage, true, false, 0.18f);
            circular.HitAllOnce = true;
            circular.AllDirections = true;
            return circular;
        }

        int length = 100;
        int width = 70;
        Rectangle body = CollisionBounds;
        if (Facing == Direction.Up)
        {
            bounds = new Rectangle(body.Center.X - width / 2, body.Top - length, width, length);
        }
        else if (Facing == Direction.Down)
        {
            bounds = new Rectangle(body.Center.X - width / 2, body.Bottom, width, length);
        }
        else if (Facing == Direction.Left)
        {
            bounds = new Rectangle(body.Left - length, body.Center.Y - width / 2, length, width);
        }
        else
        {
            bounds = new Rectangle(body.Right, body.Center.Y - width / 2, length, width);
        }
        return new AttackEffect(AssetManager.PlayerSlashes[Form], bounds, Facing, damage, true, false, 0.18f);
    }

    public void ApplyKnockback(Vector2 direction)
    {
        if (BattleInstinct && IsAttacking)
        {
            return;
        }
        _knockbackVelocity = direction * 160f;
        _knockbackTimer = 0.18f;
    }

    public void TakeDamage(int damage)
    {
        if (IsInvulnerable || IsDead)
        {
            return;
        }

        float finalDamage = damage * DamageTakenMultiplier;
        if (IronResolve && IsAttacking)
        {
            finalDamage *= 0.8f;
        }

        CurrentHp -= (int)finalDamage;
        _damageInvulnerableTimer = 0.55f;
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            IsDead = true;
        }
    }

    public void HealMissing(float ratio)
    {
        int missing = MaxHp - CurrentHp;
        CurrentHp += (int)(missing * ratio);
        if (CurrentHp > MaxHp)
        {
            CurrentHp = MaxHp;
        }
    }

    public void ApplyUpgrade(UpgradeDefinition upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        if (!AcquiredUpgrades.Contains(upgrade.Id))
        {
            AcquiredUpgrades.Add(upgrade.Id);
        }

        if (upgrade.Id == "vital_force")
        {
            int hpGain = (int)(MaxHp * 0.3f);
            MaxHp += hpGain;
            CurrentHp += hpGain;
        }
        else if (upgrade.Id == "sharpened_edge")
        {
            BaseDamage += 15;
        }

        RecalculateStats();
    }

    public void SetForm(PlayerForm form)
    {
        Form = form;
        HasForm = true;
        RecalculateStats();
    }

    public void RestoreForm(PlayerForm form, bool hasForm)
    {
        Form = form;
        HasForm = hasForm;
        RecalculateStats();
    }

    public void OnEnemyKilled()
    {
        EncounterKillCount++;
    }

    public void OnSuccessfulParry()
    {
        if (MasterDefense)
        {
            _skillCooldownTimer = SkillCooldownMax * 0.4f;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D sheet = AssetManager.PlayerWalkSheets[Form];
        int frameWidth = sheet.Width / 4;
        int frameHeight = sheet.Height;
        int directionIndex = 0;

        if (Facing == Direction.Right)
        {
            directionIndex = Form == PlayerForm.Divine ? 1 : 3;
        }
        else if (Facing == Direction.Up)
        {
            directionIndex = 2;
        }
        else if (Facing == Direction.Left)
        {
            directionIndex = Form == PlayerForm.Divine ? 3 : 1;
        }

        int walkFrame = 0;
        if (IsMoving)
        {
            walkFrame = ((int)(_walkTimer / 0.16f)) % 2;
        }

        Rectangle source = new Rectangle(directionIndex * frameWidth + walkFrame * (frameWidth / 2), 0, frameWidth / 2, frameHeight);
        Rectangle destination = new Rectangle((int)Position.X, (int)Position.Y, DrawWidth, DrawHeight);
        Color color = Color.White;
        if (_damageInvulnerableTimer > 0f)
        {
            color = Color.White * 0.55f;
        }
        else if (!HasForm)
        {
            color = Color.White * 0.7f;
        }

        spriteBatch.Draw(sheet, destination, source, color);

        if (IsParryActive)
        {
            UiHelper.DrawRectangleOutline(spriteBatch, new Rectangle(destination.X - 4, destination.Y - 4, destination.Width + 8, destination.Height + 8), new Color(100, 180, 255), 3);
        }
    }

    private void UpdateTimers(float elapsed)
    {
        bool bloodDaggersWasActive = _bloodDaggersTimer > 0f;

        ReduceTimer(ref _attackCooldownTimer, elapsed);
        ReduceTimer(ref _attackActiveTimer, elapsed);
        ReduceTimer(ref _dashTimer, elapsed);
        ReduceTimer(ref _dashInvulnerableTimer, elapsed);
        ReduceTimer(ref _dashCooldownTimer, elapsed);
        ReduceTimer(ref _skillCooldownTimer, elapsed);
        ReduceTimer(ref _parryTimer, elapsed);
        ReduceTimer(ref _bloodDaggersTimer, elapsed);
        ReduceTimer(ref _divineSpeedTimer, elapsed);
        ReduceTimer(ref _damageInvulnerableTimer, elapsed);
        ReduceTimer(ref _knockbackTimer, elapsed);

        if (bloodDaggersWasActive && _bloodDaggersTimer <= 0f)
        {
            RecalculateStats();
        }
    }

    private void Move(float elapsed, Vector2 inputMove, RoomMap room)
    {
        if (_knockbackTimer > 0f)
        {
            IsMoving = false;
            TryMoveAxis(new Vector2(_knockbackVelocity.X * elapsed, 0f), room);
            TryMoveAxis(new Vector2(0f, _knockbackVelocity.Y * elapsed), room);
            return;
        }

        Vector2 move = inputMove;
        float speed = BaseMoveSpeed * MoveSpeedMultiplier;

        if (_divineSpeedTimer > 0f)
        {
            speed *= 1.2f;
        }

        if (IsDashing)
        {
            move = DirectionToVector(Facing);
            speed = 470f;
        }

        IsMoving = move != Vector2.Zero;
        if (IsMoving)
        {
            _walkTimer += elapsed;
        }
        else
        {
            _walkTimer = 0f;
        }

        TryMoveAxis(new Vector2(move.X * speed * elapsed, 0f), room);
        TryMoveAxis(new Vector2(0f, move.Y * speed * elapsed), room);
    }

    private void TryMoveAxis(Vector2 delta, RoomMap room)
    {
        if (delta == Vector2.Zero)
        {
            return;
        }

        Vector2 oldPosition = Position;
        Position += delta;
        if (!room.IsRectangleWalkable(CollisionBounds))
        {
            Position = oldPosition;
        }
    }

    private void TryAttack()
    {
        if (!HasForm)
        {
            return;
        }

        if (_attackCooldownTimer > 0f)
        {
            return;
        }

        _attackCooldownTimer = BaseAttackCooldown * AttackCooldownMultiplier;
        _attackActiveTimer = AttackActiveTime;
        _timeSinceAttack = 0f;
        AttackStartedThisFrame = true;
    }

    private void TryDash()
    {
        if (_dashCooldownTimer > 0f)
        {
            return;
        }

        _dashTimer = DashDuration;
        _dashInvulnerableTimer = DashIFrames;
        _dashCooldownTimer = DashCooldownMax;
        DashStartedThisFrame = true;
    }

    private void TrySkill()
    {
        if (!HasForm)
        {
            return;
        }

        if (_skillCooldownTimer > 0f)
        {
            return;
        }

        if (Form == PlayerForm.Knight || Form == PlayerForm.Hero)
        {
            _parryTimer = ParryDuration;
            _skillCooldownTimer = SkillCooldownMax;
            SkillStartedThisFrame = true;
        }
        else if (Form == PlayerForm.Divine)
        {
            _skillCooldownTimer = SkillCooldownMax;
            if (DivineProtection)
            {
                _divineSpeedTimer = 1f;
            }
            SkillStartedThisFrame = true;
        }
        else if (Form == PlayerForm.Demon)
        {
            int cost = (int)(CurrentHp * 0.2f);
            if (cost < 1)
            {
                cost = 1;
            }

            CurrentHp -= cost;
            if (CurrentHp < 1)
            {
                CurrentHp = 1;
            }

            _bloodDaggersTimer = 5f;
            _skillCooldownTimer = SkillCooldownMax;
            SkillStartedThisFrame = true;
            RecalculateStats();
        }
    }

    public int GetAttackDamage()
    {
        float damage = BaseDamage * DamageMultiplier;
        if (HasForm && Form == PlayerForm.Hero)
        {
            damage *= 1.5f;
        }

        if (KillingSpree)
        {
            int cappedKills = EncounterKillCount;
            if (cappedKills > 4)
            {
                cappedKills = 4;
            }

            damage *= 1f + cappedKills * 0.1f;
        }

        if (HeavyBlade && _timeSinceAttack >= 2f)
        {
            damage *= 1.3f;
        }

        return (int)damage;
    }

    private void RecalculateStats()
    {
        AttackRangeMultiplier = 1f;
        AttackCooldownMultiplier = 1f;
        DamageMultiplier = 1f;
        DamageTakenMultiplier = 1f;
        MoveSpeedMultiplier = 1f;
        DashCooldownMax = 1.2f;
        SkillCooldownMax = 2f;

        BattleInstinct = AcquiredUpgrades.Contains("battle_instinct");
        IronResolve = AcquiredUpgrades.Contains("iron_resolve");
        DistantStrike = AcquiredUpgrades.Contains("distant_strike");
        MasterDefense = AcquiredUpgrades.Contains("master_defense");
        PhantomBlade = AcquiredUpgrades.Contains("phantom_blade");
        Omnistrike = AcquiredUpgrades.Contains("omnistrike");
        HeavyBlade = AcquiredUpgrades.Contains("heavy_blade");
        DivineProtection = AcquiredUpgrades.Contains("divine_protection");
        StarExplosion = AcquiredUpgrades.Contains("star_explosion");
        HolyReach = AcquiredUpgrades.Contains("holy_reach");
        KillingSpree = AcquiredUpgrades.Contains("killing_spree");
        StormOfBlood = AcquiredUpgrades.Contains("storm_blood");

        if (AcquiredUpgrades.Contains("berserker"))
        {
            AttackCooldownMultiplier *= 0.8f;
        }
        if (AcquiredUpgrades.Contains("fleet_foot"))
        {
            MoveSpeedMultiplier *= 1.1f;
        }
        if (AcquiredUpgrades.Contains("steady_hand"))
        {
            DashCooldownMax *= 0.7f;
        }
        if (AcquiredUpgrades.Contains("event_shield"))
        {
            DamageTakenMultiplier *= 0.9f;
        }
        if (AcquiredUpgrades.Contains("event_fury"))
        {
            AttackCooldownMultiplier *= 0.85f;
        }
        if (HasForm && Form == PlayerForm.Hero)
        {
            AttackCooldownMultiplier *= 1.43f;
            SkillCooldownMax = 7f;
        }
        else if (HasForm && Form == PlayerForm.Divine)
        {
            AttackRangeMultiplier *= 2f;
            SkillCooldownMax = 2.5f;
        }
        else if (HasForm && Form == PlayerForm.Demon)
        {
            AttackCooldownMultiplier *= 1f / 1.4f;
            AttackRangeMultiplier *= 0.65f;
            SkillCooldownMax = 15f;
        }

        if (AcquiredUpgrades.Contains("tactical_mastery"))
        {
            SkillCooldownMax *= 0.75f;
        }
        if (AcquiredUpgrades.Contains("event_curse"))
        {
            SkillCooldownMax *= 0.8f;
        }
        if (AcquiredUpgrades.Contains("royal_resolve"))
        {
            SkillCooldownMax *= 0.8f;
        }

        if (PhantomBlade)
        {
            AttackRangeMultiplier *= 1.25f;
        }
        if (HolyReach)
        {
            AttackRangeMultiplier *= 1.25f;
        }
        if (AcquiredUpgrades.Contains("mayhem"))
        {
            DashCooldownMax *= 0.65f;
        }
        if (_bloodDaggersTimer > 0f)
        {
            AttackCooldownMultiplier *= 0.85f;
        }
    }

    private void ReduceTimer(ref float timer, float elapsed)
    {
        if (timer > 0f)
        {
            timer -= elapsed;
            if (timer < 0f)
            {
                timer = 0f;
            }
        }
    }
}
