/****************************************
 * Created By Tyler Hill
 * Last Update: 16 Dec 2023
 * TODO LIST:
 * -Add Increased Pause Control (EscapeAndP)
 * -Fix Merge Logic (See Circle Logic)
 * -Add More Themes and Designs (See Image Selector)
 * *************************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;


public enum ControlType { Mouse, Controller, Keyboard };

public class GameManager : MonoBehaviour
{
    #region Varriables
    //[ARRAYS]
    public GameObject[] circles;         // array of circle prefabs
    public Sprite[] planeswalkerSprites; // array of sprites for the planeswalker theme
    public Color[] circleColors;         // array of colors for the circles
                                         
    //[CIRCLE LOGIC]                     
    public GameObject nextPoint;         // the point on screen where the next circle to be dropped is held
    private GameObject nextCircle;       // the next circle to be dropped
    private GameObject curCircle;        // the current circle being controlled
    int randCircle;                      // the random number that determines which circle is created next. Changes frequently.
    float xPosOffset = 0;

    //[OBJECT REFERENCES]                
    [SerializeField]                     
    private Scores scoreBoard;           // persistant object that holds scores between runs
                                         
    //[TIMERS]                           
    float nextCircleTimeLimit = .5f;     //how long must pass between circles being able to be dropped
    float nextCircleTime = 0;            // the current time since last circle dropped
                                         
    //[CANVAS VARIABLES + CONTROLS]      
    [SerializeField]                     
    private GameObject gameOverScreen;   // canvas screen controlling game over options
    bool gameOver = false;               //game over logic
                                         
    [SerializeField]                     
    private GameObject pauseScreen;      //canvas screen controlling the pause menu
    bool paused = false;                 //is the game paused
                                         
    [SerializeField]                     
    private GameObject bestScoreText;    // the text object that holds the best score so far
    int bestScore = 0;                   // the best score in this session
                                         
    [SerializeField]                     
    private GameObject currentScoreText; // the text object that holds the run's current score
    int curScore = 0;                    // the current score of the run
                                         
    //[OTHER VARIABLES]                  
    private Vector3 mousePos;            // the position of the cursor where the curCircle is suspended before dropping
    private float cursorSpeed = 10;      // horizontal speed control when on controller
    public GameObject gameOverLine;      // the line the determines when a game over is hit
    public int largestCircleMade = 0;    //        

    //[ENUMERATORS]
    ThemeType theme;
    ControlType controlType = ControlType.Mouse;

    #endregion

    #region Unity Functions

    // Start is called before the first frame update
    void Start()
    {
        theme = ThemeType.Colors;

        //Make sure persisting variables persist between rounds
        scoreBoard = FindObjectOfType<Scores>();
        bestScore = scoreBoard.BestScore;
        controlType = scoreBoard.ContType;
        
        //Start game inititives
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        //If game over has been reached, stop updating the game cycle
        if (gameOver)
            return;

        //if both the current and next circles are null, create a circle to be placed at the drop cursor
        //This should only trigger on scene load/new game/try again points
        if (curCircle == null && nextCircle == null)
            CreateCircle();

        //if the next circle is null, create a circle at the next circle location
        //This is used quite often through normal gameplay
        if (nextCircle == null)
            CreateCircle(nextPoint.transform.position);

        //Keep the circle in the next point from dropping or increasing velocity.
        nextCircle.transform.position = nextPoint.transform.position;
        nextCircle.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

        //if the current circle in null, wait until so much time has passed then move the next circle to the current circle
        if (curCircle == null)
        {
            TimeUntilNextCircleIsAvailable();
            return;
        }

        //Movement Logic for Mouse and Keyboard/Controller
        if (controlType == ControlType.Mouse && !paused)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);            
        }
        else if (!paused)
        {
            mousePos.x += Input.GetAxis("Horizontal") * cursorSpeed * Time.deltaTime;
        }
        xPosOffset = curCircle.transform.localScale.x / 2;
        if (mousePos.x < -2.85f + xPosOffset) { mousePos.x = (-2.85f + xPosOffset); }
        if (mousePos.x > 2.85f - xPosOffset) { mousePos.x = (2.85f - xPosOffset); }
        mousePos.y = 3.5f + xPosOffset;
        mousePos.z = 0;

        curCircle.transform.position = mousePos;
        curCircle.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

        //If the game is paused don't let the player drop the circles
        if (paused)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetAxis("Jump") > 0.5 || Input.GetAxis("Fire1") > 0.5)
        {
            curCircle.GetComponent<CircleLogic>().DropCircle();
            if (int.Parse(curCircle.gameObject.tag) > largestCircleMade)
                largestCircleMade = int.Parse(curCircle.gameObject.tag);

            curCircle = null;
                     
        }

        //If escape is pressed, pause the game.
        //EscapeAndP
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void Reset()
    {
        curScore = 0;
        currentScoreText.GetComponent<TextMeshProUGUI>().text = $"Score: {curScore.ToString()}";
        gameOver = false;
        SceneManager.LoadScene(0);
    }

    #endregion

    #region Game State Functions
    private void StartGame()
    {
        curCircle = CreateCircle();
        nextCircle = CreateCircle();

        IncrementScore(0);

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    }
    
    public void Pause()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        pauseScreen.SetActive(true);
        paused = true;
    }
    
    public void Resume()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        pauseScreen.SetActive(false);
        paused = false;
    }

    public void GameOver()
    {
        gameOver = true;
        paused = false;
        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(true);
        scoreBoard.BestScore = bestScore;
        scoreBoard.ContType = controlType;
    }

    #endregion

    #region Circle Functions
    private GameObject CreateCircle()
    {
        randCircle = Random.Range(0, 5);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        GameObject circle = Instantiate(circles[randCircle], mousePos, Quaternion.identity);

        return circle;
    }

    private GameObject CreateCircle(Vector3 instantiationPoint)
    {
        randCircle = Random.Range(0, 5);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        GameObject circle = Instantiate(circles[randCircle], mousePos, Quaternion.identity);

        return circle;
    }

    public void NextCircle()
    {
        nextCircle.transform.position = mousePos;
        curCircle = nextCircle;

        randCircle = Random.Range(0, 4);
        nextCircle = CreateCircle(nextPoint.transform.position);
    }

    private void TimeUntilNextCircleIsAvailable()
    {
        nextCircleTime += Time.deltaTime;
        if (nextCircleTime >= nextCircleTimeLimit)
        {
            NextCircle();
            nextCircleTime= 0;
        }

    }

    #endregion

    #region Accessor Functions
    public void SetControlType(int contType)
    {
        switch (contType)
        {
            case 0: controlType = ControlType.Keyboard;
                break;
            default: controlType = ControlType.Mouse;
                break;
        }
    }

    public void SetTheme(ThemeType t)
    {
        theme = t;
    }

    public ThemeType GetTheme() 
    {
        return theme; 
    }
    

    #endregion

    public void IncrementScore(int score)
    {
        if (gameOver)
            return;

        curScore += score;

        if (score - 1 > 0)
            IncrementScore(score - 1);

        if(curScore > bestScore)
            bestScore = curScore;

        currentScoreText.GetComponent<TextMeshProUGUI>().text = $"Score: {curScore.ToString()}";
        bestScoreText.GetComponent<TextMeshProUGUI>().text = $"Best: {bestScore.ToString()}";
    }

}
