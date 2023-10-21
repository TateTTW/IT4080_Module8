using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    public Button startButton;
    public TMPro.TMP_Text statusLabel;

    // Start is called before the first frame update
    void Start()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Lobby";

        startButton.onClick.AddListener(OnStartButtonClicked);

        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
    }

    private void OnStartButtonClicked()
    {
        StartGame();
    }

    private void OnClientStarted()
    {
        if (!IsHost)
        {
            statusLabel.text = "Waiting for game to start";
        }
    }

    private void OnServerStarted()
    {
        GoToLobby();
        /*startButton.gameObject.SetActive(true);
        statusLabel.text = "Press Start";*/
    }

    public void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("Arena", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void GoToLobby()
    {
        NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
