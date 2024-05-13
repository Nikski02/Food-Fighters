using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Tower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform towerRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject cutterArm;

    [Header("Attribute")]
    [SerializeField] private float targetingRange = 3f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float bps = 1f; // Bullets Per Second

    [Header("Cutter Attributes")]
    [SerializeField] private bool isCutter = false;
    [SerializeField] private float secondsActiveFor = 1;
    [SerializeField] private int rechargeTime = 4;
    [SerializeField] private float spinUpTime = 0.5f;
    [SerializeField] private float jumpTriggerProb = 0.2f;
    private bool cutterCutting = false;
    private float lastCutterPhaseChange = 0;
    private float currentCutterSpeed = 0f;


    private Transform target;
    private Transform[] cutterTargets;
    private float timeUntilFire;
    // private Weakness[] strengths;

    private float timeSince(float when)
    {
        return Time.time - when;
    }

    private void Start()
    {
        lastConsideredByCutter = new Dictionary<Object, float>();
    }

    private void FixedUpdate()
    {
        if (!isCutter)
        {
            if (target == null)
            {
                FindTarget();
                return;
            }

            RotateTowardsTarget();

            if (!CheckTargetIsInRange())
            {
                target = null;
            }
            else
            {

                timeUntilFire += Time.deltaTime;

                if (timeUntilFire >= 1f / bps)
                {
                    Shoot();
                }
            }
        }
        else
        {
            // start running it up.
            if (!cutterCutting && timeSince(lastCutterPhaseChange) > rechargeTime)
            {
                lastCutterPhaseChange = Time.time;
                cutterCutting = true;
                LevelManager.main.CutterSfxStart(this);
            }
            // ran too much. slow down to recharge
            if (cutterCutting && timeSince(lastCutterPhaseChange) > secondsActiveFor)
            {
                lastCutterPhaseChange = Time.time;
                cutterCutting = false;
                LevelManager.main.CutterSfxStop(this);
                lastConsideredByCutter.Clear();
            }

            // accelerate/deaccelerate
            if (timeSince(lastCutterPhaseChange) <= spinUpTime)
            {
                float min = cutterCutting ? 0 : rotationSpeed;
                float max = cutterCutting ? rotationSpeed : 0;
                float currRps = Mathf.Lerp(min, max, timeSince(lastCutterPhaseChange) / spinUpTime);

                // can't use clamp because min/max might be reversed.
                if (currRps < 0) currRps = 0;
                if (currRps > rotationSpeed) currRps = rotationSpeed;
                currentCutterSpeed = currRps;
            }

            RotateCutter();

            if (!cutterCutting) return;
            FindTarget();
            if (cutterTargets != null && cutterTargets.Length > 0)
            {
                CutterAttack();
            }
        }
    }

    private int bulletsShot = 0;
    private void Shoot()
    {
        /* if (this.strengths == null)
        {
            // object not yet instantiated on other thread. wait for it to finish.
            return;
        } */
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target);

        // Weakness type = strengths[bulletsShot % strengths.Length];
        // bulletScript.SetType(type);

        SpriteRenderer sr = bulletObj.GetComponent<SpriteRenderer>();

        /* switch (type)
        {
            case Weakness.GREEN:
                sr.color = Color.green; break;
            case Weakness.RED:
                sr.color = Color.red; break;
            case Weakness.BLUE:
                sr.color = Color.blue; break;
        } */

        bulletsShot++;
        timeUntilFire = 0f;
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange,
        (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;

            if (isCutter)
            {
                cutterTargets = new Transform[hits.Length];
                for (int i = 0; i < hits.Length; i++)
                {
                    cutterTargets[i] = hits[i].transform;
                }
            }
        }
    }

    private bool CheckTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x -
        transform.position.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        towerRotationPoint.rotation = Quaternion.RotateTowards(towerRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }

    /* public void SetStrengths(Weakness[] _strengths)
    {
        strengths = _strengths;
    } */

    private void RotateCutter()
    {
        float angle = -currentCutterSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle)) * towerRotationPoint.rotation;
        towerRotationPoint.rotation = targetRotation;
    }

    Dictionary<Object, float> lastConsideredByCutter;
    private void CutterAttack()
    {
        Collider2D cutterCollider = cutterArm.GetComponent<Collider2D>();

        if (cutterCollider != null)
        {
            // Prepare a contact filter to ensure we only detect objects with the "enemy" tag
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(enemyMask);
            contactFilter.useLayerMask = true;

            // Array to store all colliders that intersect with the cutter collider
            Collider2D[] colliders = new Collider2D[8]; // Adjust the size according to your needs

            // Perform the overlap check
            int colliderCount = cutterCollider.OverlapCollider(contactFilter, colliders);

            for (int i = 0; i < colliderCount; i++)
            {
                Collider2D collider = colliders[i];
                // Check if the collider belongs to an enemy
                if (collider.CompareTag("enemy"))
                {
                    float check;
                    bool found = lastConsideredByCutter.TryGetValue(collider.gameObject, out check);
                    if (found && timeSince(check) < 0.1f)
                    {
                        continue;
                    }
                    if (!found && lastConsideredByCutter.Count > 2*8) continue; // for each run, only consider around twice the number of objects. For very quick waves, each frame can check different 8 enemies per frame, effectively hitting all. Over here, we are limiting it to only hit ~16 enemies per burst of movement. This dictionary gets cleared when it recharges.
                    lastConsideredByCutter[collider.gameObject] = Time.time;
                    EnemyMovement movement = collider.gameObject.GetComponent<EnemyMovement>();
                    Health enemyHealth = collider.gameObject.GetComponent<Health>();
                    if (movement.jumping) continue;
                    if (UnityEngine.Random.value < jumpTriggerProb)
                    {
                        movement.Jump();
                    }
                    else
                    {
                        if (enemyHealth != null)
                        {
                            // Call TakeDamage on the enemy's health component
                            enemyHealth.TakeDamage(null, true, true);
                            movement.Stall();
                        }
                    }
                }
            }
        }
    }

}