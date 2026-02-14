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
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Map loading
        _noteManager = new NoteManager();
        _noteManager.LoadContent(Content);
        _noteManager.LoadMap("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\2490429_King_Gnu-AIZO_(TV Size)_[no_video]\\King_Gnu-AIZO_(TV_Size)_(keksikosu)_[EASY].txt");
        
        // Music
        _music = SoundEffect.FromFile("C:\\Users\\lovro\\Desktop\\Projects\\BetterRyn\\Assets\\2490429_King_Gnu-AIZO_(TV Size)_[no_video]\\audio.wav");
        _musicInstance = _music.CreateInstance();
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        
        if (!_started)
        {
            _musicInstance.Play();
            _startTime = gameTime.TotalGameTime.TotalMilliseconds;
            _started = true;
        }

        _songTime = (float) gameTime.TotalGameTime.TotalMilliseconds - (float) _startTime;
        
        // spawning notes
        _noteManager.SpawnNotes(_songTime);
        _noteManager.UpdateActiveNotes(_songTime, gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        
        _spriteBatch.Begin();
        
        _noteManager.Draw(_spriteBatch);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}