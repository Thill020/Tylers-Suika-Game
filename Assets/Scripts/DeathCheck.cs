using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCheck : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CircleLogic>() == null)
            return;

        if (collision.gameObject.GetComponent<CircleLogic>().isLive())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            gameManager.GetComponent<GameManager>().GameOver();
        }


    }
}
