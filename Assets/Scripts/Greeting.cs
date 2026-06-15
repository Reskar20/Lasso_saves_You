using UnityEngine;

public class Greeting : MonoBehaviour
{
    public AudioSource greetingAudio;
    private float greetingSpam = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        greetingSpam -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      //  if(greetingSpam <= 0)
     //   {
            greetingAudio.Play();
            greetingSpam = 2f;
            Debug.Log("Played");
     //   }
    }
}
