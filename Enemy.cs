using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

/*
Enemies
Normal
HP: 40
• Damage: 10

Elite
lunges in a straight line toward the player.
• HP: 60
• Damage: 20

Magic
fires projectiles at the player.
HP: 30
• Projectile Damage: 10
• Fire Interval: 1.6s

Tank
high HP and a heavy close-range hit.
HP: 80
• Damage: 20
*/

public abstract class Enemy : Entity
{
    protected const int EnemyDrawWidth = 58;
    protected const int EnemyDrawHeight = 78;

    protected int _drawWidth = EnemyDrawWidth;
    protected int _drawHeight = EnemyDrawHeight;

    protected static Random Random = new Random();
    protected float _animationTimer;
    protected float _attackTimer;
    protected float _windupTimer;
    protected float _activeTimer;
    protected float _recoveryTimer;
    protected float _stunTimer;
    protected Direction _facing;
    protected string _assetKey;
    protected bool _attackWindingUp;

    public int Hp;
    public int MaxHp;
    public int Damage;
    public bool IsDead;
    public bool AttackStartedThisFrame;
    public Rectangle AttackBounds;
    public bool AttackParryable;

    public Direction FacingDirection
    {
        get { return _facing; }
    }

    public bool IsStunned
    {
        get { return _stunTimer > 0f; }
    }

    public Enemy(Vector2 position, string assetKey, int hp, int damage)
    {
        Position = position;
        _assetKey = assetKey;
        MaxHp = hp;
        Hp = hp;
        Damage = damage;
        CollisionOffset = new Point(10, 12);
        CollisionSize = new Point(38, 52);
        _facing = Direction.Down;
    }

    public abstract Projectile CreateProjectile(Player player);

    public virtual void Update(GameTime gameTime, Player player, RoomMap room)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        AttackStartedThisFrame = false;
        _animationTimer += elapsed;
        ReduceTimer(ref _attackTimer, elapsed);
        ReduceTimer(ref _windupTimer, elapsed);
        ReduceTimer(ref _activeTimer, elapsed);
        ReduceTimer(ref _recoveryTimer, elapsed);
        ReduceTimer(ref _stunTimer, elapsed);

        if (IsDead || IsStunned)
        {
            return;
        }

        UpdateBehavior(elapsed, player, room);
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            IsDead = true;
        }
    }

    public virtual void Stun(float seconds)
    {
        _stunTimer = seconds;
        _windupTimer = 0f;
        _attackWindingUp = false;
        _activeTimer = 0f;
        _recoveryTimer = 0f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (IsStunned && AssetManager.EnemyStunnedTextures.ContainsKey(_assetKey))
        {
            spriteBatch.Draw(AssetManager.EnemyStunnedTextures[_assetKey], new Rectangle((int)Position.X, (int)Position.Y, _drawWidth, _drawHeight), Color.White);
            return;
        }

        Texture2D sheet = AssetManager.EnemyWalkSheets[_assetKey];
        int frameWidth = sheet.Width / 4;
        int frameHeight = sheet.Height;
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

        int walkFrame = ((int)(_animationTimer / 0.22f)) % 2;
        Rectangle source = new Rectangle(directionIndex * frameWidth + walkFrame * (frameWidth / 2), 0, frameWidth / 2, frameHeight);
        Rectangle dest = new Rectangle((int)Position.X, (int)Position.Y, _drawWidth, _drawHeight);
        spriteBatch.Draw(sheet, dest, source, Color.White);
    }

    protected abstract void UpdateBehavior(float elapsed, Player player, RoomMap room);

    protected void MoveToward(float elapsed, Player player, RoomMap room, float speed)
    {
        Vector2 direction = player.Center - Center;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
            _facing = VectorToDirection(direction);
        }

        TryMove(new Vector2(direction.X * speed * elapsed, 0f), room);
        TryMove(new Vector2(0f, direction.Y * speed * elapsed), room);
    }

    protected void TryMove(Vector2 delta, RoomMap room)
    {
        Vector2 old = Position;
        Position += delta;
        if (!room.IsRectangleWalkable(CollisionBounds))
        {
            Position = old;
        }
    }

    protected Rectangle GetFrontalAttackBounds(int length, int width)
    {
        Rectangle body = CollisionBounds;
        if (_facing == Direction.Up)
        {
            return new Rectangle(body.Center.X - width / 2, body.Top - length, width, length);
        }

        if (_facing == Direction.Down)
        {
            return new Rectangle(body.Center.X - width / 2, body.Bottom, width, length);
        }

        if (_facing == Direction.Left)
        {
            return new Rectangle(body.Left - length, body.Center.Y - width / 2, length, width);
        }

        return new Rectangle(body.Right, body.Center.Y - width / 2, length, width);
    }

    protected float DistanceTo(Player player)
    {
        return Vector2.Distance(Center, player.Center);
    }

    protected void StartMeleeWindup(float windup, bool parryable)
    {
        _windupTimer = windup;
        AttackParryable = parryable;
        _attackWindingUp = true;
    }

    protected void TryActivateMelee(float activeTime, int length, int width)
    {
        if (_attackWindingUp && _windupTimer <= 0f && _activeTimer <= 0f && _recoveryTimer <= 0f)
        {
            _attackWindingUp = false;
            _activeTimer = activeTime;
            _recoveryTimer = 0.45f;
            AttackBounds = GetFrontalAttackBounds(length, width);
            AttackStartedThisFrame = true;
        }
    }

    protected void FacePlayer(Player player)
    {
        Vector2 direction = player.Center - Center;
        if (direction != Vector2.Zero)
        {
            _facing = VectorToDirection(direction);
        }
    }

    protected void ReduceTimer(ref float timer, float elapsed)
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

