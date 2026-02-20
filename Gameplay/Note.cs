using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public abstract class Note
{
    public float HitTime { get; protected set; }
    public Vector2 Position { get; protected set; }
    public int Lane { get; protected set; }
    public bool IsHit;  
    
    private float _distanceFromHitline;
    protected virtual float HitAnchorOffsetY => 0f;
    
    public virtual float EndTime => HitTime;

    public Note(float HitTime, int lane, float x)
    {
        this.HitTime = HitTime;
        Lane = lane;
        Position = new Vector2(0, 100);
        IsHit = false;
        Position = new Vector2(x, 100);
    }

    public virtual void Update(GameTime gameTime, float songTime, int hitLine, float scrollSpeed)
    {
        _distanceFromHitline = (HitTime - songTime) * scrollSpeed;

        const float tapHeight = 82f;               
        float anchorOffset = tapHeight / 2f;

        Position = new Vector2(Position.X, hitLine - HitAnchorOffsetY + _distanceFromHitline);
    }

    public abstract void Draw(SpriteBatch spriteBatch);
}