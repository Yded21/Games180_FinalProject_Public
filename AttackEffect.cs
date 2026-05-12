using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public class AttackEffect
{
    private readonly Texture2D _texture;
    private readonly Direction _direction;
    private readonly float _duration;
    private Vector2 _position;

    public Rectangle Bounds;
    public bool PlayerOwned;
    public int Damage;
    public bool Parryable;
    public bool HitOnce;
    public bool HitAllOnce;
    public bool DamageDealt;
    public bool AllDirections;
    public bool ExplodesOnHit;
    public bool StopOnWall;
    public Vector2 Velocity;
    public float Timer;
    public bool Removed;

    public AttackEffect(Texture2D texture, Rectangle bounds, Direction direction, int damage, bool playerOwned, bool parryable, float duration)
    {
        _texture = texture;
        Bounds = bounds;
        _direction = direction;
        Damage = damage;
        PlayerOwned = playerOwned;
        Parryable = parryable;
        _duration = duration;
        Timer = duration;
        HitOnce = true;
        _position = new Vector2(bounds.X, bounds.Y);
    }

    public void Update(float elapsedSeconds, RoomMap room)
    {
        if (Velocity != Vector2.Zero)
        {
            _position += Velocity * elapsedSeconds;
            Bounds = new Rectangle((int)_position.X, (int)_position.Y, Bounds.Width, Bounds.Height);
            if (StopOnWall && !room.IsRectangleWalkable(Bounds))
            {
                Removed = true;
            }
        }

        Timer -= elapsedSeconds;
        if (Timer <= 0f)
        {
            Removed = true;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 center = new Vector2(Bounds.Center.X, Bounds.Center.Y);
        Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
        Vector2 scale = new Vector2((float)Bounds.Width / _texture.Width, (float)Bounds.Height / _texture.Height);

        if (AllDirections)
        {
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(_texture, center, null, Color.White * 0.85f, MathHelper.PiOver2 * i, origin, scale, SpriteEffects.None, 0f);
            }
            return;
        }

        float rotation = 0f;
        SpriteEffects effects = SpriteEffects.None;

        if (_direction == Direction.Down)
        {
            rotation = MathHelper.PiOver2;
        }
        else if (_direction == Direction.Up)
        {
            rotation = -MathHelper.PiOver2;
        }
        else if (_direction == Direction.Left)
        {
            effects = SpriteEffects.FlipHorizontally;
        }

        spriteBatch.Draw(_texture, center, null, Color.White * 0.85f, rotation, origin, scale, effects, 0f);
    }
}
