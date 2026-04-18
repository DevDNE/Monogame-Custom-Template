using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.UI;
public class UIManager
{
  private readonly MouseManager _mouseManager;
  private readonly Dictionary<string, List<SpriteSheet>> uiGroups = new();
  private readonly Dictionary<SpriteSheet, Action> clickHandlers = new();

  public SpriteSheet FocusedElement { get; private set; }
  public SpriteSheet HoveredElement { get; private set; }

  public UIManager(MouseManager mouseManager)
  {
    _mouseManager = mouseManager;
  }

  public void AddUIElement(string group, SpriteSheet uiElement)
  {
    if (!uiGroups.ContainsKey(group))
    {
      uiGroups[group] = new List<SpriteSheet>();
    }
    uiGroups[group].Add(uiElement);
  }

  public void RemoveUIElement(string group, SpriteSheet uiElement)
  {
    if (uiGroups.TryGetValue(group, out List<SpriteSheet> list))
    {
      list.Remove(uiElement);
    }
    clickHandlers.Remove(uiElement);
    if (FocusedElement == uiElement) FocusedElement = null;
    if (HoveredElement == uiElement) HoveredElement = null;
  }

  public void OnClick(SpriteSheet element, Action handler)
  {
    clickHandlers[element] = handler;
  }

  public void RemoveClickHandler(SpriteSheet element)
  {
    clickHandlers.Remove(element);
  }

  public void SetFocus(SpriteSheet element) => FocusedElement = element;
  public void ClearFocus() => FocusedElement = null;

  public SpriteSheet GetElementAt(Vector2 position)
  {
    foreach (List<SpriteSheet> group in uiGroups.Values)
    {
      for (int i = group.Count - 1; i >= 0; i--)
      {
        if (group[i].DestinationFrame.Contains(position)) return group[i];
      }
    }
    return null;
  }

  public void Update(GameTime gameTime)
  {
    Vector2 mouse = _mouseManager.GetMousePosition();
    HoveredElement = GetElementAt(mouse);

    if (_mouseManager.WasLeftMouseButtonPressed())
    {
      if (HoveredElement != null)
      {
        FocusedElement = HoveredElement;
        if (clickHandlers.TryGetValue(HoveredElement, out Action handler)) handler();
      }
      else
      {
        FocusedElement = null;
      }
    }
  }
}
