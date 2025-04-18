using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class creataccskip : MonoBehaviour
{
    [Header("Панели")]
    public GameObject firstPanel;
    public GameObject secondPanel;

    [Header("Кнопки")]
    public Button toSecondPanelButton;
    public Button toFirstPanelButton;

    [Header("Настройки анимации")]
    [Range(0.1f, 2f)] public float fadeDuration = 0.5f;

    private CanvasGroup firstPanelCG;
    private CanvasGroup secondPanelCG;

    private void Awake()
    {
        
        firstPanel.SetActive(true); 
        firstPanelCG = firstPanel.GetComponent<CanvasGroup>();
        if (firstPanelCG == null) firstPanelCG = firstPanel.AddComponent<CanvasGroup>();

        secondPanel.SetActive(true); 
        secondPanelCG = secondPanel.GetComponent<CanvasGroup>();
        if (secondPanelCG == null) secondPanelCG = secondPanel.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
       
        SetPanelState(firstPanel, firstPanelCG, true);
        SetPanelState(secondPanel, secondPanelCG, false);

        
        toSecondPanelButton.onClick.AddListener(() => StartCoroutine(SwitchToSecond()));
        toFirstPanelButton.onClick.AddListener(() => StartCoroutine(SwitchToFirst()));
    }

    private IEnumerator SwitchToSecond()
    {
        yield return StartCoroutine(FadePanel(firstPanel, firstPanelCG, 1, 0));
        yield return StartCoroutine(FadePanel(secondPanel, secondPanelCG, 0, 1));
    }

    private IEnumerator SwitchToFirst()
    {
        yield return StartCoroutine(FadePanel(secondPanel, secondPanelCG, 1, 0));
        yield return StartCoroutine(FadePanel(firstPanel, firstPanelCG, 0, 1));
    }

    private IEnumerator FadePanel(GameObject panel, CanvasGroup cg, float startAlpha, float endAlpha)
    {
        panel.SetActive(true);
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            yield return null;
        }

        panel.SetActive(endAlpha > 0);
    }

    private void SetPanelState(GameObject panel, CanvasGroup cg, bool show)
    {
        panel.SetActive(show);
        cg.alpha = show ? 1 : 0;
    }
}