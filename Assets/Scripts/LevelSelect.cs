using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class LevelSelect : MonoBehaviour {
    public static LevelSelect main;
    [Header("Attributes")]
    [SerializeField] string[] levelScenes;
    [SerializeField] public int overrideMaxLevelClearedIfSet = -1;

    [Header("Buttons")]
    [SerializeField] Button button1;
    [SerializeField] Button button2;
    [SerializeField] Button button3;

    private void Start() {
        if (!PlayerPrefs.HasKey(KEY_PROGRESS))
            PlayerPrefs.SetInt(KEY_PROGRESS, 0);
        main = this;
    }

    private void FixedUpdate() {
        int maxLevelCleared = getProgress();
        if (maxLevelCleared == 0) {
            button1.interactable = true;
            button2.interactable = false;
            button3.interactable = false;
        } else if (maxLevelCleared == 1) {
            button1.interactable = true;
            button2.interactable = true;
            button3.interactable = false;
        } else if (maxLevelCleared >= 2) {
            button1.interactable = true;
            button2.interactable = true;
            button3.interactable = true;
        }
    }

    public void LoadLevel(int levelIndex) {
        if (levelIndex >= 0 && levelIndex < levelScenes.Length) {
            SceneManager.LoadScene(levelScenes[levelIndex]);
        } else {
            Debug.LogWarning("Level index out of range.");
        }
    }

    public void LoadMainMenu() {
        SceneManager.LoadScene("Start");
    }

    private const string KEY_PROGRESS = "MaxLevelCleared";
    public void ResetProgress()
    {
        PlayerPrefs.SetInt(KEY_PROGRESS, 0);
    }

    public void BumpProgressIfNeeded(int to)
    {
        int current = PlayerPrefs.GetInt(KEY_PROGRESS);
        if (to > current)
        {
            PlayerPrefs.SetInt(KEY_PROGRESS, to);
        }
    }

    private int getProgress()
    {
        if (overrideMaxLevelClearedIfSet != -1) return overrideMaxLevelClearedIfSet;
        return PlayerPrefs.GetInt(KEY_PROGRESS);
    }
}
