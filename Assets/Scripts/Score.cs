using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score: MonoBehaviour {
    public static int score {
        get; private set;
    }
    float lastKillTime;
    int streakCount;
    float streakKillTimeThreshold = 1;

    void Start () {
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player> ().OnDeath += OnPlayerDeath;
        score = 0;
    }

    void OnEnemyKilled () {
        if (Time.time < lastKillTime + streakKillTimeThreshold) {
            streakCount++;
        } else {
            streakCount = 0;
        }
        lastKillTime = Time.time;
        score += 5 + streakCount * 2;
    }

    void OnPlayerDeath () {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
