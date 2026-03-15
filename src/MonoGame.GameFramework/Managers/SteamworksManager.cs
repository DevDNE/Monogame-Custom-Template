using System;
using Steamworks;

namespace MonoGame.GameFramework.Managers;
public class SteamworksManager
{
  private Callback<P2PSessionRequest_t> p2pSessionRequestCallback;

  public SteamworksManager()
  {
    if (!SteamAPI.Init())
    {
      throw new Exception("Failed to initialize Steamworks.");
    }

    string name = SteamFriends.GetPersonaName();
    Console.WriteLine(name);

    p2pSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
  }
  public void Update()
  {
    SteamAPI.RunCallbacks();
  }

  public void Shutdown()
  {
    SteamAPI.Shutdown();
  }

  private void OnP2PSessionRequest(P2PSessionRequest_t callback)
  {
    SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
  }
}
