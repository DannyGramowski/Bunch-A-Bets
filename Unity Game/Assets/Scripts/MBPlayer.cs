using System;
using TMPro;
using UnityEngine;

public class MBPlayer : MonoBehaviour {
    [SerializeField] CardLayout cardLayout;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] string playerName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        cardLayout = GetComponentInChildren<CardLayout>();   
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = playerName;
    }

    // Update is called once per frame
    void Update() {
        
    }
}
