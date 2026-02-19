using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class HoldNote : Note
{
    private readonly Texture2D _holdNoteTexture;
    private readonly float _duration;
    private readonly float _scrollSpeed;

    private bool _isBeingHeld = false;
    private bool _completed = false;
    private int _textureWidth = 192;

    public override float EndTime => HitTime + _duration;
    public bool IsBeingHeld => _isBeingHeld;
    public bool IsCompleted => _completed;
    

    public HoldNote(float time, float duration, int lane, float scrollSpeed, Texture2D texture, float x) : base(time, lane, x)
    {
        _holdNoteTexture = texture;
        _duration = duration;
        _scrollSpeed = scrollSpeed;
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        float tailLength = _duration * _scrollSpeed;
        Rectangle bodyRect = new Rectangle((int)Position.X, (int)Position.Y - (int)tailLength, _textureWidth, (int)tailLength);
        spriteBatch.Draw(_holdNoteTexture, bodyRect, Color.White);
    }
    
    public void StartHold()
    {
        _isBeingHeld = true;
        IsHit = true;
    }
    
    public void Release()
    {
        _isBeingHeld = false;
    }

    public void Complete()
    {
        _completed = true;
    }
}