using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public abstract class Note
{
    public float HitTime { get; protected set; }
    public Vector2 Position { get; protected set; }
    public int Lane { get; protected set; }
    public bool IsHit;  

    private readonly float _speed = 10f;
    private float _distanceFromHitline;

    public Note(float HitTime, int lane, float x)
    {
        this.HitTime = HitTime;
        Lane = lane;
        Position = new Vector2(0, 100);
        IsHit = false;
        Position = new Vector2(x, 100);
    }

    public virtual void Update(GameTime gameTime, float songTime, int hitLine)
    {
        _distanceFromHitline = (HitTime - songTime) * _speed;
        Vector2 tempPosition = Position;
        tempPosition.Y = hitLine + _distanceFromHitline;
        Position = tempPosition;
    }

    public abstract void Draw(SpriteBatch spriteBatch);
}