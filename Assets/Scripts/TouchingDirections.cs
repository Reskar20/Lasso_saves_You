using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

// Uses the  collider  to  check directions to see if the object  is currently on the ground, touching the  wall,  or  touching  the ceiling
public class TouchingDirections : MonoBehaviour
{

    public ContactFilter2D castFilter;
    public ContactFilter2D slopeFilter;
    public ContactFilter2D slickFilter;
    public float groundDistance = 0.05f;
    public float wallDistance = 0.05f;
    public float rearWallDistance = 01.05f;
    public float ceilingDistance = 0.05f;
    public float slopeDistance = 1f;
    private Vector2 wallCheckPosMid;
    private Vector2 wallCheckPosTop;
    private Vector2 colliderSize;
    [SerializeField]
    private int wallHitsNum = 0;
    private int wallHitsNumRear = 0;



    CapsuleCollider2D touchingCol;
    Animator animator;

    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] rearWallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];
    RaycastHit2D[] slopeHits = new RaycastHit2D[5];

    [SerializeField]
    private bool _isGrounded = true;
    public bool IsGrounded { get
        {
            return _isGrounded;
        }
        private set {
            _isGrounded = value;
            animator.SetBool(AnimationStrings.isGrounded, value);
        }
        }

    [SerializeField]
    private bool _isOnWall = true;
    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        private set
        {
            _isOnWall = value;
            animator.SetBool(AnimationStrings.isOnWall, value);
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
    [SerializeField]
    private bool _isOnSlope = true;
    public bool IsOnSlope
    {
        get
        {
            return _isOnSlope;
        }
        private set
        {
            _isOnSlope = value;
            animator.SetBool(AnimationStrings.isOnSlope, value);
        }
    }
    [SerializeField]
    private bool _onSlick = false;
    public bool IsOnSlick
    {
        get
        {
            return _onSlick;
        }
        private set
        {
            _onSlick = value;
            animator.SetBool(AnimationStrings.onSlick, value);
        }
    }
    [SerializeField]
    private bool _isOnCeiling = true;
    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

    public bool IsOnCeiling
    {
        get
        {
            return _isOnCeiling;
        }
        private set
        {
            _isOnCeiling = value;
            animator.SetBool(AnimationStrings.isOnCeiling, value);
        }
    }

    private void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        colliderSize = touchingCol.size;
        castFilter.SetLayerMask(LayerMask.GetMask("Ground","Slope", "Slick"));
        slopeFilter.SetLayerMask(LayerMask.GetMask("Slope"));
        slickFilter.SetLayerMask(LayerMask.GetMask("Slick"));
    }    

    private void FixedUpdate()
    {
        IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        
        //build and send out 2 rays one on top and one 1/4 up to check for a wall
        wallCheckPosMid = (transform.position + new Vector3(colliderSize.x * gameObject.transform.localScale.x / 2, colliderSize.y / -5));
        Debug.DrawRay(wallCheckPosMid, wallCheckDirection, Color.red); Debug.DrawRay(wallCheckPosMid - new Vector2 (gameObject.transform.localScale.x, 0), wallCheckDirection * new Vector2 (-1 , 1), Color.blue);
        wallCheckPosTop = (transform.position + new Vector3(colliderSize.x * gameObject.transform.localScale.x / 2, colliderSize.y / 2.75f));
        wallHitsNum = Physics2D.Raycast(wallCheckPosMid, wallCheckDirection, castFilter, wallHits, wallDistance) + Physics2D.Raycast(wallCheckPosTop, wallCheckDirection, castFilter, wallHits, wallDistance);
        wallHitsNumRear = Physics2D.Raycast(wallCheckPosMid - new Vector2 (gameObject.transform.localScale.x, 0), wallCheckDirection * new Vector2 (-1 , 1), castFilter, rearWallHits, rearWallDistance) + Physics2D.Raycast(wallCheckPosTop - new Vector2 (gameObject.transform.localScale.x, 0), wallCheckDirection * new Vector2 (-1 , 1), castFilter, rearWallHits, rearWallDistance);
        
        IsOnWall = wallHitsNum > 0;
        IsOnRearWall = wallHitsNumRear > 0;
        IsOnCeiling = touchingCol.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;
        IsOnSlope = touchingCol.Cast(Vector2.down, slopeFilter, slopeHits, slopeDistance) > 0;
        IsOnSlick = touchingCol.Cast(Vector2.down, slickFilter, slopeHits, slopeDistance) > 0;
    }
}