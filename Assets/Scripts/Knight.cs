using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]

public class Knight : MonoBehaviour
{
    public float walkStopRate = 0.02f;
    public float walkAcceleration = 31f;
    public float maxSpeed = 3f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;
    Animator animator;
    Damageable damageable;

    Rigidbody2D rb;
    TouchingDirections touchingDirections;

    public enum WalkableDirection { Right, Left };

    [SerializeField]
    private WalkableDirection _walkDirection = WalkableDirection.Right;
    private Vector2 walkDirectionVector = Vector2.right;
    private float _landingTime = 0f;

    public WalkableDirection WalkDirection
    {
        get { 
            return _walkDirection; 
            }
        set 
        { 
            if (_walkDirection != value)
            {
                //direction flipped
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);

                if(value == WalkableDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                }
                else if (value == WalkableDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }
            }
            _walkDirection = value;
        }
        
    }

    public bool _hasTarget = false;
    public bool HasTarget 
    { 
        get { return _hasTarget; } 
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        } 
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public float AttackCooldown { get 
        {
            return animator.GetFloat(AnimationStrings.attackCooldown);
        } private set 
        {
            animator.SetFloat(AnimationStrings.attackCooldown, MathF.Max(value, 0));
        } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
    }

    [SerializeField]
    private float hasFlipped = 0f;

    // Update is called once per frame
    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;
        if(AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }
       
    }
    private void FixedUpdate()
    {
        if (touchingDirections.IsGrounded != true)
        {
            _landingTime = .1f;
        }
        else if (touchingDirections.IsGrounded == true && _landingTime > 0) 
        {
            _landingTime -= Time.deltaTime;
        }
        if (touchingDirections.IsGrounded == true && (touchingDirections.IsOnWall == true) && _landingTime <= 0)
        {
            if (hasFlipped <= 0)
            {
                FlipDirection();
                hasFlipped = .5f;
            }

        }
        else if(hasFlipped >= 0)
        {
            hasFlipped -= Time.deltaTime;
        }
        //used instead of the can move variable
        if (CanMove == false) //Change as necessary depending on your substate machine/animation name
        {
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate), rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocityX + (walkAcceleration * walkDirectionVector.x * Time.fixedDeltaTime), -maxSpeed, maxSpeed), rb.linearVelocityY);
        }       
    }

   
    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            if (WalkDirection == WalkableDirection.Right)
            {
                WalkDirection = WalkableDirection.Left;
            }
        }
    }
    public void OnHit(int damage, Vector2 knockback)
    {
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocityY + knockback.y);
    }
    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded)
        {
            FlipDirection();
        }
    }
}
