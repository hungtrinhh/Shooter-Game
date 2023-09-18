using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public LayerMask targetMask;
    public SpriteRenderer dot;
    public Color highLightColor;
    Color originalColor;

    void Start () {
        Cursor.visible = false;
        originalColor = dot.color;
    }
    
    void Update()
    {
        transform.Rotate (Vector3.forward * -40 * Time.deltaTime);
    }

    public void DetectTargets (Ray ray) {
        if (Physics.Raycast (ray, 100, targetMask)) {
            dot.color = highLightColor;
        } else {
            dot.color = originalColor;
        }
    }
}
