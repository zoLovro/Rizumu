using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public class TapNote : Note
{
    private readonly Texture2D _tapNoteTexture;
    
    public TapNote(float time, int lane, Texture2D texture) : base(time, lane)
    {
        _tapNoteTexture = texture;
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_tapNoteTexture, Position, Color.White);
    }
}