using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColorChanger : MonoBehaviour
{

    [SerializeField]
    private GameManager gameManager;

    private SpriteRenderer spriteRenderer;
    private int currentCircleSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.largestCircleMade <= currentCircleSize)
            return;

        currentCircleSize = gameManager.largestCircleMade;
        spriteRenderer.color = gameManager.circleColors[currentCircleSize-1];

    }
}
