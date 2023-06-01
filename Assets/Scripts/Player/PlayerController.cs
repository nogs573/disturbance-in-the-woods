using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    private LevelManager level;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public LayerMask DetectUpperLayer;
    public LayerMask DetectLowerLayer;

    private Tilemap whichTilemap;
    private Tilemap upperTilemap;
    private Tilemap lowerTilemap;

    PlayerManager PlayerManager;
    ParticleSystem blastAttack;
    AudioSource blastSound;
    AudioSource blinkSound;
    AudioSource hurtSound;

    private Vector3 checkpointPos;
    private Vector3 startingPos;


    ParticleSystem blinkEffect;

    bool facingRight = true;

    private bool onUpper = true;

    private bool isInvuln = true;
    private float INVULN_LIMIT = 1.5f; //1 second of invuln after getting hit
    private float invulnTimer = 0;

    private PlayerInputs defaultPlayerActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction blinkAction;
    private InputAction peekAction;    

    private float DIMENSION_DIF;

    //for debuggin purposes
    private InputAction resetAction;

    private Rigidbody2D body;
    private float speed = 8f;

    private bool playerIsDead = false;
    private bool beingHurt = false;
    private bool willDie = false;

    private bool isKnockedBack = false;
    private float KNOCKBACK_TIME = 1f;
    private float knockbackTimer = 0;
    private float maxXKnockback = 5f;
    private float maxYKnockback = 4f;
    private bool knockRight = false;

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

    //GameObject[] allLights;

    public bool isPeeking = false;
    private bool isBlinking = false;

    //amount of time player should be frozen
    //if they perform a bad blink
    private float badBlinkTime = 1.5f;

    private void Awake() 
    {
        body = GetComponent<Rigidbody2D>();
        defaultPlayerActions = new PlayerInputs();

        level = GameObject.FindGameObjectsWithTag("LevelManager")[0].GetComponent<LevelManager>();       
        
        mirror = GameObject.FindGameObjectsWithTag("Mirror")[0];
        topCam = GameObject.FindGameObjectsWithTag("TopCam")[0].GetComponent<CinemachineVirtualCamera>();
        bottomCam = GameObject.FindGameObjectsWithTag("BottomCam")[0].GetComponent<CinemachineVirtualCamera>();
        peekCam = GameObject.FindGameObjectsWithTag("PeekCam")[0].GetComponent<CinemachineVirtualCamera>();
        //allLights = GameObject.FindGameObjectsWithTag("Light");

        GameObject[] terrains = GameObject.FindGameObjectsWithTag("Ground");
        upperTilemap = terrains[0].GetComponent<Tilemap>();
        lowerTilemap = terrains[1].GetComponent<Tilemap>();

        PlayerManager = GetComponent<PlayerManager>();

        blastAttack = GameObject.FindWithTag("BlastAttack").GetComponent<ParticleSystem>();
        blinkEffect = GameObject.FindWithTag("BlinkEffect").GetComponent<ParticleSystem>();
        blastAttack.Pause();
        blinkEffect.Pause();
        blastSound = blastAttack.GetComponent<AudioSource>();
        blinkSound = blinkEffect.GetComponent<AudioSource>();
        hurtSound = transform.gameObject.GetComponent<AudioSource>();

    }

    //On start instead of awake to give time for LevelManager to calculate it.
    private void Start() 
    {       
        animator.SetBool("IsAlive", true);
        DIMENSION_DIF = 21.45f;

        // foreach (GameObject light in allLights)
        // {
        //     light.SetActive(false);
        // }   

        // blastAttack.Pause();
        // blinkEffect.Pause();

        startingPos = body.position;
        checkpointPos = startingPos;
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

    public void OnQuit(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            Application.Quit();        
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            flipAttack(facingRight);
            animator.SetTrigger("Attacked");
            blastAttack.Play();   
            blastSound.Play();         
        }
    }

    public void OnPeek(InputAction.CallbackContext ctx)
    {
        if (ctx.started && isGrounded)
        {
            isPeeking = true;
            if (onUpper)
            {
                peekCamPos = topCam.transform.position;
                peekCam.transform.position = peekCamPos;
                peekDir = -1;
            }
            else
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
        if (!playerIsDead)
        {
            if (ctx.started && !isPeeking)
            {
                PerformBlink();
                animator.SetTrigger("Blinked");
            }  
        }      
    }

    private void PerformBlink()
    {
        blinkEffect.Emit(30);
        blinkSound.Play();
        isBlinking = true;       
        
        //Swap places with mirror and switch active camera
        transform.position = mirror.transform.position;
        toggleCameraFollow(onUpper);
        //toggleLights(onUpper);
        toggleActiveCamera(onUpper);
        onUpper = !onUpper;

        bool blinkedInEnemy = false;
        bool stuckInGround = false;

        float rayDistance = 0.01f;
        Vector2[] dirs = {Vector2.up, Vector2.down, Vector2.left, Vector2.right};
        int count = 0;

        while (count < 4 && !blinkedInEnemy)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirs[count], rayDistance);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                blinkedInEnemy = true;
            count++;
        }

        if (!blinkedInEnemy)
        {
            Vector3 playerPos = transform.position;
            if (!onUpper)
            {
                whichTilemap = lowerTilemap;
            }
            else
            {
                whichTilemap = upperTilemap; 
            }

            //Can't raycast for tilemap colliders inside the ground
            //using a composite collider, so we get the closest two
            //tiles to the shape of the player. If there is a tile
            //in either of those spots, we're stuck in the ground.
            float adjustY = 0;
            if (whichTilemap == lowerTilemap)
            {
                adjustY = DIMENSION_DIF;
            }

            Vector3Int topCheck = new Vector3Int(Mathf.RoundToInt(playerPos.x - 0.5f), Mathf.RoundToInt(playerPos.y + 0.5f + adjustY), 0);
            Vector3Int bottomCheck = new Vector3Int(Mathf.RoundToInt(playerPos.x - 0.5f), Mathf.RoundToInt(playerPos.y - 0.5f + adjustY), 0);

            stuckInGround = whichTilemap.GetTile(topCheck) != null || whichTilemap.GetTile(bottomCheck) != null; 
        }       

        if (stuckInGround || blinkedInEnemy)
        {
            PlayerManager.takeDamage(10);
            //Player.playSound("Player stuck in wall") (gasp?)
            //Debug.Log("GOT HERE");
            animator.SetBool("BeingHurt", true);
            //animator.SetTrigger("DamageTaken");
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            // StartCoroutine(HurtPlayer());
            StartCoroutine(FreezePlayer(badBlinkTime));
            //Player.immuneToDamage(1.5 sec);
            //Player.switchAnimation(blinking) "show that player is invulnerable for a little bit           
        }
    }

    IEnumerator HurtPlayer() 
    {
        beingHurt = true;
        hurtSound.Play();
        yield return new WaitForSeconds(0.25f);
        animator.SetBool("BeingHurt", false);
        beingHurt = false;
    }

    IEnumerator FreezePlayer(float t)
    {       
        beingHurt = true;
        hurtSound.Play();
        yield return new WaitForSeconds(0.25f);
        animator.SetBool("BeingHurt", false);
        beingHurt = false;
        animator.SetBool("Frozen", true);
        yield return new WaitForSeconds(t);    
        animator.SetBool("Frozen", false);
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
            if (!isInvuln)
            {
                GameObject other = collision.gameObject;
                if (other.CompareTag("Enemy"))
                {
                    EnemyManager currEnemy = other.GetComponent<EnemyManager>();
                    if (currEnemy.getAlive())
                    {
                        float hurtX = body.transform.position.x - other.transform.position.x;
                        if (hurtX >= 0)
                            knockRight = true;
                        else
                            knockRight = false;
                        animator.SetBool("BeingHurt", true);
                        isInvuln = true;
                        StartCoroutine(HurtPlayer());
                        PlayerManager.takeDamage(currEnemy.getAttackPower()); 
                        isKnockedBack = true;           
                        other.gameObject.GetComponent<PatrolController>().setPause();  
                    }  
                }
            }
        }
        else //player just blinked
        {
            if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Void"))
            {
                //Player.takeDamage(collision.gameObject.damageAmount);
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
            animator.SetBool("Jump_Up", true); 
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
                animator.SetBool("Jump_Down", true);
                animator.SetBool("Jump_Up", false);
                alreadyCanceled = true;
            }
        }
    }

    public void OnReset(InputAction.CallbackContext ctx) 
    {
        animator.SetBool("IsDead", false);        
        animator.SetTrigger("Respawn");  
        if (playerIsDead)
        {
            checkpointPos = startingPos;
            PlayerManager.setHP(PlayerManager.getMaxHP());   
            playerIsDead = false;
        }
        
        body.position = checkpointPos;
        
        unlockPlayer(); 

        //toggleLights(false);

        if (!onUpper)
            mirror.GetComponent<PlayerMirror>().dimensionFlip();

        onUpper = true;
        topCam.m_Follow = transform;
        bottomCam.m_Follow = mirror.transform;
        topCam.m_Priority = 10;
        bottomCam.m_Priority = 8;
    }

    IEnumerator KillPlayer() 
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("IsDying", false);
        animator.SetBool("IsDead", true);
    }

    private void FixedUpdate() 
    {        
        Vector2 moveDir = moveAction.ReadValue<Vector2>();
        Vector2 vel = body.velocity;

        if (checkPlayerDead() && !playerIsDead)
        {   
            willDie = true;
            playerIsDead = true;
        }
        if (willDie && !beingHurt)
        {
            willDie = false;
            animator.SetBool("IsDying", true);  
            StartCoroutine(KillPlayer());
        }

        if (isInvuln)
        {
            invulnTimer += Time.deltaTime;
            if (invulnTimer >= INVULN_LIMIT)
            {
                isInvuln = false;
                invulnTimer = 0;
            }
        }

        if (isKnockedBack)
        {
            float yKnock = MapRangeToPi(knockbackTimer);
            float yKnockback = Mathf.Sin(yKnock) * maxYKnockback;
            float xKnockback = maxXKnockback;

            if (knockRight == false) 
            {
                xKnockback *= -1;
            }

            Vector2 knockbackForce = new Vector2(xKnockback, yKnockback);
            vel = knockbackForce;     

            knockbackTimer += Time.deltaTime;
            if (knockbackTimer >= KNOCKBACK_TIME)
            {
                isKnockedBack = false;
                knockbackTimer = 0;
            }
        }
        else
        {       
            vel.x = speed * moveDir.x;            
        }

        animator.SetFloat("xSpeed", Mathf.Abs(vel.x));

        //Player is going down
        if (isJumping && vel.y < 0)
        {
            willLand = true;
            animator.SetBool("Jump_Down", true);
            animator.SetBool("Jump_Up", false);
        }

        if (!playerIsDead && ((vel.x < 0 && !spriteRenderer.flipX) || (vel.x > 0 && spriteRenderer.flipX)))
        {
            flipSprite();
        }

        //If we're peeking, player can't move
        if (isPeeking)
        {
            body.velocity = vel * 0;
        }
        else
        {
            body.velocity = vel;
        }

        if (playerIsDead && isGrounded)
        {
            lockPlayer();
        }

        //No choice but to check if touching the ground here, because if 
        //we collide with a vertical surface like stairs, we will slip down
        //to the ground, but it won't trigger a second collision.
        //Cast ray beneath the player to check if they're touching the ground         
        float rayDistance = 1.02f;
        //float raySides = 0.52;
        LayerMask whichLayer;
        if (onUpper)
            whichLayer = DetectUpperLayer;
        else
            whichLayer = DetectLowerLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, whichLayer);
        isGrounded = hit.collider != null;

        if (willLand && isGrounded)
        {
            isJumping = false;
            animator.SetBool("Jump_Down", false);
            willLand = false;
        }


        if (!isGrounded)
        {
            leniencyCounter += 1;           
        }
        else
        {                  
            leniencyCounter = 0;
        }
    }

    private void LateUpdate() 
    {
        float peekAmount = peekDir * peekSpeed * Time.deltaTime;
        if (isPeeking)
        {
            peekCam.transform.Translate(0,peekAmount,0);
        }

        if (isBlinking)
            isBlinking = false;
    }

    //--------------------------------------------------------------
    //Helper methods

    public void SetCheckpoint(Vector3 pos)
    {
        checkpointPos = pos;
    }

    public void lockPlayer()
    {
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }
    
    public void unlockPlayer()
    {
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public bool getPlayerDead()
    {
        return playerIsDead;
    }

    public bool getPlayerHurt()
    {
        return beingHurt;
    }

    public void toggleCameraFollow(bool onUpper)
    {
        if (onUpper)
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

    // public void toggleLights(bool onUpper)
    // {
    //     foreach (GameObject light in allLights)
    //         light.SetActive(onUpper); //SetActive(true) if going down
    // }

    public void toggleActiveCamera(bool onUpper)
    {
        if (onUpper)
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
        facingRight = !facingRight;
        if (blastAttack.isEmitting)
        {
            blastAttack.Stop();
        }
    }

    private void flipAttack(bool facingRight)
    {
        Transform blast = blastAttack.transform;
        if (facingRight)
        {
            if (blast.rotation.eulerAngles.z == 180)
            {
                blast.rotation = Quaternion.Euler(0f, 0f, 0f);
                blast.position = new Vector3(body.position.x + 0.5f, body.position.y - 0.3f, 0f);
            }   
        }
        else
        {
            if (blast.rotation.eulerAngles.z == 0)
            {
                blast.rotation = Quaternion.Euler(0f, 0f, 180f);
                blast.position = new Vector3(body.position.x - 0.5f, body.position.y - 0.3f, 0f);
            }
        }
        
    }

    public float MapRangeToPi(float value)
    {
        float mappedValue = Mathf.Lerp(Mathf.PI/2, 3*Mathf.PI/2, value);
        return mappedValue;
    }

    public bool checkPlayerDead()
    {
        return PlayerManager.getHP() <= 0;
    }
}
