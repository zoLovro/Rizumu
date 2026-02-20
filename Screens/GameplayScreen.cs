using System;
using System.IO;
using System.Transactions;
using BetterRyn.Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BetterRyn.Screens;

public class GameplayScreen : IScreen
{
    private GraphicsDevice _graphicsDevice;
    private ContentManager _content;
    private NoteManager _noteManager;
    private SoundEffect _music;
    private SoundEffectInstance _musicInstance;
    private double _startTime;
    private bool _started = false;
    private float _songTime = 0f;
    private Texture2D _rectangle;
    private KeyboardState _previousKeyboard;
    private bool _paused = false;
    private double _pauseStart;
    private double _totalPausedTime = 0;
    private SpriteFont _font;
    private Texture2D _pressEffect;

    private string _mapFilepath;
    private string _songFilepath;
    private string _backgroundFilepath;
    private Texture2D _backgroundTexture;

    private bool _key0Pressed;
    private bool _key1Pressed;
    private bool _key2Pressed;
    private bool _key3Pressed;
    
    public GameplayScreen(string mapFilepath, string songFilepath, string backgroundFilepath)
    {
        _mapFilepath = mapFilepath ?? throw new ArgumentNullException(nameof(mapFilepath));
        _songFilepath = songFilepath ?? throw new ArgumentNullException(nameof(songFilepath));
        _backgroundFilepath = backgroundFilepath; // optional
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;

        _rectangle = new Texture2D(_graphicsDevice,1, 1);
        _rectangle.SetData(new[] { Color.White });
        _font = _content.Load<SpriteFont>("GameFont");
        
        if (!string.IsNullOrEmpty(_backgroundFilepath))
        {
            using var stream = new FileStream(_backgroundFilepath, FileMode.Open);
            _backgroundTexture = Texture2D.FromStream(graphicsDevice, stream);
        }
        
        
        // Map loading
        _noteManager = new NoteManager();
        _noteManager.LoadContent(_content);
        _noteManager.LoadMap(_mapFilepath);
        
        // Music
        _music = SoundEffect.FromFile(_songFilepath);
        _musicInstance = _music.CreateInstance();
        _musicInstance.Volume = 0.5f;
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState current = Keyboard.GetState();
        _key0Pressed = current.IsKeyDown(Keys.D);
        _key1Pressed = current.IsKeyDown(Keys.F);
        _key2Pressed = current.IsKeyDown(Keys.J);
        _key3Pressed = current.IsKeyDown(Keys.K);
        
        if (!_started)
        {
            _musicInstance.Play();
            _startTime = gameTime.TotalGameTime.TotalMilliseconds;
            _started = true;
        }
        
        // Pausing
        if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
        {
            _paused = !_paused;

            if (_paused)
            {
                _musicInstance.Pause();
                _pauseStart = gameTime.TotalGameTime.TotalMilliseconds;
            }
            else
            {
                _musicInstance.Resume();
                _totalPausedTime += gameTime.TotalGameTime.TotalMilliseconds - _pauseStart;
            }
        }
        if (_paused && current.IsKeyDown(Keys.R) && _previousKeyboard.IsKeyUp(Keys.R))
        {
            RestartMap();
            return;
        }
        
        if (_paused && current.IsKeyDown(Keys.Q) && _previousKeyboard.IsKeyUp(Keys.Q))
        {
            ScreenManager.Instance.ChangeScreen(new SongSelect(
                MapParser.LoadAllMaps("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\Songs")));
        }

        _songTime = (float)(
            gameTime.TotalGameTime.TotalMilliseconds
            - _startTime
            - _totalPausedTime
        );

        if (!_paused)
        {
            // spawning notes
            _noteManager.SpawnNotes(_songTime);
            _noteManager.UpdateActiveNotes(_songTime, gameTime);

            // Hit logic
            if (current.IsKeyDown(Keys.D) && _previousKeyboard.IsKeyUp(Keys.D))
            {
                _noteManager.CheckHit(_songTime, 0);
            }

            if (current.IsKeyDown(Keys.F) && _previousKeyboard.IsKeyUp(Keys.F))
            {
                _noteManager.CheckHit(_songTime, 1);
            }

            if (current.IsKeyDown(Keys.J) && _previousKeyboard.IsKeyUp(Keys.J))
            {
                _noteManager.CheckHit(_songTime, 2);
            }

            if (current.IsKeyDown(Keys.K) && _previousKeyboard.IsKeyUp(Keys.K))
            {
                _noteManager.CheckHit(_songTime, 3);
            }

            // Release logic
            if (current.IsKeyUp(Keys.D) && _previousKeyboard.IsKeyDown(Keys.D))
            {
                _noteManager.CheckRelease(_songTime, 0);
            }

            if (current.IsKeyUp(Keys.F) && _previousKeyboard.IsKeyDown(Keys.F))
            {
                _noteManager.CheckRelease(_songTime, 1);
            }

            if (current.IsKeyUp(Keys.J) && _previousKeyboard.IsKeyDown(Keys.J))
            {
                _noteManager.CheckRelease(_songTime, 2);
            }

            if (current.IsKeyUp(Keys.K) && _previousKeyboard.IsKeyDown(Keys.K))
            {
                _noteManager.CheckRelease(_songTime, 3);
            }
        }

        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
        }
        _noteManager.Draw(spriteBatch);
        
