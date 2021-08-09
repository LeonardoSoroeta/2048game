using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour
{
    public static ScoreTracker instance;

    private static int score;
    private static int bestScore;
    public Text scoreText;
    public Text bestScoreText;

    void Awake()
    {
        instance = this;

        if (!PlayerPrefs.HasKey("BestScore"))
        {
            PlayerPrefs.SetInt("BestScore", 0);
        }

        score = 0;
        scoreText.text = "0";
        bestScore = PlayerPrefs.GetInt("BestScore");
        bestScoreText.text = bestScore.ToString();
    }

    public int GetScore()
    {
        return score;
    }

    public void SetScore(int amount)
    {
        score = amount;
        scoreText.text = score.ToString();

        if (score > bestScore)
        {
            bestScore = score;
            bestScoreText.text = bestScore.ToString();
            PlayerPrefs.SetInt("BestScore", bestScore);
        }
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        scoreText.text = score.ToString();

        if (score > bestScore)
        {
            bestScore = score;
            bestScoreText.text = bestScore.ToString();
            PlayerPrefs.SetInt("BestScore", bestScore);
        }
    }

    public void ResetBestScore()
    {
        bestScore = 0;
        bestScoreText.text = "0";
        PlayerPrefs.SetInt("BestScore", 0);
    }
}
