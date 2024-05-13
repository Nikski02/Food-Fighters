using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInfo : MonoBehaviour {
    [Header("References")]
    [SerializeField] TextMeshProUGUI healthUI;
    [SerializeField] TextMeshProUGUI moneyUI;

    private void OnGUI() {
        moneyUI.text = LevelManager.main.currency.ToString();
        healthUI.text = LevelManager.main.playerHealth.ToString();
    }
}
