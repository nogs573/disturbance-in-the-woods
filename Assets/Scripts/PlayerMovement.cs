using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInputs defaultPlayerActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction blinkAction;

    //for debuggin purposes
    private InputAction resetAction;
    private float startingX = 0.5f;
    private float DIMENSION_DIF = 20f;

    private Rigidbody2D body;
    private float speed = 8f;

    private bool isGrounded = false;

    private bool alreadyCanceled = false;
    private float jumpForce = 10f;

    GameObject mirror;
    CinemachineVirtualCamera topCam;
    CinemachineVirtualCamera bottomCam;

    private bool isBlinking = false;

    GameObject[] allLights;

    private bool isOnTop = true;


    private void Awake() 
    {
        body = GetComponent<Rigidbody2D>();
        defaultPlayerActions = new PlayerInputs();        
        
        mirror = GameObject.FindGameObjectsWithTag("Mirror")[0];
        topCam = GameObject.FindGameObjectsWithTag("TopCam")[0].GetComponent<CinemachineVirtualCamera>();
        bottomCam = GameObject.FindGameObjectsWithTag("BottomCam")[0].GetComponent<CinemachineVirtualCamera>();
        allLights = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject light in allLights)
        {
            light.SetActive(false);
        }
    }

    private void OnEnable() 
    {
        moveAction = defaultPlayerActions.Player.Move;
        moveAction.Enable();

        jumpAction = defaultPlayerActions.Player.Jump;
        jumpAction.Enable();

        attackAction = defaultPlayerActions.Player.Attack;
        attackAction.Enable();

        blinkAction = defaultPlayerActions.Player.Blink;
        blinkAction.Enable(); 

        resetAction = defaultPlayerActions.Player.ResetPos;
        resetAction.Enable();       
    }

    private void OnDisable() 
    {
        moveAction.Disable();
        jumpAction.Disable();
        attackAction.Disable();
        blinkAction.Disable();
        resetAction.Disable();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Player attacked");
    }

    public void OnBlink(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isBlinking = true;
            transform.position = mirror.transform.position;

            if (isOnTop)
            {
                topCam.m_Follow = mirror.transform;
                bottomCam.m_Follow = transform;

                foreach (GameObject light in allLights)
                {
                    light.SetActive(true);
                }

                topCam.m_Priority = 8;
                bottomCam.m_Priority = 10;
            }
            else
            {
                topCam.m_Follow = transform;
                bottomCam.m_Follow = mirror.transform;

                foreach (GameObject light in allLights)
                {
                    light.SetActive(false);
                }

                topCam.m_Priority = 10;
                bottomCam.m_Priority = 8;
            }

            isOnTop = !isOnTop;
        }        
        //Debug.Log("Player blinked");
    }

    //New variable-height jump for tighter controls
    public void OnJump(InputAction.CallbackContext ctx)
    { 
        if (ctx.started && isGrounded)
        {
            alreadyCanceled = false;
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            isGrounded = false;
        }

        //Don't want to be able to press+release jump key again
        //which sets velocity to 0. Looks bad on the way down
        if (!alreadyCanceled)
        {
            if (ctx.canceled && body.velocity.y > 0)
            {
                body.velocity = new Vector2(body.velocity.x, 0);
                alreadyCanceled = true;
            }
        }
    }

    public void OnReset(InputAction.CallbackContext ctx) 
    {
        body.position = new Vector2(startingX, 1.0f);
        mirror.transform.position = new Vector2(startingX, -1 * DIMENSION_DIF + 1);

        isOnTop = true;
        topCam.m_Follow = transform;
        bottomCam.m_Follow = mirror.transform;
        topCam.m_Priority = 10;
        bottomCam.m_Priority = 8;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("COLLISION ALERT");
        if (!isBlinking)
        {        
            if (collision.gameObject.CompareTag("Ground"))
            {                  
            }
            if (collision.gameObject.CompareTag("Enemy"))
            {
                //take damage from enemy and get pushed back
            }
        }
        else //player just blinked
        {
            if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Void"))
            {
                //Player.takeDamage(collision.gameObject.damageAmount);
                //Remove ability for player to move
                OnDisable();
            }
            else if (collision.gameObject.CompareTag("Ground"))
            {
                //Cast rays in all four cardinal directions. If there's ground within
                //x units in every direction, we're stuck in the ground.
                int count = 0;
                float rayDistance = 0.02f;
                
                Vector2[] dir = {new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1)};
                bool onBottom = false;
                for (int i=0; i<4; i++)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, dir[i], rayDistance);
                    if (hit.collider != null && hit.collider.CompareTag("Ground"))
                    {
                        if (i==3)
                            onBottom = true;
                        count++;
                    }
                }
                if (count == 4)
                {
                    Debug.Log("Stuck in ground");
                    OnDisable();
                }
            }
            isBlinking = false;
        }        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the player is no longer colliding with the ground collider
    }


    private void FixedUpdate() 
    {
        Vector2 moveDir = moveAction.ReadValue<Vector2>();
        Vector2 vel = GetComponent<Rigidbody2D>().velocity;
        vel.x = speed * moveDir.x;

        body.velocity = vel;   

        //No choice but to check if touching the ground here, because if 
        //we collide with a vertical surface like stairs, we will slip down
        //to the ground, but it won't trigger a collision.
        //Cast ray beneath the player to check if they're touching the ground 
        
        float rayDistance = 1.015f;
        //float raySides = 0.52;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance); 
        isGrounded = hit.collider != null;
    }
}
