using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Plot : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    private GameObject tower;
    private Color startColor;
    public bool Used = false;

    private void Start() {
        startColor = sr.color;
    }

    private void OnMouseEnter() {
        if (Used) return;
        sr.color = hoverColor;
    }

    private void OnMouseExit() {
        if (Used) return;
        sr.color = startColor;
    }

    private void OnMouseDown() {
        if (tower != null || LevelManager.main.gameEnded || Menu.main.isMenuOpen || Used) {
            Menu.main.ToggleMenu();
            return;
        }

        Defense towerToBuild = BuildManager.main.GetSelectedTower();


        if (TowerSize(towerToBuild.prefab) && LevelManager.main.SpendCurrency(towerToBuild.cost)) {
            tower = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
            Tower instance = tower.GetComponent<Tower>();
            markUsed(towerToBuild);
            LevelManager.main.SfxTowerCompleted();
            // instance.SetStrengths(towerToBuild.strengths);
        }
    }

    private void markUsed(Defense towerBuilt)
    {
        if (towerBuilt.name == "Torch Tower" || towerBuilt.name == "Cutter Tower")
        {
            Used = true;
            sr.color = Color.clear;
            return;
        }
        if (towerBuilt.name != "Cannon Tower")
        {
            Debug.LogError("Found invalid name of tower: " + towerBuilt.name);
            return;
        }

        GameObject[] GameObjects = GameObject.FindGameObjectsWithTag("plot");
        foreach (GameObject thisGameObject in GameObjects)
        {
            if (Vector3.Distance(thisGameObject.transform.position, transform.position) <= 1.5f)
            {
                Plot plt = thisGameObject.GetComponent<Plot>();
                plt.Used = true;
                plt.sr.color = Color.clear;
            }
        }

    }

    // TODO: Tower Size Implementation (see above in OnMouseDown() for function call)
    // Maybe add size attribute to tower prefabs for radius testing below?
    //
    private bool TowerSize(GameObject towerToBuild) {
        if (towerToBuild.name == "Torch Tower" || towerToBuild.name == "Cutter Tower") {
            return true;
        } else if (towerToBuild.name == "Cannon Tower") {
            GameObject[] GameObjects = GameObject.FindGameObjectsWithTag("plot");
            int freePlots = 0;
            foreach (GameObject thisGameObject in GameObjects) {
                if (Vector3.Distance(thisGameObject.transform.position, transform.position) <= 1.5f) {
                    Plot plt = thisGameObject.GetComponent<Plot>();
                    if (plt.Used) continue;
                    freePlots += 1;
                }
            }

            if (freePlots < 9)
                return false;
            return true;
        }

        return false;
    }
}