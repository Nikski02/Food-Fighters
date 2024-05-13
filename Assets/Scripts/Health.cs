using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] public int hitPoints = 6;
    [SerializeField] private int currencyWorth = 50;
    [SerializeField] private int scorePoints = 5;
    [SerializeField] public bool invulnerable = false;

    [Header("Splatter")]
    [SerializeField] private GameObject splatterPrefab;
    [SerializeField] private bool isRed;


    // private List<Weakness> mWeaknesses;

    private bool isDestroyed = false;

    public void TakeDamage(Bullet bullet, bool isCutter, bool showSplatterFx)
    {
        if (invulnerable) return;
        int dmg = 0;
        if (!isCutter)
        {
            dmg = bullet.GetBulletDamange();
        }
        else
        {
            dmg = 1; // Cutter damage is 1
        }

        /* Debug.Assert(mWeaknesses != null);
        if (mWeaknesses.Count >= 1)
        {
            if (bullet.GetWeakness() == mWeaknesses[mWeaknesses.Count - 1])
            {
                mWeaknesses.RemoveAt(mWeaknesses.Count - 1);
            } else
            {
                dmg = dmg / 5;
            }
        } */

        hitPoints -= dmg;

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemySpawner.onEnemyDestroy.Invoke();

            LevelManager.main.IncreaseCurrency(currencyWorth);
            LevelManager.main.UpdateScore(scorePoints);
            LevelManager.main.EnemyDestroyed();

            if (showSplatterFx)
            {
                GameObject splatterObj = Instantiate(splatterPrefab, this.transform.position, Quaternion.identity);
                var splatter = splatterObj.GetComponent<Splatter>();
                if (isRed)
                    splatter.Red();
                else
                    splatter.Blue();
            }

            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    /* public void SetWeaknesses(Weakness[] weaknesses)
    {
        mWeaknesses = new List<Weakness>();
        mWeaknesses.AddRange(weaknesses);
    } */
}