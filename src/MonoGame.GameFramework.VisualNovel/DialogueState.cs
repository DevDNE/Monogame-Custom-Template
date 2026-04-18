using System.Collections.Generic;

namespace MonoGame.GameFramework.VisualNovel;

/// <summary>
/// Serializable save payload. Kept as simple JSON-friendly types (string,
/// int, List&lt;string&gt;) so SaveSystem's Newtonsoft round-trip is trivial.
/// </summary>
public class DialogueState
{
  public string CurrentNodeId { get; set; } = DialogueScript.StartNodeId;
  public List<string> SeenChoices { get; set; } = new();
}
