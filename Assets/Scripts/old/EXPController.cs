using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerExpController : MonoBehaviour
{
    [Header("Level")]
    public int level = 1;

    [Header("Experience")]
    public int currentExp = 0;
    public int expToNextLevel = 200;

    [Header("UI")]
    public Slider expSlider;
    public TextMeshProUGUI levelText;

    void Start()
    {
        UpdateExpUI();
    }

    public void AddExp(int amount)
    {
        currentExp += amount;

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }

        UpdateExpUI();
    }

    void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;
        expToNextLevel = Mathf.RoundToInt(100 * Mathf.Pow(level, 1.2f));

        Debug.Log("Level Up! Current Level: " + level);
    }

    void UpdateExpUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentExp;
        }

        if (levelText != null)
        {
            levelText.text = "Lv " + level;
        }
    }
    //Test
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddExp(20);
        }
    }
}