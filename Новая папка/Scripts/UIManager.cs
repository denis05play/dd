using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Основные панели")]
    public GameObject mainPanel;       
    public GameObject profilePanel;    
    public GameObject waterPanel;      
    public GameObject weightPanel;     
    public GameObject caloriesPanel;   
    public GameObject achievementsPanel; 

    private GameObject currentPanel;   

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
        SwitchToPanel(mainPanel);
    }

    
    public void SwitchToPanel(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        
        targetPanel.SetActive(true);
        currentPanel = targetPanel;
    }

    
    public void ReturnToMainMenu()
    {
        SwitchToPanel(mainPanel);
    }
}