using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Train_Countdown : MonoBehaviour
{
    public float countdownTime; // Time in seconds for the countdown
    Animator playeranimator;
    Animator youanimator;
    private bool youAlive;
    private bool hasFinished;
    public TMP_Text timeText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        hasFinished = false;
        countdownTime = 120f;
        GameObject You = GameObject.FindGameObjectWithTag("You"); // Initialize the countdown time
        youanimator = You.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        youAlive = youanimator.GetBool(AnimationStrings.isAlive);
        if (countdownTime > 0 && youAlive == true)
        {
            countdownTime -= Time.deltaTime;
        }
        else if (hasFinished == false && youAlive == true)
        {
            hasFinished = true;
            countdownTime = 0;
            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            playeranimator = Player.GetComponent<Animator>();
            playeranimator.Play("Lasso_death");
            Invoke("Death", 3);
        }
        DisplayTime(countdownTime);
    }
    private void Death()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        if (countdownTime == 0f)
        {
            timeText.text = "00:00";
        }
        else
        {
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }    
}
