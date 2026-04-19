using System;
using System.Collections.Generic;

namespace MonoGame.GameFramework.Pooling;

/// <summary>
/// Wraps an <see cref="ObjectPool{T}"/> with a live list + rent/cull loop.
/// Replaces the (pool, List&lt;T&gt;) + hand-rolled update-and-remove pattern
/// that repeats per-entity-type across pooled-entity games.
/// </summary>
public sealed class PooledEntitySet<T> where T : class
{
  private readonly ObjectPool<T> _pool;
  private readonly Func<T, bool> _isAlive;
  private readonly List<T> _live = new();

  public IReadOnlyList<T> Live => _live;
  public int Count => _live.Count;
  public ObjectPool<T> Pool => _pool;

  public PooledEntitySet(ObjectPool<T> pool, Func<T, bool> isAlive)
  {
    _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    _isAlive = isAlive ?? throw new ArgumentNullException(nameof(isAlive));
  }

  public T Rent()
  {
    T item = _pool.Rent();
    _live.Add(item);
    return item;
  }

  /// <summary>
  /// Iterate live items in reverse. For each, invoke <paramref name="update"/>;
  /// if the predicate then reports the item as not alive, invoke optional
  /// <paramref name="onCull"/> (so consumers can tally side-effects like
  /// score/lives), return it to the pool, and remove from the live list.
  /// </summary>
  public void UpdateAndCull(Action<T> update, Action<T> onCull = null)
  {
    for (int i = _live.Count - 1; i >= 0; i--)
    {
      T item = _live[i];
      update(item);
      if (!_isAlive(item))
      {
        onCull?.Invoke(item);
        _pool.Return(item);
        _live.RemoveAt(i);
      }
    }
  }

  /// <summary>
  /// Cull without updating — for items whose Alive flag is flipped outside
  /// this set (e.g. by collision resolution elsewhere in the frame).
  /// </summary>
  public void Cull(Action<T> onCull = null)
  {
    for (int i = _live.Count - 1; i >= 0; i--)
    {
      T item = _live[i];
      if (!_isAlive(item))
      {
        onCull?.Invoke(item);
        _pool.Return(item);
        _live.RemoveAt(i);
      }
    }
  }

  public void ReturnAll()
  {
    foreach (T item in _live) _pool.Return(item);
    _live.Clear();
  }
}
