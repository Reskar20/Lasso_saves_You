using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public DetectionZone exitZone;
    public UIManager UIManager; 
    public AudioSource victorySong;
    private bool played = false;

    // Update is called once per frame
    void Update()
    {
        if(played == false && exitZone.detectedColliders.Count > 0)
        {            
            played = true;
            victorySong.Play();
            Invoke("Quit", 3);
            
        }
    }
    private void Quit()
    {
        UIManager.Quit();
    }
}
