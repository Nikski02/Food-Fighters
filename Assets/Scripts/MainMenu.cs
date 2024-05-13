using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenu : MonoBehaviour {
    
    public void PlayGame() {
        SceneManager.LoadScene("LevelSelect");
    }

    public void ExitGame() {
        Application.Quit();
    }
}
