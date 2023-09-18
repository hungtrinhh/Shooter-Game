using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile: MonoBehaviour {
    public LayerMask collisionMask;
    public Color tracerColor;


    float speed = 10;
    public float damage = 1;
    float lifetime = 2;
    float extraDistance = .1f; //takes into account the distance moved by the enemies

    //Methods
    void Start () {
        Destroy (gameObject, lifetime);

        Collider[] initialCollision = Physics.OverlapSphere (transform.position, .1f, collisionMask);
        if (initialCollision.Length > 0) {
            OnHitObject (initialCollision[0],transform.position);
        }

        GetComponent<TrailRenderer> ().material.color = tracerColor;
    }

    public void SetSpeed (float newSpeed) {
        speed = newSpeed;
    }

    public void SetDamage (float newDamage) {
        damage = newDamage;
    }
    void Update () {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions (moveDistance);
        transform.Translate (Vector3.forward * moveDistance);
    }


    void CheckCollisions (float moveDistance) {
        Ray ray = new Ray (transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit, moveDistance + extraDistance, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject (hit.collider,hit.point);
        }
    }

    void OnHitObject (Collider collider, Vector3 pointOfImpact) {
        IDamageable collidedObject = collider.GetComponent<IDamageable> ();
        if (collidedObject != null) {
            collidedObject.TakeHit (damage,pointOfImpact,transform.forward);
        }
        GameObject.Destroy (gameObject);
    }
}
