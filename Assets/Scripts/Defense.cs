using System;
using UnityEngine;

[Serializable]
public class Defense {
    
    public string name;
    public int cost;
    public GameObject prefab;
    public Weakness[] strengths;

    public Defense (string _name, int _cost, GameObject _prefab, Weakness[] _strengths) {
        name = _name;
        cost = _cost;
        prefab = _prefab;
        strengths = _strengths;
    }
}
