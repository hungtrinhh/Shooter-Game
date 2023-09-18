using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager: MonoBehaviour {
    public enum AudioChannel { Master, Sfx, Music };

    public float masterVolume {
        get; private set;
    }
    public float sfxVolume {
        get; private set;
    }
    public float musicVolume {
        get; private set;
    }

    AudioSource sfx2dSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    SoundLibrary library;
    Transform audioListener;
    Transform player;

    void Awake () {
        if (instance != null) {
            Destroy (gameObject); //when you return to the menu you don't want to make a new AudioManager
            return;
        }
        instance = this;
        DontDestroyOnLoad (gameObject); // AudioManager will be transfered throughout the levels

        /*Init 2 Audiosources*/
        musicSources = new AudioSource[2];
        for (int i = 0; i < musicSources.Length; i++) {
            GameObject newMusicSource = new GameObject ("Music source 0" + (i + 1));
            musicSources[i] = newMusicSource.AddComponent<AudioSource> ();
            newMusicSource.transform.parent = transform;
        }
        /*Init 2D AudioSource*/
        GameObject newSfx2DSource = new GameObject ("2D sfx source ");
        sfx2dSource = newSfx2DSource.AddComponent<AudioSource> ();
        newSfx2DSource.transform.parent = transform;

        /*Load Player Preferences or defaults*/
        masterVolume = PlayerPrefs.GetFloat ("master vol", 1);
        musicVolume = PlayerPrefs.GetFloat ("music vol", 1);
        sfxVolume = PlayerPrefs.GetFloat ("sfx vol", 1);
    }

    void Start () {
        library = GetComponent<SoundLibrary> ();
        audioListener = FindObjectOfType<AudioListener> ().transform;
    }

    void OnEnable () {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable () {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
        if (FindObjectOfType<Player> () != null) 
            player = FindObjectOfType<Player> ().transform;
    }

    void Update () {
        if (player != null)
            audioListener.position = player.position;
    }

    public void SetVolume (float volume, AudioChannel channel) {
        switch (channel) {
            case AudioChannel.Master:
                masterVolume = volume;
                break;
            case AudioChannel.Music:
                musicVolume = volume;
                break;
            case AudioChannel.Sfx:
                sfxVolume = volume;
                break;
        }
        musicSources[activeMusicSourceIndex].volume = musicVolume * masterVolume;

        PlayerPrefs.SetFloat ("master vol", masterVolume);
        PlayerPrefs.SetFloat ("sfx vol", sfxVolume);
        PlayerPrefs.SetFloat ("music vol", musicVolume);
        PlayerPrefs.Save ();
    }

    public void PlayMusic (AudioClip clip, float fadeDuration = 1) {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex; //toggle the active audioSource
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play ();
        StartCoroutine (MusicFadeIn (fadeDuration)); //fade between audioSources
    }

    //Smoothly raise the volume of the active audioSources and lowers the volume of the inactive audioSource
    IEnumerator MusicFadeIn (float duration) {
        float percent = 0;
        while (percent < 1) {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp (0, musicVolume * masterVolume, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp (musicVolume * masterVolume, 0, percent);
            yield return null;
        }
    }

    public void PlaySoundEffect (AudioClip clip, Vector3 pos) {
        if (clip != null) {
            AudioSource.PlayClipAtPoint (clip, pos, sfxVolume * masterVolume);
        }
    }

    public void PlaySoundEffect (string name, Vector3 pos) {
        PlaySoundEffect (library.GetClipFromName (name), pos);
    }

    public void PlaySoundEffect2D (string name) {
        sfx2dSource.PlayOneShot (library.GetClipFromName (name), sfxVolume * masterVolume);
    }
}
