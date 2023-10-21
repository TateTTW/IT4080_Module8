using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;

public class Lobby : NetworkBehaviour
{
    public LobbyUi lobbyUi;
    public NetworkedPlayers networkedPlayers;

    private ulong[] clientId = new ulong[1];
    private Regex regExp = new Regex("^[a-zA-Z0-9]*$");
    void Start()
    {
        if (IsServer) {
            lobbyUi.OnStartClicked += ServerStartClicked;
            lobbyUi.ShowStart(true);
            ServerPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ServerOnNetworkedPlayersChanged;
        } else {
            lobbyUi.ShowStart(false);
            lobbyUi.OnReadyToggled += ClientOnReadyToggled;
            ClientPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ClientOnNetworkedPlayersChanged;
            NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnect;
        }

        lobbyUi.OnChangeNameClicked += OnChangeNameClicked;
    }

    private void ServerStartClicked()
    {
        NetworkManager.SceneManager.LoadScene("Arena", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void OnChangeNameClicked(string value)
    {
        UpdatePlayerNameServerRpc(value);
    }

    private void ClientOnClientDisconnect(ulong clientId)
    {
        lobbyUi.gameObject.SetActive(false);
    }

    private void ClientOnReadyToggled(bool value)
    {
        UpdateReadyServerRpc(value);
    }

    private void ClientOnNetworkedPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent)
    {
        ClientPopulateCards();
        PopulateMyInfo();
    }

    private void ServerOnNetworkedPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent)
    {
        ServerPopulateCards();
        PopulateMyInfo();
        lobbyUi.EnableStart(networkedPlayers.AllPlayersReady());
    }

    private void ServerPopulateCards()
    {
        NetworkHelper.Log($"net players: " + networkedPlayers.allNetPlayers.Count);
        lobbyUi.playerCards.Clear();
        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            PlayerCard pc = lobbyUi.playerCards.AddCard("Player");
            pc.playerName = info.playerName.ToString();
            pc.ready = info.ready;
            pc.clientId = info.clientId;
            pc.color = info.color;
            if (info.clientId == NetworkManager.LocalClientId) {
                pc.ShowKick(false);
            } else {
                pc.ShowKick(true);
            }
            pc.OnKickClicked += ServerOnKickClicked;
            pc.UpdateDisplay();
        }
    }

    private void ServerOnKickClicked(ulong clientId)
    {
        NetworkManager.DisconnectClient(clientId);
    }

    private void ClientPopulateCards()
    {
        NetworkHelper.Log($"net players: " + networkedPlayers.allNetPlayers.Count);
        lobbyUi.playerCards.Clear();
        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            PlayerCard pc = lobbyUi.playerCards.AddCard("Player");
            pc.playerName = info.playerName.ToString();
            pc.ready = info.ready;
            pc.clientId = info.clientId;
            pc.color = info.color;
            pc.ShowKick(false);
            pc.UpdateDisplay();
        }
    }

    private void PopulateMyInfo()
    {
        NetworkPlayerInfo myInfo = networkedPlayers.GetMyPlayerInfo();
        if (myInfo.clientId != ulong.MaxValue)
        {
            lobbyUi.SetPlayerName(myInfo.playerName.ToString());
        }
    }

    [ClientRpc]
    public void PlayerNameDeniedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        PopulateMyInfo();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerNameServerRpc(string value, ServerRpcParams rpcParams = default)
    {
        if (value.Length < 20 && regExp.IsMatch(value)) {
            networkedPlayers.UpdatePlayerName(rpcParams.Receive.SenderClientId, value);
        } else {
            clientId[0] = rpcParams.Receive.SenderClientId;

            ClientRpcParams clientRpcParams = default;
            clientRpcParams.Send.TargetClientIds = clientId;

            PlayerNameDeniedClientRpc(clientRpcParams);
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyServerRpc(bool value, ServerRpcParams rpcParams = default)
    {
        networkedPlayers.UpdateReady(rpcParams.Receive.SenderClientId, value);
    }
}
