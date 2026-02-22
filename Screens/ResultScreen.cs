using System;
using System.IO;
using BetterRyn.Gameplay;
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

    public ResultScreen(GameplayScreen gameplayScreen, NoteManager notemanager)
    {
        _gameplayScreen = gameplayScreen;
        _noteManager = notemanager;
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;
        _resultScreenManager = new ResultScreenManager();
        _font = content.Load<SpriteFont>("GameFont");
        
        _resultScreenManager.LoadContent(content);
        _screenWidth = _graphicsDevice.Viewport.Width;
        _screenHeight = _graphicsDevice.Viewport.Height;
        _rankAchieved = _resultScreenManager.WhatRankingIsIt(_noteManager);
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState current = Keyboard.GetState();

        if (current.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
        {
            string songsPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Songs"
            );
            ScreenManager.Instance.ChangeScreen(new SongSelect(
                MapParser.LoadAllMaps(songsPath)));
        }

        if (current.IsKeyDown(Keys.R) && _previousKeyboard.IsKeyUp(Keys.R))
        {
            ScreenManager.Instance.ChangeScreen(new GameplayScreen(_gameplayScreen.MapFilepath,
                _gameplayScreen.SongFilepath, _gameplayScreen.BackgroundFilepath));
        }
        
        _previousKeyboard = current;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // TODO: Show the ammount of 300, 100 and misses
        
        spriteBatch.Draw(_rankAchieved, new Vector2(_screenWidth - 700, _screenHeight - 1000), Color.White);
        spriteBatch.DrawString(_font, _noteManager.HighestCombo.ToString(), new Vector2(150, _screenHeight - 150), Color.White);
        
        //TODO: Format accuracy to 2 digits
        spriteBatch.DrawString(_font, (_noteManager.Accuracy * 100).ToString(), new Vector2(400, _screenHeight - 150), Color.White);
    }
    
    public void Dispose()
    {
        
    }
}