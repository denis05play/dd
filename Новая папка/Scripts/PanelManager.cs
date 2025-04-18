using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Indices from Build Settings")]
    public int[] sceneIndexes;

    [Header("Switch Buttons")]
    public Button[] sceneButtons;

    [Header("Transition Effects")]
    public Image loadingOverlay;
    public float fadeDuration = 1f;

    private void Start()
    {
        InitializeButtons();
        SetupVisuals();
    }

    void InitializeButtons()
    {
        if (sceneButtons != null && sceneButtons.Length == sceneIndexes.Length)
        {
            for (int i = 0; i < sceneButtons.Length; i++)
            {
                int index = i;
                sceneButtons[index].onClick.AddListener(() => LoadScene(index));
            }
        }
    }

    void SetupVisuals()
    {
        if (loadingOverlay != null)
        {
            loadingOverlay.gameObject.SetActive(false);
            loadingOverlay.color = new Color(0, 0, 0, 0);
        }
    }

    public void LoadScene(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= sceneIndexes.Length)
        {
            Debug.LogError("Invalid button index!");
            return;
        }

        int targetSceneIndex = sceneIndexes[buttonIndex];

        if (targetSceneIndex < 0 || targetSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("Invalid scene index: " + targetSceneIndex);
            return;
        }

        StartCoroutine(LoadSceneWithFade(targetSceneIndex));
    }

    System.Collections.IEnumerator LoadSceneWithFade(int sceneIndex)
    {
        SetButtonsInteractable(false);

        if (loadingOverlay != null)
        {
            loadingOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0, 1));
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        if (loadingOverlay != null)
        {
            yield return StartCoroutine(Fade(1, 0));
            loadingOverlay.gameObject.SetActive(false);
        }
    }

    System.Collections.IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (loadingOverlay != null)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
                loadingOverlay.color = new Color(0, 0, 0, alpha);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in sceneButtons)
        {
            if (btn != null) btn.interactable = state;
        }
    }

    public void ReturnToMainMenu()
    {
        LoadScene(0);
    }
}