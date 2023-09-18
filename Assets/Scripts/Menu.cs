using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu: MonoBehaviour {
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public Text highScore;

    [Header ("Options UI")]
    public Slider[] volumeSliders;
    public Toggle fullscreenToggle;

    void Start () {
        fullscreenToggle.isOn = PlayerPrefs.GetInt ("fullscreen") == 1 ? true : false;
        SetFullScreen (fullscreenToggle.isOn);
        volumeSliders[0].value = AudioManager.instance.masterVolume;
        volumeSliders[1].value = AudioManager.instance.sfxVolume;
        volumeSliders[2].value = AudioManager.instance.musicVolume;
        highScore.text = "- " + PlayerPrefs.GetInt ("highscore", 0).ToString ("D6")+ " -";
    }

    public void Play () {
        SceneManager.LoadScene ("Game");
    }

    public void Quit () {
        Application.Quit ();
    }

    public void OptionsMenu () {
        mainMenu.SetActive (false);
        optionsMenu.SetActive (true);
    }

    public void MainMenu () {
        mainMenu.SetActive (true);
        optionsMenu.SetActive (false);
    }
    
    public void SetMasterVolume (float value) {
        AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume (float value) {
        AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume (float value) {
        AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Sfx);
    }

    public void SetFullScreen (bool isFullScreen) {
        Resolution maxResolution = Screen.resolutions[Screen.resolutions.Length - 1];
        Screen.SetResolution (maxResolution.width, maxResolution.height, isFullScreen);
        PlayerPrefs.SetInt ("fullscreen", ((isFullScreen) ? 1 : 0));
        PlayerPrefs.Save ();
    }
}
