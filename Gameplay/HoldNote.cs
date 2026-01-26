using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class HoldNote : Note
{
    private readonly Texture2D _holdNoteTexture;
    private readonly float _duration;
    private readonly float _scrollSpeed;
    
    public HoldNote(float time, float duration, int lane, float scrollSpeed, Texture2D texture) : base(time, lane)
    {
        _holdNoteTexture = texture;
        _duration = duration;
        _scrollSpeed = scrollSpeed;
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        float tailLength = _duration * _scrollSpeed;
        Rectangle bodyRect = new Rectangle((int)Position.X, (int)Position.Y - (int)tailLength, _holdNoteTexture.Width, (int)tailLength);
        spriteBatch.Draw(_holdNoteTexture, bodyRect, Color.White);
    }
}