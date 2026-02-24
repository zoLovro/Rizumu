using System;
using System.Diagnostics;
using System.IO;
using System.Transactions;
using BetterRyn.Logic;
using BetterRyn.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BetterRyn.Screens;

public class MainMenuScreen : IScreen
{
    private Texture2D _rectangle;
    private SpriteFont _font;
    private int _selectedIndex;
    private string[] _menuItems;
    private string[] _exitPromptItems;
    private int _exitPromptSelectedIndex;
    private KeyboardState _previousKeyboard;
    private bool _askingIfExit = false;

    public MainMenuScreen()
    {
        _selectedIndex = 0;
        _exitPromptSelectedIndex = 0;
        _menuItems = new[] { "Start game", "Settings", "GitHub", "Quit" };
        _exitPromptItems = new[] { "Yes", "Nah" };
    }
    
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _rectangle = new Texture2D(graphicsDevice, 1, 1);
        _rectangle.SetData(new[] {Color.White});
        
        _font = content.Load<SpriteFont>("GameFont");
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState current = Keyboard.GetState();

        if (!_askingIfExit && current.IsKeyDown(Keys.Down) && _previousKeyboard.IsKeyUp(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
        }
        if (!_askingIfExit && current.IsKeyDown(Keys.Up) && _previousKeyboard.IsKeyUp(Keys.Up))
        {
            _selectedIndex--;
            if (_selectedIndex < 0) _selectedIndex = _menuItems.Length - 1;
        }

        if (!_askingIfExit && current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
        {
            switch (_selectedIndex)
            {
                case 0:
                    string songsPath = Path.Combine(
                        AppContext.BaseDirectory,
                        "Assets",
                        "Songs"
                    );
                    ScreenManager.Instance.ChangeScreen(new SongSelectScreen(
                        MapParser.LoadAllMaps(songsPath)));
                    break;
                case 1:
                    ScreenManager.Instance.ChangeScreen(new SettingsScreen());
                    break;
                case 2:
                    OpenURL("https://github.com/zoLovro/BetterRyn");
                    break;
                case 3:
                    _askingIfExit = true;
                    break;
            }

            if (_askingIfExit && current.IsKeyDown(Keys.Right) && _previousKeyboard.IsKeyUp(Keys.Right))
            { 
                _exitPromptSelectedIndex = (_exitPromptSelectedIndex + 1) % _exitPromptItems.Length;
            }
            if (_askingIfExit && current.IsKeyDown(Keys.Left) && _previousKeyboard.IsKeyUp(Keys.Left))
            {
                _exitPromptSelectedIndex--;
                if (_exitPromptSelectedIndex < 0) _exitPromptSelectedIndex = _exitPromptItems.Length - 1;
            }
            // TODO: It has to prompt before exiting yet it doesn't
            if (_askingIfExit)
                if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
                {
                    {
                        switch (_exitPromptSelectedIndex)
                        {
                            case 0:
                                RynGame.Instance.Exit();
                                break;
                            case 1:
                                _askingIfExit = false;
                                break;
                        }
                    }
                }
        }
        
        
        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int yOffset = 50;

        for (int i = 0; i < _menuItems.Length; i++)
        {
            string item = _menuItems[i];

            if (i == _selectedIndex)
            {
                spriteBatch.Draw(_rectangle, new Rectangle(40, yOffset, 300, 40), Color.Blue * 0.5f);
            }
            spriteBatch.DrawString(_font, item, new Vector2(50, yOffset), Color.White);
            yOffset += 50;
        }

        if (_askingIfExit)
        {
            int xOffset = 300;

            for (int i = 0; i < _exitPromptItems.Length; i++)
            {
                string item = _exitPromptItems[i];

                if (i == _exitPromptSelectedIndex)
                {
                    spriteBatch.Draw(_rectangle, new Rectangle(xOffset, 500, 300, 40), Color.Blue * 0.5f);
                }
                spriteBatch.DrawString(_font, item, new Vector2(xOffset, 500), Color.White);
                xOffset += 300;
            }
        }
    }   

    public void Dispose()
    {
       
    }
    
    private void OpenURL(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Handle cases where the browser fails to open
            Debug.WriteLine($"Failed to open link: {ex.Message}");
        }
    }
    
}