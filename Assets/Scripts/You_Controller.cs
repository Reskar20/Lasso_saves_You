using UnityEngine;
using UnityEngine.SceneManagement;
public class You_Controller : MonoBehaviour
{
    public float walkStopRate = 0.00f;
     public UIManager UIManager;
    public float walkAcceleration = 31f;
    public float maxSpeed = 3f;
    [SerializeField]
    private  float chatTime = 0f;
    Animator animator;
    Animator playeranimator;
     //public bool isAlive = true;
    Rigidbody2D rb;
    Damageable damageable;
    private Vector2 walkDirectionVector = Vector2.right;

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
        private set 
        {
            animator.SetBool(AnimationStrings.canMove, value);
        } 
    }
    public bool IsHit
    {
        get
        {
            return animator.GetBool(AnimationStrings.isHit);
        }
        private set
        {
            animator.SetBool(AnimationStrings.isHit, value);
        }
    }
      public bool IsAlive
    {
        get
        {
            return animator.GetBool(AnimationStrings.isAlive);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        damageable = GetComponent<Damageable>();
        UIManager = FindFirstObjectByType<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
            chatTime -= Time.deltaTime;
    }
    void FixedUpdate()
    {
        if(IsHit == true && chatTime <= 0 && CanMove == true)
        {
            CanMove = false;
            chatTime = 2f;
        }

        if (chatTime <= 0 && IsHit == true && CanMove == false)
        {
            IsHit = false;
        }
        if(chatTime <= -1f)
        {
            CanMove = true;
        }
        if (CanMove == false) //Change as necessary depending on your substate machine/animation name
        {
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate), rb.linearVelocity.y);
        }
        else
        {

            rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocityX + (walkAcceleration * walkDirectionVector.x * Time.fixedDeltaTime), -maxSpeed, maxSpeed), rb.linearVelocityY);
        }

        if (IsAlive == false)
         {
            CanMove = false;
            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            playeranimator = Player.GetComponent<Animator>();
            playeranimator.Play("Lasso_death");
            Invoke("Death", 3);
         }
    } 

        private void Death()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }    
}
