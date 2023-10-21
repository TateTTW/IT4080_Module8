using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkHandler : NetworkBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
    }

    private void PrintMe()
    {
        if (IsServer) {
            NetworkHelper.Log($"I AM a server! {NetworkManager.ServerClientId}");
        }
        if (IsHost) {
            NetworkHelper.Log($"I AM a host! {NetworkManager.ServerClientId}/{NetworkManager.LocalClientId}");
        }
        if (IsClient) {
            NetworkHelper.Log($"I AM a client! {NetworkManager.LocalClientId}");
        }
        if (!IsServer && !IsClient) {
            NetworkHelper.Log("I AM nothing yet.");
        }
    }

    // ------------------------
    // Client Actions
    // ------------------------
    private void OnClientStarted()
    {
        NetworkHelper.Log("Client Started!");
        NetworkManager.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnected;
        NetworkManager.OnClientStopped += ClientOnServerStopped;
    }

    private void ClientOnServerStopped(bool flag)
    {
        NetworkHelper.Log("Client Stopped!!");
        NetworkManager.OnClientConnectedCallback -= ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ClientOnClientDisconnected;
        NetworkManager.OnServerStopped -= ClientOnServerStopped;
    }

    private void ClientOnClientConnected(ulong clientId)
    {
        if (NetworkManager.LocalClientId == clientId || NetworkManager.ServerClientId == clientId)
        {
            NetworkHelper.Log($"I have connected {clientId}");
        }
    }
    private void ClientOnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.LocalClientId == clientId || NetworkManager.ServerClientId == clientId)
        {
            NetworkHelper.Log($"I have disconnected {clientId}");
        }
    }

    // ------------------------
    // Server Actions
    // ------------------------
    private void OnServerStarted()
    {
        NetworkHelper.Log("Server Started!!");
        NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        NetworkManager.OnServerStopped += ServerOnServerStopped;
    }

    private void ServerOnServerStopped(bool flag)
    {
        NetworkHelper.Log("Server stopped!!");
        NetworkManager.OnClientConnectedCallback -= ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ServerOnClientDisconnected;
        NetworkManager.OnServerStopped -= ServerOnServerStopped;
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        NetworkHelper.Log($"Client {clientId} connected to the server.");
    }
    private void ServerOnClientDisconnected(ulong clientId)
    {
        NetworkHelper.Log($"Client {clientId} disconnected from the server.");
    }

}
