using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BMICalculator : MonoBehaviour
{
    [Header("Ссылки на UI")]
    public Text bmiValueText;
    public Text bmiStatusText;
    public Button openBmiInfoButton;

    [Header("Настройки")]
    public string bmiInfoUrl = "https://krichmar.ru/company/kalkulyator-imt/";
    public Color underweightColor = Color.blue;
    public Color normalColor = Color.green;
    public Color overweightColor = Color.yellow;
    public Color obeseColor = Color.red;

    private const string WEIGHT_KEY = "user_weight";
    private const string HEIGHT_KEY = "user_height";

    private void Start()
    {
        if (bmiValueText == null) Debug.LogError("bmiValueText не назначен!");
        if (bmiStatusText == null) Debug.LogError("bmiStatusText не назначен!");

        UpdateBMIDisplay();

        if (openBmiInfoButton != null)
        {
            openBmiInfoButton.onClick.AddListener(OpenBmiInfoLink);
        }
    }

    public void SaveNewUserData(float weight, float height)
    {
       
        PlayerPrefs.SetFloat(WEIGHT_KEY, weight);
        PlayerPrefs.SetFloat(HEIGHT_KEY, height);

       
        PlayerPrefs.SetFloat("weight_data_weight", weight);
        PlayerPrefs.SetFloat("weight_data_height", height);

        PlayerPrefs.Save();
        UpdateBMIDisplay();
    }

    public void UpdateBMIDisplay()
    {
        float weight = GetWeight();
        float height = GetHeight();
        float bmi = CalculateBMI(weight, height);
        DisplayBMI(bmi);
    }

    private float GetWeight()
    {
       
        if (PlayerPrefs.HasKey(WEIGHT_KEY))
            return PlayerPrefs.GetFloat(WEIGHT_KEY);

       
        if (PlayerPrefs.HasKey("weight_data_weight"))
            return PlayerPrefs.GetFloat("weight_data_weight");

       
        if (PlayerPrefs.HasKey("user_body_data"))
        {
            var bodyData = JsonUtility.FromJson<LocalAuthManager.BodyData>(
                PlayerPrefs.GetString("user_body_data"));
            return bodyData.weight;
        }

      
        return 70f;
    }

    private float GetHeight()
    {
       
        if (PlayerPrefs.HasKey(HEIGHT_KEY))
            return PlayerPrefs.GetFloat(HEIGHT_KEY);

        if (PlayerPrefs.HasKey("weight_data_height"))
            return PlayerPrefs.GetFloat("weight_data_height");

        if (PlayerPrefs.HasKey("user_body_data"))
        {
            var bodyData = JsonUtility.FromJson<LocalAuthManager.BodyData>(
                PlayerPrefs.GetString("user_body_data"));
            return bodyData.height;
        }

        return 175f;
    }

    private float CalculateBMI(float weight, float height)
    {
        if (height <= 0) return 0;
        return weight / Mathf.Pow(height / 100f, 2);
    }

    private void DisplayBMI(float bmi)
    {
        if (bmiValueText == null || bmiStatusText == null) return;

        bmiValueText.text = bmi.ToString("F1");

        string status;
        Color color;

        if (bmi < 16.5f)
        {
            status = "Выраженный дефицит массы";
            color = underweightColor;
        }
        else if (bmi < 18.5f)
        {
            status = "Недостаточная масса";
            color = underweightColor;
        }
        else if (bmi < 25f)
        {
            status = "Нормальный вес";
            color = normalColor;
        }
        else if (bmi < 30f)
        {
            status = "Избыточная масса";
            color = overweightColor;
        }
        else if (bmi < 35f)
        {
            status = "Ожирение 1 степени";
            color = obeseColor;
        }
        else if (bmi < 40f)
        {
            status = "Ожирение 2 степени";
            color = obeseColor;
        }
        else
        {
            status = "Ожирение 3 степени";
            color = obeseColor;
        }

        bmiStatusText.text = status;
        bmiStatusText.color = color;
    }

    private void OpenBmiInfoLink()
    {
        if (!string.IsNullOrEmpty(bmiInfoUrl))
        {
            Application.OpenURL(bmiInfoUrl);
        }
    }

    public void UpdateData(float newWeight, float newHeight)
    {
        SaveNewUserData(newWeight, newHeight);
    }
}