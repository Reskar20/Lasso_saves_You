using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]

public class playercontroller : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airWalkSpeed = 3f;
    public float jumpImpulse = 10f;
    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Damageable damageable;
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
                            return runSpeed;
                        }
                        else
                        {
                            return walkSpeed;
                        }
                    }
                    else
                    {
                        //Air Move
                        return airWalkSpeed;
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
    public bool IsFaceingRight { get { return _isFacingRight; } private set
        {
            if (_isFacingRight != value)
            {
                //flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2(-1, 1);
            }
            _isFacingRight = value;
        } }

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

    Rigidbody2D rb;
    Animator animator;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
    }

    private void FixedUpdate()
    {
        if (damageable.IsAlive == false)
        {
            Invoke("Death", 3);
        }
        
        if (damageable.IsHit == false)
        {
            //need to add alot of stuff here
            float targetSpeed = moveInput.x * CurrentMoveSpeed;
            float speedDif = targetSpeed - rb.linearVelocityX;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
            float movement = Mathf.Pow(MathF.Abs(speedDif) * accelRate, velpower) * Mathf.Sign(speedDif);
            rb.AddForce(movement * Vector2.right);  
            
/*            
                                                                                    if (touchingDirections.IsOnSlick == false)
                                                                                    {
                                                                                        if (touchingDirections.IsOnSlope == true && isJumping <= 0 && touchingDirections.IsGrounded == false)
                                                                                        {
                                                                                            rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocityY - 3);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocityY);
                                                                                        }
                                                                                    }
                                                                                    else if (rb.linearVelocityX > -CurrentMoveSpeed * 1.5f && rb.linearVelocityX < CurrentMoveSpeed * 1.5f)
                                                                                    {
                                                                                        rb.AddForce(new Vector2(moveInput.x * CurrentMoveSpeed * 10, 0));
                                                                                    }
*/
        }
                                                                        
            animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);


    }

    private void Update()
    {
        if(isJumping > 0)
        {
           isJumping -= Time.deltaTime;
        }
        if(touchingDirections.IsGrounded == true && doubleJumped == true)
        {
            doubleJumped = false;
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (IsAlive)
        {
        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
        }
        else
        {
            IsMoving = false;
        }

    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if(moveInput.x > 0 && !IsFaceingRight)
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
        if(context.started)
        {
            IsRunning = true;
        } else if(context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && CanMove && doubleJumped == false)
        {
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            if(touchingDirections.IsGrounded == true)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpImpulse);
                isJumping = 1f;
            }
            else
            {
                doubleJumped = true;
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpImpulse / 1.5f);
            }
        }
    }
    public void  OnAttack(InputAction.CallbackContext context)
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
