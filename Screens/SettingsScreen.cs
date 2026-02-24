using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace BetterRyn.Screens;

public class SettingsScreen : IScreen
{
    private string[] _menuItems;
    private int _currentIndex;
    private KeyboardState _previousKeyboard;
    private Texture2D _rectangle;
    private SpriteFont _font;
    private string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private string _gameFolder;
    private string[] _textFileLines;

    public SettingsScreen()
    {
        _menuItems = new[] { "KEYBINDS", "OFFSET", "RESOLUTION", "FULLSCREEN", "VOLUME", "SAVE", "DISCARD" };
        _textFileLines = new[]
        {
            "keybinds=D, F, J, K",
            "offset=120",
            "resolution=1920x1080",
            "fullscreen=true",
            "volume=100"
        };
        _currentIndex = 0;
        
        _gameFolder = Path.Combine(_appDataPath, "BetterRyn");
        Directory.CreateDirectory(_gameFolder);
        string fullFilePath = Path.Combine(_gameFolder, "settings.txt");
        File.WriteAllLines(fullFilePath, _textFileLines);
    }
    
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("GameFont");
    }

    public void Update(GameTime gameTime)
    { // TODO: Make input detection
        KeyboardState current = Keyboard.GetState();
        
        
        
        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // TODO: Draw markers on buttons relative to _currentIndex
        int yOffset = 500;

        foreach (string element in _menuItems)
        {
            spriteBatch.DrawString(_font, element, new Vector2(50, yOffset), Color.White);
            yOffset += 50;
        }
    }

    public void Dispose()
    {
        
    }
}