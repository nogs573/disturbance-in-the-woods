using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    private LevelManager level;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public LayerMask DetectGroundLayer;

    private PlayerInputs defaultPlayerActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction blinkAction;
    private InputAction peekAction;

    private float DIMENSION_DIF;

    //for debuggin purposes
    private InputAction resetAction;
    private float startingX = 0.5f;

    private Rigidbody2D body;
    private float speed = 8f;

    private bool isGrounded = true;
    private bool isJumping = false;
    private bool willLand = false;

    private bool alreadyCanceled = false;
    private float jumpForce = 10f;
    //Number of frames after which you no longer have
    //ground underneath you when you can still jump
    private int JUMP_LENIENCY = 5;
    private int leniencyCounter = 0;

    GameObject mirror;
    Vector3 peekCamPos;
    CinemachineVirtualCamera topCam;
    CinemachineVirtualCamera bottomCam;
    CinemachineVirtualCamera peekCam;

    [SerializeField] float peekSpeed = 1f;
    private int peekDir = -1;

    GameObject[] allLights;

    private bool goingDown = true;
    public bool isPeeking = false;
    private bool isBlinking = false;

    //amount of time player should be frozen
    //if they perform a bad blink
    private float badBlinkTime = 1.5f;

    private void Awake() 
    {
        body = GetComponent<Rigidbody2D>();
        defaultPlayerActions = new PlayerInputs();
        animator.SetBool("IsAlive", true);

        level = GameObject.FindGameObjectsWithTag("LevelManager")[0].GetComponent<LevelManager>();       
        
        mirror = GameObject.FindGameObjectsWithTag("Mirror")[0];
        topCam = GameObject.FindGameObjectsWithTag("TopCam")[0].GetComponent<CinemachineVirtualCamera>();
        bottomCam = GameObject.FindGameObjectsWithTag("BottomCam")[0].GetComponent<CinemachineVirtualCamera>();
        peekCam = GameObject.FindGameObjectsWithTag("PeekCam")[0].GetComponent<CinemachineVirtualCamera>();
        allLights = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject light in allLights)
        {
            light.SetActive(false);
        }
    }

    //On start instead of awake to give time for LevelManager to calculate it.
    private void Start() 
    {
        DIMENSION_DIF = level.getDimDiff();        
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
        
        peekAction = defaultPlayerActions.Player.Peek;
        peekAction.Enable();

        resetAction = defaultPlayerActions.Player.ResetPos;
        resetAction.Enable();       
    }

    private void OnDisable() 
    {
        moveAction.Disable();
        jumpAction.Disable();
        attackAction.Disable();
        blinkAction.Disable();
        peekAction.Disable();
        resetAction.Disable();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Player attacked");
        animator.SetBool("IsAlive", false);
        animator.SetTrigger("PlayDeath");
    }

    public void OnPeek(InputAction.CallbackContext ctx)
    {
        if (ctx.started && isGrounded)
        {
            isPeeking = true;
            if (goingDown)
            {
                peekCamPos = topCam.transform.position;
                peekCam.transform.position = peekCamPos;
                peekDir = -1;
            }
            else if(!goingDown)
            {
                peekCamPos = bottomCam.transform.position;
                peekCam.transform.position = peekCamPos;
                peekDir = 1;
            }
            
            peekCam.m_Priority = 11;
        }

        if (ctx.canceled)
        {
            isPeeking = false;
            peekCam.m_Priority = 5;
        }
        //Debug.Log("Player is peeking down");
    }

    public void OnBlink(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !isPeeking)
        {
           PerformBlink();
        }        
    }

    private void PerformBlink()
    {
        isBlinking = true;
        
        //Swap places with mirror and switch active camera
        transform.position = mirror.transform.position;
        toggleCameraFollow(goingDown);
        toggleLights(goingDown);
        toggleActiveCamera(goingDown);
        goingDown = !goingDown;

        //Cast rays in all four cardinal directions. If there's ground within
        //x units in every direction, we're stuck in the ground.

        //float detectionRadius = 1f;
        int count = 0;
        float rayDistance = 0.01f;
        //Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);
        
        Vector2[] dir = {new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1)};

        for (int i=0; i<4; i++)
        {
            //Vector2 detectionPoint = playerPos + dir[i] * detectionRadius;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir[i], rayDistance, DetectGroundLayer);
            //Collider2D hit = Physics2D.OverlapCircle(detectionPoint, detectionRadius, DetectGroundLayer);
            if (hit.collider != null && hit.collider.CompareTag("Ground"))
            {
                count++;
            }
        }

        Debug.Log(count);

        if (count == 4)
        {
            Debug.Log("Stuck in ground");
            //Player.takeDamage(GROUND_DAMAGE);
            //Player.playSound("Player stuck in wall") (gasp?)
            animator.SetTrigger("DamageTaken");
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            StartCoroutine(FreezePlayer(badBlinkTime));
            //Player.immuneToDamage(1.5 sec);
            //Player.switchAnimation(blinking) "show that player is invulnerable for a little bit           
        }
    }

    IEnumerator FreezePlayer(float t)
    {
        yield return new WaitForSeconds(t);
        mirror.GetComponent<PlayerMirror>().dimensionFlip(); 
        PerformBlink();
        body.constraints = RigidbodyConstraints2D.FreezeRotation;        
    }

    //Can't use this to check the ground anymore because the ground is so slippery
    //it doesn't register as a collision.
    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if (!isBlinking)
        {        
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
            isBlinking = false;
        }                
    }

    //New variable-height jump for tighter controls
    public void OnJump(InputAction.CallbackContext ctx)
    { 
        if (ctx.started && (isGrounded || leniencyCounter <= JUMP_LENIENCY))
        {
            isJumping = true;
            animator.SetBool("IsJumping", true); 
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

        animator.SetTrigger("Respawn");
        animator.SetBool("IsAlive", true);

        goingDown = true;
        topCam.m_Follow = transform;
        bottomCam.m_Follow = mirror.transform;
        topCam.m_Priority = 10;
        bottomCam.m_Priority = 8;
    }

    private void FixedUpdate() 
    {
        Vector2 moveDir = moveAction.ReadValue<Vector2>();
        Vector2 vel = body.velocity;
        vel.x = speed * moveDir.x;
        animator.SetFloat("xSpeed", Mathf.Abs(vel.x));

        //Player is going down
        if (isJumping && vel.y < 0)
        {
            willLand = true;
        }

        if ((vel.x < 0 && !spriteRenderer.flipX) || (vel.x > 0 && spriteRenderer.flipX))
        {
            flipSprite();
        }

        if (isPeeking)
        {
            body.velocity = vel * 0; 
        }
        else
        {
            body.velocity = vel; 
        }

        //Debug.Log(vel.y);

        //No choice but to check if touching the ground here, because if 
        //we collide with a vertical surface like stairs, we will slip down
        //to the ground, but it won't trigger a second collision.
        //Cast ray beneath the player to check if they're touching the ground 
        
        float rayDistance = 1.02f;
        //float raySides = 0.52;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, DetectGroundLayer);
        isGrounded = hit.collider != null;

        if (willLand && isGrounded)
        {
            isJumping = false;
            animator.SetBool("IsJumping", false);
            willLand = false;
        }


        if (!isGrounded)
        {
            leniencyCounter += 1;           
        }
        else
        {      
            //player is on the first frame of ground as they're coming down
            
            leniencyCounter = 0;
        }

        isBlinking = false;
    }

    private void LateUpdate() 
    {
        float peekAmount = peekDir * peekSpeed * Time.deltaTime;
        if (isPeeking)
        {
            peekCam.transform.Translate(0,peekAmount,0);
        }
    }

    //--------------------------------------------------------------
    //Helper methods

    public void toggleCameraFollow(bool goingDown)
    {
        if (goingDown)
        {
            topCam.m_Follow = mirror.transform;
            bottomCam.m_Follow = transform;
        }
        else
        {
            topCam.m_Follow = transform;
            bottomCam.m_Follow = mirror.transform;
        }
    }

    public void toggleLights(bool goingDown)
    {
        foreach (GameObject light in allLights)
            light.SetActive(goingDown); //SetActive(true) if going down
    }

    public void toggleActiveCamera(bool goingDown)
    {
        if (goingDown)
        {
            topCam.m_Priority = 8;
            bottomCam.m_Priority = 10;
        }
        else
        {
            topCam.m_Priority = 10;
            bottomCam.m_Priority = 8;
        }        
    }

    private void flipSprite()
    {
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }
}
