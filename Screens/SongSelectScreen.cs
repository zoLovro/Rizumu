using System.Collections.Generic;
using System.Linq;
using BetterRyn.Logic;
using BetterRyn.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace BetterRyn.Screens;

public class SongSelectScreen : IScreen
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
    private int _scrollOffset = 0;
    private const int SongRowHeight = 50;
    private const int DiffRowHeight = 40;
    private const int ListStartY = 50;
    
    
    public SongSelectScreen(List<MapMetadata> rawDifficulties)
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
        
        LoadBackground();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState current = Keyboard.GetState();

        // MapSets
        if (_state == SelectState.SongList)
        {
            if (current.IsKeyDown(Keys.Right) && _previousKeyboard.IsKeyUp(Keys.Right))
            {
                _selectedSongIndex = (_selectedSongIndex + 1) % _mapsets.Count;
                UpdateScrollOffset();
                LoadBackground();
            }

            if (current.IsKeyDown(Keys.Left) && _previousKeyboard.IsKeyUp(Keys.Left))
            {
                _selectedSongIndex--;
                if (_selectedSongIndex < 0) _selectedSongIndex = _mapsets.Count - 1;
                UpdateScrollOffset();
                LoadBackground();
            }
            // Enter for diff selection
            if (current.IsKeyDown(Keys.Enter) && _previousKeyboard.IsKeyUp(Keys.Enter))
            {
                _mapsets[_selectedSongIndex].IsExpanded = true;
                _selectedDifficultyIndex = 0;
                _state = SelectState.DifficultyList;
                UpdateScrollOffset();
            }

            if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
            {
                ScreenManager.Instance.ChangeScreen(new MainMenuScreen());
            }

        }

        // Diff
        else if (_state == SelectState.DifficultyList)
        {
            MapsetGroup currentMapset = _mapsets[_selectedSongIndex];

            if (current.IsKeyDown(Keys.Down) && _previousKeyboard.IsKeyUp(Keys.Down))
            {
                _selectedDifficultyIndex = (_selectedDifficultyIndex + 1) % currentMapset.Difficulties.Count;
                UpdateScrollOffset();
            }
            
            if (current.IsKeyDown(Keys.Up) && _previousKeyboard.IsKeyUp(Keys.Up))
            {
                _selectedDifficultyIndex--;
                if (_selectedDifficultyIndex < 0) _selectedDifficultyIndex = currentMapset.Difficulties.Count - 1;
                UpdateScrollOffset();
            }

            //
            if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
            {
                currentMapset.IsExpanded = false;
                _state = SelectState.SongList;
                UpdateScrollOffset();
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
        if (_backgroundPreview != null)
            spriteBatch.Draw(_backgroundPreview, _graphicsDevice.Viewport.Bounds, Color.White * 0.4f);

        int screenHeight = _graphicsDevice.Viewport.Height;
        int yOffset = ListStartY - _scrollOffset;

        for (int i = 0; i < _mapsets.Count; i++)
        {
            MapsetGroup mapset = _mapsets[i];

            if (yOffset + SongRowHeight > 0 && yOffset < screenHeight)
            {
                if (i == _selectedSongIndex && _state == SelectState.SongList)
                {
                    spriteBatch.Draw(_rectangle, new Rectangle(40, yOffset, 300, 40), Color.Blue * 0.5f);
                }
                spriteBatch.DrawString(_font, $"{mapset.Artist} - {mapset.SongTitle}", new Vector2(50, yOffset), Color.White);
            }
            yOffset += SongRowHeight;

            // Diffs
            if (mapset.IsExpanded)
            {
                for (int j = 0; j < mapset.Difficulties.Count; j++)
                {
                    if (yOffset + DiffRowHeight > 0 && yOffset < screenHeight)
                    {
                        MapMetadata diff = mapset.Difficulties[j];
                        if (j == _selectedDifficultyIndex && _state == SelectState.DifficultyList)
                        {
                            spriteBatch.Draw(_rectangle, new Rectangle(70, yOffset, 200, 30), Color.DarkRed * 0.5f);
                        }
                        spriteBatch.DrawString(_font, $"[{diff.DifficultyName}]", new Vector2(80, yOffset), Color.LightGray);
                    }
                    yOffset += DiffRowHeight;
                }
            }
        }
    }

    private void UpdateScrollOffset()
    {
        // Calculate Y pos
        int targetY = ListStartY;

        for (int i = 0; i < _mapsets.Count; i++)
        {
            if (_state == SelectState.SongList && i == _selectedSongIndex)
                break;

            targetY += SongRowHeight;

            if (_mapsets[i].IsExpanded)
            {
                int diffCount = _mapsets[i].Difficulties.Count;

                if (_state == SelectState.DifficultyList && i == _selectedSongIndex)
                {
                    targetY += _selectedDifficultyIndex * DiffRowHeight;
                    break;
                }

                targetY += diffCount * DiffRowHeight;
            }
        }

        int screenHeight = _graphicsDevice?.Viewport.Height ?? 720;

        _scrollOffset = targetY - screenHeight / 2;
    }

    private void LoadBackground()
    {
        _backgroundPreview?.Dispose();
        _backgroundPreview = null;

        string path = _mapsets[_selectedSongIndex].Difficulties[0].BackgroundPath;
        if (path == null || !System.IO.File.Exists(path)) return;

        using var stream = System.IO.File.OpenRead(path);
        _backgroundPreview = Texture2D.FromStream(_graphicsDevice, stream);
    }

    public void Dispose()
    {
        _backgroundPreview?.Dispose();
    }

    private enum SelectState
    {
        SongList,
        DifficultyList
    }
}