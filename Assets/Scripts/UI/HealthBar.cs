using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class HealthBar : MonoBehaviour
{
    private float maxHP;
    Image barFill;
    PlayerManager player;

    private void Start() 
    {
        barFill = transform.GetChild(0).GetComponent<Image>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerManager>();
        maxHP = player.getMaxHP();
    }

    // Always place camera at the top left corner of the screen.
    void LateUpdate()
    {
        updateHealth();
    }

    public void updateHealth()
    {
        float currHealth = player.getHP();
        barFill.fillAmount = currHealth/maxHP;
    }
}
