namespace MonoGame.GameFramework.AutoBattler;

/// <summary>
/// Mutable cross-state model. Shared by Shop / Combat / PostCombat so
/// round + HP + gold + board survive state transitions without needing
/// to serialize through ChangeState arguments.
/// </summary>
public class GameModel
{
  public const int StartingHeroHp = 30;
  public const int StartingGold = 15;

  public Board Board { get; } = new();
  public int Round = 1;
  public int Gold = StartingGold;
  public int PlayerHeroHp = StartingHeroHp;
  public int EnemyHeroHp = StartingHeroHp;

  public void Reset()
  {
    Board.Reset();
    Round = 1;
    Gold = StartingGold;
    PlayerHeroHp = StartingHeroHp;
    EnemyHeroHp = StartingHeroHp;
  }
}
