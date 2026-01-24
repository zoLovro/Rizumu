using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Gameplay;

public abstract class Note
{
    public float HitTime { get; protected set; }
    public Vector2 Position { get; protected set; }
    public int Lane { get; protected set; }
    public bool IsHit { get; protected set; }

    private readonly float _speed = 100f;
    private readonly int _hitLine = 50;
    private float _distanceFromHitline;

    public Note(float HitTime, int lane)
    {
        this.HitTime = HitTime;
        Lane = lane;
        Position = new Vector2(0, 100);
        IsHit = false;
    }

    public virtual void Update(float gameTime, float songTime)
    {
        _distanceFromHitline = (HitTime - songTime) * _speed;
        Vector2 tempPosition = Position;
        tempPosition.Y = _hitLine + _distanceFromHitline;
        Position = tempPosition;
    }

    public abstract void Draw(SpriteBatch spriteBatch);
}