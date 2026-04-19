using FluentAssertions;
using MonoGame.GameFramework.Pooling;
using Xunit;

namespace MonoGame.GameFramework.Tests.Pooling;

public class PooledEntitySetTests
{
  private sealed class Bullet
  {
    public bool Alive = true;
    public int Ticks;
  }

  private static PooledEntitySet<Bullet> MakeSet(int prewarm = 0) =>
    new(new ObjectPool<Bullet>(() => new Bullet(), prewarm: prewarm, onReturn: b => b.Alive = false),
        isAlive: b => b.Alive);

  [Fact]
  public void Rent_AddsToLiveList()
  {
    PooledEntitySet<Bullet> set = MakeSet();
    Bullet b = set.Rent();
    b.Alive = true;
    set.Count.Should().Be(1);
    set.Live[0].Should().BeSameAs(b);
  }

  [Fact]
  public void UpdateAndCull_RemovesDeadReturnsToPool()
  {
    PooledEntitySet<Bullet> set = MakeSet(prewarm: 2);
    Bullet a = set.Rent();
    Bullet b = set.Rent();
    a.Alive = true;
    b.Alive = true;

    set.UpdateAndCull(x => { x.Ticks++; if (x.Ticks > 0 && x == b) x.Alive = false; });

    set.Count.Should().Be(1);
    set.Live[0].Should().BeSameAs(a);
    set.Pool.AvailableCount.Should().BeGreaterOrEqualTo(1); // b returned
  }

  [Fact]
  public void Cull_InvokesOnCull()
  {
    PooledEntitySet<Bullet> set = MakeSet();
    Bullet dead = set.Rent();
    dead.Alive = false;
    int cullCalls = 0;
    set.Cull(_ => cullCalls++);
    cullCalls.Should().Be(1);
    set.Count.Should().Be(0);
  }

  [Fact]
  public void ReturnAll_ReturnsEveryLiveItem()
  {
    PooledEntitySet<Bullet> set = MakeSet();
    set.Rent();
    set.Rent();
    set.Rent();
    set.ReturnAll();
    set.Count.Should().Be(0);
    set.Pool.AvailableCount.Should().Be(3);
  }

  [Fact]
  public void UpdateAndCull_CalledRepeatedly_LeavesNoLeak()
  {
    PooledEntitySet<Bullet> set = MakeSet(prewarm: 4);
    for (int cycle = 0; cycle < 5; cycle++)
    {
      for (int i = 0; i < 4; i++) { Bullet b = set.Rent(); b.Alive = true; }
      set.UpdateAndCull(x => x.Alive = false);
    }
    set.Count.Should().Be(0);
    set.Pool.AvailableCount.Should().Be(4);
  }
}
