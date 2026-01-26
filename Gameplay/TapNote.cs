using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class TapNote : Note
{
    private readonly Texture2D _tapNoteTexture;
    private readonly float _scrollSpeed;
    
    public TapNote(float time, int lane, float scrollSpeed, Texture2D texture) : base(time, lane)
    {
        _tapNoteTexture = texture;
        _scrollSpeed = scrollSpeed;
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_tapNoteTexture, Position, Color.White);
    }
}