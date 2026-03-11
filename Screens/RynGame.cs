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

    // Deferred resolution change — applied at the start of Draw() to avoid
    // calling ApplyChanges() mid-Update, which can corrupt SDL/OpenGL state.
    private bool _pendingResolutionChange = false;
    private int _pendingWidth, _pendingHeight;

    public RynGame()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _settingsScreenManager = new SettingsScreenManager();
        _settingsScreenManager.LoadContent(); // load saved resolution on startup

        int idx = _settingsScreenManager.CurrentResolutionIndex;
        if (idx < 0 || idx >= SettingsScreenManager.Resolutions.Length) idx = 3;
        string[] res = SettingsScreenManager.Resolutions[idx].Split('x');
        _graphics.PreferredBackBufferWidth  = int.Parse(res[0]);
        _graphics.PreferredBackBufferHeight = int.Parse(res[1]);
        // ApplyChanges() is safe here because we're still in the constructor,
        // before the game loop starts.
        _graphics.ApplyChanges();
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
        if (_pendingResolutionChange)
        {
            _pendingResolutionChange = false;
            // Only call ApplyChanges if the resolution is actually different —
            // calling it when nothing changed can disrupt the GL/SDL state on Linux.
            if (_graphics.PreferredBackBufferWidth  != _pendingWidth ||
                _graphics.PreferredBackBufferHeight != _pendingHeight)
            {
                _graphics.PreferredBackBufferWidth  = _pendingWidth;
                _graphics.PreferredBackBufferHeight = _pendingHeight;
                _graphics.ApplyChanges();
                // SpriteBatch uses a simple vertex buffer; it does NOT need to be
                // recreated on a window resize in DesktopGL — the GL context is
                // preserved. Recreating it here was causing a crash on sdl2-compat.
            }
        }

        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        ScreenManager.Instance.Draw(_spriteBatch);
        _spriteBatch.End();
    }
    
    public void ApplyResolution(int width, int height)
    {
        _pendingWidth  = width;
        _pendingHeight = height;
        _pendingResolutionChange = true;
    }
}