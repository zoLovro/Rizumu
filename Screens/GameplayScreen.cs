using System;
using System.IO;
using BetterRyn.Gameplay;
using BetterRyn.Logic;
using BetterRyn.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BetterRyn.Screens;

public class GameplayScreen : IScreen
{
    private GraphicsDevice _graphicsDevice;
    private ContentManager _content;
    private NoteManager _noteManager;
    private HealthManager _healthManager;
    private SoundEffect _music;
    private SoundEffectInstance _musicInstance;
    private double _startTime;
    private bool _started = false;
    private float _songTime = 0f;
    private Texture2D _rectangle;
    private KeyboardState _previousKeyboard;
    private bool _paused = false;
    private bool _failed = false;
    private double _pauseStart;
    private double _totalPausedTime = 0;
    private SpriteFont _font;
    private Texture2D _pressEffect;
    private const float StartDelayMs = 3000f;
    private double elapsed;

    private string _mapFilepath;
    private string _songFilepath;
    private string _backgroundFilepath;
    private Texture2D _backgroundTexture;

    private bool _key0Pressed;
    private bool _key1Pressed;
    private bool _key2Pressed;
    private bool _key3Pressed;
    
    private const float AudioOffsetMs = 120f;

    public string MapFilepath => _mapFilepath;
    public string SongFilepath => _songFilepath;
    public string BackgroundFilepath => _backgroundFilepath;

    private int hitLineWidth;
    private Rectangle hitLine;
    private Rectangle fullscreen;
    private Rectangle noteBackground;
    
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
        _musicInstance.Volume = 0.2f;

        _healthManager = new HealthManager();
        _healthManager.LoadContent(_content);
        _noteManager.OnHit += () => _healthManager.AddHealth(10);
        _noteManager.OnGoodHit += () => _healthManager.AddHealth(5);
        _noteManager.OnMiss += () => _healthManager.SubtractHealth(10);
        
        // Drawing stuff
        hitLineWidth = (_graphicsDevice.Viewport.Width / 2 + _noteManager.NoteTextureWidth * 2) -
                           (_graphicsDevice.Viewport.Width / 2 - _noteManager.NoteTextureWidth * 2);
        hitLine = new Rectangle(_graphicsDevice.Viewport.Width/2 - _noteManager.NoteTextureWidth * 2,
            _noteManager.HitLine, hitLineWidth, 5);
        fullscreen = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        noteBackground = new Rectangle(_graphicsDevice.Viewport.Width/2 - _noteManager.NoteTextureWidth * 2, 0, hitLineWidth, _graphicsDevice.Viewport.Height);
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
            _startTime = gameTime.TotalGameTime.TotalMilliseconds;
            _started = true;
        }

        elapsed = gameTime.TotalGameTime.TotalMilliseconds - _startTime;
        if (_started && !_musicInstance.State.Equals(SoundState.Playing))
        {
            if (elapsed >= StartDelayMs)
            {
                _musicInstance.Play();
            }
        }
        
        // Pausing
        if (!_healthManager.isDead && current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
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
        if ((_paused || _healthManager.isDead) && current.IsKeyDown(Keys.R) && _previousKeyboard.IsKeyUp(Keys.R))
        {
            RestartMap();
            return;
        }
        
        if ((_paused || _healthManager.isDead) && current.IsKeyDown(Keys.Q) && _previousKeyboard.IsKeyUp(Keys.Q))
        {
            string songsPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Songs"
            );
            ScreenManager.Instance.ChangeScreen(new SongSelectScreen(
                MapParser.LoadAllMaps(songsPath)));
        }
        _songTime = (float)(gameTime.TotalGameTime.TotalMilliseconds - _startTime - _totalPausedTime - AudioOffsetMs - StartDelayMs);
        
        // Death screen
        if (_healthManager.isDead)
        {
            _musicInstance.Pause();
        }
        

        if (!_paused && !_healthManager.isDead)
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

        
        // Ending the map
        if (_noteManager.NoteQueue.Count == 0 && _noteManager.ActiveNotes.Count == 0)
        {
            Dispose();
            ScreenManager.Instance.ChangeScreen(new ResultScreen(this, _noteManager));
        }
        
        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            // TODO: Make the brightness adjustable
            spriteBatch.Draw(_rectangle, fullscreen, Color.Black * 0.1f);
            spriteBatch.Draw(_rectangle, noteBackground, Color.Black * 0.7f);
        }
        _noteManager.Draw(spriteBatch);
        
        // Scoring
        spriteBatch.DrawString(_font, (_noteManager.Accuracy * 100 ).ToString("F2"), new Vector2(100, 100), Color.White);
        spriteBatch.DrawString(_font, _noteManager.Score.ToString(), new Vector2(100, 200), Color.White);
        spriteBatch.DrawString(_font, _noteManager.Combo.ToString(), new Vector2(100, _graphicsDevice.Viewport.Height - 100), Color.White);
        
        // Hitline
        spriteBatch.Draw(_rectangle, hitLine, Color.White);
        
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
        
        _healthManager.Draw(spriteBatch, new Vector2(50, 50));
        
        
        // Pause screen
        if (_paused && !_healthManager.isDead)
        {
            // Dimming
            Rectangle fullscreen = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            spriteBatch.Draw(_rectangle, fullscreen, Color.Black * 0.3f);
            
            spriteBatch.DrawString(_font, "PAUSED", new Vector2(900, 200), Color.White);
            
            spriteBatch.DrawString(_font, "ESC - Continue", new Vector2(200, 150), Color.White);
            spriteBatch.DrawString(_font, "Q - Quit to song select", new Vector2(200, 200), Color.White);
            spriteBatch.DrawString(_font, "R - Restart map", new Vector2(200, 250), Color.White);
        }
        
        if (_healthManager.isDead)
        {
            // Dimming
            Rectangle fullscreen = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            spriteBatch.Draw(_rectangle, fullscreen, Color.Black * 0.6f);
            
            spriteBatch.DrawString(_font, "DEAD", new Vector2(900, 200), Color.White);
            
            spriteBatch.DrawString(_font, "Q - Quit to song select", new Vector2(200, 200), Color.White);
            spriteBatch.DrawString(_font, "R - Restart map", new Vector2(200, 250), Color.White);
        }
        
        if (elapsed < StartDelayMs)
        {
            int countdown = (int)Math.Ceiling((StartDelayMs - elapsed) / 1000.0);
            spriteBatch.DrawString(_font, countdown.ToString(), new Vector2(900, 400), Color.White);
        }
    }

    private void RestartMap()
    {
        // Music
        _musicInstance.Dispose();
        _music = SoundEffect.FromFile(_songFilepath);
        _musicInstance = _music.CreateInstance();
        _musicInstance.Volume = 0.2f;
        
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
        
        // HealthManager initialization
        _healthManager = new HealthManager();
        _healthManager.LoadContent(_content);
        _noteManager.OnHit += () => _healthManager.AddHealth(10);
        _noteManager.OnGoodHit += () => _healthManager.AddHealth(5);
        _noteManager.OnMiss += () => _healthManager.SubtractHealth(10);
    }
    
    public void Dispose()
    {
        if (_musicInstance.State == SoundState.Playing)
        {
            _musicInstance.Stop();
        }
    }
}