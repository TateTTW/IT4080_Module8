using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using System;

public class Player : NetworkBehaviour
{
    public float speed;
    public NetworkVariable<Color> playerColorNetVar = new NetworkVariable<Color>(Color.red);
    public NetworkVariable<int> playerScoreNetVar = new NetworkVariable<int>(0);
    public CannonBallSpawner frontRightCannonBallSpawner;
    public CannonBallSpawner frontLeftCannonBallSpawner;
    public CannonBallSpawner rearRightCannonBallSpawner;
    public CannonBallSpawner rearLeftCannonBallSpawner;

    private Camera playerCamera;
    private GameObject playerFlag;

    private void OnNetworkInit()
    {
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;
        playerFlag = transform.GetChild(0).GetChild(0).gameObject;

        ApplyPlayerColor();
        playerColorNetVar.OnValueChanged += OnPlayerColorChanged;

        if (IsClient)
        {
            playerScoreNetVar.OnValueChanged += ClientOnScoreValueChanged;
        }
    }

    private void ClientOnScoreValueChanged(int previousValue, int newValue)
    {
        if (IsOwner)
        {
            NetworkHelper.Log(this, $"My score is {playerScoreNetVar.Value}");
        }
    }

    private void OnPlayerColorChanged(Color previousValue, Color newValue)
    {
        ApplyPlayerColor();
    }

    private void Awake()
    {
        NetworkHelper.Log(this, "Awake");
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkHelper.Log(this, "Start");
    }

    public override void OnNetworkSpawn()
    {
        NetworkHelper.Log(this, "OnNetworkSpawn");
        OnNetworkInit();
        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            OwnerHandleInput();
        }
    }

    private void OwnerHandleInput()
    {
        float adjSpeed = Mathf.Abs(speed) * Time.deltaTime;
        float rotateSpeed = adjSpeed * (float)1.3;
        float rotationSpeed = Input.GetKey(KeyCode.W) ? rotateSpeed * (float)1.5 : rotateSpeed;

        Vector3 translation = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            translation = new Vector3(0, 0, adjSpeed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rotation = new Vector3(0, rotationSpeed, 0);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rotation = new Vector3(0, -rotationSpeed, 0);
        }

        if (translation != Vector3.zero || rotation != Vector3.zero)
        {
            MoveServerRpc(translation, rotation);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            frontLeftCannonBallSpawner.FireServerRpc();
            rearLeftCannonBallSpawner.FireServerRpc();

        }

        if (Input.GetButtonDown("Fire2"))
        {
            frontRightCannonBallSpawner.FireServerRpc();
            rearRightCannonBallSpawner.FireServerRpc();
        }
    }

    private void ApplyPlayerColor()
    {
        NetworkHelper.Log(this, $"Apply color {playerColorNetVar.Value}");
        playerFlag.GetComponent<MeshRenderer>().material.color = playerColorNetVar.Value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            NetworkHelper.Log($"You triggered {other.gameObject.tag}");
            if (other.gameObject.CompareTag("Cannonball"))
            {
                ulong ownerId = other.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                NetworkHelper.Log(this, $"Hit by {other.gameObject.name} owned by {ownerId}");
                Player shooter = NetworkManager.Singleton.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
                shooter.playerScoreNetVar.Value += 1;
                Destroy(other.gameObject);
            }
            else if (other.gameObject.CompareTag("PowerUp"))
            {
                other.GetComponent<BasePowerUp>().ServerPickUp(this);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            ServerHandleCollision(collision);
        }
    }

    private void ServerHandleCollision(Collision collision)
    {
        NetworkHelper.Log($"You collided with {collision.gameObject.tag}");
        if (collision.gameObject.CompareTag("Cannonball"))
        {
            ulong ownerId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            NetworkHelper.Log(this, $"Hit by {collision.gameObject.name} owned by {ownerId}");
            Player shooter = NetworkManager.Singleton.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
            shooter.playerScoreNetVar.Value += 1;
            Destroy(collision.gameObject);
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 translation, Vector3 rotation)
    {
        ulong clientId = GetComponent<NetworkObject>().OwnerClientId;

        Transform boatTransform = transform.Find("boat").transform;
        Vector3 boatPosition = boatTransform.position;
        Vector3 nextPosition = boatPosition += boatTransform.TransformVector(translation);

        if (NetworkManager.ServerClientId != clientId && (nextPosition.x > 25 || nextPosition.x < -25 || nextPosition.z > 25 || nextPosition.z < -25))
        {
            return;
        }

        transform.Find("boat").Translate(translation);
        transform.Find("boat").Rotate(rotation);

        Transform cameraTransform = transform.Find("Camera").transform;
        cameraTransform.position = boatPosition += boatTransform.TransformVector(new Vector3(0, 10, -20));
        cameraTransform.rotation = boatTransform.rotation;
    }
}
