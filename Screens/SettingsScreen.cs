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
    private int _currentSubIndex;
    private KeyboardState _previousKeyboard;
    private Texture2D _rectangle;
    private SpriteFont _font;
    private string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private string _gameFolder;
    private string[] _textFileLines;
    private bool _inSubMenu;
    
    private int _offset;
    private string[] _resolution;
    private string[] _fullscreen;
    private int _volume;
    

    public SettingsScreen()
    {
        _menuItems = new[] { "KEYBINDS", "OFFSET", "RESOLUTION", "FULLSCREEN", "VOLUME", "SAVE", "DISCARD" };
        _textFileLines = new[]
        {
            "keybinds=D, F, J, K",
            "offset=120",
            "resolution=1920x1080",
            "fullscreen=NO",
            "volume=100"
        };

        _resolution = new[] { "1280x720", "1360x768", "1600x900", "1920x1080" };
        _fullscreen = new[] { "ON", "OFF" };
        
        
        _currentIndex = 0;
        _currentSubIndex = 0;
        
        _gameFolder = Path.Combine(_appDataPath, "BetterRyn");
        Directory.CreateDirectory(_gameFolder);
        string fullFilePath = Path.Combine(_gameFolder, "settings.txt");
        File.WriteAllLines(fullFilePath, _textFileLines);
    }
    
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _rectangle = new Texture2D(graphicsDevice, 1, 1);
        _rectangle.SetData(new[] {Color.White});
        
        _font = content.Load<SpriteFont>("GameFont");
    }

    public void Update(GameTime gameTime)
    { // TODO: Make submenu input detection (after making the submenus :D)
        KeyboardState current = Keyboard.GetState();

        // Menu
        if (!_inSubMenu && current.IsKeyDown(Keys.Down) && _previousKeyboard.IsKeyUp(Keys.Down))
        {
            _currentIndex = (_currentIndex + 1) % _menuItems.Length;
        }
        if (!_inSubMenu && current.IsKeyDown(Keys.Up) && _previousKeyboard.IsKeyUp(Keys.Up))
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _menuItems.Length - 1;
        }
        if (!_inSubMenu && current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
        {
            _inSubMenu = true;
        }
        
        // Submenu
        // TODO: _menuItems is incorrect, fix it :D
        if (_inSubMenu && current.IsKeyDown(Keys.Left) && _previousKeyboard.IsKeyUp(Keys.Left))
        {
            _currentSubIndex = (_currentSubIndex + 1) % _menuItems.Length;
        }
        if (_inSubMenu && current.IsKeyDown(Keys.Right) && _previousKeyboard.IsKeyUp(Keys.Right))
        {
            _currentSubIndex--;
            if (_currentSubIndex < 0) _currentSubIndex = _menuItems.Length - 1;
        }
        if (_inSubMenu && ((current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter)) ||
                           current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape)))
        {
            _inSubMenu = false;
            _currentSubIndex = 0;
        }
        
        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int yOffset = 500;

        for (int i = 0; i < _menuItems.Length; i++)
        {
            string item = _menuItems[i];
            
            if (i == _currentIndex)
            {
                spriteBatch.Draw(_rectangle, new Rectangle(40, yOffset, 300, 40), Color.Blue * 0.5f);
            }
            
            spriteBatch.DrawString(_font, item, new Vector2(50, yOffset), Color.White);
            yOffset += 50;
        }
    }

    public void Dispose()
    {
        
    }
}