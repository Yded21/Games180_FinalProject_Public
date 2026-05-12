using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Games180_FinalProject_EddyZhang;

public class EndScene : GameScene
{
    private readonly bool _won;

    public EndScene(Game1 game, bool won) : base(game)
    {
        _won = won;
    }

    public override void Update(GameTime gameTime)
    {
        if (InputState.IsKeyPressed(Keys.Enter))
        {
            StartRun();
        }

        if (InputState.IsKeyPressed(Keys.Escape))
        {
            ShowStartScene();
        }
    }

    public override void Draw()
    {
        string title = "RUN LOST";
        Color titleColor = new Color(210, 70, 70);
        if (_won)
        {
            title = "BOSS DEFEATED";
            titleColor = new Color(220, 190, 90);
        }

        SpriteBatch.Draw(AssetManager.Pixel, new Rectangle(0, 0, Game1.ScreenWidth, Game1.ScreenHeight), new Color(14, 14, 18));
        UiHelper.DrawCenteredText(SpriteBatch, title, new Rectangle(0, 210, Game1.ScreenWidth, 60), titleColor, 1.5f);
        UiHelper.DrawCenteredText(SpriteBatch, "Press Enter to start a new run", new Rectangle(0, 300, Game1.ScreenWidth, 40), Color.White, 1f);
        UiHelper.DrawCenteredText(SpriteBatch, "Press Escape for title", new Rectangle(0, 340, Game1.ScreenWidth, 40), Color.Gray, 0.8f);
    }
}
