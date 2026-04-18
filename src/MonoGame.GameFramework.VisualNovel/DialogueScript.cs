using System.Collections.Generic;

namespace MonoGame.GameFramework.VisualNovel;

public enum Portrait { None, Alex, Morgan }

public record DialogueChoice(string Text, string NextNodeId);

public record DialogueNode(
  string Id,
  Portrait Speaker,
  string Text,
  string NextNodeId,                 // non-null if no choices
  IReadOnlyList<DialogueChoice> Choices);   // non-null if this is a branch

/// <summary>
/// Tiny story graph: meeting at a diner, one choice branch, three endings.
/// Node IDs are referenced from save files so their string identity
/// must stay stable across versions — that's a real save-system
/// constraint that only surfaces with a real consumer.
/// </summary>
public static class DialogueScript
{
  public const string StartNodeId = "intro_1";
  public const string EndNodeId = "end";

  public static readonly Dictionary<string, DialogueNode> Nodes = new()
  {
    ["intro_1"] = new("intro_1", Portrait.None, "The diner is almost empty. Fluorescent buzz, coffee that's been on the burner too long.", "intro_2", null),
    ["intro_2"] = new("intro_2", Portrait.Alex, "You're late, Morgan.", "intro_3", null),
    ["intro_3"] = new("intro_3", Portrait.Morgan, "Traffic. You know how it gets on the bridge.", "intro_4", null),
    ["intro_4"] = new("intro_4", Portrait.Alex, "Sure. So - are you in?", "choice_1", null),

    ["choice_1"] = new("choice_1", Portrait.Morgan, "You want my answer right now? Here?", null, new[]
    {
      new DialogueChoice("Yes. Tonight or never.", "path_push"),
      new DialogueChoice("Tomorrow. I need to think.", "path_wait"),
      new DialogueChoice("I can't. I'm out.", "path_out"),
    }),

    ["path_push"] = new("path_push", Portrait.Morgan, "Fine. I'm in. But if this goes sideways, we both go down.", "end_push", null),
    ["end_push"] = new("end_push", Portrait.None, "Ending: Both in. Fade to black.", "end", null),

    ["path_wait"] = new("path_wait", Portrait.Alex, "Tomorrow, then. Same table. Don't be late again.", "end_wait", null),
    ["end_wait"] = new("end_wait", Portrait.None, "Ending: Postponed. Fade to black.", "end", null),

    ["path_out"] = new("path_out", Portrait.Alex, "Then why did you even show up?", "path_out_2", null),
    ["path_out_2"] = new("path_out_2", Portrait.Morgan, "To tell you to my face. You earned that much.", "end_out", null),
    ["end_out"] = new("end_out", Portrait.None, "Ending: Out. Fade to black.", "end", null),

    ["end"] = new("end", Portrait.None, "The End.", null, null),
  };
}