        // Scoring
        spriteBatch.DrawString(_font, (_noteManager.Accuracy * 100 ).ToString("F2"), new Vector2(100, 100), Color.White);
        spriteBatch.DrawString(_font, _noteManager.Score.ToString(), new Vector2(100, 200), Color.White);
        
        // Hitline
        int hitLineWidth = (_graphicsDevice.Viewport.Width / 2 + _noteManager.NoteTextureWidth * 2) -
                           (_graphicsDevice.Viewport.Width / 2 - _noteManager.NoteTextureWidth * 2);
        Rectangle hitLine = new Rectangle(_graphicsDevice.Viewport.Width/2 - _noteManager.NoteTextureWidth * 2,
            _noteManager.HitLine, hitLineWidth, 5);
        spriteBatch.Draw(_rectangle, hitLine, Color.White);
        Console.WriteLine(hitLineWidth);
        
        // Vertical lines for notes
        Rectangle line1 = new Rectangle(_graphicsDevice.Viewport.Width/2 - _noteManager.NoteTextureWidth * 2,
            -10, 5, _graphicsDevice.Viewport.Height + 10);
        Rectangle line2 = new Rectangle(_graphicsDevice.Viewport.Width/2 - _noteManager.NoteTextureWidth,
            -10, 5, _graphicsDevice.Viewport.Height + 10);
        Rectangle line3 = new Rectangle(_graphicsDevice.Viewport.Width/2,
            -10, 5, _graphicsDevice.Viewport.Height + 10);
        Rectangle line4 = new Rectangle(_graphicsDevice.Viewport.Width/2 + _noteManager.NoteTextureWidth,
            -10, 5, _graphicsDevice.Viewport.Height + 10);
        Rectangle line5 = new Rectangle(_graphicsDevice.Viewport.Width/2 + _noteManager.NoteTextureWidth * 2,
            -10, 5, _graphicsDevice.Viewport.Height + 10);
        spriteBatch.Draw(_rectangle, line1, Color.White);
        spriteBatch.Draw(_rectangle, line2, Color.White);
        spriteBatch.Draw(_rectangle, line3, Color.White);
        spriteBatch.Draw(_rectangle, line4, Color.White);
        spriteBatch.Draw(_rectangle, line5, Color.White);
        
        // Hit effects
        Rectangle lane0PressEffect = new Rectangle(_graphicsDevice.Viewport.Width/2 - _noteManager.NoteTextureWidth * 2,
            -10, _noteManager.NoteTextureWidth, _graphicsDevice.Viewport.Height + 10);
        Rectangle lane1PressEffect = new Rectangle(_graphicsDevice.Viewport.Width/2 - _noteManager.NoteTextureWidth,
            -10, _noteManager.NoteTextureWidth, _graphicsDevice.Viewport.Height + 10);
        Rectangle lane2PressEffect = new Rectangle(_graphicsDevice.Viewport.Width/2,
            -10, _noteManager.NoteTextureWidth, _graphicsDevice.Viewport.Height + 10);
        Rectangle lane3PressEffect = new Rectangle(_graphicsDevice.Viewport.Width/2 + _noteManager.NoteTextureWidth,
            -10, _noteManager.NoteTextureWidth, _graphicsDevice.Viewport.Height + 10);
        
        if(_key0Pressed) spriteBatch.Draw(_rectangle, lane0PressEffect, Color.White * 0.3f);
        if(_key1Pressed) spriteBatch.Draw(_rectangle, lane1PressEffect, Color.White * 0.3f);
        if(_key2Pressed) spriteBatch.Draw(_rectangle, lane2PressEffect, Color.White * 0.3f);
        if(_key3Pressed) spriteBatch.Draw(_rectangle, lane3PressEffect, Color.White * 0.3f);
        
        
        // Pause screen
        if (_paused)
        {
            // Dimming
            Rectangle fullscreen = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            spriteBatch.Draw(_rectangle, fullscreen, Color.Black * 0.6f);
            
            spriteBatch.DrawString(_font, "PAUSED", new Vector2(900, 200), Color.White);
            
            spriteBatch.DrawString(_font, "Q - Quit to song select", new Vector2(200, 200), Color.White);
            spriteBatch.DrawString(_font, "R - Restart map", new Vector2(200, 250), Color.White);
        }
        
    }

    private void RestartMap()
    {
        // Music
        _musicInstance.Dispose();
        _music = SoundEffect.FromFile(_songFilepath);
        _musicInstance = _music.CreateInstance();
        _musicInstance.Volume = 0.1f;
        
        // Map
        // Map loading
        _noteManager = new NoteManager();
        _noteManager.LoadContent(_content);
        _noteManager.LoadMap(_mapFilepath);

        // Reseting variables
        _songTime = 0;
        _started = false;
        _totalPausedTime = 0;
        _pauseStart = 0;
        _previousKeyboard = Keyboard.GetState();
        _paused = false;
    }
}