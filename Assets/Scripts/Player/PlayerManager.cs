using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private float playerHP;
    private float maxHP = 100;
    private float ATTACK_POWER = 5;
    
    void Awake()
    {
        playerHP = maxHP;
    }

    public void takeDamage(float damage)
    {
        if (playerHP - damage >= 0)
            playerHP -= damage;
        else
            playerHP = 0;
    }

    public void gainHP(float amount)
    {
        if (playerHP + amount > maxHP)
            playerHP = maxHP;
        else
            playerHP += amount;
    }

    public float getHP()
    {
        return playerHP;
    }

    public float getMaxHP()
    {
        return maxHP;
    }

    public float getAttackPower()
    {
        return ATTACK_POWER;
    }
}
