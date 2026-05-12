using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

/*
Boss
Boss Max HP: 320
• Slash: frontal melee attack; damage 15; parryable.
• Double Slash: two frontal slashes in sequence; damage 15 per hit; parryable.
• Smash: short-range AoE around the boss; damage 20; not parryable.
• Charge: boss locks direction, pauses, then rushes forward in a straight line; damage 20; parryable.
• Magic: long-range projectile attack used mainly when the player is far away; damage 10; not parryable.
*/


public class BossEnemy : Enemy
{
    private const int BossDrawSize = 122;

    private static readonly Rectangle[] WalkFrames = new Rectangle[]
    {
        new Rectangle(0,    0, 526, 443), // Down  walk0
        new Rectangle(555,  0, 504, 443), // Down  walk1
        new Rectangle(1059, 0, 411, 443), // Right walk0
        new Rectangle(1470, 0, 422, 443), // Right walk1
        new Rectangle(1920, 0, 324, 443), // Up    walk0
        new Rectangle(2310, 0, 329, 443), // Up    walk1
        new Rectangle(2659, 0, 426, 443), // Left  walk0
        new Rectangle(3085, 0, 428, 443), // Left  walk1
    };

    private string _pendingAttack;
    private float _decisionTimer;
    private int _doubleSlashStep;
    private Vector2 _chargeVelocity;

    public BossEnemy(Vector2 position) : base(position, "normal", 320, 20)
    {
        CollisionOffset = new Point(12, 10);
        CollisionSize = new Point(53, 74);
        _decisionTimer = 1f;
    }

    public override Projectile CreateProjectile(Player player)
    {
        if (_pendingAttack != "magic_ready")
        {
            return null;
        }

        _pendingAttack = string.Empty;
        Vector2 direction = player.Center - Center;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        return new Projectile(AssetManager.ProjectileEnemy, Center - new Vector2(16f, 16f), direction * 250f, 10, false, false);
    }

    public override void Stun(float seconds)
    {
        base.Stun(seconds);
        _pendingAttack = string.Empty;
        _doubleSlashStep = 0;
    }

    public Texture2D GetAttackTexture()
    {
        if (_pendingAttack == "smash")
        {
            return AssetManager.SmashEnemy;
        }
        if (_pendingAttack == "charge")
        {
            return AssetManager.ChargeEnemy;
        }
        return AssetManager.SlashEnemy;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (IsStunned)
        {
            spriteBatch.Draw(AssetManager.BossStunned, GetDrawRectangle(), AssetManager.GetSourceBounds(AssetManager.BossStunned), Color.White);
            return;
        }

        Texture2D sheet = AssetManager.BossWalkSheet;
        int directionIndex = 0;

        if (_facing == Direction.Right)
        {
            directionIndex = 1;
        }
        else if (_facing == Direction.Up)
        {
            directionIndex = 2;
        }
        else if (_facing == Direction.Left)
        {
            directionIndex = 3;
        }

        int walkFrame = ((int)(_animationTimer / 0.20f)) % 2;
        Rectangle source = WalkFrames[directionIndex * 2 + walkFrame];
        Rectangle drawRect = GetDrawRectangle();
        spriteBatch.Draw(sheet, drawRect, source, Color.White);

        if (_attackWindingUp && _windupTimer > 0f)
        {
            Texture2D indicator = AttackParryable ? AssetManager.BossIndicatorParryable : AssetManager.BossIndicatorUnparryable;
            spriteBatch.Draw(indicator, new Rectangle(drawRect.Center.X - 16, drawRect.Top - 36, 32, 32), Color.White);
        }
    }

    private Rectangle GetDrawRectangle()
    {
        return new Rectangle((int)Center.X - BossDrawSize / 2, (int)Center.Y - BossDrawSize / 2, BossDrawSize, BossDrawSize);
    }

    protected override void UpdateBehavior(float elapsed, Player player, RoomMap room)
    {
        FacePlayer(player);
        ReduceTimer(ref _decisionTimer, elapsed);

        if (_attackWindingUp)
        {
            if (_pendingAttack == "charge" && _windupTimer <= 0.03f)
            {
                _chargeVelocity = DirectionToVector(_facing) * 390f;
            }

            if (_pendingAttack == "slash")
            {
                TryActivateMelee(0.14f, 110, 90);
            }
            else if (_pendingAttack == "double")
            {
                TryActivateMelee(0.12f, 105, 86);
            }
            else if (_pendingAttack == "smash")
            {
                if (_windupTimer <= 0f)
                {
                    _attackWindingUp = false;
                    _activeTimer = 0.18f;
                    _recoveryTimer = 0.65f;
                    AttackBounds = new Rectangle((int)Center.X - 120, (int)Center.Y - 120, 240, 240);
                    AttackParryable = false;
                    Damage = 20;
                    AttackStartedThisFrame = true;
                }
            }
            else if (_pendingAttack == "charge")
            {
                TryActivateMelee(0.35f, 115, 90);
            }
            else if (_pendingAttack == "magic")
            {
                if (_windupTimer <= 0f)
                {
                    _attackWindingUp = false;
                    _pendingAttack = "magic_ready";
                    _recoveryTimer = 0.55f;
                }
            }

            return;
        }

        if (_activeTimer > 0f)
        {
            if (_pendingAttack == "charge")
            {
                TryMove(_chargeVelocity * elapsed, room);
            }
            return;
        }

        if (_recoveryTimer > 0f)
        {
            if (_pendingAttack == "double" && _doubleSlashStep == 1 && _recoveryTimer <= 0.22f)
            {
                _doubleSlashStep = 2;
                StartMeleeWindup(0.10f, true);
                Damage = 15;
            }
            return;
        }

        if (_decisionTimer > 0f)
        {
            MoveToward(elapsed, player, room, 62f);
            return;
        }

        ChooseAttack(player);
    }

    private void ChooseAttack(Player player)
    {
        float distance = DistanceTo(player);
        int roll = Random.Next(100);
        Damage = 15;
        _doubleSlashStep = 0;

        if (distance < 120f)
        {
            if (roll < 35)
            {
                _pendingAttack = "slash";
                StartMeleeWindup(0.30f, true);
            }
            else if (roll < 70)
            {
                _pendingAttack = "double";
                _doubleSlashStep = 1;
                StartMeleeWindup(0.30f, true);
            }
            else
            {
                _pendingAttack = "smash";
                StartMeleeWindup(0.50f, false);
            }
        }
        else if (distance < 288f)
        {
            if (roll < 55)
            {
                _pendingAttack = "slash";
                StartMeleeWindup(0.30f, true);
            }
            else
            {
                _pendingAttack = "charge";
                StartMeleeWindup(0.45f, true);
            }
        }
        else
        {
            if (roll < 55)
            {
                _pendingAttack = "charge";
                StartMeleeWindup(0.45f, true);
            }
            else
            {
                _pendingAttack = "magic";
                StartMeleeWindup(0.35f, false);
            }
        }

        _decisionTimer = 1.7f;
    }
}
