using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    //public
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    //private
    string currentScene;

    void OnEnable () {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable () {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
        if (scene.name != currentScene)
            Invoke ("PlayMusic",.2f);
        currentScene = scene.name;
    }

    void PlayMusic () {
        AudioClip clipToPlay = null;
        switch (currentScene) {
            case "Menu":
                clipToPlay = menuTheme;
                break;
            case "Game":
                clipToPlay = mainTheme;
                break;
            default:
                Debug.Log ("Scene does not have a music theme");
                break;
        }
        if (clipToPlay != null) {
            AudioManager.instance.PlayMusic (clipToPlay, 2);
            Invoke ("PlayMusic", clipToPlay.length);
        }
    }
}
