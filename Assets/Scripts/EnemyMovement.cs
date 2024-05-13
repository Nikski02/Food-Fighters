using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private int damage = 1;

    private float timeToJump = 2.2f;
    public bool jumping = false;
    private float jumpStart = 0f;
    private Vector3 jumpTo;

    private Transform target;
    private int pathIndex = 0;
    private SpriteRenderer _renderer;

    private float timeSince(float when)
    {
        return Time.time - when;
    }

    private void Start() {
        target = LevelManager.main.path[pathIndex];
        _renderer = this.GetComponent<SpriteRenderer>();
    }

    private void Update() {
        if (jumping) return;
        if (Vector2.Distance(target.position, transform.position) <= 0.1f) {
            pathIndex++;

            if (pathIndex == LevelManager.main.path.Length) {
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);

                LevelManager.main.DamagePlayer(damage);
                return;
            } else {
                target = LevelManager.main.path[pathIndex];
            }
        }
    }

    private Vector2 originalSize;
    public void Jump()
    {
        if (jumping) return;
        // pick any two points on the path.
        float fInitPathIndex = UnityEngine.Random.value * (LevelManager.main.path.Length - 1);
        fInitPathIndex = Mathf.Clamp(fInitPathIndex, 0, LevelManager.main.path.Length - 2);
        int a = (int)fInitPathIndex;
        int b = a + 1;

        var start = LevelManager.main.path[a];
        var end = LevelManager.main.path[b];

        this.pathIndex = b;
        target = LevelManager.main.path[this.pathIndex];

        float randomX = UnityEngine.Random.Range(start.position.x, end.position.x);
        float randomY = UnityEngine.Random.Range(start.position.y, end.position.y);
        float randomZ = UnityEngine.Random.Range(start.position.z, end.position.z);

        jumpTo = new Vector3(randomX, randomY, randomZ);
        jumping = true;
        jumpStart = Time.time;
        originalSize = _renderer.size;

        var health = gameObject.GetComponent<Health>();
        health.invulnerable = true;
        gameObject.GetComponent<Animator>().speed = 0;

        rb.velocity = (jumpTo - transform.position) / timeToJump;
    }

    public void ScaleSpeed(float scaleSpeed)
    {
        moveSpeed *= scaleSpeed;
    }

    private void FixedUpdate() {
        if (jumping)
        {
            // stop jumping
            if (timeSince(jumpStart) > timeToJump)
            {
                jumping = false;
                var health = gameObject.GetComponent<Health>();
                health.invulnerable = false;
                _renderer.size = originalSize;
                transform.Rotate(0, 0, 0);
                gameObject.GetComponent<Animator>().speed = 1;
                return;
            }

            // rotate and scale as being jumped around.
            float rotationThisFrame = 360 / timeToJump * Time.deltaTime;
            transform.Rotate(0, 0, rotationThisFrame);

            float jumpPeakTime = timeToJump / 2.0f;
            float pathTime = timeSince(jumpStart);

            if (pathTime < jumpPeakTime)
            {
                _renderer.size = originalSize * Mathf.Lerp(1f, 3f, pathTime / jumpPeakTime);
            } else
            {
                _renderer.size = originalSize * Mathf.Lerp(3f, 1f, (pathTime - jumpPeakTime) / jumpPeakTime);
            }
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;

        rb.velocity = direction * moveSpeed;
        if (Time.time - stallStartTime < .5f)
        {
            rb.velocity *= 0.5f;
        }
        _renderer.flipX = direction.x < 0;
    }

    private float stallStartTime = 0;
    public void Stall()
    {
        stallStartTime = Time.time;
    }
}
