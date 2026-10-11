using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cloudmove : MonoBehaviour {
    public float amplitude = 0.5f; // 晃動幅度
    public float frequency = 1f;   // 晃動速度
    float startY;

    void Start() {
        startY = transform.position.y;
    }

    void Update() {
        float newY = startY + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
