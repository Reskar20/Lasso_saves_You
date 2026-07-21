using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class menu_select : MonoBehaviour
{
    private int position = 1;
    private int max_position = 3;
    private bool input_changed = false;
    Vector2 moveInput;

    // Update is called once per frame
    void Update()
    {

        if (input_changed == true)
        {
            if(position == 1)
            {
                transform.position = new Vector3( -0.62f, 3.228f, 0 );
                input_changed = false;
            }
            if(position == 2)
            {
                transform.position = new Vector3( -0.62f, 1.99f, 0 );
                input_changed = false;
            }
            if(position == 3)
            {
                transform.position = new Vector3( 6.86f, -5.19f, 0 );
                input_changed = false;
            }
        }
        
    }


    public void CursorMove(InputAction.CallbackContext context)
    {
        Debug.Log("cursor movement noticed.");
         moveInput = context.ReadValue<Vector2>();   

        if(moveInput.y < 0)
        {
            if(position == max_position)
            {
                position = 1;
            }
            else
            {
                position++;
            }
            input_changed = true;
        }
        if(moveInput.y > 0)
        {
            Debug.Log("noticed up");
            if(position == 1)
            {
                position = max_position;
            }
            else
            {
                position--;
            }
            input_changed = true;
        }
    }
    public void Selected()
    {
        if(position == 1)
        {
            SceneManager.LoadSceneAsync("gameplay Scene"); 
        }
        if(position == 2)
        {
            SceneManager.LoadSceneAsync("train"); 
        }
        if(position == 3)
        {
            #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            #endif

            #if (UNITY_EDITOR)
                UnityEditor.EditorApplication.isPlaying = false;
            #elif (UNITY_STANDALONE)
                Application.Quit();
            #elif (UNITY_WEBGL)
                SceneManager.LoadScene("QuitScene");
            #endif
        }
    }
}
