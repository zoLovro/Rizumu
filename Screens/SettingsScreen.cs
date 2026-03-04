using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using BetterRyn.Managers;

namespace BetterRyn.Screens;

public class SettingsScreen : IScreen
{
    private string[] _menuItems;
    private int _currentIndex;
    private int _currentSubIndex;
    private KeyboardState _previousKeyboard;
    private Texture2D _rectangle;
    private SpriteFont _font;
    private string[] _textFileLines;
    private bool _inSubMenu;

    private string[] _keybinds;
    private float _offset;
    private string[] _resolution;
    private bool _fullscreen;
    private float _volume;

    private int _savedResolutionIndex;
    private int _savedFullscreenIndex;

    private SettingsScreenManager _settingsScreenManager;
    

    public SettingsScreen()
    {
        _settingsScreenManager = new SettingsScreenManager();
        
        _textFileLines = _settingsScreenManager.TextFileLines;
        File.WriteAllLines(_settingsScreenManager.CreateSettingsFileIfExists(), _textFileLines);
    }
    
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _rectangle = new Texture2D(graphicsDevice, 1, 1);
        _rectangle.SetData(new[] {Color.White});
        
        _font = content.Load<SpriteFont>("GameFont");
        
        _menuItems = _settingsScreenManager.MenuItems;

        _keybinds = _settingsScreenManager.Keybinds;
        _resolution = _settingsScreenManager.Resolutions;
        _fullscreen = _settingsScreenManager.Fullscreen;
        _volume = _settingsScreenManager.Volume;
        _offset = _settingsScreenManager.Offset;
        
        
        _currentIndex = 0;
        _currentSubIndex = 0;

        _savedFullscreenIndex = 0;
        _savedResolutionIndex = 3;
    }

    public void Update(GameTime gameTime)
    { 
        KeyboardState current = Keyboard.GetState();

        // Menu
        if (!_inSubMenu)
        {
            if (current.IsKeyDown(Keys.Down) && _previousKeyboard.IsKeyUp(Keys.Down))
            {
                _currentIndex = (_currentIndex + 1) % _menuItems.Length;
                _currentSubIndex = 0;
            }

            if (current.IsKeyDown(Keys.Up) && _previousKeyboard.IsKeyUp(Keys.Up))
            {
                _currentIndex--;
                if (_currentIndex < 0) _currentIndex = _menuItems.Length - 1;
                _currentSubIndex = 0;
            }

            if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
            {
                if (_currentIndex < 5) 
                {
                    _inSubMenu = true;
                }
                else
                {
                    Console.WriteLine($"Action executed for: {_menuItems[_currentIndex]}");
                }
            }
        }
        else
        {
            int maxSubItems = 0;
            switch (_currentIndex)
            {
                case 2: // RESOLUTION
                    maxSubItems = _resolution.Length;
                    break;
                case 3: // FULLSCREEN
                    maxSubItems = 2; 
                    break;
                // TODO: add other cases for other choices
            }
            
            if (maxSubItems > 0)
            {
                if (current.IsKeyDown(Keys.Right) && _previousKeyboard.IsKeyUp(Keys.Right))
                {
                    _currentSubIndex = (_currentSubIndex + 1) % maxSubItems;
                }
                if (current.IsKeyDown(Keys.Left) && _previousKeyboard.IsKeyUp(Keys.Left))
                {
                    _currentSubIndex--;
                    if (_currentSubIndex < 0) _currentSubIndex = maxSubItems - 1;
                }
            }
            
            if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
            {
                ApplySetting(_currentIndex, _currentSubIndex);
                _inSubMenu = false;
            }
            if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
            {
                _inSubMenu = false;
            }
        }
    
        _previousKeyboard = current;
    }


    public void Draw(SpriteBatch spriteBatch)
{
    int yOffset = 500;
    int rightColumnX = 350;

    for (int i = 0; i < _menuItems.Length; i++)
    {
        string item = _menuItems[i];
        
        if (i == _currentIndex && !_inSubMenu)
        {
            spriteBatch.Draw(_rectangle, new Rectangle(40, yOffset, 300, 40), Color.Blue * 0.5f);
        }
        spriteBatch.DrawString(_font, item, new Vector2(50, yOffset), Color.White);
        
        if (i == 2) // RESOLUTION
        {
            DrawOptionsList(spriteBatch, _resolution, rightColumnX, yOffset, i, _savedResolutionIndex);
        }
        else if (i == 3) // FULLSCREEN
        {
            string[] fullscreenOptions = { "NO", "YES" };
            DrawOptionsList(spriteBatch, fullscreenOptions, rightColumnX, yOffset, i, _savedFullscreenIndex);
        }
        // TODO: add drawing logic for other menu items

        yOffset += 50;
    }
}
    
    private void DrawOptionsList(SpriteBatch spriteBatch, string[] options, int startX, int yOffset, int menuIndex, int savedOptionIndex)
    {
        int currentX = startX;

        for (int j = 0; j < options.Length; j++)
        {
            Color textColor = Color.Gray * 0.5f; 
            
            if (menuIndex == _currentIndex && _inSubMenu)
            {
                if (j == _currentSubIndex)
                {
                    textColor = Color.Yellow; // The one we are hovering over right now
                }
                else if (j == savedOptionIndex)
                {
                    textColor = Color.White; 
                }
                else
                {
                    textColor = Color.Gray; 
                }
            }
            else
            {
                if (j == savedOptionIndex) 
                {
                    textColor = Color.White; 
                }
            }

            spriteBatch.DrawString(_font, options[j], new Vector2(currentX, yOffset), textColor);
            currentX += (int)_font.MeasureString(options[j]).X + 30; 
        }
    }

    private void ApplySetting(int mainIndex, int subIndex)
    {
        if (mainIndex == 2) // RESOLUTION
        {
            _savedResolutionIndex = subIndex;
            Console.WriteLine($"Resolution changed to: {_resolution[subIndex]}");
            // TODO: add actual resolution change
        }
        else if (mainIndex == 3) // FULLSCREEN
        {
            _savedFullscreenIndex = subIndex;
            _fullscreen = (subIndex == 1);
            Console.WriteLine($"Fullscreen changed to: {_fullscreen}");
        }
    }

    public void Dispose()
    {
        
    }
    
}