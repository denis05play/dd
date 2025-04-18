using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PanelController : MonoBehaviour
{
    [Header("Панели (5 элементов)")]
    public GameObject[] panels;
    public float transitionTime = 0.5f;

    [Header("Кнопки (4 элемента)")]
    public Button[] panelButtons;
    public Button finalButton;

    [Header("Настройки сцены")]
    public string nextSceneName = "RegisterApp"; 

    private int currentPanelIndex = 0;
    private const string TutorialCompletedKey = "TutorialCompleted";

    void Start()
    {
        
        if (!ValidateComponents()) return;

        
        if (PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1)
        {
           
            LoadNextScene();
            return;
        }

       
        InitializePanels();

        SetupButtonHandlers();

        currentPanelIndex = 0;
    }

    private bool ValidateComponents()
    {
        if (panels == null || panels.Length != 5)
        {
            Debug.LogError("Необходимо назначить ровно 5 панелей!");
            return false;
        }

        if (panelButtons == null || panelButtons.Length != 4)
        {
            Debug.LogError("Необходимо назначить ровно 4 кнопки переключения!");
            return false;
        }

        if (finalButton == null)
        {
            Debug.LogError("Финальная кнопка не назначена!");
            return false;
        }

        return true;
    }

    private void InitializePanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] == null)
            {
                Debug.LogError($"Панель {i + 1} не назначена!");
                continue;
            }

            CanvasGroup cg = GetOrAddCanvasGroup(panels[i]);
            if (cg == null) continue;

            bool shouldActivate = (i == 0);

            panels[i].SetActive(shouldActivate);
            cg.alpha = shouldActivate ? 1 : 0;
            cg.interactable = shouldActivate;
            cg.blocksRaycasts = shouldActivate;
        }
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.AddComponent<CanvasGroup>();
            Debug.Log($"Добавлен CanvasGroup к {panel.name}", panel);
        }
        return cg;
    }

    private void SetupButtonHandlers()
    {
        
        for (int i = 0; i < panelButtons.Length; i++)
        {
            if (panelButtons[i] == null)
            {
                Debug.LogError($"Кнопка {i + 1} не назначена!");
                continue;
            }

            int index = i + 1; 
            panelButtons[i].onClick.RemoveAllListeners();
            panelButtons[i].onClick.AddListener(() => SwitchToPanel(index));
        }

        
        finalButton.onClick.RemoveAllListeners();
        finalButton.onClick.AddListener(CompleteTutorial);
    }

    public void SwitchToPanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= panels.Length)
        {
            Debug.LogError($"Неверный индекс панели: {panelIndex}");
            return;
        }

        if (panelIndex == currentPanelIndex) return;

        StartCoroutine(TransitionToPanel(panelIndex));
    }

    private IEnumerator TransitionToPanel(int newIndex)
    {
        CanvasGroup currentPanel = GetValidCanvasGroup(currentPanelIndex);
        CanvasGroup nextPanel = GetValidCanvasGroup(newIndex);

        if (currentPanel == null || nextPanel == null) yield break;

        nextPanel.gameObject.SetActive(true);
        nextPanel.interactable = true;
        nextPanel.blocksRaycasts = true;

        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            float progress = elapsedTime / transitionTime;
            currentPanel.alpha = Mathf.Lerp(1, 0, progress);
            nextPanel.alpha = Mathf.Lerp(0, 1, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentPanel.alpha = 0;
        currentPanel.interactable = false;
        currentPanel.blocksRaycasts = false;
        currentPanel.gameObject.SetActive(false);

        currentPanelIndex = newIndex;
    }

    private CanvasGroup GetValidCanvasGroup(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= panels.Length || panels[panelIndex] == null)
        {
            Debug.LogError($"Неверный индекс панели: {panelIndex}");
            return null;
        }

        return GetOrAddCanvasGroup(panels[panelIndex]);
    }

    private void CompleteTutorial()
    {
        
        PlayerPrefs.SetInt(TutorialCompletedKey, 1);
        PlayerPrefs.Save();

        
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (IsSceneAvailable(nextSceneName))
        {
            finalButton.interactable = false;
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"Сцена {nextSceneName} не найдена!");
            SwitchToPanel(panels.Length - 1);
        }
    }

    private bool IsSceneAvailable(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(scenePath) == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    [ContextMenu("Сбросить прогресс")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(TutorialCompletedKey);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}