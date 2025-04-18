using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class Switgfht : MonoBehaviour
{
    
    [System.Serializable]
    public class PanelData
    {
        public Button switchButton;  
        public GameObject panel;     
        public bool isDefault = false; 
    }

    [Header("Настройки панелей")]
    public List<PanelData> panels = new List<PanelData>();

    [Header("Настройки анимации")]
    public float fadeDuration = 0.3f;
    public bool useFadeAnimation = true;

    private GameObject currentActivePanel;

    private void Start()
    {
        
        ValidatePanels();

       
        SetupButtons();

      
        ActivateDefaultPanel();
    }

    private void ValidatePanels()
    {
        foreach (var panelData in panels)
        {
            if (panelData.switchButton == null)
            {
                Debug.LogError("Не назначена кнопка для одной из панелей!");
                continue;
            }

            if (panelData.panel == null)
            {
                Debug.LogError("Не назначена панель для кнопки: " + panelData.switchButton.name);
            }
        }
    }

    private void SetupButtons()
    {
        foreach (var panelData in panels)
        {
            
            GameObject panelToActivate = panelData.panel;

            panelData.switchButton.onClick.AddListener(() =>
            {
                SwitchToPanel(panelToActivate);
            });
        }
    }

    private void ActivateDefaultPanel()
    {
        foreach (var panelData in panels)
        {
            if (panelData.isDefault)
            {
                SwitchToPanel(panelData.panel);
                return;
            }
        }

        
        if (panels.Count > 0)
        {
            SwitchToPanel(panels[0].panel);
        }
    }

    public void SwitchToPanel(GameObject panel)
    {
       
        if (currentActivePanel == panel) return;

       
        foreach (var panelData in panels)
        {
            if (useFadeAnimation)
            {
                StartCoroutine(FadePanel(panelData.panel, panelData.panel == panel));
            }
            else
            {
                panelData.panel.SetActive(panelData.panel == panel);
            }
        }

        currentActivePanel = panel;
    }

    private IEnumerator FadePanel(GameObject panel, bool fadeIn)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        if (fadeIn)
        {
            panel.SetActive(true);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (fadeIn)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            panel.SetActive(false);
        }
    }

    
    public void SwitchToPanelByIndex(int index)
    {
        if (index >= 0 && index < panels.Count)
        {
            SwitchToPanel(panels[index].panel);
        }
        else
        {
            Debug.LogError("Неверный индекс панели: " + index);
        }
    }
}