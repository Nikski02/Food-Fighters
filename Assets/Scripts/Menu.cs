using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour {
    public static Menu main;
    
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] Animator anim;
    [SerializeField] GameObject menuButton;

    public bool isMenuOpen = true;

    private void Awake() {
        main = this;
    }
    private void Update() {
        if (LevelManager.main.gameEnded) {
            CloseMenu();
            menuButton.SetActive(false);
        } else if (Input.GetKeyDown(KeyCode.Tab)) {
            ToggleMenu();
        } else {
            menuButton.SetActive(true);
        }
    }
    public void ToggleMenu() {
        isMenuOpen = !isMenuOpen;
        anim.SetBool("MenuOpen", isMenuOpen);
    }
    private void OnGUI() {
        currencyUI.text = LevelManager.main.currency.ToString();
    }
    private void CloseMenu() {
        if (isMenuOpen) {
            ToggleMenu();
        }
    }
}
