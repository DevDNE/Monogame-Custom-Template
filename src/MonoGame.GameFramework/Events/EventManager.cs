using System;
using System.Collections.Generic;
using MonoGame.GameFramework.Events;

namespace MonoGame.GameFramework.Events;
public class EventManager
{
  private readonly Dictionary<string, EventHandler<GameEventArgs>> eventHandlers = new();
  private readonly Dictionary<Type, Delegate> typedHandlers = new();

  /// <summary>
  /// Fires after any event is dispatched — string or typed. Intended for
  /// diagnostics (debug overlay event tail, telemetry) that want to observe
  /// the bus without pre-subscribing to every event name.
  /// </summary>
  public event Action<string, object, GameEventArgs> AnyEvent;

  public void Subscribe(string eventName, EventHandler<GameEventArgs> handler)
  {
    if (!eventHandlers.ContainsKey(eventName))
    {
      eventHandlers[eventName] = handler;
    }
    else
    {
      eventHandlers[eventName] += handler;
    }
  }

  public void Unsubscribe(string eventName, EventHandler<GameEventArgs> handler)
  {
    if (eventHandlers.ContainsKey(eventName))
    {
      eventHandlers[eventName] -= handler;
    }
  }

  public void TriggerEvent(string eventName, object sender, GameEventArgs args)
  {
    if (eventHandlers.ContainsKey(eventName))
    {
      eventHandlers[eventName]?.Invoke(sender, args);
    }
    AnyEvent?.Invoke(eventName, sender, args);
  }

  public void Subscribe<T>(Action<T> handler) where T : class
  {
    if (typedHandlers.TryGetValue(typeof(T), out Delegate existing))
      typedHandlers[typeof(T)] = Delegate.Combine(existing, handler);
    else
      typedHandlers[typeof(T)] = handler;
  }

  public void Unsubscribe<T>(Action<T> handler) where T : class
  {
    if (!typedHandlers.TryGetValue(typeof(T), out Delegate existing)) return;
    Delegate updated = Delegate.Remove(existing, handler);
    if (updated == null) typedHandlers.Remove(typeof(T));
    else typedHandlers[typeof(T)] = updated;
  }

  public void Publish<T>(T payload) where T : class
  {
    if (typedHandlers.TryGetValue(typeof(T), out Delegate existing))
      ((Action<T>)existing)?.Invoke(payload);
    AnyEvent?.Invoke(typeof(T).Name, payload, new GameEventArgs(typeof(T).Name));
  }
}
