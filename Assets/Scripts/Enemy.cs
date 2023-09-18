using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent (typeof (NavMeshAgent))]
public class Enemy: LivingEntity {
    //Variables
    public ParticleSystem deathEffect;
    public static event System.Action OnDeathStatic;

    public enum State { Idle, Chasing, Attacking };
    State currentState;
    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material myMaterial;
    Color originalColor;
    public Color attackColor = Color.red;
    float myCollisionRadius;
    float targetCollisionRadius;


    float attack_Distance = 1.5f;
    float attack_TimeBetween = 1;
    float attack_NextTime;
    float attack_Damage = 1;

    bool hasTarget;
    //Methods
    void OnTargetDeath () {
        hasTarget = false;
        currentState = State.Idle;
    }

    protected override void Awake () {
        base.Awake ();
        pathfinder = GetComponent<NavMeshAgent> ();

        if (GameObject.FindGameObjectWithTag ("Player") != null) {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag ("Player").transform;
            targetEntity = target.GetComponent<LivingEntity> ();

            myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
            targetCollisionRadius = GetComponent<CapsuleCollider> ().radius;
        }
    }

    protected override void Start () {
        base.Start ();
        if (hasTarget) {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;
            StartCoroutine (UpdatePath ());
        }
    }

    void Update () {
        if (hasTarget) {
            if (Time.time > attack_NextTime) {
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDistanceToTarget < Mathf.Pow (attack_Distance + myCollisionRadius + targetCollisionRadius, 2)) {
                    RaycastHit hit;
                    if (Physics.Linecast (new Vector3(transform.position.x, transform.position.y-0.95f, transform.position.z), target.transform.position,out hit)) {
                        if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("Obstacle"))
                            return;
                    }
                    attack_NextTime = Time.time + attack_TimeBetween;
                    AudioManager.instance.PlaySoundEffect ("Enemy Attack", transform.position);
                    StartCoroutine (Attack ());
                }
            }
        }
    }

    public override void TakeHit (float damage, Vector3 pointOfImpact, Vector3 impactDirection) {
        AudioManager.instance.PlaySoundEffect ("Impact", transform.position);
        if (damage >= health) {
            OnDeathStatic?.Invoke ();
            AudioManager.instance.PlaySoundEffect ("Enemy Death", transform.position);
            Destroy(Instantiate (deathEffect.gameObject, pointOfImpact, Quaternion.FromToRotation (Vector3.forward, impactDirection)) as GameObject, deathEffect.startLifetime);
        }
        base.TakeHit (damage, pointOfImpact, impactDirection);
    }

    public void SetCharacteristics (float moveSpeed, float newHealth, int attackDamage, Color color) {
        pathfinder.speed = moveSpeed;
        attack_Damage = attackDamage;
        health = newHealth;
        myMaterial = GetComponent<Renderer> ().material;
        myMaterial.color = color;
        originalColor = color;
        deathEffect.GetComponent<ParticleSystemRenderer> ().sharedMaterial.color = color;
    }

    IEnumerator Attack () {
        currentState = State.Attacking;
        Vector3 startPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0;

        bool hasDamagedTarget = false;
        myMaterial.color = attackColor;
        while (percent <= 1) {
            if (percent >= .5f && !hasDamagedTarget) {
                hasDamagedTarget = true;
                targetEntity.TakeDamage(attack_Damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = Utility.InterpolateOnParabol (percent);
            transform.position = Vector3.Lerp (startPosition, attackPosition, interpolation);
            yield return null;
        }
        myMaterial.color = originalColor;
        currentState = State.Chasing;
    }

    IEnumerator UpdatePath () {
        float waitTime = 0.25f;

        while (hasTarget) {
            if (currentState == State.Chasing) {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attack_Distance / 2);
                if (!dead) {
                    pathfinder.SetDestination (targetPosition);
                }
            }
            yield return new WaitForSeconds (waitTime);
        }
    }
}
