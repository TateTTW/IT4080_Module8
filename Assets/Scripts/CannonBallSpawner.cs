using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CannonBallSpawner : NetworkBehaviour
{
    public Rigidbody BulletPrefab;
    public float timeBetweenCannonFire = 0.5f;
    private float cannonFireCountDown = 0f;
    private float bulletSpeed = 13f;

    private void Update()
    {
        if (IsServer)
        {
            if (cannonFireCountDown > 0)
            {
                cannonFireCountDown -= Time.deltaTime;
            }
        }
    }

    [ServerRpc]
    public void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        if (cannonFireCountDown > 0)
        {
            return;
        }

        Rigidbody newBullet = Instantiate(BulletPrefab, transform.position, transform.rotation);
        newBullet.velocity = newBullet.gameObject.transform.TransformVector(new Vector3(bulletSpeed, bulletSpeed, 0));
        newBullet.gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        Destroy(newBullet.gameObject, 5);

        cannonFireCountDown = timeBetweenCannonFire;
    }
}
