using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class RegistrationManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField loginInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;
    public Button registerButton;
    public TMP_Text statusText;

    [Header("Server Settings")]
    [SerializeField] private string serverURL = "https://my-api-image.onrender.com/api/registration";

    [Header("Panel Settings")]
    public GameObject registrationPanel; 
    public GameObject bodyDataPanel;    
    public TMP_InputField weightInput;
    public TMP_InputField heightInput;
    public Dropdown genderDropdown;
    public Button saveBodyDataButton;

    private void Start()
    {
       
        registrationPanel.SetActive(true);
        bodyDataPanel.SetActive(false);

        
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowError("Нет интернет-соединения");
            registerButton.interactable = false;
        }

        SetupMobileInput();
        CheckSavedUser();

        
        saveBodyDataButton.onClick.AddListener(SaveBodyData);
    }

    void CheckSavedUser()
    {
        if (PlayerPrefs.HasKey("saved_username") && PlayerPrefs.HasKey("saved_password"))
        {
            loginInput.text = PlayerPrefs.GetString("saved_username");
            passwordInput.text = PlayerPrefs.GetString("saved_password");
        }
    }

    void SetupMobileInput()
    {
#if UNITY_ANDROID || UNITY_IOS
        loginInput.shouldHideMobileInput = false;
        passwordInput.shouldHideMobileInput = false;
        emailInput.shouldHideMobileInput = false;
        weightInput.shouldHideMobileInput = false;
        heightInput.shouldHideMobileInput = false;
#endif
    }

    public void Register()
    {
        if (string.IsNullOrEmpty(loginInput.text))
        {
            ShowError("Введите логин");
            return;
        }

        if (string.IsNullOrEmpty(passwordInput.text))
        {
            ShowError("Введите пароль");
            return;
        }

        if (string.IsNullOrEmpty(emailInput.text))
        {
            ShowError("Введите email");
            return;
        }

        StartCoroutine(RegisterUser());
    }

    IEnumerator RegisterUser()
    {
        statusText.text = "Отправка данных...";
        registerButton.interactable = false;

        var userData = new UserData
        {
            Username = loginInput.text,
            Password = passwordInput.text,
            Email = emailInput.text
        };

        string jsonData = JsonUtility.ToJson(userData);
        Debug.Log("Отправляемый JSON: " + jsonData);

        using (UnityWebRequest www = new UnityWebRequest($"{serverURL}/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            www.timeout = 10;
            yield return www.SendWebRequest();

            registerButton.interactable = true;

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                ShowError($"Ошибка: {www.error}");
                Debug.LogError(www.error);
                Debug.LogError("Ответ сервера: " + www.downloadHandler.text);
            }
            else
            {
                HandleRegistrationSuccess(www.downloadHandler.text);
            }
        }
    }

    void HandleRegistrationSuccess(string response)
    {
        var responseData = JsonUtility.FromJson<RegistrationResponse>(response);

        if (responseData != null && !string.IsNullOrEmpty(responseData.username))
        {
            statusText.color = Color.green;
            statusText.text = "Регистрация успешна!";

            PlayerPrefs.SetString("saved_username", responseData.username);
            PlayerPrefs.SetString("saved_password", passwordInput.text);
            PlayerPrefs.SetString("username", responseData.username);
            PlayerPrefs.Save();

           
            registrationPanel.SetActive(false);
            bodyDataPanel.SetActive(true);
        }
        else
        {
            ShowError("Ошибка при регистрации");
        }
    }

    void SaveBodyData()
    {
       
        if (!float.TryParse(weightInput.text, out float weight) || weight <= 0)
        {
            statusText.color = Color.red;
            statusText.text = "Введите корректный вес!";
            return;
        }

        if (!float.TryParse(heightInput.text, out float height) || height <= 0)
        {
            statusText.color = Color.red;
            statusText.text = "Введите корректный рост!";
            return;
        }

       
        PlayerPrefs.SetFloat("user_weight", weight);
        PlayerPrefs.SetFloat("user_height", height);
        PlayerPrefs.SetString("user_gender", genderDropdown.options[genderDropdown.value].text);
        PlayerPrefs.Save();

       
        SceneManager.LoadScene("MainScene");
    }

    void ShowError(string message)
    {
        statusText.color = Color.red;
        statusText.text = message;
        registerButton.interactable = true;
    }

    [System.Serializable]
    public class UserData
    {
        public string Username;
        public string Password;
        public string Email;
    }

    [System.Serializable]
    private class RegistrationResponse
    {
        public string message;
        public string username;
        public bool success;
    }
}