using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    public LobbyUi lobbyUi;
    void Start()
    {
        CreateTestCards();
        lobbyUi.OnReadyToggled += TestOnReadyToggled;
        lobbyUi.OnStartClicked += TestOnStartClicked;
        lobbyUi.OnChangeNameClicked += TestOnChangeNameClicked;
    }

    private void TestOnChangeNameClicked(string obj)
    {
        NetworkHelper.Log(obj);
    }

    private void TestOnStartClicked()
    {
        lobbyUi.ShowStart(false);
    }

    private void TestOnReadyToggled(bool obj)
    {
        
    }

    private void CreateTestCards()
    {
       PlayerCard pc = lobbyUi.playerCards.AddCard("Test player 1");
        pc.color = Color.blue;
        pc.clientId = 99;
        pc.ShowKick(true);
        pc.OnKickClicked += TestOnKickClicked;
        pc.ready = true;
        pc.UpdateDisplay();
    }

    private void TestOnKickClicked(ulong obj)
    {
        NetworkHelper.Log(obj.ToString());
    }
}
