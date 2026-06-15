using System;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]

/*
TO DO:
1 walk: working
2 run: working
3 jump: working
4 jump cut: working
5 wall jump: working
6 slick: working
7 sliding: working
8 coyote time: not started
9 gravity for slide/slick: working
*/



public class playercontroller : MonoBehaviour
{
    private bool jumpDepressed;
    Vector2 moveInput;
    TouchingDirections touchingDirections;
    //Damageable damageable;
    public PlayerData Data;
    Rigidbody2D rb;
    Animator animator;
    CapsuleCollider2D touchingCol;
    public ContactFilter2D castFilter;

    RaycastHit2D[] rearWallHits = new RaycastHit2D[5];
    RaycastHit2D[] wallJumpHits = new RaycastHit2D[5];
    private Vector2 colliderSize;
    public float wallJumpDistance = 1.05f;
    public float rearWallDistance = 01.05f;
    private RaycastHit2D GroundTeleport2D;
//    [SerializeField] private Transform rayCastOrigin;
    [SerializeField] private Transform playerfeet;
    [SerializeField] private bool isOnSlope;
    [SerializeField] private float slopeSideAngle;
    [SerializeField] private Vector2 slopeNormalPerp;
    [SerializeField] private float slopeDownAngle;
    [SerializeField] private float lastSlopeAngle;
    [SerializeField] private float isJumping = 0;
    private Boolean doubleJumped = false;

