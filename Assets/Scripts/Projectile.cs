using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector2 moveSpeed = new Vector2(8f, 0);
    public int damage = 10;
    public Vector2 knockback = new Vector2(0, 0);
    public DetectionZone projectile;

    private float lifeTime = 5f;


    Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
       rb = GetComponent<Rigidbody2D>();     
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if you wan tthe projectile to be effected by gravity  by default make it a dynamic rigid  body
        rb.linearVelocity = new Vector2(moveSpeed.x * transform.localScale.x, moveSpeed.y);
        lifeTime -= Time.deltaTime;
        if (projectile.detectedColliders.Count > 0 || lifeTime < 0)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable damageable = collision.GetComponent<Damageable>();
        lifeTime = 5f;
        if(damageable != null)
        {
            Vector2 deliveredKnockback = transform.localScale.x > 0 ? knockback : new Vector2(knockback.x * -1, knockback.y);
            bool gotHit = damageable.Hit(damage, deliveredKnockback);


            if (gotHit)
            {    
                Debug.Log(collision.name + "hit for " + damage);
                Destroy(gameObject);
            }
        }
    }
}
