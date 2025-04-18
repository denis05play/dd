using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    [Header("Profile Info")]
    public Text profileNameText;
    public Text ageText;
    public Text weightText;
    public Text heightText;
    public Image profileIcon;

    [Header("Panels")]
    public GameObject mainPanel; 
    public GameObject editPanel; 
    public GameObject friendsPanel; 

    [Header("Edit Panel Elements")]
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public TMP_InputField weightInput;
    public TMP_InputField heightInput;
    public Button changeIconButton;
    public Button saveButton;
    public Button cancelButton;

    [Header("Navigation Buttons")]
    public Button openEditButton; 
    public Button openFriendsButton;

    [Header("Settings")]
    public Sprite defaultAvatar;
    private string avatarPathKey = "avatar_path";

    private void Start()
    {
        InitializeProfile();
        SetupMobileUI();
        SetupNavigationButtons(); 
    }

    void LoadMobileAvatar()
    {
        string path = PlayerPrefs.GetString(avatarPathKey);
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadMobileAvatarCoroutine(path)); 
        }
        else
        {
            profileIcon.sprite = defaultAvatar;
        }
    }

  
    IEnumerator LoadMobileAvatarCoroutine(string path)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                profileIcon.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.one * 0.5f
                );
            }
            else
            {
                Debug.LogError("Ошибка загрузки аватара: " + request.error);
            }
        }
    }

   
    void SetupMobileUI()
    {
       
        bool wasActive = editPanel.activeSelf;
        editPanel.SetActive(true);

        changeIconButton.onClick.AddListener(PickMobileImage);
        saveButton.onClick.AddListener(SaveMobileSettings);
        cancelButton.onClick.AddListener(() => editPanel.SetActive(false));

        foreach (var input in editPanel.GetComponentsInChildren<TMP_InputField>(true)) 
        {
            input.shouldHideMobileInput = true;
        }

        
        editPanel.SetActive(wasActive);
    }

   
    public void PickMobileImage()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                StartCoroutine(LoadMobileAvatarCoroutine(path));
                PlayerPrefs.SetString(avatarPathKey, path);
            }
        },
        "Select Profile Picture",
        "image/*");
    }

    void SetupNavigationButtons()
    {
     
        openEditButton.onClick.AddListener(() => TogglePanels(editPanel));
        openFriendsButton.onClick.AddListener(() => TogglePanels(friendsPanel));
    }

    void TogglePanels(GameObject targetPanel)
    {
        Debug.Log($"Переключение на панель: {targetPanel.name}");

        mainPanel.SetActive(false);
        editPanel.SetActive(false);
        friendsPanel.SetActive(false);

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            Debug.Log($"Активные элементы на {targetPanel.name}: {targetPanel.activeInHierarchy}");
        }

        if (targetPanel == editPanel) LoadEditPanelData();
    }

    void LoadEditPanelData()
    {
        nameInput.text = PlayerPrefs.GetString("user_name");
        ageInput.text = PlayerPrefs.GetInt("user_age", 0).ToString();
        weightInput.text = PlayerPrefs.GetFloat("user_weight", 0).ToString("F1");
        heightInput.text = PlayerPrefs.GetFloat("user_height", 0).ToString("F1");
    }

   

    public void SaveMobileSettings()
    {
       
        float newWeight = ValidateFloat(weightInput.text);
        float newHeight = ValidateFloat(heightInput.text);
        int newAge = ValidateInt(ageInput.text);
        string newName = nameInput.text;

        Debug.Log($"Попытка сохранения: {newName}, " +
                 $"{newAge} лет, {newWeight} кг, {newHeight} см");

       
        if (PlayerPrefs.GetFloat("user_weight") != newWeight ||
           PlayerPrefs.GetFloat("user_height") != newHeight ||
           PlayerPrefs.GetInt("user_age") != newAge ||
           PlayerPrefs.GetString("user_name") != newName)
        {
            PlayerPrefs.SetString("user_name", newName);
            PlayerPrefs.SetInt("user_age", newAge);
            PlayerPrefs.SetFloat("user_weight", newWeight);
            PlayerPrefs.SetFloat("user_height", newHeight);
            PlayerPrefs.Save();

            Debug.Log("Данные успешно сохранены в PlayerPrefs");

           
            InitializeProfile();
            UpdateHealthSystems();
        }
        else
        {
            Debug.Log("Данные не изменились, сохранение не требуется");
        }

        TogglePanels(mainPanel);
    }

    private void InitializeProfile()
    {
       
        string userName = PlayerPrefs.GetString("user_name", "Гость");
        int userAge = PlayerPrefs.GetInt("user_age", 18);
        float userWeight = PlayerPrefs.GetFloat("user_weight", -1f);
        float userHeight = PlayerPrefs.GetFloat("user_height", -1f);

        
        if (userWeight < 0 || userHeight < 0)
        {
            Debug.LogWarning("Основные данные не найдены, проверяем BodyData...");
            if (PlayerPrefs.HasKey("user_body_data"))
            {
                LocalAuthManager.BodyData bodyData =
                    JsonUtility.FromJson<LocalAuthManager.BodyData>(
                        PlayerPrefs.GetString("user_body_data"));

                userWeight = bodyData.weight;
                userHeight = bodyData.height;

              
                PlayerPrefs.SetFloat("user_weight", userWeight);
                PlayerPrefs.SetFloat("user_height", userHeight);
                PlayerPrefs.Save();
            }
        }

        
        if (userWeight < 0) userWeight = 70f;
        if (userHeight < 0) userHeight = 170f;

        Debug.Log($"Инициализация профиля: {userName}, " +
                 $"{userAge} лет, {userWeight} кг, {userHeight} см");

        profileNameText.text = userName;
        ageText.text = userAge.ToString();
        weightText.text = userWeight.ToString("F1") + " кг";
        heightText.text = userHeight.ToString("F1") + " см";

        LoadMobileAvatar();
    }

   




  
    int ValidateInt(string value)
    {
        int.TryParse(value, out int result);
        return Mathf.Clamp(result, 0, 150);
    }

    
    float ValidateFloat(string value)
    {
        float.TryParse(value, out float result);
        return Mathf.Clamp(result, 0f, 300f);
    }


    void UpdateHealthSystems()
    {
        try
        {
            float weight = PlayerPrefs.GetFloat("user_weight");
            float height = PlayerPrefs.GetFloat("user_height");

            
            if (WeightTracker.Instance != null)
            {
                WeightTracker.Instance.UpdateWeightData(weight, height);
            }
            else
            {
                Debug.LogWarning("WeightTracker instance not found");
            }

            
            weightText.text = weight.ToString("F1") + " кг";
            heightText.text = height.ToString("F1") + " см";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка в UpdateHealthSystems: {e.Message}");
        }
    }


    public class UIInputBlocker : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"Клик по {EventSystem.current.currentSelectedGameObject}");
            }
        }
    }
}