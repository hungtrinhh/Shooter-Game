using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController: MonoBehaviour {
    //Variables
    Rigidbody myRigidBody;
    Vector3 velocity;

    //Methods
    void Start () {
        myRigidBody = GetComponent<Rigidbody> ();
    }

    public void Move (Vector3 _velocity) {
        velocity = _velocity;
    }

    public void LookAt (Vector3 lookPoint) {
        Vector3 eyeHeightPoint = new Vector3 (lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt (eyeHeightPoint);
    }

    void FixedUpdate () {
        myRigidBody.MovePosition (myRigidBody.position + velocity * Time.fixedDeltaTime);
    }
}
