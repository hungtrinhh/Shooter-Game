using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player: LivingEntity {
    [Header("Components")]
    public Crosshair crosshair;

    [Header ("Variables")]
    public float moveSpeed = 5;

    PlayerController controller;
    GunController gunController;
    Camera viewCamera;

    //Methods
    protected override void Start () {
        base.Start ();
    }

    protected override void Awake () {
        base.Awake ();
        controller = GetComponent<PlayerController> ();
        gunController = GetComponent<GunController> ();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
    }

    void OnNewWave (int waveNumber) {
        health = startingHealth;
        gunController.EquipGun (waveNumber - 1);
    }

    void Update () {
        //Movement input
        Vector3 moveInput = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move (moveVelocity);
        if (transform.position.y < -10) {
            TakeDamage (health);
        }

        //Look input
        Ray ray = viewCamera.ScreenPointToRay (Input.mousePosition);
        Plane groundPlane = new Plane (Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;
        if (groundPlane.Raycast (ray, out rayDistance)) {
            Vector3 point = ray.GetPoint (rayDistance);
            controller.LookAt (point);
            crosshair.transform.position = point;
            crosshair.DetectTargets (ray);
            float distanceCrosshairPlayer = (new Vector2 (point.x, point.y) - new Vector2 (transform.position.x, transform.position.y)).sqrMagnitude;
            if (distanceCrosshairPlayer >= 1) {
                gunController.Aim (point);
            }
        }

        //Weapon input
        if (Input.GetMouseButton (0)) {
            gunController.OnTriggerHold ();
        }
        if (Input.GetMouseButtonUp (0)) {
            gunController.OnTriggerRelease ();
        }
        if (Input.GetKeyDown (KeyCode.R)) {
            gunController.Reload ();
        }
    }

    protected override void Die () {
        AudioManager.instance.PlaySoundEffect ("Player Death", transform.position);
        base.Die ();
    }
}
