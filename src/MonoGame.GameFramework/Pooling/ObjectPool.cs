using System;
using System.Collections.Generic;

namespace MonoGame.GameFramework.Pooling;

public class ObjectPool<T> where T : class
{
  private readonly Stack<T> _available = new();
  private readonly Func<T> _factory;
  private readonly Action<T> _onRent;
  private readonly Action<T> _onReturn;

  public int AvailableCount => _available.Count;

  public ObjectPool(Func<T> factory, int prewarm = 0, Action<T> onRent = null, Action<T> onReturn = null)
  {
    _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    _onRent = onRent;
    _onReturn = onReturn;
    for (int i = 0; i < prewarm; i++)
    {
      _available.Push(_factory());
    }
  }

  public T Rent()
  {
    T item = _available.Count > 0 ? _available.Pop() : _factory();
    _onRent?.Invoke(item);
    return item;
  }

  public void Return(T item)
  {
    if (item == null) return;
    _onReturn?.Invoke(item);
    _available.Push(item);
  }

  public void Clear() => _available.Clear();
}
