using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Leave : MonoBehaviour
{
    

    [Header("Основные настройки")]
    [Tooltip("Имя сцены для возврата (из Build Settings)")]
    public string targetSceneName = "MainMenu";

    [Header("Настройки кнопки")]
    public Button returnButton;
    public float delayBeforeLoad = 0.5f;
    public bool useFadeEffect = true;
    public float fadeDuration = 0.5f;

    [Header("Визуальные эффекты")]
    public Image fadeImage;
    public Color fadeColor = Color.black;

    private void Start()
    {
       
        if (returnButton == null)
        {
            returnButton = GetComponent<Button>();
            if (returnButton == null)
            {
                Debug.LogError("Не найдена кнопка на этом объекте!");
                return;
            }
        }

        
        returnButton.onClick.AddListener(ReturnToScene);

        
        if (useFadeEffect && fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void ReturnToScene()
    {
        StartCoroutine(ReturnToSceneCoroutine());
    }

    private IEnumerator ReturnToSceneCoroutine()
    {
       
        returnButton.interactable = false;

        
        if (useFadeEffect && fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float timer = 0f;

            while (timer < fadeDuration)
            {
                float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }

       
        yield return new WaitForSeconds(delayBeforeLoad);

     
        if (!SceneExists(targetSceneName))
        {
            Debug.LogError($"Сцена {targetSceneName} не найдена в Build Settings!");
            returnButton.interactable = true;
            yield break;
        }

        
        SceneManager.LoadScene(targetSceneName);
    }

    
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var lastSlash = scenePath.LastIndexOf("/");
            var sceneNameInBuild = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

            if (string.Compare(sceneNameInBuild, sceneName, true) == 0)
                return true;
        }
        return false;
    }

   
    public void SetTargetScene(string newSceneName)
    {
        targetSceneName = newSceneName;
    }
}