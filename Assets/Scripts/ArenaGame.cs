using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArenaGame : NetworkBehaviour
{
    private NetworkedPlayers networkedPlayers;

    public Player playerPrefab;
    public Player hostPrefab;

    public Camera arenaCamera;

    private int positionIndex = 0;
    private Vector3[] startPositions = new Vector3[]
    {
        new Vector3(4, 6, 0),
        new Vector3(-4, 6, 0),
        new Vector3(0, 6, 4),
        new Vector3(0, 6, -4)
    };

    void Start()
    {
        if (IsClient && arenaCamera != null)
        {
            arenaCamera.enabled = false;
            arenaCamera.GetComponent<AudioListener>().enabled = false;
        }

        if (IsServer)
        {
            networkedPlayers = GameObject.Find("NetworkedPlayers").GetComponent<NetworkedPlayers>();
            SpawnPlayers();
        }
    }

    private void SpawnPlayers()
    {
        
        foreach(NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            Debug.Log("clientId: " + info.clientId);
            Player playerSpawn = Instantiate(playerPrefab, NextPosition(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.clientId);
            playerSpawn.playerColorNetVar.Value = info.color;
        }
    }

    private Vector3 NextPosition()
    {
        Vector3 pos = startPositions[positionIndex];
        positionIndex += 1;
        if (positionIndex > startPositions.Length - 1)
        {
            positionIndex = 0;
        }
        return pos;
    }

}
