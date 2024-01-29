using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CircleLogic : MonoBehaviour
{
    private bool live = false;
    public static bool merging = false;
    public static float mergeTime;

    public void DropCircle()
    {
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
    }

    public bool isLive()
    {
        return live;
    }

    public bool isMerging()
    {
        return merging;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<CircleLogic>() != null)
            live = true;
        
        if (collision.gameObject.tag == gameObject.tag) 
        {
            CircleLogic cl = collision.gameObject.GetComponent<CircleLogic>();

            if (mergeTime != Time.time)
                merging = false;

            if (cl != null && cl.isMerging())
            {
                return;
            }

            merging = true;
            mergeTime = Time.time;

            //If colliding circles are both the largest type, delete both circles
            if (gameObject.tag == "11")
            {
                Destroy(collision.gameObject);
                Destroy(gameObject);
                GameManager tempGameManager = FindObjectOfType<GameManager>();
                tempGameManager.IncrementScore(int.Parse(gameObject.tag));
                return;
            }

            GameManager gameManager = FindObjectOfType<GameManager>();
            gameManager.IncrementScore(int.Parse(gameObject.tag));

            float newCircleX = (gameObject.transform.position.x + collision.gameObject.transform.position.x) / 2;
            float newCircleY = (gameObject.transform.position.y + collision.gameObject.transform.position.y) / 2;
            float newCircleZ = (gameObject.transform.position.z + collision.gameObject.transform.position.z) / 2;

            Destroy(collision.gameObject);

            Vector3 newCirclePos = new Vector3(newCircleX, newCircleY, newCircleZ);
            GameObject newCircle = Instantiate(gameManager.circles[int.Parse(gameObject.tag)], newCirclePos, Quaternion.identity);
            newCircle.GetComponent<CircleLogic>().live = true;
            newCircle.GetComponent<CircleCollider2D>().enabled = true;

            if (int.Parse(newCircle.gameObject.tag) > gameManager.largestCircleMade)
                gameManager.largestCircleMade = int.Parse(newCircle.gameObject.tag);

            Destroy(gameObject);
        }

    }
}
