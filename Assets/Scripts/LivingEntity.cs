using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity: MonoBehaviour, IDamageable {
    [Header ("Living Entity Variables")]
    public float startingHealth;
    public float health { get; protected set; }
    protected bool dead;

    //Methods
    public event System.Action OnDeath;


    protected virtual void Awake () {
        health = startingHealth;
    }
    
    //Called some time after Instantiate
    protected virtual void Start () {
    }

    public virtual void TakeHit (float damage, Vector3 pointOfImpact, Vector3 impactDirection) {
        TakeDamage (damage);
    }

    public virtual void TakeDamage (float damage) {
        health -= damage;
        if (health <= 0 && !dead) {
            Die ();
        }
    }

    [ContextMenu("Self Destruct")]
    protected virtual void Die () {
        dead = true;
        if (OnDeath != null) {
            OnDeath ();
        }
        GameObject.Destroy (gameObject);
    }
}
