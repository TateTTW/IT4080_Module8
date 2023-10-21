using Unity.Netcode;
using UnityEngine;

public class NetworkedPlayers : NetworkBehaviour
{
    public NetworkList<NetworkPlayerInfo> allNetPlayers;

    private int colorIndex = 0;
    private Color[] playerColors = new Color[]
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.cyan
    };

    private void Awake()
    {
        allNetPlayers = new NetworkList<NetworkPlayerInfo>();
    }
    void Start()
    {
        if (IsServer)
        {
            ServerStart();
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void ServerStart()
    {
        NetworkPlayerInfo info = new NetworkPlayerInfo();
        info.ready = true;
        info.color = NextColor();
        info.playerName = "The Host";
        allNetPlayers.Add(info); 
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        NetworkPlayerInfo info = new NetworkPlayerInfo(clientId);
        info.ready = false;
        info.color = NextColor();
        info.playerName = $"Player {clientId}";
        allNetPlayers.Add(info);
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        NetworkHelper.Log($"Player Disconnected: " + clientId);
        int idx = FindPlayerIndex(clientId);
        if (idx != -1)
        {
            allNetPlayers.RemoveAt(idx);
        }
    }

    private Color NextColor()
    {
        Color color = playerColors[colorIndex];
        colorIndex++;
        if (colorIndex > playerColors.Length - 1)
        {
            colorIndex = 0;
        }
        return color;
    }

    public int FindPlayerIndex(ulong clientId)
    {
        var idx = 0;
        var found = false;

        while (idx < allNetPlayers.Count && !found)
        {
            if (allNetPlayers[idx].clientId == clientId)
            {
                found = true;
            }
            else
            {
                idx += 1;
            }
        }

        if (!found)
        {
            idx = -1;
        }

        return idx;
    }

    public void UpdatePlayerName(ulong clientId, string playerName)
    {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1)
        {
            return;
        }

        NetworkPlayerInfo info = allNetPlayers[idx];
        info.playerName = playerName;
        allNetPlayers[idx] = info;
    }

    public void UpdateReady(ulong clientId, bool ready)
    {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1)
        {
            return;
        }

        NetworkPlayerInfo info = allNetPlayers[idx];
        info.ready = ready;
        allNetPlayers[idx] = info;
    }

    public NetworkPlayerInfo GetMyPlayerInfo()
    {
        NetworkPlayerInfo toReturn = new NetworkPlayerInfo(ulong.MaxValue);

        int idx = FindPlayerIndex(NetworkManager.LocalClientId);
        if (idx != -1)
        {
            toReturn = allNetPlayers[idx];
        }

        return toReturn;
    }

    public bool AllPlayersReady()
    {
        bool theyAre = true;
        int idx = 0;
        while (theyAre && idx < allNetPlayers.Count) {
            theyAre = allNetPlayers[idx].ready;
            idx += 1;
        }
        return theyAre;
    }
}
