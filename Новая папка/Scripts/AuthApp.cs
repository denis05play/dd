using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class LocalAuthManager : MonoBehaviour
{
    [Header("Auth Panels")]
    public GameObject registrationPanel;
    public GameObject loginPanel;
    public GameObject bodyDataPanel;

    [Header("Registration UI")]
    public TMP_InputField regNameInput;
    public TMP_InputField regEmailInput;
    public TMP_InputField regPasswordInput;
    public Button registerButton;
    public Button goToLoginButton;

    [Header("Login UI")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;
    public Button goToRegisterButton;

    [Header("Body Data UI")]
    public TMP_InputField weightInput;
    public TMP_InputField heightInput;
    public TMP_Dropdown genderDropdown;
    public Button saveBodyDataButton;

    [Header("Common UI")]
    public TMP_Text statusText;
    public Button rememberMeButton;
    public Image rememberMeCheckmark;

    [Header("Settings")]
    public float sceneLoadDelay = 0.5f;

    private bool rememberMe = false;
    private const string USER_DATA_KEY = "local_users_data";
    private const string BODY_DATA_KEY = "user_body_data";

    private void Start()
    {
        InitializeGenderDropdown();

        
        rememberMeButton.onClick.AddListener(ToggleRememberMe);
        UpdateRememberMeVisual();

       
        registerButton.onClick.AddListener(Register);
        loginButton.onClick.AddListener(Login);
        goToLoginButton.onClick.AddListener(() => SwitchToPanel(loginPanel));
        goToRegisterButton.onClick.AddListener(() => SwitchToPanel(registrationPanel));
        saveBodyDataButton.onClick.AddListener(SaveBodyData);

        
        SwitchToPanel(loginPanel);

       
        if (PlayerPrefs.HasKey("rememberMe") && PlayerPrefs.GetInt("rememberMe") == 1)
        {
            loginEmailInput.text = PlayerPrefs.GetString("saved_email");
            loginPasswordInput.text = PlayerPrefs.GetString("saved_password");
            rememberMe = true;
            UpdateRememberMeVisual();
        }
    }

    private void InitializeGenderDropdown()
    {
        genderDropdown.ClearOptions();
        List<string> options = new List<string> { "Мужской", "Женский", "Другой" };
        genderDropdown.AddOptions(options);

        if (genderDropdown.template != null)
        {
            RectTransform templateRect = genderDropdown.template.GetComponent<RectTransform>();
            if (templateRect != null)
            {
                templateRect.sizeDelta = new Vector2(templateRect.sizeDelta.x, 300f);
            }

            var scrollRect = genderDropdown.template.GetComponent<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                var layout = scrollRect.content.GetComponent<VerticalLayoutGroup>();
                if (layout == null) layout = scrollRect.content.AddComponent<VerticalLayoutGroup>();

                layout.spacing = 40f;
                layout.padding = new RectOffset(10, 10, 20, 20);
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childControlHeight = false;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;

                ContentSizeFitter sizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
                if (sizeFitter == null) sizeFitter = scrollRect.content.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            TMP_Dropdown dropdown = genderDropdown.GetComponent<TMP_Dropdown>();
            if (dropdown != null)
            {
                dropdown.itemText.fontSize = 72;
                dropdown.itemText.margin = new Vector4(20, 10, 20, 10);

                if (dropdown.itemText.rectTransform != null)
                {
                    dropdown.itemText.rectTransform.sizeDelta = new Vector2(
                        dropdown.itemText.rectTransform.sizeDelta.x,
                        80f
                    );
                }
            }
        }

        if (genderDropdown.captionText != null)
        {
            genderDropdown.captionText.fontSize = 42;
            genderDropdown.captionText.margin = new Vector4(10, 0, 10, 0);
        }
    }

    private void SwitchToPanel(GameObject activePanel)
    {
        registrationPanel.SetActive(false);
        loginPanel.SetActive(false);
        bodyDataPanel.SetActive(false);
        activePanel.SetActive(true);
        statusText.text = "";
    }

    private void ShowBodyDataPanel()
    {
        SwitchToPanel(bodyDataPanel);
    }

    private void ToggleRememberMe()
    {
        rememberMe = !rememberMe;
        UpdateRememberMeVisual();
    }

    private void UpdateRememberMeVisual()
    {
        rememberMeCheckmark.gameObject.SetActive(rememberMe);
    }

    public void Register()
    {
        string name = regNameInput.text.Trim();
        string email = regEmailInput.text.Trim().ToLower();
        string password = regPasswordInput.text;

        if (string.IsNullOrEmpty(name)) { ShowError("Введите имя", false); return; }
        if (string.IsNullOrEmpty(email)) { ShowError("Введите email", false); return; }
        if (string.IsNullOrEmpty(password)) { ShowError("Введите пароль", false); return; }
        if (!IsValidEmail(email)) { ShowError("Некорректный email", false); return; }
        if (password.Length < 6) { ShowError("Пароль должен быть не менее 6 символов", false); return; }

        if (UserExists(email))
        {
            ShowError("Пользователь с таким email уже существует", false);
            return;
        }

        RegisterNewUser(name, email, password);
        ShowSuccess("Регистрация успешна!");

       
        if (rememberMe)
        {
            PlayerPrefs.SetString("saved_email", email);
            PlayerPrefs.SetString("saved_password", password);
            PlayerPrefs.SetInt("rememberMe", 1);
            PlayerPrefs.Save();
        }

        ShowBodyDataPanel();
    }

    public void Login()
    {
        string email = loginEmailInput.text.Trim().ToLower();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email)) { ShowError("Введите email", true); return; }
        if (string.IsNullOrEmpty(password)) { ShowError("Введите пароль", true); return; }

        if (AuthenticateUser(email, password))
        {
            if (rememberMe)
            {
                PlayerPrefs.SetString("saved_email", email);
                PlayerPrefs.SetString("saved_password", password);
                PlayerPrefs.SetInt("rememberMe", 1);
            }
            else
            {
                PlayerPrefs.DeleteKey("saved_email");
                PlayerPrefs.DeleteKey("saved_password");
                PlayerPrefs.DeleteKey("rememberMe");
            }
            PlayerPrefs.Save();

            ShowSuccess("Вход выполнен!");

            if (PlayerPrefs.HasKey(BODY_DATA_KEY))
            {
                StartCoroutine(LoadSceneAfterDelay("SampleScene", sceneLoadDelay));
            }
            else
            {
                ShowBodyDataPanel();
            }
        }
        else
        {
            ShowError("Неверный email или пароль", true);
        }
    }

    private void SaveBodyData()
    {
        if (!float.TryParse(weightInput.text, out float weight) || weight <= 0)
        {
            ShowError("Введите корректный вес", true);
            return;
        }

        if (!float.TryParse(heightInput.text, out float height) || height <= 0)
        {
            ShowError("Введите корректный рост", true);
            return;
        }

        
        BodyData bodyData = new BodyData
        {
            weight = weight,
            height = height,
            gender = genderDropdown.options[genderDropdown.value].text,
            bmi = CalculateBMI(weight, height)
        };
        PlayerPrefs.SetString(BODY_DATA_KEY, JsonUtility.ToJson(bodyData));

      
        var weightTracker = FindObjectOfType<WeightTracker>();
        if (weightTracker != null)
        {
            weightTracker.SaveNewUserData(weight, height);
        }

        PlayerPrefs.Save();
        StartCoroutine(LoadSceneAfterDelay("SampleScene", sceneLoadDelay));
    }

    private float CalculateBMI(float weight, float height)
    {
        return weight / Mathf.Pow(height / 100f, 2);
    }

    private bool UserExists(string email)
    {
        string usersData = PlayerPrefs.GetString(USER_DATA_KEY, "");
        return usersData.Contains($"\"email\":\"{email}\"");
    }

    private void RegisterNewUser(string name, string email, string password)
    {
        List<UserData> users = GetAllUsers();
        users.Add(new UserData { name = name, email = email, password = password });
        SaveAllUsers(users);
    }

    private bool AuthenticateUser(string email, string password)
    {
        List<UserData> users = GetAllUsers();
        var user = users.Find(u => u.email == email);
        return user != null && user.password == password;
    }

    private List<UserData> GetAllUsers()
    {
        string usersJson = PlayerPrefs.GetString(USER_DATA_KEY, "{\"users\":[]}");
        try
        {
            UserList userList = JsonUtility.FromJson<UserList>(usersJson);
            return userList?.users ?? new List<UserData>();
        }
        catch
        {
            return new List<UserData>();
        }
    }

    private void SaveAllUsers(List<UserData> users)
    {
        UserList userList = new UserList { users = users };
        PlayerPrefs.SetString(USER_DATA_KEY, JsonUtility.ToJson(userList));
        PlayerPrefs.Save();
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ShowError(string message, bool isLoginPanel)
    {
        statusText.color = Color.red;
        statusText.text = message;
    }

    private void ShowSuccess(string message)
    {
        statusText.color = Color.green;
        statusText.text = message;
    }

    [System.Serializable]
    private class UserData
    {
        public string name;
        public string email;
        public string password;
    }

    [System.Serializable]
    private class UserList
    {
        public List<UserData> users;
    }

    [System.Serializable]
    public class BodyData 
    {
        public float weight;
        public float height;
        public string gender;
        public float bmi;
    }
}