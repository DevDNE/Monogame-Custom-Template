using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Roguelike;

public abstract class Actor
{
  public int Col;
  public int Row;
  public int Hp;
  public int MaxHp;
  public int AttackPower;
  public Color Tint;
  public bool Alive => Hp > 0;

  protected Actor(int col, int row, int maxHp, int attackPower, Color tint)
  {
    Col = col; Row = row;
    Hp = MaxHp = maxHp;
    AttackPower = attackPower;
    Tint = tint;
  }

  public void Damage(int amount)
  {
    Hp -= amount;
    if (Hp < 0) Hp = 0;
  }
}

public class PlayerActor : Actor
{
  public PlayerActor(int col, int row) : base(col, row, maxHp: 30, attackPower: 6, tint: new Color(220, 220, 240)) { }
}

public class MonsterActor : Actor
{
  public string Name;
  public MonsterActor(int col, int row, string name, int maxHp, int attack, Color tint)
    : base(col, row, maxHp, attack, tint)
  {
    Name = name;
  }
}
