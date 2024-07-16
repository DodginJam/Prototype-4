using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI WaveNumberTextDisplay
    { get; set; }
    [field: SerializeField] public TextMeshProUGUI PlayerLivesTextDisplay
    { get; set; }
    [field: SerializeField] public TextMeshProUGUI GameOverTextDisplay
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        InitialiseUIOnStart();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitialiseUIOnStart()
    {
        UpdateAbilityColor("DashTextDisplay", Color.green);
        UpdateAbilityColor("SmashTextDisplay", Color.black);
        UpdateAbilityColor("RepulseTextDisplay", Color.black);
        UpdateAbilityColor("LasersTextDisplay", Color.black);

        UpdateWaveNumberDisplay(GameObject.Find("SpawnManager").GetComponent<SpawnManager>().WaveNumber);
        UpdateLivesNumberDisplay(GameObject.Find("Player").GetComponent<PlayerController>().PlayerLives);
        GameOverTextDisplay.enabled = false;
    }

    public void UpdateWaveNumberDisplay(int currentWave, bool isBossWave = false)
    {
        WaveNumberTextDisplay.text = $"Wave:{currentWave}";
        if (isBossWave)
        {
            WaveNumberTextDisplay.text += " BOSSWAVE";
        }
    }

    public void UpdateLivesNumberDisplay(int currentLives)
    {
        PlayerLivesTextDisplay.text = $"Lives:{currentLives}";
    }

    public void UpdateAbilityColor(string textBoxName, Color newColor)
    {
        TextMeshProUGUI[] UITexts = GetComponentsInChildren<TextMeshProUGUI>();

        for (int i = 0; i < UITexts.Length; i++)
        {
            if (UITexts[i].name == textBoxName)
            {
                UITexts[i].color = newColor;
                break;
            }
        }
    }

    void UpdateTextBox(string textBoxName, string newText)
    {
        TextMeshProUGUI[] UITexts =  GetComponentsInChildren<TextMeshProUGUI>();

        for (int i = 0; i < UITexts.Length; i++)
        {
            if (UITexts[i].name == textBoxName)
            {
                UITexts[i].text = newText;
                break;
            }
        }
    }
}
