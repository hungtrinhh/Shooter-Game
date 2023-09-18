using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;
    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]> ();

    void Awake () {
        foreach (SoundGroup group in soundGroups) {
            groupDictionary.Add (group.groupID, group.clips);
        }
    }

    //Returns a random clip from the group with that name
    public AudioClip GetClipFromName (string name) {
        if (groupDictionary.ContainsKey (name)) {
            return groupDictionary[name][Random.Range (0, groupDictionary[name].Length)];
        }
        Debug.Log ("Selected invalid groupname");
        return null;
    }

    [System.Serializable]
    public class SoundGroup {
        public string groupID;
        public AudioClip[] clips;
    }
}
