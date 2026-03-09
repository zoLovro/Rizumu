using System;
using System.IO;
using BetterRyn.Gameplay;
using BetterRyn.Logic;
using BetterRyn.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BetterRyn.Screens;

public class ResultScreen : IScreen
{
    private ContentManager _content;
    private GraphicsDevice _graphicsDevice;
    private ResultScreenManager _resultScreenManager;
    private KeyboardState _previousKeyboard;
    private GameplayScreen _gameplayScreen;
    private NoteManager _noteManager;
    private Texture2D _rankAchieved;
    private SpriteFont _font;
    private int _screenWidth;
    private int _screenHeight;

    private Texture2D _pixel;
    private Texture2D _backgroundTexture;
    private string _songTitle = "Unknown Title";
    private string _artist = "Unknown Artist";
    private string _difficulty = "";

    // Layout constants — everything is expressed as fractions of screen size
    // so it scales across resolutions.
    private const float PanelAlpha      = 0.55f;
    private const float BgDimAlpha      = 0.65f;

    public ResultScreen(GameplayScreen gameplayScreen, NoteManager notemanager)
    {
        _gameplayScreen = gameplayScreen;
        _noteManager = notemanager;
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;
        _screenWidth  = graphicsDevice.Viewport.Width;
        _screenHeight = graphicsDevice.Viewport.Height;

        _font = content.Load<SpriteFont>("GameFont");

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Load background from gameplay
        if (!string.IsNullOrEmpty(_gameplayScreen.BackgroundFilepath) &&
            File.Exists(_gameplayScreen.BackgroundFilepath))
        {
            using var stream = File.OpenRead(_gameplayScreen.BackgroundFilepath);
            _backgroundTexture = Texture2D.FromStream(graphicsDevice, stream);
        }

        // Parse song metadata from the .osu file
        ParseMapMetadata(_gameplayScreen.MapFilepath);

        _resultScreenManager = new ResultScreenManager();
        _resultScreenManager.LoadContent(content);
        _rankAchieved = _resultScreenManager.WhatRankingIsIt(_noteManager);
    }

