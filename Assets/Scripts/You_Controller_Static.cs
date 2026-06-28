using UnityEngine;
using UnityEngine.SceneManagement;
public class You_Controller_Static : MonoBehaviour
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
    [SerializeField]
    private int spawnLocation = 0;

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
        UIManager = FindObjectOfType<UIManager>();
        spawnLocation = Random.Range(1, 3);
        if(spawnLocation == 1)
        {
            transform.position = new Vector3( -152.32f, -23.78f, 0 );
        }
        else if(spawnLocation == 2)
        {
            transform.position = new Vector3( -119.2f, -23.78f, 0 );
        }
    }

    // Update is called once per frame
    void Update()
    {
            chatTime -= Time.deltaTime;
    }
    void FixedUpdate()
    {
        if (IsAlive == false)
         {
            CanMove = false;
            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            playeranimator = Player.GetComponent<Animator>();
            playeranimator.Play("Lasso_Swoon");
            playeranimator.SetBool(AnimationStrings.canMove, false);
            Invoke("Death", 3);
         }
    } 

        private void Death()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }    
}
