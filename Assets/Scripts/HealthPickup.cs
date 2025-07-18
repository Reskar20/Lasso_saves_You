using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthRestore = 20;
    public Vector3 spinRotationSpeed = new Vector3(0, 180, 0);
    AudioSource pickupSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pickupSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += spinRotationSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable damageable = collision.GetComponent<Damageable>();

        if(damageable && (damageable.Health < damageable.MaxHealth))
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
