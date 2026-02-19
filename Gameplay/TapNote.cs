using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class TapNote : Note
{
    private readonly Texture2D _tapNoteTexture;
    private readonly float _scrollSpeed;
    private int _textureWidth = 192;
    private int _textureHeight = 82;
    
    public TapNote(float time, int lane, float scrollSpeed, Texture2D texture, float x) : base(time, lane, x)
    {
        _tapNoteTexture = texture;
        _scrollSpeed = scrollSpeed;
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle destination = new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            _textureWidth,
            _textureHeight
        );

        spriteBatch.Draw(_tapNoteTexture, destination, Color.White);
    }
}