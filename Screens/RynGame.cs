using BetterRyn.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Screens;

public class RynGame : Game
{
    public static RynGame Instance { get; private set; }

    private SpriteBatch _spriteBatch;
    private GraphicsDeviceManager _graphics;

    public RynGame()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        ScreenManager.Instance.ChangeScreen(new MainMenuScreen());
    }

    protected override void Update(GameTime gameTime)
    {
        ScreenManager.Instance.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        ScreenManager.Instance.Draw(_spriteBatch);
        _spriteBatch.End();
    }
}