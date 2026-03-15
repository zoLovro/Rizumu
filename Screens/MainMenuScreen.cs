using System;
using System.Diagnostics;
using System.IO;
using BetterRyn.Logic;
using BetterRyn.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
                    _previousKeyboard = current; // Reasoning: When I tried to break, it remembered the last inputted key and went straight to exiting the game
                    break;
            }
        }
        
        if (_askingIfExit)
        {
            if (current.IsKeyDown(Keys.Right) && _previousKeyboard.IsKeyUp(Keys.Right))
                _exitPromptSelectedIndex = (_exitPromptSelectedIndex + 1) % _exitPromptItems.Length;

            if (current.IsKeyDown(Keys.Left) && _previousKeyboard.IsKeyUp(Keys.Left))
            {
                _exitPromptSelectedIndex--;
                if (_exitPromptSelectedIndex < 0) _exitPromptSelectedIndex = _exitPromptItems.Length - 1;
            }   

            if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
            {
                if (_exitPromptSelectedIndex == 0)
                    RynGame.Instance.Exit();
                else
                    _askingIfExit = false;
            }
        }
        
        
        _previousKeyboard = current;
    }

   public void Draw(SpriteBatch spriteBatch)
{
    int screenWidth = _rectangle.GraphicsDevice.Viewport.Width;
    int screenHeight = _rectangle.GraphicsDevice.Viewport.Height;
    
    int menuX = 20;                 
    int menuWidth = screenWidth - 40;
    int menuHeight = 60;
    int spacing = 20;
    int yOffset = 150;

    // Main menu
    for (int i = 0; i < _menuItems.Length; i++)
    {
        string item = _menuItems[i];
        bool selected = (!_askingIfExit && i == _selectedIndex);

        Color bgColor = selected ? Color.Lerp(new Color(40, 40, 40), new Color(70, 130, 180), 0.7f)
                                 : new Color(40, 40, 40);

        Rectangle rect = new Rectangle(menuX, yOffset, menuWidth, menuHeight);
        spriteBatch.Draw(_rectangle, rect, bgColor);

        Vector2 textSize = _font.MeasureString(item);
        Vector2 textPos = new Vector2(menuX + 30, yOffset + (menuHeight - textSize.Y) / 2);
        spriteBatch.DrawString(_font, item, textPos, Color.White);

        if (selected)
        {
            Vector2 arrowPos = new Vector2(menuX + 5, yOffset + (menuHeight - textSize.Y) / 2);
            spriteBatch.DrawString(_font, ">", arrowPos, Color.White);
        }

        yOffset += menuHeight + spacing;
    }

    // Exit prompt
    if (_askingIfExit)
    {
        string prompt = "Are you sure you want to quit?";
        Vector2 promptSize = _font.MeasureString(prompt);
        Vector2 promptPos = new Vector2((screenWidth - promptSize.X) / 2, screenHeight / 2 - 60);
        spriteBatch.DrawString(_font, prompt, promptPos, Color.White);

        // Options centered
        int optionWidth = 180;
        int optionHeight = 50;
        int optionSpacing = 50;
        int totalWidth = _exitPromptItems.Length * optionWidth + (_exitPromptItems.Length - 1) * optionSpacing;
        int startX = (screenWidth - totalWidth) / 2;
        int promptY = screenHeight / 2;

        for (int i = 0; i < _exitPromptItems.Length; i++)
        {
            string option = _exitPromptItems[i];
            bool selected = i == _exitPromptSelectedIndex;

            Color bgColor = selected ? Color.Lerp(new Color(40, 40, 40), new Color(70, 130, 180), 0.7f)
                                     : new Color(40, 40, 40);

            Rectangle rect = new Rectangle(startX, promptY, optionWidth, optionHeight);
            spriteBatch.Draw(_rectangle, rect, bgColor);

            Vector2 textSize = _font.MeasureString(option);
            Vector2 textPos = new Vector2(startX + (optionWidth - textSize.X) / 2, promptY + (optionHeight - textSize.Y) / 2);
            spriteBatch.DrawString(_font, option, textPos, Color.White);

            startX += optionWidth + optionSpacing;
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