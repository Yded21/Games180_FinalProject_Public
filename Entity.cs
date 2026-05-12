using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public abstract class Entity
{
    public Vector2 Position;
    public Point CollisionOffset;
    public Point CollisionSize;
    public bool Removed;

    public Rectangle CollisionBounds
    {
        get
        {
            return new Rectangle((int)Position.X + CollisionOffset.X, (int)Position.Y + CollisionOffset.Y, CollisionSize.X, CollisionSize.Y);
        }
    }

    public Vector2 Center
    {
        get
        {
            Rectangle bounds = CollisionBounds;
            return new Vector2(bounds.Center.X, bounds.Center.Y);
        }
    }

    protected Vector2 DirectionToVector(Direction direction)
    {
        if (direction == Direction.Up)
        {
            return new Vector2(0f, -1f);
        }

        if (direction == Direction.Down)
        {
            return new Vector2(0f, 1f);
        }

        if (direction == Direction.Left)
        {
            return new Vector2(-1f, 0f);
        }

        return new Vector2(1f, 0f);
    }

    protected Direction VectorToDirection(Vector2 vector)
    {
        if (System.Math.Abs(vector.X) > System.Math.Abs(vector.Y))
        {
            if (vector.X < 0f)
            {
                return Direction.Left;
            }

            return Direction.Right;
        }

        if (vector.Y < 0f)
        {
            return Direction.Up;
        }

        return Direction.Down;
    }

    public abstract void Draw(SpriteBatch spriteBatch);
}
