using UnityEngine;
using TMPro;
using XCharts.Runtime;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;

public class DailyCalorieTracker : MonoBehaviour
{
    public static DailyCalorieTracker Instance;
    [System.Serializable]
    public class FoodItem
    {
        public string name;
        public int calories;
        public Sprite icon;
        public Button button;
    }

    [Header("Food Settings")]
    public List<FoodItem> foodItems = new List<FoodItem>();

    [Header("Chart Settings")]
    public LineChart calorieChart;
    public int maxHistoryDays = 30;
    public int dailyCalorieGoal = 2000;

    [Header("UI References")]
    public Text currentDateText;
    public Text totalCaloriesText;
    public Text remainingCaloriesText;
    public Text goalText;

    private const string DATA_KEY = "CalorieData";
    private const string HISTORY_KEY = "CalorieHistory";
    private DailyData currentDayData;
    private CalorieHistory history;

    [System.Serializable]
    public class DailyData 
    {
        public string date; 
        public int totalCalories;
        public List<ConsumedFood> consumedFoods = new List<ConsumedFood>();
    }

    [System.Serializable]
    public class ConsumedFood 
    {
        public string name;
        public int calories;
        public string time;
    }

    [System.Serializable]
    public class CalorieHistory 
    {
        public List<DailyData> days = new List<DailyData>();
    }

    private void Awake()
    {
        InitializeFoodButtons();
        InitializeData();
    }

    private void Start()
    {
        UpdateDisplay();
    }

    private void InitializeFoodButtons()
    {
        foreach (var item in foodItems)
        {
            if (item.button != null)
            {
               
                var iconImage = item.button.GetComponent<Image>();
                if (iconImage != null && item.icon != null)
                {
                    iconImage.sprite = item.icon;
                }

               
                var text = item.button.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = item.name; 
                }

               
                item.button.onClick.AddListener(() => AddFood(item));
            }
        }
    }

    private void AddFood(FoodItem food)
    {
        currentDayData.consumedFoods.Add(new ConsumedFood
        {
            name = food.name,
            calories = food.calories,
            time = DateTime.Now.ToString("HH:mm")
        });

        currentDayData.totalCalories += food.calories;

     
        if (AchievementSystem.Instance != null)
        {
            AchievementSystem.Instance.UpdateProgress(
                AchievementSystem.AchievementType.Calories,
                food.calories
            );
        }

        SaveData();
        UpdateDisplay();
        StartCoroutine(AnimateButton(food.button));
    }

    private IEnumerator AnimateButton(Button button)
    {
        if (button == null) yield break;

        RectTransform rect = button.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;

       
        rect.localScale = originalScale * 0.9f;
        yield return new WaitForSeconds(0.1f);

        
        rect.localScale = originalScale;
    }

    private void InitializeData()
    {
        currentDayData = new DailyData();
        history = new CalorieHistory();
        LoadData();
        InitializeChart();
    }

    private void LoadData()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");

      
        string jsonData = PlayerPrefs.GetString(DATA_KEY, "");
        if (!string.IsNullOrEmpty(jsonData))
        {
            currentDayData = JsonUtility.FromJson<DailyData>(jsonData);
        }

       
        if (currentDayData == null || currentDayData.date != today)
        {
            if (currentDayData != null && currentDayData.date != today)
            {
                SaveCurrentDayToHistory();
            }
            currentDayData = new DailyData { date = today };
        }

       
        string historyJson = PlayerPrefs.GetString(HISTORY_KEY, "");
        if (!string.IsNullOrEmpty(historyJson))
        {
            history = JsonUtility.FromJson<CalorieHistory>(historyJson);
        }

        if (history == null)
        {
            history = new CalorieHistory();
        }
    }

    private void SaveCurrentDayToHistory()
    {
        if (currentDayData == null || history == null) return;

        var existingDay = history.days.Find(d => d.date == currentDayData.date);
        if (existingDay != null)
        {
            existingDay.totalCalories = currentDayData.totalCalories;
            existingDay.consumedFoods = new List<ConsumedFood>(currentDayData.consumedFoods);
        }
        else
        {
            history.days.Add(new DailyData
            {
                date = currentDayData.date,
                totalCalories = currentDayData.totalCalories,
                consumedFoods = new List<ConsumedFood>(currentDayData.consumedFoods)
            });
        }

        
        while (history.days.Count > maxHistoryDays)
        {
            history.days.RemoveAt(0);
        }

        SaveData();
    }

    private void UpdateDisplay()
    {
        if (currentDayData == null) return;

       
        currentDateText.text = DateTime.Now.ToString("dd.MM.yyyy");

      
        totalCaloriesText.text = $"Съедено: {currentDayData.totalCalories} ккал";

       
        int remaining = Mathf.Max(0, dailyCalorieGoal - currentDayData.totalCalories);
        remainingCaloriesText.text = $"Осталось: {remaining} ккал";

      
        goalText.text = $"Цель: {dailyCalorieGoal} ккал";

       
        bool isOverLimit = currentDayData.totalCalories > dailyCalorieGoal;
        totalCaloriesText.color = isOverLimit ? Color.red : Color.green;
        remainingCaloriesText.color = isOverLimit ? Color.red : Color.white;

        UpdateChart();
    }

    private void UpdateChart()
    {
        if (calorieChart == null || history == null || currentDayData == null) return;

        calorieChart.ClearData();

        
        if (history.days.Count > 0)
        {
            history.days.Sort((a, b) => DateTime.Parse(a.date).CompareTo(DateTime.Parse(b.date)));

            foreach (var day in history.days)
            {
                calorieChart.AddXAxisData(day.date);
                calorieChart.AddData(0, day.totalCalories);
                calorieChart.AddData(1, dailyCalorieGoal);
            }
        }

       
        if (!history.days.Exists(d => d.date == currentDayData.date))
        {
            calorieChart.AddXAxisData(currentDayData.date);
            calorieChart.AddData(0, currentDayData.totalCalories);
            calorieChart.AddData(1, dailyCalorieGoal);
        }

        calorieChart.RefreshChart();
    }

    private void InitializeChart()
    {
        if (calorieChart == null) return;

        calorieChart.RemoveData();

       
        var mainSerie = calorieChart.AddSerie<Line>("Калории");
        mainSerie.symbol.show = true;
        mainSerie.lineStyle.width = 3;
        mainSerie.lineStyle.color = new Color32(75, 175, 255, 255);

        
        var goalSerie = calorieChart.AddSerie<Line>("Цель");
        goalSerie.lineStyle.type = LineStyle.Type.Dashed;
        goalSerie.lineStyle.color = Color.red;
        goalSerie.lineStyle.width = 2;
    }

    private void SaveData()
    {
        if (currentDayData == null || history == null) return;

        PlayerPrefs.SetString(DATA_KEY, JsonUtility.ToJson(currentDayData));
        PlayerPrefs.SetString(HISTORY_KEY, JsonUtility.ToJson(history));
        PlayerPrefs.Save();
    }

    public void ResetDay()
    {
        SaveCurrentDayToHistory();
        currentDayData = new DailyData { date = DateTime.Now.ToString("yyyy-MM-dd") };
        SaveData();
        UpdateDisplay();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
   
}