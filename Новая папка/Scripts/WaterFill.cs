using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WaterIntakeData
{
    public float dailyGoal = 4000f;
    public float currentIntake;
    public DateTime lastUpdateDate;
}

public class WaterFill : MonoBehaviour
{
    public static WaterFill Instance;

    [Header("UI Elements")]
    public TMP_Text totalAmountText;
    public Image backgroundBar;
    public RectTransform blocksContainer;
    public GameObject redBlockPrefab;
    public Button[] amountButtons;
    public TMP_InputField customAmountInput;

    [Header("Settings")]
    public float maxBarHeight = 400f;
    public float mlToPixelRatio = 0.1f;

    [Header("Effects")]
    public ParticleSystem waterParticles;
    public AudioClip waterAddSound;

    public WaterIntakeData waterData; 
    private const string WATER_DATA_KEY_PREFIX = "WaterData_";
    private List<GameObject> currentBlocks = new List<GameObject>();

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

    private void Start()
    {
        InitializeSystem();
        LoadData();
        UpdateUI();
        SetupButtons();
    }

    private string GetCurrentUserEmail()
    {
       
        return PlayerPrefs.GetString("LastLoginEmail", "");
    }

    private string GetSaveKey()
    {
        return WATER_DATA_KEY_PREFIX + GetCurrentUserEmail();
    }

    private void InitializeSystem()
    {
        backgroundBar.rectTransform.sizeDelta = new Vector2(
            backgroundBar.rectTransform.sizeDelta.x,
            maxBarHeight
        );
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString(GetSaveKey(), string.Empty);

        if (!string.IsNullOrEmpty(json))
        {
            waterData = JsonUtility.FromJson<WaterIntakeData>(json);

        
            if (waterData.lastUpdateDate.Date < DateTime.Today)
            {
                ResetDailyIntake();
                return;
            }
        }
        else
        {
            waterData = new WaterIntakeData
            {
                lastUpdateDate = DateTime.Today
            };
        }
    }

    private void SaveData()
    {
        waterData.lastUpdateDate = DateTime.Now;
        PlayerPrefs.SetString(GetSaveKey(), JsonUtility.ToJson(waterData));
        PlayerPrefs.Save();
    }

    private void UpdateUI()
    {
        totalAmountText.text = $"{waterData.currentIntake}ml / {waterData.dailyGoal}ml";
        ClearBlocks();
        CreateBlocksForCurrentIntake();

        if (waterData.currentIntake >= waterData.dailyGoal && waterParticles != null)
        {
            waterParticles.Play();
        }
    }

    private void ClearBlocks()
    {
        foreach (var block in currentBlocks)
        {
            Destroy(block);
        }
        currentBlocks.Clear();
    }

    private void CreateBlocksForCurrentIntake()
    {
        float remainingAmount = waterData.currentIntake;
        float currentHeight = 0f;
        int blockCount = 0;

        while (remainingAmount > 0 && blockCount < 20)
        {
            float blockAmount = Mathf.Min(200f, remainingAmount);
            float blockHeight = blockAmount * mlToPixelRatio;

            CreateBlock(currentHeight, blockHeight);

            currentHeight += blockHeight;
            remainingAmount -= blockAmount;
            blockCount++;
        }
    }

    private void CreateBlock(float yPosition, float height)
    {
        GameObject newBlock = Instantiate(redBlockPrefab, blocksContainer);
        RectTransform rt = newBlock.GetComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(0, yPosition);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);

        currentBlocks.Add(newBlock);
    }

    public void AddWater(float amount)
    {
        waterData.currentIntake = Mathf.Clamp(waterData.currentIntake + amount, 0f, waterData.dailyGoal);
        SaveData();
        UpdateUI();

       
        if (AchievementSystem.Instance != null)
        {
            AchievementSystem.Instance.UpdateProgress(AchievementSystem.AchievementType.Water, amount);
        }

        if (waterAddSound != null)
        {
            AudioSource.PlayClipAtPoint(waterAddSound, Camera.main.transform.position);
        }
    }

    public void AddCustomAmount()
    {
        if (float.TryParse(customAmountInput.text, out float amount))
        {
            AddWater(amount);
            customAmountInput.text = string.Empty;
        }
    }

    public void ResetDailyIntake()
    {
        waterData.currentIntake = 0f;
        waterData.lastUpdateDate = DateTime.Today;
        SaveData();
        UpdateUI();
    }

    private void SetupButtons()
    {
        for (int i = 0; i < amountButtons.Length; i++)
        {
            int index = i;
            amountButtons[index].onClick.AddListener(() => AddWater(GetAmountByButtonIndex(index)));
        }
    }

    private float GetAmountByButtonIndex(int index)
    {
        float[] amounts = { 200f, 500f, 1000f };
        return index < amounts.Length ? amounts[index] : 0f;
    }

    private void CheckForDayChange()
    {
        if (waterData != null && waterData.lastUpdateDate.Date < DateTime.Today)
        {
            ResetDailyIntake();
        }
    }

    private void Update()
    {
        CheckForDayChange();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
    
}
