using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Games180_FinalProject_EddyZhang;

public class Input
{
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public void Update()
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
    }

    public bool IsKeyDown(Keys key)
    {
        return _currentState.IsKeyDown(key);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);
    }

    public Vector2 GetMoveVector()
    {
        Vector2 move = Vector2.Zero;

        if (IsKeyDown(Keys.A) || IsKeyDown(Keys.Left))
        {
            move.X -= 1f;
        }

        if (IsKeyDown(Keys.D) || IsKeyDown(Keys.Right))
        {
            move.X += 1f;
        }

        if (IsKeyDown(Keys.W) || IsKeyDown(Keys.Up))
        {
            move.Y -= 1f;
        }

        if (IsKeyDown(Keys.S) || IsKeyDown(Keys.Down))
        {
            move.Y += 1f;
        }

        if (move != Vector2.Zero)
        {
            move.Normalize();
        }

        return move;
    }
}
