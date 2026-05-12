using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public class Projectile : Entity
{
    private readonly Texture2D _texture;
    private readonly bool _playerOwned;
    private readonly bool _piercing;
    private readonly int _drawSize;
    private float _lifeTimer;

    public Vector2 Velocity;
    public int Damage;
    public bool PlayerOwned
    {
        get { return _playerOwned; }
    }

    public bool Piercing
    {
        get { return _piercing; }
    }

    public Projectile(Texture2D texture, Vector2 position, Vector2 velocity, int damage, bool playerOwned, bool piercing, int drawSize = 32, float lifeTime = 2.5f)
    {
        _texture = texture;
        _playerOwned = playerOwned;
        _piercing = piercing;
        _drawSize = drawSize;
        Position = position;
        Velocity = velocity;
        Damage = damage;
        int halfSize = drawSize / 4;
        CollisionOffset = new Point(halfSize, halfSize);
        CollisionSize = new Point(drawSize / 2, drawSize / 2);
        _lifeTimer = lifeTime;
    }

    public void Update(float elapsedSeconds, RoomMap room)
    {
        Position += Velocity * elapsedSeconds;
        _lifeTimer -= elapsedSeconds;

        if (_lifeTimer <= 0f)
        {
            Removed = true;
        }

        if (!room.IsRectangleWalkable(CollisionBounds))
        {
            Removed = true;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, new Rectangle((int)Position.X, (int)Position.Y, _drawSize, _drawSize), Color.White);
    }
}
