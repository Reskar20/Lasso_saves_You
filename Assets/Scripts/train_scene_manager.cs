using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class train_scene_manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*public void OnExitGame(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Quit();
        }
    } */
    public void Quit()
    {
        SceneManager.LoadSceneAsync("menu"); 
    }
}
