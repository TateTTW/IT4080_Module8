using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Xml.Serialization;
using System.Linq;

public class ChatServer : NetworkBehaviour
{
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];
    private ulong[] dmClientId = new ulong[1];

    public ChatUi chatUi;

    // Start is called before the first frame update
    void Start()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
            if (IsHost) {
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}");
            } else {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
            }
        } else {
            DisplayMessageLocally(SYSTEM_ID, "You are a client");
        }
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        SendUserConnectedMessageServerRpc(clientId);
        ServerSendWelcomeMessage(clientId);
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        SendUserDisconnectedMessageServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendUserConnectedMessageServerRpc(ulong clientId)
    {
        ReceiveChatMessageClientRpc($"User {clientId} has entered the chat.", SYSTEM_ID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendUserDisconnectedMessageServerRpc(ulong clientId)
    {
        ReceiveChatMessageClientRpc($"User {clientId} has left the chat.", SYSTEM_ID);
    }

    private void DisplayMessageLocally(ulong from, string message)
    {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;

        if (from == NetworkManager.LocalClientId) {
            fromStr = "you";
            textColor = Color.magenta;
        } else if (from == SYSTEM_ID) {
            fromStr = "SYS";
            textColor = Color.green;
        }

        chatUi.addEntry(fromStr, message, textColor);
    }

    private void OnChatUiMessageEntered(string message)
    {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        if (message.StartsWith("@")) {
            int spaceIndex = message.IndexOf(" ");
            string clientIdStr = message.Substring(0, spaceIndex > 0 ? spaceIndex : message.Length).Replace("@", "");

            ulong toClientId = 0;
            if (ulong.TryParse(clientIdStr, out toClientId) && NetworkManager.ConnectedClients.ContainsKey(toClientId)) {
                ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);
            } else {
                ServerSendDirectMessage($"User {clientIdStr} could not be found.", SYSTEM_ID, serverRpcParams.Receive.SenderClientId);
            }
            
        } else {
            ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

        [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(from, message);
    }

    private void ServerSendDirectMessage(string message, ulong from, ulong to)
    {
        dmClientIds[0] = from;
        dmClientIds[1] = to;

        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;

        ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
    }

    private void ServerSendWelcomeMessage(ulong to)
    {
        dmClientId[0] = to;

        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientId;

        ReceiveChatMessageClientRpc($"Welcome to the chat, user {to}.", SYSTEM_ID, rpcParams);
    }
}