public class NormalEnemy : Enemy
{
    public NormalEnemy(Vector2 position) : base(position, "normal", 40, 10)
    {
        _attackTimer = 0.6f;
    }

    public override Projectile CreateProjectile(Player player)
    {
        return null;
    }

    protected override void UpdateBehavior(float elapsed, Player player, RoomMap room)
    {
        FacePlayer(player);
        if (_attackWindingUp)
        {
            TryActivateMelee(0.12f, 53, 43);
            return;
        }

        if (_activeTimer > 0f || _recoveryTimer > 0f)
        {
            return;
        }

        if (DistanceTo(player) < 75f && _attackTimer <= 0f)
        {
            StartMeleeWindup(0.30f, true);
            _attackTimer = 1.95f;
            return;
        }

        MoveToward(elapsed, player, room, 95f);
    }
}

public class EliteEnemy : Enemy
{
    private Vector2 _lungeVelocity;

    public EliteEnemy(Vector2 position) : base(position, "elite", 60, 15)
    {
        _drawWidth = 80;
        _drawHeight = 108;
        _attackTimer = 1f;
    }

    public override Projectile CreateProjectile(Player player)
    {
        return null;
    }

    protected override void UpdateBehavior(float elapsed, Player player, RoomMap room)
    {
        FacePlayer(player);
        if (_attackWindingUp)
        {
            if (_windupTimer <= 0.02f)
            {
                _lungeVelocity = DirectionToVector(_facing) * 310f;
            }
            TryActivateMelee(0.18f, 65, 48);
            return;
        }

        if (_activeTimer > 0f)
        {
            TryMove(_lungeVelocity * elapsed, room);
            return;
        }

        if (_recoveryTimer > 0f)
        {
            return;
        }

        if (DistanceTo(player) < 192f && _attackTimer <= 0f)
        {
            StartMeleeWindup(0.40f, true);
            _attackTimer = 2.55f;
            return;
        }

        MoveToward(elapsed, player, room, 75f);
    }
}

public class MagicEnemy : Enemy
{
    private float _fireTimer;

    public MagicEnemy(Vector2 position) : base(position, "magic", 30, 10)
    {
        _fireTimer = 0.9f;
    }

    public override Projectile CreateProjectile(Player player)
    {
        if (_fireTimer > 0f || IsDead || IsStunned)
        {
            return null;
        }

        _fireTimer = 3.2f;
        Vector2 direction = player.Center - Center;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }
        _facing = VectorToDirection(direction);

        return new Projectile(AssetManager.ProjectileEnemy, Center - new Vector2(16f, 16f), direction * 210f, Damage, false, false, 32, 3.75f);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (IsStunned && AssetManager.EnemyStunnedTextures.ContainsKey(_assetKey))
        {
            spriteBatch.Draw(AssetManager.EnemyStunnedTextures[_assetKey], new Rectangle((int)Position.X, (int)Position.Y, _drawWidth, _drawHeight), Color.White);
            return;
        }

        Texture2D sheet = AssetManager.EnemyWalkSheets[_assetKey];
        int frameWidth = sheet.Width / 4;
        int frameHeight = sheet.Height;
        int directionIndex = 0;

        if (_facing == Direction.Right)
        {
            directionIndex = 3;
        }
        else if (_facing == Direction.Up)
        {
            directionIndex = 2;
        }
        else if (_facing == Direction.Left)
        {
            directionIndex = 1;
        }

        int walkFrame = ((int)(_animationTimer / 0.22f)) % 2;
        Rectangle source = new Rectangle(directionIndex * frameWidth + walkFrame * (frameWidth / 2), 0, frameWidth / 2, frameHeight);
        Rectangle dest = new Rectangle((int)Position.X, (int)Position.Y, _drawWidth, _drawHeight);
        spriteBatch.Draw(sheet, dest, source, Color.White);
    }

    protected override void UpdateBehavior(float elapsed, Player player, RoomMap room)
    {
        ReduceTimer(ref _fireTimer, elapsed);
        FacePlayer(player);
        float distance = DistanceTo(player);
        if (distance < 190f)
        {
            Vector2 away = Center - player.Center;
            if (away != Vector2.Zero)
            {
                away.Normalize();
                TryMove(away * elapsed * 80f, room);
            }
        }
    }
}

public class TankEnemy : Enemy
{
    public TankEnemy(Vector2 position) : base(position, "tank", 80, 15)
    {
        _drawWidth = 87;
        _drawHeight = 117;
        CollisionOffset = new Point(7, 10);
        CollisionSize = new Point(43, 58);
        _attackTimer = 1.1f;
    }

    public override Projectile CreateProjectile(Player player)
    {
        return null;
    }

    protected override void UpdateBehavior(float elapsed, Player player, RoomMap room)
    {
        FacePlayer(player);
        if (_attackWindingUp)
        {
            TryActivateMelee(0.16f, 67, 62);
            return;
        }

        if (_activeTimer > 0f || _recoveryTimer > 0f)
        {
            return;
        }

        if (DistanceTo(player) < 84f && _attackTimer <= 0f)
        {
            StartMeleeWindup(0.45f, true);
            _attackTimer = 2.4f;
            return;
        }

        MoveToward(elapsed, player, room, 55f);
    }
}
