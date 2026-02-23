using System.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BetterRyn.Managers;

public class HealthManager
{
    private float _healthAmount = 100f;
    private const float MaxHealth = 100f;
    private Texture2D _healthBarTexture;
    private Texture2D _healthBarBg;
    
    public float HealthAmmount => _healthAmount;
    public bool isDead => _healthAmount <= 0;

    public HealthManager()
    {
    }

    public void LoadContent(ContentManager content)
    {
        _healthBarTexture = content.Load<Texture2D>("scorebar-colour@2x");
        _healthBarBg = content.Load<Texture2D>("scorebar-bg@2x");
    }
    

    public void HpLossCycle(GameTime gameTime)
    {
        _healthAmount -= 0.05f * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 16f;
        ClampHealth();
    }

    public void AddHealth(float amount)
    {
        _healthAmount += amount;
        ClampHealth();
    }

    public void SubtractHealth(float amount)
    {
        _healthAmount -= amount;
        ClampHealth();
    }
    
    private void ClampHealth()
    {
        if (_healthAmount > MaxHealth) _healthAmount = MaxHealth;
        if (_healthAmount < 0) _healthAmount = 0;
    }
    
    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        if (_healthBarTexture == null || _healthBarBg == null) return;

        float healthPercent = _healthAmount / MaxHealth;
        int sourceWidth = (int)(_healthBarTexture.Width * healthPercent);
        Rectangle sourceRect = new Rectangle(0, 0, sourceWidth, _healthBarTexture.Height);
        
        spriteBatch.Draw(_healthBarBg, position, Color.White);
        spriteBatch.Draw(_healthBarTexture, position, sourceRect, Color.White, 0f, 
            Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
    }
}