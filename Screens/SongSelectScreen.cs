using System.Collections.Generic;
using System.IO;
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
    private Texture2D _coverTexture;
    private KeyboardState _previousKeyboard;
    private SelectState _state = SelectState.SongList;
    private const int SongRowHeight = 70;
    private const int DiffRowHeight = 50;
    private const int ListStartY = 50;
    private float _targetScrollOffset;
    private float _visualScrollOffset;
    private Dictionary<MapsetGroup, Texture2D> _coverTextures = new();
    
    
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

        // Load all map thumbnails
        foreach (var mapset in _mapsets)
        {
            string path = mapset.Difficulties[0].BackgroundPath;
            if (path != null && File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                _coverTextures[mapset] = Texture2D.FromStream(_graphicsDevice, stream);
            }
            else
            {
                _coverTextures[mapset] = null; // optional fallback
            }
        }

        LoadBackground(); // load the selected map's background
        UpdateScrollOffset();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState current = Keyboard.GetState();

        // If there are no songs, ESC is the only valid action
        if (_mapsets.Count == 0)
        {
            if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                ScreenManager.Instance.ChangeScreen(new MainMenuScreen());
            _previousKeyboard = current;
            return;
        }

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

        _visualScrollOffset = MathHelper.Lerp(
            _visualScrollOffset,
            _targetScrollOffset,
            0.15f);

        _previousKeyboard = current;
    }

   public void Draw(SpriteBatch spriteBatch)
{
    if (_mapsets.Count == 0)
    {
        spriteBatch.DrawString(_font, "No songs found in Assets/Songs/", new Vector2(50, 50), Color.White);
        spriteBatch.DrawString(_font, "Press ESC to go back", new Vector2(50, 100), Color.Gray);
        return;
    }

    if (_backgroundPreview != null)
        spriteBatch.Draw(_backgroundPreview, _graphicsDevice.Viewport.Bounds, Color.White * 0.35f);

    int screenHeight = _graphicsDevice.Viewport.Height;
    int screenWidth = _graphicsDevice.Viewport.Width;

    int wheelX = 0;
    int wheelWidth = (4*screenWidth) / 9;

    int thumbnailSize = SongRowHeight - 10;

    int yOffset = (int)(- _visualScrollOffset);

    for (int i = 0; i < _mapsets.Count; i++)
    {
        MapsetGroup mapset = _mapsets[i];

        if (yOffset + SongRowHeight > 0 && yOffset < screenHeight)
        {
            bool selected = i == _selectedSongIndex && _state == SelectState.SongList;

            Color panelColor = selected
                ? new Color(70,130,180)
                : new Color(40,40,40);

            Rectangle panelRect = new Rectangle(
                wheelX,
                yOffset,
                wheelWidth,
                SongRowHeight
            );

            spriteBatch.Draw(_rectangle, panelRect, panelColor);

            // Thumbnail
            if (_coverTextures.TryGetValue(mapset, out Texture2D thumb) && thumb != null)
            {
                spriteBatch.Draw(
                    thumb,
                    new Rectangle(
                        wheelX + 5,
                        yOffset + 5,
                        thumbnailSize,
                        thumbnailSize
                    ),
                    Color.White
                );
            }

            float textStartX = wheelX + thumbnailSize + 15;
            float textMaxWidth = wheelWidth - thumbnailSize - 25;

            string title = $"{mapset.Artist} - {mapset.SongTitle}";
            string truncated = TruncateText(title, textMaxWidth);

            spriteBatch.DrawString(
                _font,
                truncated,
                new Vector2(textStartX, yOffset + 10),
                Color.White
            );
        }

        yOffset += SongRowHeight;

        // DIFFICULTIES
        if (mapset.IsExpanded)
        {
            for (int j = 0; j < mapset.Difficulties.Count; j++)
            {
                if (yOffset + DiffRowHeight > 0 && yOffset < screenHeight)
                {
                    MapMetadata diff = mapset.Difficulties[j];

                    bool selectedDiff =
                        j == _selectedDifficultyIndex &&
                        _state == SelectState.DifficultyList;

                    Color diffColor = selectedDiff
                        ? new Color(150,60,60)
                        : new Color(60,60,60);

                    Rectangle diffRect = new Rectangle(
                        wheelX + 25,
                        yOffset,
                        wheelWidth - 25,
                        DiffRowHeight
                    );

                    spriteBatch.Draw(_rectangle, diffRect, diffColor);

                    float diffTextWidth = wheelWidth - 60;

                    string truncated =
                        TruncateText(diff.DifficultyName, diffTextWidth);

                    spriteBatch.DrawString(
                        _font,
                        truncated,
                        new Vector2(wheelX + 40, yOffset + 6),
                        Color.LightGray
                    );
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

        _targetScrollOffset = targetY - (screenHeight / 2) + (SongRowHeight / 2);
    }

    private void LoadBackground()
    {
        _backgroundPreview?.Dispose();
        _backgroundPreview = null;

        if (_mapsets.Count == 0) return;

        string path = _mapsets[_selectedSongIndex].Difficulties[0].BackgroundPath;
        if (path == null || !System.IO.File.Exists(path)) return;

        using var stream1 = File.OpenRead(path);
        _backgroundPreview = Texture2D.FromStream(_graphicsDevice, stream1);

        using var stream2 = File.OpenRead(path);
        _coverTexture = Texture2D.FromStream(_graphicsDevice, stream2);
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
    
    string TruncateText(string text, float maxWidth)
    {
        if (_font.MeasureString(text).X <= maxWidth)
            return text;

        while (text.Length > 0)
        {
            text = text.Substring(0, text.Length - 1);
            if (_font.MeasureString(text + "...").X <= maxWidth)
                return text + "...";
        }

        return "...";
    }
}