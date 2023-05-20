using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMirror : MonoBehaviour
{
    private GameObject Player;
    private int DIMENSION_DIF = -20;

    private void Awake() 
    {
        Player = GameObject.FindGameObjectsWithTag("Player")[0];
    }
    
    private void FixedUpdate() 
    {
        //Copy player position at all times DIMENSION_DIF units below
        Vector3 playerPos = Player.transform.position;
        transform.position = new Vector3(playerPos.x, playerPos.y + DIMENSION_DIF, 0);        
    }

    public void OnBlink(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            DIMENSION_DIF *= -1;
    }
}
