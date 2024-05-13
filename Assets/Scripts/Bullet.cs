using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Audio")]
    [SerializeField] private AudioClip[] shootClips;
    [SerializeField] private AudioClip impactClip;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Cannon tower")]
    [SerializeField] public float damageRadius = 0f;
    [SerializeField] public int maxEnemiesToHit = 8;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;



    private Transform target;
    private float birthTime;
    private const float TTL = 5.0f;
    // private Weakness type;

    private void Start()
    {
        if (shootClips != null)
        {
            int which = (int)(UnityEngine.Random.value * (shootClips.Length - 1));
            LevelManager.main.WeaponSfx(shootClips[which]);
        }
        birthTime = Time.time;
    }


    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    /* public void SetType(Weakness _type)
    {
        type = _type;
    } */
    public int GetBulletDamange()
    {
        return bulletDamage;
    }
    /* public Weakness GetWeakness()
    {
        return type;
    } */

    private void FixedUpdate()
    {
        if (Time.time - birthTime > TTL)
        {
            Destroy(gameObject);
            return;
        }
        if (!target) return;
        Vector2 direction = (target.position - transform.position).normalized;

        rb.velocity = direction * bulletSpeed;
        FaceTarget();
    }

    private void FaceTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 180f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        this.transform.rotation = targetRotation;
    }

    private bool hasCollided = false;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (hasCollided) return;
        hasCollided = true; // when there are a spawn of overlapping enemies, it used to target all.
        other.gameObject.GetComponent<Health>().TakeDamage(this, false, true);

        if (impactClip != null)
        {
            LevelManager.main.WeaponSfx(impactClip);
        }

        if (damageRadius > 0f)
        {
            BulletExplode();
        }
        Destroy(gameObject);
    }

    private void BulletExplode()
    {
        GameObject explosionObj = Instantiate(explosionPrefab, this.transform.position, Quaternion.identity);
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject enemy in enemies)
        {
            if (damageRadius >= Vector3.Distance(transform.position, enemy.transform.position))
            {
                var health = enemy.GetComponent<Health>();
                health.TakeDamage(this, false, false); // don't show splatter when obviously covered by explosion fx.
                maxEnemiesToHit -= 1;
                if (maxEnemiesToHit == 0) return;
            }
        }
    }

    

}