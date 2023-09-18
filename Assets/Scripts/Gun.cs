using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun: MonoBehaviour {

    public enum FireMode { Auto, Burst, Single };
    int burstSize = 3;

    [Header ("Components")]
    public Projectile bullet;
    public Transform[] bulletSpawnPoints;
    public Transform shell;
    public Transform shellEjectionPoint;

    [Header ("Variables")]
    public FireMode fireMode;
    public float rateOfFire = 10;
    public float muzzleVelocity = 35;
    public float horizontalRecoil = 0.4f;
    public float verticalRecoil = 5;
    public int magazineCapacity = 2;
    public float reloadTime = 1;
    public bool ejectShellsAtReload = false;
    public AudioClip shootSoundEffect;
    public AudioClip reloadSoundEffect;

    Muzzleflash muzzleflash;
    bool isReloading;
    float nextShotTime;
    int shotsRemainingInBurst;
    int shotsRemainingInMagazine;
    bool triggerReleasedSinceLastShot;

    //recoil variables
    Vector3 horizontalRecoilSmoothDampVelocity;
    float verticalRecoilSmoothDampVelocity;
    float recoilAngle;

    void Start () {
        muzzleflash = GetComponent<Muzzleflash> ();
        shotsRemainingInBurst = burstSize;
        shotsRemainingInMagazine = magazineCapacity;
    }

    void LateUpdate () {
        //animateRecoil
        transform.localPosition = Vector3.SmoothDamp (transform.localPosition, Vector3.zero, ref horizontalRecoilSmoothDampVelocity, .1f);
        recoilAngle = Mathf.SmoothDamp (recoilAngle, 0, ref verticalRecoilSmoothDampVelocity, .1f);
        transform.localEulerAngles = Vector3.left * recoilAngle;

        if (!isReloading && shotsRemainingInMagazine == 0) {
            Reload ();
        }
    }

    void Shoot () {
        if (Time.time > nextShotTime && shotsRemainingInMagazine > 0) {
            if (fireMode == FireMode.Burst) {
                if (shotsRemainingInBurst == 0)
                    return;
                shotsRemainingInBurst--;
            } else if (fireMode == FireMode.Single) {
                if (!triggerReleasedSinceLastShot)
                    return;
            }
            //bullet spawning
            shotsRemainingInMagazine--;
            for (int i = 0; i < bulletSpawnPoints.Length; i++) {
                nextShotTime = Time.time + 1 / rateOfFire;
                Projectile newBullet = Instantiate (bullet, bulletSpawnPoints[i].position, bulletSpawnPoints[i].rotation) as Projectile;
                newBullet.SetSpeed (muzzleVelocity);
            }
            //shells and muzzleflash
            muzzleflash.Activate ();
            if (!ejectShellsAtReload)
                Instantiate (shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            AudioManager.instance.PlaySoundEffect (shootSoundEffect, transform.position);

            //recoil
            transform.localPosition -= Vector3.forward * horizontalRecoil;
            recoilAngle += verticalRecoil;
            recoilAngle = Mathf.Clamp (recoilAngle, 0, 30);
        }
    }

    public void Reload () {
        if (!isReloading) {
            StartCoroutine (AnimateReload ());
            AudioManager.instance.PlaySoundEffect (reloadSoundEffect, transform.position);
        }
    }

    IEnumerator AnimateReload () {
        isReloading = true;
        yield return new WaitForSeconds (.2f);
        float percent = 0;
        float reloadSpeed = 1 / reloadTime;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;
        if (ejectShellsAtReload) {
            for (int i = 0; i < magazineCapacity; i++) {
                Instantiate (shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            }
        }
        while (percent < 1) {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = Utility.InterpolateOnParabol (percent);
            float reloadAngle = Mathf.Lerp (0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.back * reloadAngle;
            yield return null;
        }
        isReloading = false;
        shotsRemainingInMagazine = magazineCapacity;
    }

    public void Aim (Vector3 aimPoint) {
        transform.LookAt (aimPoint);
    }

    public void OnTriggerHold () {
        Shoot ();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease () {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstSize;
    }
}
