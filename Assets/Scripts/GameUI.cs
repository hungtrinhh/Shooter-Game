using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI: MonoBehaviour {
    [Header ("In Game UI")]
    public Text scoreUI;
    public RectTransform healthBar;
    [Header ("New Wave UI")]
    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    [Header("Game Over UI")]
    public Image fadePlane;
    public GameObject gameOverUI;
    public Text gameOverScoreUI;
    public Text newHighScore;

    Spawner spawner;
    Player player;

    string[] numbers = { "One", "Two", "Three", "Four", "Five" };

    void Awake () {
        spawner = FindObjectOfType<Spawner> ();
        spawner.OnNewWave += OnNewWave;
    }

    void Start () {
        player = FindObjectOfType<Player> ();
        player.OnDeath += OnGameOver;
    }

    void Update () {
        scoreUI.text = "- "+Score.score.ToString("D6")+" -";
        float healthPercent = 0;
        if (player != null) {
            healthPercent = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3 (healthPercent, 1, 1);
    }

    void OnNewWave (int waveNumber) {
        newWaveTitle.text = " - Wave " + numbers[waveNumber - 1] + " -";
        string enemyCountString = ((spawner.waves[waveNumber - 1].infiniteWave) ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount+"");
        newWaveEnemyCount.text = enemyCountString + " enemies";

        StartCoroutine (AnimateNewWaveBanner ());
    }

    IEnumerator AnimateNewWaveBanner () {
        float percent = 1;
        float animationSpeed = 3f;
        float delayTime = 1f;
        float endDelayTime = Time.time + 1 / animationSpeed + delayTime;
        int dir = 1;
        while (percent >= 0) {
            percent += Time.deltaTime * animationSpeed * dir;
            if (percent >= 1) {
                percent = 1;
                if (Time.time > endDelayTime) {
                    dir = -1;
                }
            }
            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp (-150, 200, percent);
            yield return null;
        }
    }

    void OnGameOver () {
        StartCoroutine (Fade (Color.clear, new Color(0,0,0,.85f), 1));
        gameOverUI.SetActive (true);
        scoreUI.gameObject.SetActive (false);
        gameOverScoreUI.text = scoreUI.text;
        Cursor.visible = true;
        int highscore = PlayerPrefs.GetInt ("highscore");
        if (Score.score > highscore) {
            PlayerPrefs.SetInt ("highscore", Score.score);
            newHighScore.gameObject.SetActive (true);
        }
    }

    IEnumerator Fade (Color from, Color to, float time) {
        float speed = 1 / time;
        float percent = 0;
        while (percent < 1) {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp (from, to, percent);
            yield return null;
        }
    }

    //GAMEOVER INPUT HANDLING
    public void StartNewGame () {
        SceneManager.LoadScene ("Game");
    }

    public void MainMenu () {
        SceneManager.LoadScene ("Menu");
    }
}
