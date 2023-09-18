using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController: MonoBehaviour {
    //Variables
    public Transform weaponSocket;
    public Gun[] guns;
    Gun currentGun;

    //Methods
    void Start () {
    }

    public void EquipGun (Gun newGun) {
        if (currentGun != null) {
            Destroy (currentGun.gameObject);
        }
        currentGun = Instantiate (newGun, weaponSocket.position, weaponSocket.rotation) as Gun;
        currentGun.transform.parent = weaponSocket;
    }

    public void EquipGun (int weaponIndex) {
        EquipGun (guns[weaponIndex]);
    }

    public void OnTriggerHold () {
        if (currentGun != null) {
            currentGun.OnTriggerHold ();
        }
    }

    public void OnTriggerRelease () {
        if (currentGun != null) {
            currentGun.OnTriggerRelease ();
        }
    }

    public float GunHeight {
        get{
            return weaponSocket.position.y;
        }
    }

    public void Aim (Vector3 aimPoint) {
        if (currentGun != null) {
            currentGun.Aim (aimPoint);
        }
    }

    public void Reload () {
        if (currentGun != null) {
            currentGun.Reload ();
        }
    }
}
