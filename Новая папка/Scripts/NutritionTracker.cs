using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class NutritionTracker : MonoBehaviour
{
    public static NutritionTracker Instance;

    [Header("Water Tracking")]
    public TMP_Text waterTotalText;
    public GameObject waterInputPanel;
    public TMP_InputField waterInputField;
    public Button waterAddButton;

    [Header("Food Tracking")]
    public TMP_Text caloriesTotalText;
    public GameObject foodInputPanel;
    public TMP_InputField foodInputField;
    public Button foodAddButton;

    [Header("Updates List")]
    public Transform updatesContent;
    public GameObject updateItemTemplate;

 
    public float totalWater = 0;
    public int totalCalories = 0;
    private List<string> updates = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            
        }
    }

    void Start()
    {
        
        waterAddButton.onClick.AddListener(AddWater);
        foodAddButton.onClick.AddListener(AddFood);

        LoadData();
        UpdateUI();
    }

    public void AddWater()
    {
        if (float.TryParse(waterInputField.text, out float amount))
        {
            totalWater += amount;
            updates.Add($"+{amount}ml воды");

            waterInputField.text = "";
            waterInputPanel.SetActive(false);

            SaveData();
            UpdateUI();
        }
    }

    public void AddFood()
    {
        if (int.TryParse(foodInputField.text, out int calories))
        {
            totalCalories += calories;
            updates.Add($"+{calories} ккал");

            foodInputField.text = "";
            foodInputPanel.SetActive(false);

            SaveData();
            UpdateUI();
        }
    }

    void UpdateUI()
    {
       
        waterTotalText.text = $"{totalWater / 1000}L";
        caloriesTotalText.text = $"{totalCalories}kCal";

     
        foreach (Transform child in updatesContent)
        {
            Destroy(child.gameObject);
        }

       
        foreach (var update in updates)
        {
            CreateUpdateItem(update);
        }
    }

    void CreateUpdateItem(string text)
    {
       
        GameObject newItem = new GameObject("UpdateItem");
        newItem.transform.SetParent(updatesContent);

        
        var textComponent = newItem.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 24;
        textComponent.color = Color.white;

        
        var layout = newItem.AddComponent<LayoutElement>();
        layout.minHeight = 30;
    }

    void SaveData()
    {
        PlayerPrefs.SetFloat("TotalWater", totalWater);
        PlayerPrefs.SetInt("TotalCalories", totalCalories);
       
    }

    void LoadData()
    {
        totalWater = PlayerPrefs.GetFloat("TotalWater", 0);
        totalCalories = PlayerPrefs.GetInt("TotalCalories", 0);
        
    }
}