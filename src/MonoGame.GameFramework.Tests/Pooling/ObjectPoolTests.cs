using FluentAssertions;
using MonoGame.GameFramework.Pooling;
using Xunit;

namespace MonoGame.GameFramework.Tests.Pooling;

public class ObjectPoolTests
{
  private class Bullet { public int Hits; }

  [Fact]
  public void Rent_EmptyPool_CreatesNewInstance()
  {
    ObjectPool<Bullet> pool = new(() => new Bullet());
    Bullet b = pool.Rent();
    b.Should().NotBeNull();
    pool.AvailableCount.Should().Be(0);
  }

  [Fact]
  public void Return_StoresForReuse()
  {
    ObjectPool<Bullet> pool = new(() => new Bullet());
    Bullet b = pool.Rent();
    pool.Return(b);
    pool.AvailableCount.Should().Be(1);
    pool.Rent().Should().BeSameAs(b);
  }

  [Fact]
  public void Prewarm_PopulatesAvailable()
  {
    ObjectPool<Bullet> pool = new(() => new Bullet(), prewarm: 5);
    pool.AvailableCount.Should().Be(5);
  }

  [Fact]
  public void Rent_InvokesOnRentCallback()
  {
    int rented = 0;
    ObjectPool<Bullet> pool = new(() => new Bullet(), onRent: _ => rented++);
    pool.Rent();
    pool.Rent();
    rented.Should().Be(2);
  }

  [Fact]
  public void Return_InvokesOnReturnCallback()
  {
    int returned = 0;
    ObjectPool<Bullet> pool = new(() => new Bullet(), onReturn: b => { b.Hits = 0; returned++; });
    Bullet b = pool.Rent();
    b.Hits = 7;
    pool.Return(b);
    returned.Should().Be(1);
    b.Hits.Should().Be(0);
  }

  [Fact]
  public void Return_NullItem_IsIgnored()
  {
    ObjectPool<Bullet> pool = new(() => new Bullet());
    pool.Return(null);
    pool.AvailableCount.Should().Be(0);
  }

  [Fact]
  public void Clear_EmptiesPool()
  {
    ObjectPool<Bullet> pool = new(() => new Bullet(), prewarm: 3);
    pool.Clear();
    pool.AvailableCount.Should().Be(0);
  }
}
