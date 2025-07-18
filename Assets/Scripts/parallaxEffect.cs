using UnityEngine;

public class parallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    // Starting position for the parallax game object
    Vector2 startingPosition;

    // Start Z value of the parallax game object
    float startingZ;

    //Distance that the camera has moved from the starting position of the parallax object
    Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startingPosition;
     
    float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;

    float clippingPlane => (cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane));

    // The further the object from the player, the faster the parallaxEffect object will move. Drag it's Z value close to the target to make it move slower
    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPosition = transform.position;
        startingZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        // when the target moves, move the parallax ojbect the same distance times a multiplier
        Vector2 newPosition = startingPosition + (camMoveSinceStart * parallaxFactor);

        // the X/Y postion changes based on target travel speed times the paralllax factor, but z stays contast.
        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);
    }
}
