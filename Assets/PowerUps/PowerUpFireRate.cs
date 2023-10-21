using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpFireRate : BasePowerUp
{
    public float timeBetweenCannonFire = 0.1f;
    protected override bool ApplyToPlayer(Player player)
    {
        player.frontRightCannonBallSpawner.timeBetweenCannonFire = timeBetweenCannonFire;
        player.frontLeftCannonBallSpawner.timeBetweenCannonFire = timeBetweenCannonFire;
        player.rearRightCannonBallSpawner.timeBetweenCannonFire = timeBetweenCannonFire;
        player.rearLeftCannonBallSpawner.timeBetweenCannonFire = timeBetweenCannonFire;

        return true;
    }
}
