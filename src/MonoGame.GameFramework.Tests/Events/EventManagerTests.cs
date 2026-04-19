using FluentAssertions;
using MonoGame.GameFramework.Events;
using Xunit;

namespace MonoGame.GameFramework.Tests.Events;

public class EventManagerTests
{
  private class Ping { public string Msg; }

  // ---- String-keyed API ----

  [Fact]
  public void StringEvent_SubscribeAndTrigger_InvokesHandler()
  {
    EventManager em = new();
    string received = null;
    em.Subscribe("hit", (_, e) => received = e.Message);
    em.TriggerEvent("hit", null, new GameEventArgs("ouch"));
    received.Should().Be("ouch");
  }

  [Fact]
  public void StringEvent_MultipleSubscribers_AllInvoked()
  {
    EventManager em = new();
    int a = 0, b = 0;
    em.Subscribe("tick", (_, _) => a++);
    em.Subscribe("tick", (_, _) => b++);
    em.TriggerEvent("tick", null, new GameEventArgs(""));
    a.Should().Be(1);
    b.Should().Be(1);
  }

  [Fact]
  public void StringEvent_Unsubscribe_StopsInvocations()
  {
    EventManager em = new();
    int fired = 0;
    void Handler(object _, GameEventArgs __) { fired++; }
    em.Subscribe("tick", Handler);
    em.TriggerEvent("tick", null, new GameEventArgs(""));
    em.Unsubscribe("tick", Handler);
    em.TriggerEvent("tick", null, new GameEventArgs(""));
    fired.Should().Be(1);
  }

  [Fact]
  public void StringEvent_UnknownName_IsNoOp()
  {
    EventManager em = new();
    em.Invoking(x => x.TriggerEvent("unknown", null, new GameEventArgs("")))
      .Should().NotThrow();
  }

  // ---- Typed API ----

  [Fact]
  public void TypedEvent_SubscribeAndPublish_InvokesHandler()
  {
    EventManager em = new();
    Ping received = null;
    em.Subscribe<Ping>(p => received = p);
    Ping sent = new() { Msg = "hello" };
    em.Publish(sent);
    received.Should().BeSameAs(sent);
  }

  [Fact]
  public void TypedEvent_MultipleSubscribers_AllInvoked()
  {
    EventManager em = new();
    int a = 0, b = 0;
    em.Subscribe<Ping>(_ => a++);
    em.Subscribe<Ping>(_ => b++);
    em.Publish(new Ping());
    a.Should().Be(1);
    b.Should().Be(1);
  }

  [Fact]
  public void TypedEvent_Unsubscribe_RemovesOnlyMatchingDelegate()
  {
    EventManager em = new();
    int a = 0, b = 0;
    void HandlerA(Ping _) => a++;
    void HandlerB(Ping _) => b++;
    em.Subscribe<Ping>(HandlerA);
    em.Subscribe<Ping>(HandlerB);
    em.Unsubscribe<Ping>(HandlerA);
    em.Publish(new Ping());
    a.Should().Be(0);
    b.Should().Be(1);
  }

  [Fact]
  public void TypedEvent_PublishWithNoSubscribers_IsNoOp()
  {
    EventManager em = new();
    em.Invoking(x => x.Publish(new Ping())).Should().NotThrow();
  }

  [Fact]
  public void TypedEvent_SegregatedByType()
  {
    EventManager em = new();
    int pings = 0;
    em.Subscribe<Ping>(_ => pings++);
    em.Publish("string payload");
    pings.Should().Be(0);
  }

  // ---- AnyEvent hook ----

  [Fact]
  public void AnyEvent_FiresForStringTrigger_EvenWithoutSubscribers()
  {
    EventManager em = new();
    string capturedName = null;
    em.AnyEvent += (name, _, _) => capturedName = name;
    em.TriggerEvent("hit", null, new GameEventArgs("msg"));
    capturedName.Should().Be("hit");
  }

  [Fact]
  public void AnyEvent_FiresForTypedPublish_WithTypeName()
  {
    EventManager em = new();
    string capturedName = null;
    em.AnyEvent += (name, _, _) => capturedName = name;
    em.Publish(new Ping());
    capturedName.Should().Be(nameof(Ping));
  }

  [Fact]
  public void AnyEvent_FiresAfterRegularSubscribers()
  {
    EventManager em = new();
    var order = new System.Collections.Generic.List<string>();
    em.Subscribe("hit", (_, _) => order.Add("sub"));
    em.AnyEvent += (_, _, _) => order.Add("any");
    em.TriggerEvent("hit", null, new GameEventArgs(""));
    order.Should().Equal("sub", "any");
  }
}
