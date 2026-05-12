using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public abstract class GameScene
{
    protected readonly Game1 Game;

    protected Input InputState
    {
        get { return Game.InputState; }
    }

    protected SpriteBatch SpriteBatch
    {
        get { return Game.SceneSpriteBatch; }
    }

    protected GameScene(Game1 game)
    {
        Game = game;
    }

    public virtual void Open()
    {
    }

    public virtual void Close()
    {
    }

    protected void StartRun()
    {
        Game.StartRun();
    }

    protected void ShowStartScene()
    {
        Game.ShowStartScene();
    }

    protected void ShowEndScene(bool won)
    {
        Game.ShowEndScene(won);
    }

    public abstract void Update(GameTime gameTime);
    public abstract void Draw();
}
