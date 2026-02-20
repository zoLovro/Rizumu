namespace BetterRyn.Screens;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class ScreenManager
{
    private static ScreenManager _instance;
    public static ScreenManager Instance => _instance ??= new ScreenManager();

    private IScreen _currentScreen;

    private ScreenManager() { }

    public void ChangeScreen(IScreen newScreen)
    {
        _currentScreen = newScreen;
        _currentScreen.LoadContent(RynGame.Instance.Content, RynGame.Instance.GraphicsDevice);
    }

    public void Update(GameTime gameTime)
    {
        _currentScreen?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _currentScreen?.Draw(spriteBatch);
    }
}