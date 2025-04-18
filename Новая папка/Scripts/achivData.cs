using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class achivData : MonoBehaviour
{
    [System.Serializable]
    public class Achievement
    {
        public string achievementID;
        public string title;
        public string description;
        public Sprite icon;
        public bool isUnlocked;
        public DateTime unlockDate;
        public bool isPermanent; 
    }

    [Header("Настройки")]
    public TMP_InputField inputField;
    public Button addButton;
    public Sprite defaultIcon;
    public int spacing = 20;
    public Color panelColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("Главное достижение")]
    public Achievement registrationAchievement;

    [Header("Ссылки")]
    public Transform contentParent;
    public ScrollRect scrollRect;

    private List<Achievement> allAchievements = new List<Achievement>();
    private VerticalLayoutGroup layoutGroup;
    private const string SAVE_KEY = "AchievementsData";
    private const string REGISTRATION_KEY = "RegistrationCompleted";

    private void Start()
    {
        InitializeSystem();
        LoadAchievements();
        CheckRegistrationAchievement();
    }

    private void InitializeSystem()
    {
        if (!CheckComponents()) return;

     
        layoutGroup = contentParent.GetComponent<VerticalLayoutGroup>() ?? contentParent.gameObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = spacing;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);

        ContentSizeFitter sizeFitter = contentParent.GetComponent<ContentSizeFitter>() ?? contentParent.gameObject.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

       
        addButton.onClick.AddListener(AddCustomAchievement);
    }

    private bool CheckComponents()
    {
        if (contentParent == null) Debug.LogError("Content Parent не назначен!");
        if (scrollRect == null) Debug.LogError("Scroll Rect не назначен!");
        if (addButton == null) Debug.LogError("Add Button не назначен!");
        if (inputField == null) Debug.LogError("Input Field не назначен!");

        return contentParent != null && scrollRect != null && addButton != null && inputField != null;
    }

    private void CheckRegistrationAchievement()
    {
        if (!PlayerPrefs.HasKey(REGISTRATION_KEY))
        {
            
            Achievement regAchievement = new Achievement
            {
                achievementID = "REG_ACHIEVEMENT",
                title = registrationAchievement.title,
                description = registrationAchievement.description,
                icon = registrationAchievement.icon,
                isUnlocked = true,
                unlockDate = DateTime.Now,
                isPermanent = true
            };

            allAchievements.Add(regAchievement);
            CreateAchievementItem(regAchievement);
            SaveAchievements();

            PlayerPrefs.SetInt(REGISTRATION_KEY, 1);
            PlayerPrefs.Save();
        }
        else
        {
            
            if (!allAchievements.Any(a => a.achievementID == "REG_ACHIEVEMENT"))
            {
                allAchievements.Add(new Achievement
                {
                    achievementID = "REG_ACHIEVEMENT",
                    title = registrationAchievement.title,
                    description = registrationAchievement.description,
                    icon = registrationAchievement.icon,
                    isUnlocked = true,
                    unlockDate = DateTime.Now,
                    isPermanent = true
                });
            }
        }
    }

    public void AddCustomAchievement()
    {
        string sportName = inputField.text.Trim();
        if (string.IsNullOrEmpty(sportName)) return;

        Achievement newAchievement = new Achievement
        {
            achievementID = Guid.NewGuid().ToString(),
            title = sportName,
            description = $"Успешное выступление в {sportName}",
            icon = defaultIcon,
            isUnlocked = true,
            unlockDate = DateTime.Now
        };

        allAchievements.Add(newAchievement);
        CreateAchievementItem(newAchievement);
        SaveAchievements();
        inputField.text = "";
    }

    private void UnlockAchievement(Achievement achievement)
    {
        if (achievement.isUnlocked) return;

        achievement.isUnlocked = true;
        achievement.unlockDate = DateTime.Now;
        CreateAchievementItem(achievement);
        SaveAchievements();
    }

    private void CreateAchievementItem(Achievement achievement)
    {
        
        GameObject panel = new GameObject("AchievementPanel");
        panel.transform.SetParent(contentParent, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = panelColor;
        LayoutElement panelLayout = panel.AddComponent<LayoutElement>();
        panelLayout.preferredHeight = 150; 

       
        HorizontalLayoutGroup hLayout = panel.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 20;
        hLayout.padding = new RectOffset(15, 15, 15, 15);
        hLayout.childAlignment = TextAnchor.UpperLeft;
        hLayout.childControlWidth = true;
        hLayout.childForceExpandWidth = false;

       
        GameObject iconContainer = new GameObject("IconContainer");
        iconContainer.transform.SetParent(panel.transform, false);
        LayoutElement iconContainerLayout = iconContainer.AddComponent<LayoutElement>();
        iconContainerLayout.preferredWidth = 120;
        iconContainerLayout.flexibleWidth = 0;

        
        GameObject sportIcon = new GameObject("SportIcon");
        sportIcon.transform.SetParent(iconContainer.transform, false);
        Image iconImage = sportIcon.AddComponent<Image>();
        iconImage.sprite = achievement.icon ?? defaultIcon;
        iconImage.preserveAspect = true;
        iconImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        iconImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        iconImage.rectTransform.sizeDelta = new Vector2(100, 100);
        iconImage.rectTransform.anchoredPosition = Vector2.zero;

        
        GameObject textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(panel.transform, false);
        LayoutElement textContainerLayout = textContainer.AddComponent<LayoutElement>();
        textContainerLayout.flexibleWidth = 1;

        
        VerticalLayoutGroup textLayout = textContainer.AddComponent<VerticalLayoutGroup>();
        textLayout.spacing = 5;
        textLayout.childControlWidth = true;
        textLayout.childForceExpandWidth = true;
        textLayout.childAlignment = TextAnchor.UpperLeft;

        
        GameObject statusIcon = new GameObject("StatusIcon");
        statusIcon.transform.SetParent(textContainer.transform, false);
        Image statusImage = statusIcon.AddComponent<Image>();
        statusImage.sprite = achievement.isUnlocked ?
            Resources.Load<Sprite>("Unlocked") ?? defaultIcon :
            Resources.Load<Sprite>("Locked") ?? defaultIcon;
        statusImage.preserveAspect = true;
        statusImage.rectTransform.sizeDelta = new Vector2(30, 30);

        LayoutElement statusLayout = statusIcon.AddComponent<LayoutElement>();
        statusLayout.ignoreLayout = true;
        statusImage.rectTransform.anchorMin = new Vector2(1, 1);
        statusImage.rectTransform.anchorMax = new Vector2(1, 1);
        statusImage.rectTransform.pivot = new Vector2(1, 1);
        statusImage.rectTransform.anchoredPosition = new Vector2(-10, -10);

       
        CreateTextElement(textContainer, "Title", achievement.title.ToUpper(), 40, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);

        
        CreateTextElement(textContainer, "Description", achievement.description, 35, FontStyles.Normal, new Color(0.9f, 0.9f, 0.9f), TextAlignmentOptions.Left);

      
        CreateTextElement(textContainer, "Date",
            achievement.unlockDate.ToString("dd.MM.yyyy HH:mm"),
            18,
            FontStyles.Italic,
            new Color(0.7f, 0.7f, 0.7f),
            TextAlignmentOptions.Left);

        StartCoroutine(ScrollToBottom());
    }

    private void CreateTextElement(GameObject parent, string name, string text, int size, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        TMP_Text tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = size;
        tmpText.fontStyle = style;
        tmpText.color = color;
        tmpText.alignment = alignment;
        tmpText.margin = new Vector4(0, 0, 10, 0); 

        LayoutElement layout = textObj.AddComponent<LayoutElement>();
        layout.preferredHeight = size * 1.5f;
    }

    private void SaveAchievements()
    {
       
        var achievementsToSave = allAchievements
            .Where(a => !a.isPermanent)
            .ToList();

        AchievementWrapper wrapper = new AchievementWrapper
        {
            achievements = achievementsToSave
        };

        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    private void LoadAchievements()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            AchievementWrapper wrapper = JsonUtility.FromJson<AchievementWrapper>(PlayerPrefs.GetString(SAVE_KEY));
            allAchievements = wrapper.achievements;

            
            if (!allAchievements.Any(a => a.isPermanent))
            {
                allAchievements.Add(new Achievement
                {
                    achievementID = "REG_ACHIEVEMENT",
                    title = registrationAchievement.title,
                    description = registrationAchievement.description,
                    icon = registrationAchievement.icon,
                    isUnlocked = true,
                    unlockDate = DateTime.Now,
                    isPermanent = true
                });
            }

            foreach (var achievement in allAchievements)
            {
                CreateAchievementItem(achievement);
            }
        }
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();
    }

    [System.Serializable]
    private class AchievementWrapper
    {
        public List<Achievement> achievements;
    }
}