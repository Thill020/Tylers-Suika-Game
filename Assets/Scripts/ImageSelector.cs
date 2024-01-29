using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ThemeType { Colors, Planeswalkers}

public class ImageSelector : MonoBehaviour
{ 
    
    private GameManager gameManager;
    private ThemeType theme;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        theme = gameManager.GetTheme();

        switch (theme)
        {
            case ThemeType.Planeswalkers:
                GetComponent<SpriteRenderer>().sprite = gameManager.planeswalkerSprites[int.Parse(gameObject.tag) - 1];
                break;
            case ThemeType.Colors:
                GetComponent<SpriteRenderer>().color = gameManager.circleColors[int.Parse(gameObject.tag) - 1];
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
