using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Games180_FinalProject_EddyZhang;

public class StartScene : GameScene
{
    public StartScene(Game1 game) : base(game)
    {
    }

    public override void Update(GameTime gameTime)
    {
        if (InputState.IsKeyPressed(Keys.E) || InputState.IsKeyPressed(Keys.Enter) || InputState.IsKeyPressed(Keys.Space))
        {
            StartRun();
        }

        if (InputState.IsKeyPressed(Keys.Escape))
        {
            Game.Exit();
        }
    }

    public override void Draw()
    {
        SpriteBatch.Draw(AssetManager.Pixel, new Rectangle(0, 0, Game1.ScreenWidth, Game1.ScreenHeight), new Color(18, 18, 22));
        UiHelper.DrawCenteredText(SpriteBatch, "FINAL PROJECT", new Rectangle(0, 170, Game1.ScreenWidth, 60), Color.White, 1.6f);
        UiHelper.DrawCenteredText(SpriteBatch, "Press E to start", new Rectangle(0, 260, Game1.ScreenWidth, 40), Color.Gainsboro, 1f);
        UiHelper.DrawCenteredText(SpriteBatch, "WASD/Arrows move   Space attack   Shift dash   F skill   E interact", new Rectangle(0, 320, Game1.ScreenWidth, 40), Color.Gray, 0.8f);
        UiHelper.DrawCenteredText(SpriteBatch, "T save game   Y load game", new Rectangle(0, 355, Game1.ScreenWidth, 30), Color.Gray, 0.75f);
    }
}
