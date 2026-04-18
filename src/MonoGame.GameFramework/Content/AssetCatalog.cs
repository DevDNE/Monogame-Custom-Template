using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace MonoGame.GameFramework.Content;

public class AssetCatalog
{
  private ContentManager _content;
  private readonly Dictionary<string, string> _aliases = new();

  public void Initialize(ContentManager content)
  {
    _content = content;
  }

  public void RegisterAlias(string key, string contentPath)
  {
    _aliases[key] = contentPath;
  }

  public T Get<T>(string key)
  {
    if (_content == null)
      throw new InvalidOperationException("AssetCatalog has not been initialized with a ContentManager.");

    string path = _aliases.TryGetValue(key, out string alias) ? alias : key;
    return _content.Load<T>(path);
  }

  public bool Has(string key) => _aliases.ContainsKey(key);
}
