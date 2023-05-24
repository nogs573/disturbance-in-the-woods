using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public LayerMask CanDetectPlayer;

    //Cone of vision for the enemy
    public bool detectPlayer(float visionRange, float coneAngle, int rayCount, bool facingRight)
    {
        bool playerFound = false;
        float angleIncrement = coneAngle / rayCount;
        //Which direction to aim the cone
        Vector2 facingDir = transform.right;
        if (!facingRight)
            facingDir *= -1;
            

        for (int i=0; i<= rayCount; i++)
        {
            float currentAngle = transform.eulerAngles.z - coneAngle / 2 + i * angleIncrement;

            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * facingDir;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, visionRange, CanDetectPlayer);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
                playerFound = true;
        }

        return playerFound;
    }
}
