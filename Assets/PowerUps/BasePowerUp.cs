using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class BasePowerUp : NetworkBehaviour
{
    public void ServerPickUp(Player player)
    {
        if (IsServer)
        {
            if (ApplyToPlayer(player))
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    protected abstract bool ApplyToPlayer(Player player);
}
