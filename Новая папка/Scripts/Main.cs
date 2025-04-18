using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("������ (4 ��������)")]
    [SerializeField] private GameObject[] panels;
    [SerializeField] private float transitionTime = 0.5f;

    [Header("������ ������������ �������")]
    [SerializeField] private Button buttonPanel1;
    [SerializeField] private Button buttonPanel2;
    [SerializeField] private Button buttonPanel3;
    [SerializeField] private Button buttonPanel4;

    [Header("������ �������� �� �����")]
    [SerializeField] private Button sceneSwitchButton;
    [SerializeField] private int targetSceneIndex = 1;

    private int currentPanelIndex = 0;

    private void Start()
    {
        if (!ValidateComponents())
        {
            Debug.LogError("�� ��� ���������� ��������� � ����������!", this);
            enabled = false;
            return;
        }

        InitializePanels();
        SetupButtonListeners();
    }

    private bool ValidateComponents()
    {
        if (panels == null || panels.Length != 4)
        {
            Debug.LogError("������ panels ������ ��������� 4 ��������!", this);
            return false;
        }

        foreach (var panel in panels)
        {
            if (panel == null)
            {
                Debug.LogError("���������� null-������ � �������!", this);
                return false;
            }
        }

        if (buttonPanel1 == null || buttonPanel2 == null || buttonPanel3 == null || sceneSwitchButton == null)
        {
            Debug.LogError("�� ��� ������ ���������!", this);
            return false;
        }

        return true;
    }

    private void InitializePanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
           
            CanvasGroup cg = GetOrCreateCanvasGroup(panels[i]);
            if (cg == null)
            {
                Debug.LogError($"�� ������� ������� CanvasGroup ��� ������ {i}", panels[i]);
                continue;
            }

            bool isActive = (i == 0);
            panels[i].SetActive(isActive);
            cg.alpha = isActive ? 1 : 0;
            cg.interactable = isActive;
            cg.blocksRaycasts = isActive;
        }
    }

    private CanvasGroup GetOrCreateCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.AddComponent<CanvasGroup>();
            Debug.Log($"�������� CanvasGroup � ������� {panel.name}", panel);
        }
        return cg;
    }

    private void SetupButtonListeners()
    {
        buttonPanel1.onClick.AddListener(() => SwitchToPanel(0));
        buttonPanel2.onClick.AddListener(() => SwitchToPanel(1));
        buttonPanel3.onClick.AddListener(() => SwitchToPanel(2));
        buttonPanel4.onClick.AddListener(() => SwitchToPanel(3));
        sceneSwitchButton.onClick.AddListener(LoadSceneByIndex);
    }

    public void SwitchToPanel(int panelIndex)
    {
        if (panelIndex == currentPanelIndex) return;
        if (panelIndex < 0 || panelIndex >= panels.Length)
        {
            Debug.LogError($"�������� ������ ������: {panelIndex}", this);
            return;
        }

       
        for (int i = 0; i < panels.Length; i++)
        {
            if (i != panelIndex && i != currentPanelIndex)
            {
                GameObject panel = panels[i];
                if (panel != null)
                {
                    CanvasGroup cg = panel.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = 0;
                        cg.interactable = false;
                        cg.blocksRaycasts = false;
                    }
                    panel.SetActive(false);
                }
            }
        }

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
            if (currentPanel != null)
            {
                currentPanel.alpha = Mathf.Lerp(1, 0, progress);
            }
            nextPanel.alpha = Mathf.Lerp(0, 1, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        
        if (currentPanel != null)
        {
            currentPanel.alpha = 0;
            currentPanel.interactable = false;
            currentPanel.blocksRaycasts = false;
            currentPanel.gameObject.SetActive(false);
        }

       
        foreach (var panel in panels)
        {
            if (panel != panels[newIndex] && panel != null)
            {
                panel.SetActive(false);
            }
        }

        currentPanelIndex = newIndex;
    }

    private CanvasGroup GetValidCanvasGroup(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= panels.Length || panels[panelIndex] == null)
        {
            Debug.LogError($"�������� ������ ������: {panelIndex}", this);
            return null;
        }
        return GetOrCreateCanvasGroup(panels[panelIndex]);
    }

    public void LoadSceneByIndex()
    {
        if (targetSceneIndex < 0 || targetSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"�������� ������ �����: {targetSceneIndex}", this);
            return;
        }

        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        sceneSwitchButton.interactable = false;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    [ContextMenu("��������� ���������")]
    private void CheckSettings()
    {
        Debug.Log($"������� ������������:\n" +
                 $"�������: {panels?.Length ?? 0}\n" +
                 $"������ ������� �����: {targetSceneIndex}\n" +
                 $"����� ���� � Build Settings: {SceneManager.sceneCountInBuildSettings}");
    }
}