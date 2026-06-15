using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthRestore = 20;
    public Vector3 spinRotationSpeed = new Vector3(0, 180, 0);
    Damageable damageable;
    AudioSource pickupSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pickupSource = GetComponent<AudioSource>();
        GameObject You = GameObject.FindGameObjectWithTag("You");
        damageable = You.GetComponent<Damageable>();  
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += spinRotationSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       

        if(damageable.Health < damageable.MaxHealth)
        {
            damageable.Heal(healthRestore);
            if(pickupSource == true)
            {
               AudioSource.PlayClipAtPoint(pickupSource.clip, gameObject.transform.position, pickupSource.volume);
            }
            Destroy(gameObject);
        }
    }

}