    public float CurrentMoveSpeed
    {
        get
        {
            if (CanMove == true)
            {
                if (IsMoving && !touchingDirections.IsOnWall)
                {
                    if (IsMoving && touchingDirections.IsGrounded)
                    {
                        if (IsRunning)
                        {
                            return Data.sprintMaxSpeed;
                        }
                        else
                        {
                            return Data.runMaxSpeed;
                        }
                    }
                    else
                    {
                        //Air Move
                        return Data.airMoveSpeed;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }


    [SerializeField] private bool _isMoving = false;
    public bool IsMoving
    {
        get
        {
            return _isMoving;
        }
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);

        }
    }


    [SerializeField] private bool _isRunning = false;

    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }
    [SerializeField]
    public bool _isFacingRight = true;
    public bool IsFaceingRight
    {
        get { return _isFacingRight; }
        private set
        {
            if (_isFacingRight != value)
            {
                //flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2(-1, 1);
            }
            _isFacingRight = value;
        }
    }
    [SerializeField] private bool _isOnRearWall = true;
    public bool IsOnRearWall
    {
        get
        {
            return _isOnRearWall;
        }
        private set
        {
            _isOnRearWall = value;
            animator.SetBool(AnimationStrings.isOnRearWall, value);
        }
    }

    [SerializeField] private bool _canWallJump = true;
    public bool CanWallJump
    {
        get
        {
            return _canWallJump;
        }
        private set
        {
            _canWallJump = value;
            animator.SetBool(AnimationStrings.canWallJump, value);
        }
    }



    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public bool IsAlive
    {
        get
        {
            return animator.GetBool(AnimationStrings.isAlive);
        }
    }
    public bool IsWallJumping { get; private set; }
    public bool IsSliding { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    //Jump
    private bool _isJumpCut;
    private bool _isJumpFalling;

    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;
    private float _lastOnRearWallTime;
    private float _lastOnFrontWallTime;
    private float _lastOnGroundTime;
    private int wallHitsNumRear = 0;
    private int wallJumpHitsNum = 0;

    private Vector2 wallCheckPosMid;
    private float wallJumpDelay = 0;
    private float resetDelay = 0;

    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
    //    damageable = GetComponent<Damageable>();
        castFilter.SetLayerMask(LayerMask.GetMask("Ground", "Slope", "Slick"));
        colliderSize = touchingCol.size;
    }

    #region fixed update
    #endregion
    private void FixedUpdate()
    {
        SlopeCheck();
        //Higher gravity if we've released the jump input or are falling
        if (IsSliding == true) //need to implement this check
        {
            SetGravityScale(0);
        }
        else if (rb.linearVelocity.y < 0 && moveInput.y < 0)
        {
            //Much higher gravity if holding down
            SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -Data.maxFastFallSpeed));
        }
        else if (jumpDepressed == false && touchingDirections.IsGrounded == false) // need to implement this check
        {
            //Higher gravity if jump button released
            SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -Data.maxFallSpeed));
        }
        else if ((isJumping > 0f) && Mathf.Abs(rb.linearVelocity.y) < Data.jumpHangTimeThreshold)
        {
            SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
        }
        else if (rb.linearVelocity.y < 0)
        {
            //Higher gravity if falling
            SetGravityScale(Data.gravityScale * Data.fallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -Data.maxFallSpeed));
        }
        else
        {
            //Default gravity if standing on a platform or moving upwards
            SetGravityScale(Data.gravityScale);
        }
 /*       if (damageable.IsAlive == false)
        {
            Invoke("Death", 3);
        } */

        #region wallchecks
        wallCheckPosMid = (transform.position + new Vector3(colliderSize.x * gameObject.transform.localScale.x / 2, colliderSize.y / -5));
        wallHitsNumRear = Physics2D.Raycast(wallCheckPosMid - new Vector2(gameObject.transform.localScale.x, 0), wallCheckDirection * new Vector2(-1, 1), castFilter, rearWallHits, rearWallDistance);
        wallJumpHitsNum = Physics2D.Raycast(wallCheckPosMid, wallCheckDirection, castFilter, wallJumpHits, wallJumpDistance);
        IsOnRearWall = wallHitsNumRear > 0;
        CanWallJump = wallJumpHitsNum > 0;
        #endregion

        #region movement speed
        if (/*damageable.IsHit == false && */ wallJumpDelay <= 0)
        {
            //need to add alot of stuff here
            float targetSpeed = moveInput.x * CurrentMoveSpeed;
            if (touchingDirections.IsOnSlick == true)
            {
                targetSpeed *= Data.slickSpeedMultiplier;
            }
            //We can reduce are control using Lerp() this smooths changes to are direction and speed
            targetSpeed = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, 1);

            //Gets an acceleration value based on if we are accelerating (includes turning) 
            //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
            float accelRate;
            if (touchingDirections.IsOnSlick == true)
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelOnSlick : Data.runDeccelAmount * Data.deccelOnSlick;
            }
            else if (touchingDirections.IsGrounded == true)
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
            }
            else if (wallJumpDelay > 0)
            {
                accelRate = 0;
            }
            else
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
            }
            if (Data.doConserveMomentum == true && Mathf.Abs(rb.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && _lastOnGroundTime < 0)
            {
                //Prevent any deceleration from happening, or in other words conserve are current momentum
                //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
                accelRate = 0;
            }
            float speedDif = targetSpeed - rb.linearVelocityX;

            float movement = speedDif * accelRate;


            if (isOnSlope == true && isJumping <= 0)
            {
                rb.AddForce(new Vector2(movement, rb.linearVelocityX * slopeNormalPerp.y * -wallCheckDirection.x));
            }
            else
            {
                rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
            }
        }
        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);
        #endregion
    }
    #region update

    private void Update()
    {
        if (touchingDirections.IsGrounded == true && doubleJumped == true)
        {
            doubleJumped = false;
        }
        if (touchingDirections.IsGrounded == true)
        {
            _lastOnGroundTime = Data.coyoteTime;
        }
        if (CanWallJump == true)
        {
            _lastOnFrontWallTime = Data.coyoteTime;
        }
        if (IsOnRearWall == true)
        {
            _lastOnRearWallTime = Data.coyoteTime;
        }

        isJumping -= Time.deltaTime;
        _lastOnGroundTime -= Time.deltaTime;
        _lastOnFrontWallTime -= Time.deltaTime;
        _lastOnRearWallTime -= Time.deltaTime;
        wallJumpDelay -= Time.deltaTime;
        resetDelay -= Time.deltaTime;

    }
    #endregion
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (IsAlive)
        {
            IsMoving = moveInput.x != 0;//!= Vector2.zero;

            SetFacingDirection(moveInput);
        }
        else
        {
            IsMoving = false;
        }

    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFaceingRight)
        {
            //face the right
            IsFaceingRight = true;
        }
        else if (moveInput.x < 0 && IsFaceingRight)
        {
            //face the left
            IsFaceingRight = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        }
        else if (context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (touchingDirections.IsGrounded == false && context.started && (_lastOnRearWallTime > 0 || _lastOnFrontWallTime > 0) && wallJumpDelay <= 0)
        {
            wallJumpDelay = Data.wallJumpDelay;
            Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
            Vector2 forceDirection = gameObject.transform.localScale.x > 0 ? Vector2.left : Vector2.right;
            if (_lastOnRearWallTime > 0)
            {
                force.x *= wallCheckDirection.x;
            }
            if (_lastOnFrontWallTime > 0)
            {
                force.x *= -wallCheckDirection.x;
                SetFacingDirection(force);
            }

            if (Mathf.Sign(rb.linearVelocityX) != Mathf.Sign(force.x))
            {
                force.x -= rb.linearVelocityX;
            }
            if (rb.linearVelocityY < 0)
            {
                force.y -= rb.linearVelocityY;
            }
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.AddForce(force, ForceMode2D.Impulse);

        }
        else if (context.started && CanMove && doubleJumped == false)
        {
            jumpDepressed = true;
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            if (touchingDirections.IsGrounded == true)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, Data.jumpHeight);
                isJumping = 1f;
            }
            else
            {
                doubleJumped = true;
                rb.linearVelocity = new Vector2(rb.linearVelocityX, Data.jumpHeight * Data.doubleJumpMultiplier);
            }
        }
        if (context.canceled)
        {
            jumpDepressed = false;
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);
        }
    }
    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger(AnimationStrings.rangedAttackTrigger);
        }
    }
    public void OnHit(int damage, Vector2 knockback)
    {
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocityY + knockback.y);
    }
    public void Death()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    
    #region slope angle checks
    void SlopeCheck()
    {
        Vector2 checkPos = transform.position - (Vector3)(new Vector2(0.0f, colliderSize.y / 2));

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, Data.slopeCheckDistance, castFilter.layerMask);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, Data.slopeCheckDistance, castFilter.layerMask);

        if (slopeHitFront)
        {
            isOnSlope = true;
            animator.SetBool(AnimationStrings.isOnSlope, true);

            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;
            animator.SetBool(AnimationStrings.isOnSlope, true);

            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
            animator.SetBool(AnimationStrings.isOnSlope, false);
        }

    }

    void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, Data.slopeCheckDistance, castFilter.layerMask);

        if (hit)
        {

            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
                animator.SetBool(AnimationStrings.isOnSlope, true);
            }

            lastSlopeAngle = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);

        }
        #endregion
    }
}