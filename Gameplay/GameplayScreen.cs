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
        
        // Map loading
        _noteManager = new NoteManager();
        _noteManager.LoadContent(Content);
        _noteManager.LoadMap("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\2490429_King_Gnu-AIZO_(TV Size)_[no_video]\\King_Gnu-AIZO_(TV_Size)_(keksikosu)_[EASY].txt");
        
        // Music
        _music = SoundEffect.FromFile("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\2490429_King_Gnu-AIZO_(TV Size)_[no_video]\\audio.wav");
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
        
        // Hitline
        Rectangle hitLine = new Rectangle(-10, 100, 2000, 5);
        _spriteBatch.Draw(_rectangle, hitLine, Color.White);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}