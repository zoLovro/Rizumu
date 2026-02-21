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
    private List<MapsetGroup> _mapsets;
    private int _selectedSongIndex;
    private int _selectedDifficultyIndex;
    private Texture2D _backgroundPreview;
    private KeyboardState _previousKeyboard;
    private SelectState _state = SelectState.SongList;
    private bool _mapsetIsPicked = false;
    
    
    public SongSelect(List<MapMetadata> rawDifficulties)
    {
        _mapsets = rawDifficulties.GroupBy(m => m.Folder)
            .Select(group => new MapsetGroup
            {
                SongTitle = group.First().SongTitle,
                Artist = group.First().Artist,
                Folder = group.Key,
                Difficulties = group.OrderBy(diff => diff.DifficultyRating).ToList()
            })
            .OrderBy(m => m.SongTitle) 
            .ToList();
        
        _selectedSongIndex = 0;
        _selectedDifficultyIndex = 0;
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

        // MapSets
        if (_state == SelectState.SongList)
        {
            if (current.IsKeyDown(Keys.Right) && _previousKeyboard.IsKeyUp(Keys.Right))
                _selectedSongIndex = (_selectedSongIndex + 1) % _mapsets.Count;

            if (current.IsKeyDown(Keys.Left) && _previousKeyboard.IsKeyUp(Keys.Left))
            {
                _selectedSongIndex--;
                if (_selectedSongIndex < 0) _selectedSongIndex = _mapsets.Count - 1;
            }
            // Enter for diff selection
            if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
            {
                _mapsets[_selectedSongIndex].IsExpanded = true;
                _selectedDifficultyIndex = 0;
                _state = SelectState.DifficultyList;
            }

        }

        // Diff
        else if (_state == SelectState.DifficultyList)
        {
            MapsetGroup currentMapset = _mapsets[_selectedSongIndex];

            if (current.IsKeyDown(Keys.Down) && _previousKeyboard.IsKeyUp(Keys.Down))
                _selectedDifficultyIndex = (_selectedDifficultyIndex + 1) % currentMapset.Difficulties.Count;
            
            if (current.IsKeyDown(Keys.Up) && _previousKeyboard.IsKeyUp(Keys.Up))
            {
                _selectedDifficultyIndex--;
                if (_selectedDifficultyIndex < 0) _selectedDifficultyIndex = currentMapset.Difficulties.Count - 1;
            }

            //
            if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
            {
                currentMapset.IsExpanded = false;
                _state = SelectState.SongList;
            }
            
            if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
            {
                MapMetadata chosenDifficulty = currentMapset.Difficulties[_selectedDifficultyIndex];
            
                GameplayScreen gameplayScreen = new GameplayScreen(
                    chosenDifficulty.MapPath,
                    chosenDifficulty.AudioPath, 
                    chosenDifficulty.BackgroundPath);
                
                ScreenManager.Instance.ChangeScreen(gameplayScreen);
            }
        }

        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int yOffset = 50; // Starting Y position

        for (int i = 0; i < _mapsets.Count; i++)
        {
            MapsetGroup mapset = _mapsets[i];
            
            if (i == _selectedSongIndex && _state == SelectState.SongList)
            {
                spriteBatch.Draw(_rectangle, new Rectangle(40, yOffset, 300, 40), Color.Blue * 0.5f);
            }
            spriteBatch.DrawString(_font, $"{mapset.Artist} - {mapset.SongTitle}", new Vector2(50, yOffset), Color.White);
            yOffset += 50;
            
            // Diffs
            if (mapset.IsExpanded)
            {
                for (int j = 0; j < mapset.Difficulties.Count; j++)
                {
                    MapMetadata diff = mapset.Difficulties[j];
                    if (j == _selectedDifficultyIndex && _state == SelectState.DifficultyList)
                    {
                        spriteBatch.Draw(_rectangle, new Rectangle(70, yOffset, 200, 30), Color.DarkRed * 0.5f);
                    }
                    
                    spriteBatch.DrawString(_font, $"[{diff.DifficultyName}]", new Vector2(80, yOffset), Color.LightGray);
                    yOffset += 40;
                }
            }
        }
    }

    private enum SelectState
    {
        SongList,
        DifficultyList
    }
}