using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedDisplay : MonoBehaviour
{
    private CharacterMovement characterMovement;
    public TextMeshProUGUI textMeshPro;
    private float speedText;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        characterMovement = GameObject.FindObjectOfType<CharacterMovement>();
    }

    void FixedUpdate()
    {
        speedText = characterMovement.player.velocity.magnitude;
        textMeshPro.text = speedText.ToString();
    }
}
