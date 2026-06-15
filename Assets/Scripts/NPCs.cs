using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCs : MonoBehaviour
{
    public float flightSpeed = 2f;
    public List<Transform> waypoints;
    public float waypointReachedDistance = 0.1f;

    Animator animator;
    Rigidbody2D rb;

    Transform nextWaypoint;
    int waypointNumber = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        nextWaypoint = waypoints[waypointNumber];
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void FixedUpdate()
    {

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
            if (rb.linearVelocityX > 0)
            {
                transform.localScale = new Vector3(-1 * locScale.x, locScale.y, locScale.z);
            }
        }
        else
        {
            if (rb.linearVelocityX < 0)
            {
                transform.localScale = new Vector3(-1 * locScale.x, locScale.y, locScale.z);
            }
        }
    }
}
