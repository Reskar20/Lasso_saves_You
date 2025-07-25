using System;
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
5 wall jump: not started
6 slick: working
7 sliding: not started
*/

public class playercontroller : MonoBehaviour
{
    private bool jumpDepressed;
    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Damageable damageable;
    public PlayerData Data;
    Rigidbody2D rb;
    Animator animator;
  
    


    [SerializeField]
    private float isJumping = 0;
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

    [SerializeField]
    private bool _isMoving = false;
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

    [SerializeField]
    private bool _isRunning = false;

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
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    //Jump
    private bool _isJumpCut;
    private bool _isJumpFalling;

    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;
    public float LastPressedJumpTime { get; private set; }

    [SerializeField] private Transform _backWallCheckPoint;
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
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();

    }

    private void FixedUpdate()
    {
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
        else if ((isJumping > 0) && Mathf.Abs(rb.linearVelocity.y) < Data.jumpHangTimeThreshold)
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
        if (damageable.IsAlive == false)
        {
            Invoke("Death", 3);
        }

        #region movement speed
        if (damageable.IsHit == false)
        {
            //need to add alot of stuff here
            float targetSpeed = moveInput.x * CurrentMoveSpeed;
            if (touchingDirections.IsOnSlick == true)
            {
                targetSpeed *= Data.slickMultiplier;
                Debug.Log("slick TIme");
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
            else
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
            }
            if(Data.doConserveMomentum == true && Mathf.Abs(rb.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
            {
                //Prevent any deceleration from happening, or in other words conserve are current momentum
                //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
                accelRate = 0; 
            }
            float speedDif = targetSpeed - rb.linearVelocityX;

            float movement = speedDif * accelRate;


            if (touchingDirections.IsOnSlope == true && isJumping <= 0 && touchingDirections.IsGrounded == false)
            {
                rb.AddForce(new Vector2(movement, -3), ForceMode2D.Force); //rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocityY - 3);
            }
            else
            {
                rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
            }
        }
        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);
        #endregion
    }

    private void Update()
    {
        if (isJumping > 0)
        {
            isJumping -= Time.deltaTime;
        }
        if (touchingDirections.IsGrounded == true && doubleJumped == true)
        {
            doubleJumped = false;
        }
        //        LastOnGroundTime -= Time.deltaTime;
        //        LastOnWallTime -= Time.deltaTime;
        //        LastOnWallRightTime -= Time.deltaTime;
        //        LastOnWallLeftTime -= Time.deltaTime;
        //        LastPressedJumpTime -= Time.deltaTime;

    }
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
        if (context.started && CanMove && doubleJumped == false)
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
                rb.linearVelocity = new Vector2(rb.linearVelocityX, Data.jumpHeight / 1.5f);
            }
        }
        else if (context.canceled)
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
}