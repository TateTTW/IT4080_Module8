using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpChangeColor : BasePowerUp
{
    protected override bool ApplyToPlayer(Player player)
    {
        player.playerColorNetVar.Value = Color.black;
        return true;
    }
}
