using System;
using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BetterRyn.Gameplay;

public class GameplayScreen : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
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
    
    public GameplayScreen()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Setting window size
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080; 
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _rectangle = new Texture2D(GraphicsDevice,1, 1);
        _rectangle.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("GameFont");
        
        // Map loading
        _noteManager = new NoteManager();
        _noteManager.LoadContent(Content);
        _noteManager.LoadMap("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\Songs\\1911190 Leah Kate - 10 Things I Hate About You (Sped Up & Cut Ver.)\\Leah Kate - 10 Things I Hate About You (Sped Up & Cut Ver.) (Kibitz) [Ihram's Easy].txt");
        
        // Music
        _music = SoundEffect.FromFile("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\Songs\\1911190 Leah Kate - 10 Things I Hate About You (Sped Up & Cut Ver.)\\audio.wav");
        _musicInstance = _music.CreateInstance();
        _musicInstance.Volume = 0.1f;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        KeyboardState current = Keyboard.GetState();
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
        if (_paused &&
            current.IsKeyDown(Keys.R) &&
            _previousKeyboard.IsKeyUp(Keys.R))
        {
            RestartMap();
            return;
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
    
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin();
        
        _noteManager.Draw(_spriteBatch);
        
        // Scoring
        _spriteBatch.DrawString(_font, (_noteManager.Accuracy * 100 ).ToString("F2"), new Vector2(100, 100), Color.White);
        _spriteBatch.DrawString(_font, _noteManager.Score.ToString(), new Vector2(100, 200), Color.White);
        
        // Hitline
        Rectangle hitLine = new Rectangle(576, 100, 768, 5);
        _spriteBatch.Draw(_rectangle, hitLine, Color.White);
        
        // Vertical lines for notes
        Rectangle line1 = new Rectangle(576, -10, 5, 2000);
        Rectangle line2 = new Rectangle(768, -10, 5, 2000);
        Rectangle line3 = new Rectangle(960, -10, 5, 2000);
        Rectangle line4 = new Rectangle(1152, -10, 5, 2000);
        Rectangle line5 = new Rectangle(1344, -10, 5, 2000);
        _spriteBatch.Draw(_rectangle, line1, Color.White);
        _spriteBatch.Draw(_rectangle, line2, Color.White);
        _spriteBatch.Draw(_rectangle, line3, Color.White);
        _spriteBatch.Draw(_rectangle, line4, Color.White);
        _spriteBatch.Draw(_rectangle, line5, Color.White);
        
        // Pause screen
        if (_paused)
        {
            // Dimming
            Rectangle fullscreen = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_rectangle, fullscreen, Color.Black * 0.6f);
            
            _spriteBatch.DrawString(_font, "PAUSED", new Vector2(900, 200), Color.White);
        }
        
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void RestartMap()
    {
        // Music
        _musicInstance.Dispose();
        _music = SoundEffect.FromFile("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\Songs\\1911190 Leah Kate - 10 Things I Hate About You (Sped Up & Cut Ver.)\\audio.wav");
        _musicInstance = _music.CreateInstance();
        _musicInstance.Volume = 0.1f;
        
        // Map
        // Map loading
        _noteManager = new NoteManager();
        _noteManager.LoadContent(Content);
        _noteManager.LoadMap("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\Songs\\1911190 Leah Kate - 10 Things I Hate About You (Sped Up & Cut Ver.)\\Leah Kate - 10 Things I Hate About You (Sped Up & Cut Ver.) (Kibitz) [Ihram's Easy].txt");

        // Reseting variables
        _songTime = 0;
        _started = false;
        _totalPausedTime = 0;
        _pauseStart = 0;
        _previousKeyboard = Keyboard.GetState();
        _paused = false;
    }
}