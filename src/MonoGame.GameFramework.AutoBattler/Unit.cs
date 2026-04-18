using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.AutoBattler;

public enum Side { Player, Enemy }
public enum UnitType { Warrior, Archer, Tank }

public static class UnitStats
{
  public record Stats(int MaxHp, int Attack, int Range, int Cost, Color PlayerTint, Color EnemyTint, string Name);

  public static Stats Of(UnitType t) => t switch
  {
    UnitType.Warrior => new(30, 6, 1, 10, new Color(80, 180, 255), new Color(255, 120, 100), "Warrior"),
    UnitType.Archer  => new(15, 4, 3, 15, new Color(120, 220, 140), new Color(220, 180, 110), "Archer"),
    UnitType.Tank    => new(60, 3, 1, 20, new Color(160, 150, 220), new Color(200, 100, 200), "Tank"),
    _ => throw new System.ArgumentOutOfRangeException(),
  };
}

public class Unit
{
  public UnitType Type;
  public Side Side;
  public int Col;
  public int Row;
  public int Hp;

  public UnitStats.Stats Stats => UnitStats.Of(Type);
  public bool Alive => Hp > 0;

  public Unit(UnitType type, Side side, int col, int row)
  {
    Type = type;
    Side = side;
    Col = col;
    Row = row;
    Hp = Stats.MaxHp;
  }

  public void Damage(int amount)
  {
    Hp -= amount;
    if (Hp < 0) Hp = 0;
  }
}