    private void ParseMapMetadata(string mapPath)
    {
        if (mapPath == null || !File.Exists(mapPath)) return;
        foreach (var line in File.ReadAllLines(mapPath))
        {
            var parts = line.Split(':', 2);
            if (parts.Length < 2) continue;
            string key   = parts[0].Trim();
            string value = parts[1].Trim();
            switch (key)
            {
                case "Title":   _songTitle  = value; break;
                case "Artist":  _artist     = value; break;
                case "Version": _difficulty = value; break;
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState current = Keyboard.GetState();

        if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
        {
            string songsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Songs");
            ScreenManager.Instance.ChangeScreen(new SongSelectScreen(MapParser.LoadAllMaps(songsPath)));
        }

        if (current.IsKeyDown(Keys.R) && _previousKeyboard.IsKeyUp(Keys.R))
        {
            ScreenManager.Instance.ChangeScreen(new GameplayScreen(
                _gameplayScreen.MapFilepath,
                _gameplayScreen.SongFilepath,
                _gameplayScreen.BackgroundFilepath));
        }

        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // BG
        var fullscreen = new Rectangle(0, 0, _screenWidth, _screenHeight);
        if (_backgroundTexture != null)
            spriteBatch.Draw(_backgroundTexture, fullscreen, Color.White);
        else
            spriteBatch.Draw(_pixel, fullscreen, new Color(15, 15, 25));

        // Dim
        spriteBatch.Draw(_pixel, fullscreen, Color.Black * BgDimAlpha);

        // Song info
        int headerH = (int)(_screenHeight * 0.10f);
        var headerRect = new Rectangle(0, 0, _screenWidth, headerH);
        spriteBatch.Draw(_pixel, headerRect, Color.Black * 0.70f);

        // Accent line below header
        spriteBatch.Draw(_pixel, new Rectangle(0, headerH - 3, _screenWidth, 3), new Color(255, 102, 170));

        // Song title + artist 
        string titleLine  = $"{_artist} - {_songTitle}";
        string diffLine   = $"[{_difficulty}]";
        Vector2 titlePos  = new Vector2(40, headerH * 0.15f);
        Vector2 diffPos   = new Vector2(40, headerH * 0.58f);
        spriteBatch.DrawString(_font, titleLine, titlePos + Vector2.One, Color.Black * 0.6f);
        spriteBatch.DrawString(_font, titleLine, titlePos, Color.White);
        spriteBatch.DrawString(_font, diffLine,  diffPos,  new Color(200, 200, 200));

        // Main layout
        // Left panel: rank image   |   Right panel: stats
        int contentTop  = headerH + (int)(_screenHeight * 0.04f);
        int contentH    = (int)(_screenHeight * 0.74f);
        int margin      = (int)(_screenWidth  * 0.03f);

        int leftW  = (int)(_screenWidth * 0.30f);
        int rightX = margin + leftW + margin;
        int rightW = _screenWidth - rightX - margin;

        // Left panel background
        var leftPanel = new Rectangle(margin, contentTop, leftW, contentH);
        spriteBatch.Draw(_pixel, leftPanel, Color.Black * PanelAlpha);
        DrawBorder(spriteBatch, leftPanel, new Color(255, 102, 170), 2);

        // Right panel background
        var rightPanel = new Rectangle(rightX, contentTop, rightW, contentH);
        spriteBatch.Draw(_pixel, rightPanel, Color.Black * PanelAlpha);
        DrawBorder(spriteBatch, rightPanel, new Color(100, 100, 140), 2);

        // ── Rank image ────────────────────────────────────────────────────────
        if (_rankAchieved != null)
        {
            int rankSize  = (int)(Math.Min(leftW, contentH) * 0.72f);
            int rankX     = leftPanel.X + (leftPanel.Width  - rankSize) / 2;
            int rankY     = leftPanel.Y + (leftPanel.Height - rankSize) / 2;
            spriteBatch.Draw(_rankAchieved, new Rectangle(rankX, rankY, rankSize, rankSize), Color.White);
        }

        // ── Score (top of right panel) ────────────────────────────────────────
        int rInner = rightX + 30;
        int rTop   = contentTop + 25;

        string scoreLabel = "SCORE";
        string scoreValue = _noteManager.Score.ToString();

        spriteBatch.DrawString(_font, scoreLabel, new Vector2(rInner, rTop), new Color(180, 180, 180));
        rTop += 28;
        spriteBatch.DrawString(_font, scoreValue, new Vector2(rInner, rTop) + Vector2.One, Color.Black * 0.5f);
        spriteBatch.DrawString(_font, scoreValue, new Vector2(rInner, rTop), Color.White);
        rTop += 44;

        // Divider
        spriteBatch.Draw(_pixel, new Rectangle(rInner, rTop, rightW - 60, 1), new Color(100, 100, 140));
        rTop += 14;

        // ── Accuracy + Combo (side by side) ───────────────────────────────────
        string accLabel   = "ACCURACY";
        string accValue   = (_noteManager.Accuracy * 100).ToString("F2") + "%";
        string comboLabel = "MAX COMBO";
        string comboValue = _noteManager.HighestCombo.ToString() + "x";

        int halfRight = rightW / 2 - 30;

        spriteBatch.DrawString(_font, accLabel,   new Vector2(rInner,              rTop), new Color(180, 180, 180));
        spriteBatch.DrawString(_font, comboLabel, new Vector2(rInner + halfRight,  rTop), new Color(180, 180, 180));
        rTop += 26;

        Color accColor   = AccuracyColor(_noteManager.Accuracy * 100);
        spriteBatch.DrawString(_font, accValue,   new Vector2(rInner,             rTop) + Vector2.One, Color.Black * 0.5f);
        spriteBatch.DrawString(_font, accValue,   new Vector2(rInner,             rTop), accColor);
        spriteBatch.DrawString(_font, comboValue, new Vector2(rInner + halfRight, rTop) + Vector2.One, Color.Black * 0.5f);
        spriteBatch.DrawString(_font, comboValue, new Vector2(rInner + halfRight, rTop), new Color(255, 220, 80));
        rTop += 44;

        // Divider
        spriteBatch.Draw(_pixel, new Rectangle(rInner, rTop, rightW - 60, 1), new Color(100, 100, 140));
        rTop += 18;

        // ── Hit breakdown ─────────────────────────────────────────────────────
        DrawHitRow(spriteBatch, rInner, rTop, "300  PERFECT", _noteManager.HitNotes,     new Color(100, 200, 255));
        rTop += 46;
        DrawHitRow(spriteBatch, rInner, rTop, "100  GOOD",    _noteManager.HitGoodNotes, new Color(100, 230, 130));
        rTop += 46;
        DrawHitRow(spriteBatch, rInner, rTop, "  0  MISS",    _noteManager.MissedNotes,  new Color(255, 100, 100));

        // ── Footer hint ───────────────────────────────────────────────────────
        int footerY = _screenHeight - (int)(_screenHeight * 0.07f);
        spriteBatch.Draw(_pixel, new Rectangle(0, footerY - 2, _screenWidth, 2), new Color(255, 102, 170));
        spriteBatch.Draw(_pixel, new Rectangle(0, footerY, _screenWidth, _screenHeight - footerY), Color.Black * 0.70f);

        string hint = "[ESC] Song Select          [R] Retry";
        Vector2 hintSize = _font.MeasureString(hint);
        spriteBatch.DrawString(_font, hint,
            new Vector2((_screenWidth - hintSize.X) / 2f, footerY + 8),
            new Color(200, 200, 200));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void DrawHitRow(SpriteBatch sb, int x, int y, string label, int count, Color color)
    {
        // Colored label
        sb.DrawString(_font, label, new Vector2(x, y), color);

        // Right-aligned count
        string countStr = count.ToString();
        Vector2 countSize = _font.MeasureString(countStr);
        float countX = x + 380 - countSize.X; // right-align within 380px
        sb.DrawString(_font, countStr, new Vector2(countX, y) + Vector2.One, Color.Black * 0.5f);
        sb.DrawString(_font, countStr, new Vector2(countX, y), Color.White);
    }

    private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color, int thickness)
    {
        sb.Draw(_pixel, new Rectangle(rect.X,                         rect.Y,                          rect.Width, thickness),  color); // top
        sb.Draw(_pixel, new Rectangle(rect.X,                         rect.Bottom - thickness,         rect.Width, thickness),  color); // bottom
        sb.Draw(_pixel, new Rectangle(rect.X,                         rect.Y,                          thickness,  rect.Height), color); // left
        sb.Draw(_pixel, new Rectangle(rect.Right - thickness,         rect.Y,                          thickness,  rect.Height), color); // right
    }

    private Color AccuracyColor(double acc)
    {
        if (acc >= 100.0) return new Color(255, 220, 80);   // gold  — SS
        if (acc >=  95.0) return new Color(255, 180, 60);   // orange — S
        if (acc >=  90.0) return new Color(100, 230, 130);  // green  — A
        if (acc >=  80.0) return new Color(100, 180, 255);  // blue   — B
        if (acc >=  70.0) return new Color(200, 200, 200);  // grey   — C
        return new Color(255, 100, 100);                     // red    — D
    }

    public void Dispose()
    {
        _backgroundTexture?.Dispose();
        _pixel?.Dispose();
    }
}