using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public Transform launchPoint;
    public GameObject projectilePrefab;
    public AudioSource glibYell;

    public void FireProjectile()
    {
        if (Object.FindFirstObjectByType<Projectile>() == null)
        {
        GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, projectilePrefab.transform.rotation);
        Vector3 origScale = projectile.transform.localScale;
        projectile.transform.localScale = new Vector3(origScale.x * (transform.localScale.x > 0 ? 1 : -1), origScale.y, origScale.z);
        glibYell.Play();
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
