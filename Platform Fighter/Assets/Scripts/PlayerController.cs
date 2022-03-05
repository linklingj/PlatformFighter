using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float jumpForce;
    [SerializeField] private float shortHopForce;
    [SerializeField] private float doubleJumpForce;
    [SerializeField] private float dashForce;
    [Range(0,.3f)] [SerializeField] private float movementSmoothing;
    [SerializeField] private LayerMask whatIsGround;
	[SerializeField] private Collider2D groundCheck;				
	[SerializeField] private Collider2D ceilingCheck;							
	[SerializeField] private Collider2D crouchDisableCollider;
    [SerializeField] private float runSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private float airMomentum;
    [SerializeField] private float fastFallMultiply;
    [SerializeField] private float gravity;
    public Vector3 resetPos;
    private bool grounded;
    private bool airdodging = false;
    private bool doubleJump;
    private bool airdodgeAvailable;
    private bool fastFalling = false;
    private Rigidbody2D rigidbody2D;
    private bool facingRight = true;
    private Vector2 velocity = Vector2.zero;
	private bool wasCrouching = false;
    private bool wasDown;
    private bool wasJump;
    private bool wasDodge;
    private int jumpState; //0 no jump  1 shorthop  2 fullhop
    public Collision2D groundTouching;
    public Animator animator;
    private string currentState;
    private int animationStateType; //0 free  1 ground  2 air  3 lock  4 hardlock
    private int movementStateType; //0 free  1 stuck
    
    [Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;
    [System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent OnCrouchEvent;

    private void Awake() 
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.gravityScale = gravity;
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }
    private void Start() 
    {
        animationStateType = 0;
        movementStateType = 0;
        currentState = "Sandbert_idle";
        transform.position = resetPos;
        rigidbody2D.velocity = Vector2.zero;
    }
    private void FixedUpdate()
    {
        if(airdodging)
            rigidbody2D.gravityScale = 0f;
        else
            rigidbody2D.gravityScale = gravity;
    }
    public void Move(float move,float vMove, bool crouch, bool jump, bool dodge)
    {
        if(movementStateType == 0)
        {
            if(grounded)
            {
                fastFalling = false;
                if(crouch)
                {
                    if(groundTouching.gameObject.CompareTag("Ground") || groundTouching == null)
                    {
                        if(!wasCrouching)
                        {
                            wasCrouching = true;
                            OnCrouchEvent.Invoke(true);
                        }
                    }
                    else if(groundTouching.gameObject.CompareTag("Platform") && wasDown == false)
                    {
                        ChangeAnimationState("Sandbert_pratFall");
                        grounded = false;
                        animationStateType = 2;
                        groundTouching.gameObject.GetComponent<PlatformController>().StartCoroutine("P1Drop");
                    }
                    
                }
                else
                {
                    if(wasCrouching)
                    {
                        wasCrouching = false;
                        OnCrouchEvent.Invoke(false);
                    }
                }
                Vector2 targetVelocity = new Vector2(move * 10f * runSpeed, rigidbody2D.velocity.y);
                rigidbody2D.velocity = Vector2.SmoothDamp(rigidbody2D.velocity,targetVelocity,ref velocity,movementSmoothing);
                if(move > 0 && !facingRight)
                {
                    Flip();
                }
                else if(move<0 && facingRight)
                {
                    Flip();
                }
                if(jump && !wasJump && jumpState == 0)
                {
                    GroundJump();
                }
            }
            else
            {
                if(crouch)
                {
                    if(!wasDown && !fastFalling && rigidbody2D.velocity.y<=0)
                    {
                        fastFalling = true;
                    }
                }
                Vector2 targetVelocity = new Vector2(move * 10f * airSpeed, rigidbody2D.velocity.y);
                if(fastFalling)
                    targetVelocity.y *= fastFallMultiply;
                rigidbody2D.velocity = Vector2.SmoothDamp(rigidbody2D.velocity,targetVelocity,ref velocity,movementSmoothing * airMomentum);
                if(jump && !wasJump && doubleJump)
                {
                    fastFalling = false;
                    doubleJump = false;
                    grounded = false;
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x,0f);
                    rigidbody2D.AddForce(new Vector2(0f, doubleJumpForce));
                    ChangeAnimationState("Sandbert_doubleJump");
                    if(move > 0 && !facingRight)
                    {
                        Flip();
                    }
                    else if(move<0 && facingRight)
                    {
                        Flip();
                    }
                }
                if(dodge && !wasDodge && airdodgeAvailable)
                {
                    movementStateType = 1;
                    animationStateType = 3;
                    airdodging = true;
                    airdodgeAvailable = false;
                    rigidbody2D.velocity = Vector2.zero;
                    rigidbody2D.AddForce(new Vector2(move,vMove) * dashForce * 100f);
                    ChangeAnimationState("Sandbert_airdodge");
                }
            }
        }
        wasDown = crouch;
        wasJump = jump;
        wasDodge = dodge;
        if(jumpState == 2 && !jump)
            jumpState = 1;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if(collision.gameObject.CompareTag("Platform") || collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
            groundTouching = collision;
            ChangeAnimationState("Sandbert_land");
            animationStateType = 1;
            jumpState = 0;
            if(airdodging)
            {
                airdodging = false;
                movementStateType = 0;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Platform") || collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
            groundTouching = null;
            ChangeAnimationState("Sandbert_fall");
            animationStateType = 2;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("BlastZone"))
        {
            Die();
        }
    }    
    private void Update() 
    {
        if(grounded)
        {
            doubleJump = true;
            airdodgeAvailable = true;
        }
    }
    void ChangeAnimationState(string newState)
    {
        if (currentState == newState)
            return;
        animator.Play(newState);
        currentState = newState;
    }
    void GroundJump()
    {
        jumpState = 2;
        ChangeAnimationState("Sandbert_jumpstart");
        animationStateType = 3;
        movementStateType = 1;
        // float animationDelay = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        // Debug.Log(animationDelay);
        // Invoke("JumpStart",animationDelay);
    }
    void JumpStart()
    {
        if(jumpState == 2)
        {
            ChangeAnimationState("Sandbert_jump");
            //grounded = false;
            rigidbody2D.AddForce(new Vector2(0f, jumpForce));
            movementStateType = 0;
        }
        else if(jumpState == 1)
        {
            ChangeAnimationState("Sandbert_shortHop");
            //grounded = false;
            rigidbody2D.AddForce(new Vector2(0f, shortHopForce));
            movementStateType = 0;
        }
        jumpState = 0;
    }
    public void AnimationEnd(string name)
    {
        if(name == "jumpstart")
        {
            JumpStart();
        }
        else if(name == "land")
        {
            movementStateType = 0;
            ChangeAnimationState("Sandbert_Idle");
        }
        else
        {
            if(!grounded)
            {
                ChangeAnimationState("Sandbert_fall");
            }
            if(name == "airdodge")
            {
                airdodging = false;
                animationStateType = 2;
                movementStateType = 0;
            }
        }
    }
    public void AnimationMid(string name)
    {
        if(name == "airdodge" && airdodging)
        {
            rigidbody2D.velocity *= 0.2f;
        }
    }

    private void Die() 
    {
        animationStateType = 0;
        movementStateType = 0;
        currentState = "Sandbert_idle";
        transform.position = resetPos;
        rigidbody2D.velocity = Vector2.zero;
    }
}
