using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour {
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private Defense[] defenses;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    private int selectedTower = 0;
    private GameObject oldButton;

    private void Awake() {
        main = this;

        if (gridLayoutGroup.transform.childCount > 0) {
            GameObject firstObject = gridLayoutGroup.transform.GetChild(0).gameObject;

            ColorSwap(firstObject);
        }
    }

    public Defense GetSelectedTower() {
        return defenses[selectedTower];
    }

    public void SetSelectedTower(int _selectedTower) {
        selectedTower = _selectedTower;
    }

    public void ColorSwap(GameObject newButton) {
        var colors = newButton.GetComponent<Button> ().colors;
        colors.normalColor = Color.yellow;

        newButton.GetComponent<Button> ().colors = colors;

        if (oldButton != null) {
            colors.normalColor = Color.white;
            oldButton.GetComponent<Button>().colors = colors;
        }

        oldButton = newButton;
    }
}