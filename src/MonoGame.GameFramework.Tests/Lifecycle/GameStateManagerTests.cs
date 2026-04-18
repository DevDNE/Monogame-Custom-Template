using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Lifecycle;
using Xunit;

namespace MonoGame.GameFramework.Tests.Lifecycle;

public class GameStateManagerTests
{
  private class FakeState : GameState
  {
    public int EnteredCount;
    public int LeavingCount;
    public int ObscuringCount;
    public int RevealedCount;

    public override void Entered() { EnteredCount++; IsActive = true; }
    public override void Leaving() { LeavingCount++; }
    public override void Obscuring() { ObscuringCount++; IsActive = false; }
    public override void Revealed() { RevealedCount++; IsActive = true; }
    public override void Update(GameTime gameTime) { }
  }

  [Fact]
  public void PushState_EmptyStack_FiresEnteredOnly()
  {
    GameStateManager m = new();
    FakeState s = new();
    m.PushState(s);
    s.EnteredCount.Should().Be(1);
    s.RevealedCount.Should().Be(0);
  }

  [Fact]
  public void PushState_NonEmpty_FiresObscuringOnOldAndEnteredOnNew()
  {
    GameStateManager m = new();
    FakeState a = new();
    FakeState b = new();
    m.PushState(a);
    m.PushState(b);
    a.ObscuringCount.Should().Be(1);
    b.EnteredCount.Should().Be(1);
  }

  [Fact]
  public void PopState_FiresLeavingOnPoppedAndRevealedOnNewTop()
  {
    GameStateManager m = new();
    FakeState a = new();
    FakeState b = new();
    m.PushState(a);
    m.PushState(b);
    m.PopState();
    b.LeavingCount.Should().Be(1);
    a.RevealedCount.Should().Be(1);
  }

  [Fact]
  public void PopState_EmptyStack_DoesNotThrow()
  {
    GameStateManager m = new();
    m.Invoking(x => x.PopState()).Should().NotThrow();
  }

  [Fact]
  public void ChangeState_FiresLeavingOnOldAndEnteredOnNew_WithoutRevealed()
  {
    GameStateManager m = new();
    FakeState a = new();
    FakeState b = new();
    m.PushState(a);
    m.ChangeState(b);
    a.LeavingCount.Should().Be(1);
    b.EnteredCount.Should().Be(1);
    b.RevealedCount.Should().Be(0);
  }

  // Regression test: AutoBattler's CombatState.Update calls ChangeState on the
  // manager when the battle ends. That mutation during a foreach over the
  // internal Stack<GameState> threw InvalidOperationException. Fix was to
  // snapshot the stack before iterating.
  private class SelfTransitioningState : GameState
  {
    public GameStateManager Manager;
    public GameState Replacement;
    public int UpdateCount;
    public override void Entered() { IsActive = true; }
    public override void Leaving() { }
    public override void Obscuring() { IsActive = false; }
    public override void Revealed() { IsActive = true; }
    public override void Update(GameTime gameTime)
    {
      UpdateCount++;
      if (UpdateCount == 1) Manager.ChangeState(Replacement);
    }
  }

  [Fact]
  public void Update_AllowsStateToChangeStateMidIteration()
  {
    GameStateManager m = new();
    FakeState next = new();
    SelfTransitioningState first = new() { Manager = m, Replacement = next };
    m.PushState(first);

    m.Invoking(x => x.Update(new GameTime())).Should().NotThrow();

    first.UpdateCount.Should().Be(1);
    next.EnteredCount.Should().Be(1);
    m.PeekState().Should().BeSameAs(next);
  }
}
