using UnityEngine;
using TMPro;
using UnityEngine.UI;
using XCharts;
using XCharts.Runtime;
using System;
using System.Collections.Generic;


public class WeightTracker : MonoBehaviour
{
        public static WeightTracker Instance;

        [Header("Основные элементы")]
        public LineChart weightChart;
        public TMP_InputField weightInput;
        public TMP_InputField heightInput;
        public Button saveButton;

        [Header("Текущие данные")]
        public TMP_Text currentWeightText;
        public TMP_Text currentBMIText;
        public TMP_Text lastUpdateText;

    private const string WEIGHT_DATA_KEY = "weight_data";
    private const string HISTORY_KEY = "weight_history";
    private const int MAX_HISTORY_ITEMS = 3;

    private WeightHistory GetHistory()
    {
        if (!PlayerPrefs.HasKey(HISTORY_KEY))
            return new WeightHistory { entries = new List<WeightEntry>() };

        return JsonUtility.FromJson<WeightHistory>(PlayerPrefs.GetString(HISTORY_KEY));
    }


    private void Start()
    {
        Instance = this; 
        InitializeChart();
        LoadInitialData();
        saveButton.onClick.AddListener(SaveData);
    }

    private void InitializeChart()
    {
        weightChart.RemoveData();
        var serie = weightChart.AddSerie<Line>("Вес");
        serie.symbol.show = true;
        serie.symbol.size = 15;
        serie.lineStyle.width = 3;
        serie.animation.enable = true;
    }
    public void SaveNewUserData(float weight, float height)
    {
        var history = GetHistory(); 
        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

      
        var existingEntry = history.entries.Find(e => e.date == currentDate);

        if (existingEntry != null)
        {
           
            existingEntry.weight = weight;
            existingEntry.bmi = CalculateBMI(weight, height);
        }
        else
        {
            
            history.entries.Add(new WeightEntry
            {
                date = currentDate,
                weight = weight,
                bmi = CalculateBMI(weight, height)
            });
        }

       
        while (history.entries.Count > 5)
        {
            history.entries.RemoveAt(0);
        }

       
        SaveWeightData(new WeightData
        {
            weight = weight,
            height = height,
            lastUpdate = currentDate
        });

        SaveHistory(history);
        RefreshAllData();
    }
    public void UpdateWeightData(float newWeight, float newHeight)
    {
        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        var history = GetHistory() ?? new WeightHistory(); // Инициализация если null

        // Обновляем или добавляем запись
        var todayEntry = history.entries.Find(e => e.date == currentDate);
        if (todayEntry == null)
        {
            history.entries.Add(new WeightEntry
            {
                date = currentDate,
                weight = newWeight,
                bmi = CalculateBMI(newWeight, newHeight)
            });
        }
        else
        {
            todayEntry.weight = newWeight;
            todayEntry.bmi = CalculateBMI(newWeight, newHeight);
        }

        // Оставляем только последние 3 дня
        while (history.entries.Count > MAX_HISTORY_ITEMS)
        {
            history.entries.RemoveAt(0);
        }

        SaveWeightData(new WeightData
        {
            weight = newWeight,
            height = newHeight,
            lastUpdate = currentDate
        });

        SaveHistory(history);
        RefreshAllData();
    }


    private void LoadInitialData()
    {
       
        var weightData = GetWeightData();
        var history = GetHistory();

        
        if (weightData == null || history == null)
        {
            
            if (PlayerPrefs.HasKey("user_body_data"))
            {
                var bodyData = JsonUtility.FromJson<LocalAuthManager.BodyData>(
                    PlayerPrefs.GetString("user_body_data"));

                SaveNewUserData(bodyData.weight, bodyData.height);
            }
            else
            {
                
                CreateDefaultData();
            }
        }

        
        RefreshAllData();
    }

    private void CreateDefaultData()
    {
        var defaultData = new WeightData
        {
            weight = 70f,
            height = 175f,
            lastUpdate = DateTime.Now.ToString("yyyy-MM-dd")
        };
        SaveWeightData(defaultData);

       
        var history = new WeightHistory
        {
            entries = new List<WeightEntry>
            {
                new WeightEntry
                {
                    date = defaultData.lastUpdate,
                    weight = defaultData.weight,
                    bmi = CalculateBMI(defaultData.weight, defaultData.height)
                }
            }
        };
        SaveHistory(history);
    }

    private void RefreshAllData()
    {
        var weightData = GetWeightData();
        var history = GetHistory();

        UpdateBasicInfo(weightData);
        UpdateChart(history);
    }

    private void UpdateBasicInfo(WeightData data)
    {
        currentWeightText.text = $"Текущий вес: {data.weight} кг";
        currentBMIText.text = $"ИМТ: {CalculateBMI(data.weight, data.height):F1}";
        lastUpdateText.text = $"Обновлено: {data.lastUpdate}";
    }

    private void UpdateChart(WeightHistory history)
    {
        weightChart.ClearData();
        if (history?.entries == null || history.entries.Count == 0) return;

        // Сортируем по дате
        history.entries.Sort((a, b) => DateTime.Parse(a.date).CompareTo(DateTime.Parse(b.date)));

        // Добавляем данные
        foreach (var entry in history.entries)
        {
            weightChart.AddXAxisData(entry.date.Split('-')[2]); // Показываем только день
            weightChart.AddData(0, entry.weight);
        }

        // Автоматический масштаб графика
        var yAxis = weightChart.GetChartComponent<YAxis>();
        yAxis.minMaxType = Axis.AxisMinMaxType.Default;
        weightChart.RefreshChart();
    }
    

    public void SaveData()
    {
        if (float.TryParse(weightInput.text, out float newWeight) &&
            float.TryParse(heightInput.text, out float newHeight))
        {
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var history = GetHistory();

            
            var todayEntry = history.entries.Find(e => e.date == currentDate);

            if (todayEntry != null)
            {
                
                todayEntry.weight = newWeight;
                todayEntry.bmi = CalculateBMI(newWeight, newHeight);
            }
            else
            {
                
                history.entries.Add(new WeightEntry
                {
                    date = currentDate,
                    weight = newWeight,
                    bmi = CalculateBMI(newWeight, newHeight)
                });

               
                if (history.entries.Count > MAX_HISTORY_ITEMS)
                {
                    history.entries.RemoveAt(0); 
                }
            }

          
            var newData = new WeightData
            {
                weight = newWeight,
                height = newHeight,
                lastUpdate = currentDate
            };

            SaveWeightData(newData);
            SaveHistory(history);
            RefreshAllData();

           
            weightInput.text = "";
            heightInput.text = "";
        }
        
    }

    private float CalculateBMI(float weight, float height)
    {
        return weight / Mathf.Pow(height / 100f, 2);
    }

   
    private WeightData GetWeightData()
    {
        return JsonUtility.FromJson<WeightData>(PlayerPrefs.GetString(WEIGHT_DATA_KEY));
    }
   

    private void SaveWeightData(WeightData data)
    {
        PlayerPrefs.SetString(WEIGHT_DATA_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

   

    private void SaveHistory(WeightHistory history)
    {
        PlayerPrefs.SetString(HISTORY_KEY, JsonUtility.ToJson(history));
        PlayerPrefs.Save();
    }

    [System.Serializable]
    public class WeightData 
    {
        public float weight;
        public float height;
        public string lastUpdate;
    }

    [System.Serializable]
    public class WeightEntry
    {
        public string date;
        public float weight;
        public float bmi;
    }

    [System.Serializable]
    public class WeightHistory 
    {
        public List<WeightEntry> entries = new List<WeightEntry>();
    }

}