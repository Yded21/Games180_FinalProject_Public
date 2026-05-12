using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Games180_FinalProject_EddyZhang;

public class Game1 : Game
{
    public const int ScreenWidth = 960;
    public const int ScreenHeight = 648;
    public const int HudHeight = 120;
    public const int GameplayHeight = 528;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Input _input;
    private GameScene _scene;

    internal Input InputState
    {
        get { return _input; }
    }

    internal SpriteBatch SceneSpriteBatch
    {
        get { return _spriteBatch; }
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
    }

    protected override void Initialize()
    {
        _input = new Input();
        ChangeScene(new StartScene(this));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        AssetManager.LoadContent(Content, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
        {
            Exit();
        }

        _scene.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _scene.Draw();
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    internal void StartRun()
    {
        ChangeScene(new GameplayScene(this));
    }

    internal void ShowStartScene()
    {
        ChangeScene(new StartScene(this));
    }

    internal void ShowEndScene(bool won)
    {
        ChangeScene(new EndScene(this, won));
    }

    private void ChangeScene(GameScene nextScene)
    {
        if (_scene != null)
        {
            _scene.Close();
        }

        _scene = nextScene;
        _scene.Open();
    }
}
