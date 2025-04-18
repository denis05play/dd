using UnityEngine;
using UnityEngine.UI;

public class ViewPancake : MonoBehaviour
{
    [Header("Панели")]
    public GameObject mainPanel;    
    public GameObject chartPanel;   

    [Header("Кнопки")]
    public Button showChartButton;  
    public Button backButton;       

    private void Start()
    {
     
        if (mainPanel == null || chartPanel == null ||
            showChartButton == null || backButton == null)
        {
            Debug.LogError("Не все ссылки назначены в инспекторе!");
            return;
        }

        mainPanel.SetActive(true);
        chartPanel.SetActive(false);

        
        if (showChartButton != null)
            showChartButton.onClick.AddListener(ShowChartPanel);

        if (backButton != null)
            backButton.onClick.AddListener(ShowMainPanel);
    }

   
    private void ShowChartPanel()
    {
        mainPanel.SetActive(false);
        chartPanel.SetActive(true);
    }

   
    private void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        chartPanel.SetActive(false);
    }

}