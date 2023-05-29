using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private float enemyHP;
    [SerializeField] float maxHP;
    private float ATTACK_POWER = 25;

    private bool isAlive = true;
    
    void Awake()
    {
        enemyHP = maxHP;
    }

    void OnParticleCollision(GameObject other) 
    {
        takeDamage(0.5f);        
    }

    public void takeDamage(float damage)
    {
        if (enemyHP - damage > 0)
            enemyHP -= damage;
        else
        {
            enemyHP = 0;
            isAlive = false;
        }
    }

    public bool getAlive() { return isAlive; }

    public void gainHP(float amount)
    {
        if (enemyHP + amount > maxHP)
            enemyHP = maxHP;
        else
            enemyHP += amount;
    }

    public void setHP(float hp)
    {
        enemyHP = hp;
    }

    public float getHP()
    {
        return enemyHP;
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
