using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour {
    public static LevelManager main;

    [Header("References")]
    [SerializeField] public GameObject introUI;
    [SerializeField] public GameObject gameOverUI;
    [SerializeField] public TextMeshProUGUI scoreUI;

    [Header("Audio")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioClip bgmAudioClip;
    [SerializeField] private AudioClip infinitePlayAudioClip;
    [SerializeField] private AudioSource[] enemyAudioSources; // we need multiple sources to prevent sfx from eating into each other and clipping. 
    [SerializeField] private AudioClip[] enemyDeathClips;
    [SerializeField] private AudioSource[] weaponAudioSources; // we need multiple for same reason as above.
    [SerializeField] private AudioClip towerCompletedClip;
    [SerializeField] private AudioClip cutterTowerClip;

    public Transform startPoint;
    public Transform[] path;

    [Header("Attributes")]
    public bool gameEnded = false;
    [SerializeField] public int currency = 0;
    [SerializeField] public int playerHealth = 10;
    [SerializeField] public int score = 0;

    private void Awake() {
        main = this;
    }

    private void Start() {
        if (introUI) {
            introUI.SetActive(true);
        }
        gameEnded = false;
        this.bgmAudioSource.clip = bgmAudioClip;
        this.bgmAudioSource.loop = true;
        this.bgmAudioSource.Play();

        this.cutterSfxChannel = new Dictionary<Object, int>();
    }

    private void Update() {
        if (gameEnded) return;

        if (LevelManager.main.playerHealth <= 0 || Input.GetKeyDown(KeyCode.Escape)) {
            EndGame();
        }

        if (Input.GetMouseButtonDown(0) && introUI) {
            introUI.SetActive(false);
        }
    }

    public void IncreaseCurrency(int amount) {
        if (!gameEnded)
            currency += amount;
    }

    public bool SpendCurrency(int amount) {
        if (amount <= currency) {
            currency -= amount;
            return true;
        } else {
            Debug.Log("You do not have enough to purchase this item");
            return false;
        }
    }

    public void UpdateScore(int amount) {
        if (!gameEnded)
            score += amount;
    }

    public void DamagePlayer(int amount) {
        playerHealth -= amount;
    }

    private void EndGame() {
        gameEnded = true;
        gameOverUI.SetActive(true);
    }

    private void OnGUI() {
        scoreUI.text = "Final Score: " + LevelManager.main.score.ToString();
    }

    public void RestartGame() {
        gameOverUI.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame() {
        SceneManager.LoadScene("Start");
    }

    int lastDeathClipPlayed = 0;
    int lastSfxAudioSourceUsed = 0;
    public void EnemyDestroyed()
    {
        lastSfxAudioSourceUsed = (lastSfxAudioSourceUsed + 1) % enemyAudioSources.Length;
        lastDeathClipPlayed = (lastDeathClipPlayed + 1) % enemyDeathClips.Length;
        enemyAudioSources[lastSfxAudioSourceUsed].PlayOneShot(enemyDeathClips[lastDeathClipPlayed]);
    }

    int lastWeaponSfxAsUsed = 0;
    public void WeaponSfx(AudioClip clip)
    {
        if (clip == null) return;
        lastWeaponSfxAsUsed = (lastWeaponSfxAsUsed + 1) % weaponAudioSources.Length;
        weaponAudioSources[lastWeaponSfxAsUsed].PlayOneShot(clip);
    }

    public void SfxTowerCompleted()
    {
        lastWeaponSfxAsUsed = (lastWeaponSfxAsUsed + 1) % weaponAudioSources.Length;
        weaponAudioSources[lastWeaponSfxAsUsed].PlayOneShot(towerCompletedClip);
    }

    private Dictionary<Object, int> cutterSfxChannel;
    public void CutterSfxStart(Tower tower)
    {
        lastWeaponSfxAsUsed = (lastWeaponSfxAsUsed + 1) % weaponAudioSources.Length;
        weaponAudioSources[lastWeaponSfxAsUsed].clip = cutterTowerClip;
        weaponAudioSources[lastWeaponSfxAsUsed].Play();
        cutterSfxChannel.Add(tower, lastWeaponSfxAsUsed);
    }

    public void CutterSfxStop(Tower tower)
    {
        int channel = 0;
        bool found = cutterSfxChannel.TryGetValue(tower, out channel);
        if (!found) return;
        weaponAudioSources[channel].Stop();
        cutterSfxChannel.Remove(tower);
    }

    bool sandboxMode = false;
    public void StartSandboxMode() {
        if (sandboxMode) return;
        bgmAudioSource.clip = infinitePlayAudioClip;
        bgmAudioSource.Play();
        sandboxMode = true;
    }

    public void StopSandboxMode()
    {
        if (!sandboxMode) return;
        bgmAudioSource.clip = bgmAudioClip;
        bgmAudioSource.Play();
        sandboxMode = false;
    }
}