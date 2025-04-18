using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryItem : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text bmiText;
    [SerializeField] private Image background;

    public void SetData(string date, string weight, string bmi, bool isLatest)
    {
        dateText.text = date;
        weightText.text = weight;
        bmiText.text = bmi;

       
        background.color = isLatest ? new Color(0.8f, 1f, 0.8f) : new Color(1f, 1f, 1f, 0.5f);
    }
}