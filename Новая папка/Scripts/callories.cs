using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class callories : MonoBehaviour
{
    [Header("Ссылки на UI")]
    public TextMeshProUGUI caloriesConsumedText; 
   

    [Header("Настройки")]
    public int dailyCalorieGoal = 2000;          

    void Start()
    {
        DisplayDailyCalories();
    }

    public void DisplayDailyCalories()
    {
        string jsonData = PlayerPrefs.GetString("CalorieData", "");
        int caloriesConsumed = 0;
        string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");

        if (!string.IsNullOrEmpty(jsonData))
        {
            
            var calorieData = JsonUtility.FromJson<DailyCalorieTracker.DailyData>(jsonData);

            
            if (calorieData.date == currentDate)
            {
                caloriesConsumed = calorieData.totalCalories;
            }
        }

       
        caloriesConsumedText.text = $"{caloriesConsumed} ккал";

        

       

       
        bool isOverLimit = caloriesConsumed > dailyCalorieGoal;
        caloriesConsumedText.color = isOverLimit ? Color.red : Color.black;
       
    }

    
    public void ResetDay()
    {
        string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");
        var newData = new DailyCalorieTracker.DailyData
        {
            date = currentDate,
            totalCalories = 0,
            consumedFoods = new System.Collections.Generic.List<DailyCalorieTracker.ConsumedFood>()
        };

        PlayerPrefs.SetString("CalorieData", JsonUtility.ToJson(newData));
        DisplayDailyCalories();
    }
}