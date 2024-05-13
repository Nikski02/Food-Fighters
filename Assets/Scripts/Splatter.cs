using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splatter : MonoBehaviour
{
    [SerializeField] Sprite redSplatter;
    [SerializeField] Sprite blueSplatter;

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

    public void Red()
    {
        this.GetComponent<SpriteRenderer>().sprite = redSplatter;
    }

    public void Blue()
    {
        this.GetComponent<SpriteRenderer>().sprite = blueSplatter;
    }

    void FixedUpdate()
    {
        if (Time.time > endTime)
        {
            Destroy(gameObject);
            return;
        }
        float delta = Time.time - startTime;
        float timeDone = delta / (endTime - startTime);
        float alpha = Mathf.Lerp(0.8f, 0, timeDone);

        var col = rend.color;
        col.a = alpha;
        rend.color = col;
    }
}
