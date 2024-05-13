using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour {

    [Serializable] public struct SpawnWave {
        public int nMobs;
        public int nTanks;
        public int durationSeconds;
        public int waitBefore;
        public int waveReward;
    }

    [Serializable] public struct ControlToolTip
    {
        public int waveIdx;
        public bool afterWave;
        public String message;
        public int duration;
    }

    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private AudioClip waveSpawnAudio;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 8;
    [SerializeField] private float enemiesPerSecond = 0.5f;
    //[SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyScalingFactor = 0.75f;
    [SerializeField] private SpawnWave[] wavesAttrs;

    [Header("Tooltips")]
    [SerializeField] private ControlToolTip[] toolTips;
    [SerializeField] private GameObject tipBar;

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int currentWave;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;

    private void Awake() {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    private void Start() {
        currentWave = 0;
        StartCoroutine(StartWave());
    }

    private void FixedUpdate() {
        if (!isSpawning || LevelManager.main.gameEnded) return;

        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= (1f / enemiesPerSecond) && enemiesLeftToSpawn > 0) {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            enemiesAlive++;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive == 0 && enemiesLeftToSpawn == 0) {
            EndWave();
        }
    }

    private void EnemyDestroyed() {
        enemiesAlive--;
    }


    private IEnumerator StartWave() {
        Debug.Log("Triggering wave!" + currentWave);

        if (sandboxActivated)
        {
            sandboxWaveBump++;
            wavesAttrs[currentWave].waitBefore = UnityEngine.Random.Range(1, 3);
            wavesAttrs[currentWave].durationSeconds = UnityEngine.Random.Range(5, 6);
            wavesAttrs[currentWave].nTanks = SandboxDiffMultiplier(UnityEngine.Random.Range(8, 14));
            int total = SandboxDiffMultiplier(UnityEngine.Random.Range(25, 40));
            int mobs = total - wavesAttrs[currentWave].nTanks;
            mobs = Math.Clamp(mobs, 10, total);
            wavesAttrs[currentWave].nMobs = mobs;
            wavesAttrs[currentWave].waveReward = 100;
        }

        yield return new WaitForSeconds(wavesAttrs[currentWave].waitBefore);

        isSpawning = true;
        enemiesLeftToSpawn = wavesAttrs[currentWave].nMobs + wavesAttrs[currentWave].nTanks;
        // enemiesLeftToSpawn = EnemiesPerWave();
        enemiesPerSecond = EnemiesPerSecond();
        this.sfxAudioSource.PlayOneShot(waveSpawnAudio);

        showNextValidTip(currentWave, false);
    }

    private bool sandboxActivated = false;
    private void EndWave() {
        LevelManager.main.UpdateScore(currentWave * 25);
        LevelManager.main.IncreaseCurrency(wavesAttrs[currentWave].waveReward);
        isSpawning = false;
        timeSinceLastSpawn = 0f;

        showNextValidTip(currentWave, true);

        currentWave++;

        if (currentWave >= wavesAttrs.Length)
        {
            LevelManager.main.StartSandboxMode();
            sandboxActivated = true;
            showNextValidTip(currentWave, false);
            currentWave -= 1;
            string currScene = SceneManager.GetActiveScene().name;
            if (currScene == "Level_1")
                LevelSelect.main.BumpProgressIfNeeded(1);
            else if (currScene == "Level_2")
                LevelSelect.main.BumpProgressIfNeeded(2);
            else
                LevelSelect.main.BumpProgressIfNeeded(3);
        }

        StartCoroutine(StartWave());
    }


    int whichPrefab = 0;
    private void SpawnEnemy() {
        GameObject prefabToSpawn = enemyPrefabs[whichPrefab];
        whichPrefab += 1;
        whichPrefab %= enemyPrefabs.Length;
        GameObject newEnemy = Instantiate(prefabToSpawn, LevelManager.main.startPoint.position, Quaternion.identity);
        Health health = newEnemy.GetComponent<Health>();
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        int mobsLeft = wavesAttrs[currentWave].nMobs;
        int tanksLeft = wavesAttrs[currentWave].nTanks;

        bool isTank;
        if (mobsLeft == 0) isTank = true;
        else if (tanksLeft == 0) isTank = false;
        else {
            isTank = UnityEngine.Random.value > 0.5f;
        }

        if (isTank)
        {
            enemyMovement.ScaleSpeed(0.8f);
            health.hitPoints *= 2;
            SpriteRenderer renderer = newEnemy.GetComponent<SpriteRenderer>();

            // health.SetWeaknesses(new Weakness[] { Weakness.RED });
            renderer.color = Color.red;
            renderer.size *= 1.8f;
        } else
        {
            enemyMovement.ScaleSpeed(1.2f);
            // health.SetWeaknesses(new Weakness[] { Weakness.BLUE });
            // newEnemy.GetComponent<SpriteRenderer>().color = Color.blue;
        }

        if (isTank) {
            wavesAttrs[currentWave].nTanks -= 1;
        } else {
            wavesAttrs[currentWave].nMobs -= 1;
        } 
    }


    private int sandboxWaveBump = 0;
    private int SandboxDiffMultiplier(float what) {
        float multiplier = Mathf.Pow(currentWave, difficultyScalingFactor);
        return Mathf.RoundToInt(multiplier * what);
    }

    private float EnemiesPerSecond()
    {
        SpawnWave wave = wavesAttrs[currentWave];
        return (float)(wave.nMobs + wave.nTanks) / wave.durationSeconds;
    }

    private int _nextTipToConsume = 0;
    private bool _sanboxTooltipShown = false;
    private bool _showNextValidTipMessage(int waveIdx, bool after)
    {
        if (sandboxActivated)
        {
            if (!_sanboxTooltipShown)
            {
                _sanboxTooltipShown = true;
                StartCoroutine(tipBar.GetComponent<ToolTips>().ShowMessage("Congratulations! You can play next the next level by going to the main menu. Survive, if you can.", 16));
                return true;
            }
            return false;
        }

        if (_nextTipToConsume >= toolTips.Length) return false;
        ControlToolTip curr = toolTips[_nextTipToConsume];
        if (curr.waveIdx == waveIdx && curr.afterWave == after)
        {
            StartCoroutine(tipBar.GetComponent<ToolTips>().ShowMessage(curr.message, curr.duration));
            _nextTipToConsume++;
            return true;
        }

        return false;
    }

    private void showNextValidTip(int waveIdx, bool after)
    {
        bool shownSomething = _showNextValidTipMessage(waveIdx, after);
        if (!shownSomething && !after && !sandboxActivated)
        {
            StartCoroutine(tipBar.GetComponent<ToolTips>().ShowMessage("Wave " + (waveIdx + 1) + " / " + (wavesAttrs.Length + 1), 2));
        }
    }
}