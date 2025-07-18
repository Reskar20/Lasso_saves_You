using System;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEye : MonoBehaviour
{
    public float flightSpeed = 2f;
    public List<Transform> waypoints;
    public float waypointReachedDistance = 0.1f;

    Animator animator;
    Rigidbody2D rb;
    public DetectionZone whirlDetectionZone;
    Damageable damageable;

    Transform nextWaypoint;
    int waypointNumber = 0;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        damageable = GetComponent<Damageable>();
    }
    void Start()
    {
        nextWaypoint = waypoints[waypointNumber];
    }

    // Update is called once per frame
    void Update()
    {
        HasTarget = whirlDetectionZone.detectedColliders.Count > 0;
    }
    private void FixedUpdate()
    {
        if(damageable.IsAlive)
        {
            if(CanMove)
            {
                Flight();
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
        else
        {
            rb.gravityScale = 2f;
            rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
        }
    }

    private void Flight()
    {
        //fly to the next waypoint
        Vector2 directionToWaypoint = (nextWaypoint.position - transform.position).normalized;

        //check  if we have reached waypoint already
        float distance = Vector2.Distance(nextWaypoint.position, transform.position);

        rb.linearVelocity = directionToWaypoint * flightSpeed;
        UpdateDirection();

        //check if we need to switch waypoints
        if(distance  <= waypointReachedDistance)
        {
            //switch to next waypoint
            waypointNumber++;

            if(waypointNumber >= waypoints.Count)
            {
                waypointNumber = 0;
            }
            nextWaypoint = waypoints[waypointNumber];
        }
    }
    private void UpdateDirection()
    {
        Vector3 locScale = transform.localScale;
        if (transform.localScale.x > 0)
        {
            if (rb.linearVelocityX < 0)
            {
                transform.localScale = new Vector3(-1 * locScale.x, locScale.y, locScale.z);
            }
        }
        else
        {
            if (rb.linearVelocityX > 0)
            {
                transform.localScale = new Vector3(-1 * locScale.x, locScale.y, locScale.z);
            }
        }
    }
}
