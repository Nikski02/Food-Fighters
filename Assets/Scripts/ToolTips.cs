using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTips : MonoBehaviour {
    public static ToolTips main;
    
    [Header("References")]
    [SerializeField] TextMeshProUGUI toolTipsUI;
    [SerializeField] Animator anim;

    [Header("Attributes")]
    [SerializeField] public string textVal;

    public bool isOpen = true;

    private void Awake() {
        main = this;
        anim.SetBool("TipsOpen", isOpen);
    }
    private void Update() {
        if (LevelManager.main.gameEnded) {
            CloseTips();
        }
    }
    private void OnGUI() {
        toolTipsUI.text = textVal;
    }
    public void CloseTips() {
        if (isOpen) {
            isOpen = !isOpen;
            anim.SetBool("TipsOpen", isOpen);
        }
    }
    public void OpenTips() {
        if (!isOpen) {
            isOpen = !isOpen;
            anim.SetBool("TipsOpen", isOpen);
        }
    }

    public IEnumerator ShowMessage(string msg, float duration)
    {
        this.textVal = msg;
        toolTipsUI.text = msg;
        this.OpenTips();
        yield return new WaitForSeconds(duration);
        this.CloseTips();
    }
}
