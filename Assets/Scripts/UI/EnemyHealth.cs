using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class EnemyHealth : MonoBehaviour
{
    private float maxHP;
    Image barFill;
    EnemyManager enemy;
    Transform enemyTransform;
    Transform healthTransform;
    Vector3 posOffset;

    private void Start() 
    {
        barFill = transform.GetChild(0).GetComponent<Image>();
        enemyTransform = transform.parent.parent;
        healthTransform = transform;
        enemy = enemyTransform.GetComponent<EnemyManager>();
        maxHP = enemy.getMaxHP();
        posOffset = new Vector3(0f, 2.75f, 0f);
    }

    // Always place camera at the top left corner of the screen.
    void LateUpdate()
    {
        updateHealth();
        // healthTransform.position = Camera.main.WorldToScreenPoint(enemyTransform.position + posOffset);
        if (enemyTransform != null)
        {
            // Get the enemy's position in world space
            Vector3 enemyPosition = enemyTransform.position;

            // Set the health bar's position to be above the enemy's position
            healthTransform.position = enemyPosition + posOffset;
        }
    }

    public void updateHealth()
    {
        float currHealth = enemy.getHP();
        barFill.fillAmount = currHealth/maxHP;
    }
}
