using System;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Debugging;
public class PerformanceMonitor : GameComponent
{
    private TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
    private TimeSpan _timer = TimeSpan.Zero;
    private int _framesCounter = 0;

    public float Fps { get; private set; }
    public float MemoryUsageMb { get; private set; }

    public PerformanceMonitor(Game game) : base(game)
    {
    }

    public override void Update(GameTime gameTime)
    {
        _framesCounter++;

        _timer += gameTime.ElapsedGameTime;
        if (_timer > _oneSecond)
        {
            Fps = _framesCounter;
            _framesCounter = 0;
            _timer -= _oneSecond;
            MemoryUsageMb = GC.GetTotalMemory(false) / 1024f / 1024f;
        }

        base.Update(gameTime);
    }
}
