using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] GameObject explosion;

    [Header("Duration")]
    [SerializeField] private float min;
    [SerializeField] private float max;

    private float startTime;
    private float endTime;
    SpriteRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        endTime = startTime + UnityEngine.Random.Range(min, max);
        rend = GetComponent<SpriteRenderer>();
        rend.size *= UnityEngine.Random.Range(0.9f, 1.1f);
        this.transform.Rotate(0, 0, (int)UnityEngine.Random.Range(1, 179));
    }

    void FixedUpdate()
    {
        if (Time.time > endTime)
        {
            Destroy(gameObject);
            return;
        }
    }
}
