using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace BetterRyn.Screens;

public class SongSelect : IScreen
{
    private GraphicsDevice _graphicsDevice;
    private SpriteFont _font;
    private Texture2D _rectangle;
    private List<MapMetadata> _songs;
    private int _selectedIndex;
    private Texture2D _backgroundPreview;
    private KeyboardState _previousKeyboard;
    
    
    public SongSelect(List<MapMetadata> songs)
    {
        _songs = songs.OrderBy(s => s.SongTitle).ToList();
        _selectedIndex = 0;
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        _rectangle = new Texture2D(graphicsDevice, 1, 1);
        _rectangle.SetData(new[] { Color.White });

        _font = content.Load<SpriteFont>("GameFont");
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState current = Keyboard.GetState();

        if (current.IsKeyDown(Keys.Right) && _previousKeyboard.IsKeyUp(Keys.Right))
        {
            _selectedIndex = (_selectedIndex + 1) % _songs.Count;
        }
        if (current.IsKeyDown(Keys.Left) && _previousKeyboard.IsKeyUp(Keys.Left))
        {
            _selectedIndex = (_selectedIndex - 1 + _songs.Count) % _songs.Count;
        }

        if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
        {
            GameplayScreen gameplayScreen = new GameplayScreen(_songs[_selectedIndex].MapPath,
                _songs[_selectedIndex].AudioPath, _songs[_selectedIndex].BackgroundPath);
            ScreenManager.Instance.ChangeScreen(gameplayScreen);
        }
        
        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle chosenSong = new Rectangle(50, 50 * _selectedIndex + 50, 200, 50);
        spriteBatch.Draw(_rectangle, chosenSong, Color.Blue);
        
        var height = 50;
        foreach (var song in _songs)
        {
            spriteBatch.DrawString(_font, song.SongTitle, new Vector2(50, height), Color.White);
            height += 50;
        }
    }
}