namespace MonoGame.GameFramework.AutoBattler;

// Typed event payloads for EventManager.Subscribe<T>/Publish<T>.
// FIRST consumer of the library's typed-event API across all 9 sample games.

public class UnitDamaged { public Unit Attacker; public Unit Victim; public int Amount; }
public class UnitKilled { public Unit Killer; public Unit Victim; }
public class CombatEnded { public Side Winner; }
