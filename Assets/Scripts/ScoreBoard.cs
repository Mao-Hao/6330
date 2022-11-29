using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    public static TextMeshProUGUI scoreText;

    public static int score = 0;

    public static void UpdateScore(int _score)
    {
        score += _score;
        scoreText.text = "Score: " + score.ToString();
        Debug.Log(score);
    }

    // Start is called before the first frame Update
    void Start()
    {
        scoreText = gameObject.GetComponent<TextMeshProUGUI>();
        scoreText.text = "Score: " + score.ToString();
    }

}
