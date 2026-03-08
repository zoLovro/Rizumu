using System;
using BetterRyn.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Screens;

public class RynGame : Game
{
    public static RynGame Instance { get; private set; }

    private SpriteBatch _spriteBatch;
    private GraphicsDeviceManager _graphics;
    private SettingsScreenManager _settingsScreenManager;

    public RynGame()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _settingsScreenManager = new SettingsScreenManager();

        string[] res = SettingsScreenManager.Resolutions[_settingsScreenManager.CurrentResolutionIndex].Split('x');
        ApplyResolution(int.Parse(res[0]), int.Parse(res[1]));
        Console.WriteLine(_settingsScreenManager.CurrentResolutionIndex);
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
    
    public void ApplyResolution(int width, int height)
    {
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges(); 
    }
}