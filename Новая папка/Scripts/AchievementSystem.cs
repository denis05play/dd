using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }
    public static event Action<Achievement> OnAchievementUnlocked;

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public AchievementType type;
        public float targetValue;
        public bool isUnlocked;
        public DateTime unlockDate;
    }

    public enum AchievementType { Water, Workout, Calories, WeightLoss }

    [Header("Achievement Settings")]
    public List<Achievement> allAchievements = new List<Achievement>
    {
        new Achievement
        {
            id = "water_200",
            title = "Новичок в гидратации",
            description = "Выпить 200ml воды",
            type = AchievementType.Water,
            targetValue = 200
        }
    };

    [Header("UI Settings")]
    public TMP_Text unlockedText;
    public TMP_Text lockedText;
    public GameObject achievementPanel;
    public Button closeButton;

    public Dictionary<AchievementType, float> progressValues = new Dictionary<AchievementType, float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSystem();
        }
        else
        {
            Destroy(gameObject);
            return; 
        }

        
        if (unlockedText == null) Debug.LogError("Unlocked Text не назначен!");
        if (lockedText == null) Debug.LogError("Locked Text не назначен!");
        if (achievementPanel == null) Debug.LogError("Panel не назначена!");
        if (closeButton == null) Debug.LogError("Close Button не назначен!");
    }

    private void InitializeSystem()
    {
        InitializeProgress();
        LoadAchievements();
        SetupUI();
    }

    private void InitializeProgress()
    {
        foreach (AchievementType type in Enum.GetValues(typeof(AchievementType)))
        {
            progressValues[type] = PlayerPrefs.GetFloat(type.ToString(), 0f);
        }
    }

    private void SetupUI()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => ToggleUI(false));
        }
        else
        {
            Debug.LogError("Close Button не назначен в инспекторе!");
        }

        if (achievementPanel != null)
        {
            ToggleUI(false);
        }
        else
        {
            Debug.LogError("Achievement Panel не назначен!");
        }
    }

    public void UpdateProgress(AchievementType type, float value)
    {
        if (!progressValues.ContainsKey(type)) return;

        progressValues[type] += value;
        PlayerPrefs.SetFloat(type.ToString(), progressValues[type]);
        CheckForAchievements(type);
        UpdateUI();
    }

    private void CheckForAchievements(AchievementType type)
    {
        foreach (var achievement in allAchievements
            .Where(a => a.type == type && !a.isUnlocked && progressValues[type] >= a.targetValue))
        {
            UnlockAchievement(achievement);
        }
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;
        achievement.unlockDate = DateTime.Now;
        PlayerPrefs.SetInt(achievement.id, 1);
        PlayerPrefs.SetString($"{achievement.id}_date", achievement.unlockDate.ToString());

        OnAchievementUnlocked?.Invoke(achievement);
        Debug.Log($"Достижение получено: {achievement.title}");
        UpdateUI();
    }

    private void LoadAchievements()
    {
        foreach (var achievement in allAchievements)
        {
            achievement.isUnlocked = PlayerPrefs.GetInt(achievement.id, 0) == 1;
            if (achievement.isUnlocked)
            {
                var dateStr = PlayerPrefs.GetString($"{achievement.id}_date");
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    achievement.unlockDate = date;
                }
            }
        }
    }

    public void ToggleUI(bool show)
    {
        achievementPanel.SetActive(show);
        if (show) UpdateUI();
    }

    private void UpdateUI()
    {
        unlockedText.text = "Полученные достижения:\n";
        lockedText.text = "Недоступные достижения:\n";

        foreach (var achievement in allAchievements)
        {
            if (achievement.isUnlocked)
            {
                unlockedText.text += $"{achievement.title}\n({achievement.unlockDate:dd.MM.yyyy})\n\n";
            }
            else
            {
                lockedText.text += $"{achievement.title}\nПрогресс: {progressValues[achievement.type]}/{achievement.targetValue}\n\n";
            }
        }
    }

    public List<Achievement> GetUnlockedAchievements()
    {
        return allAchievements.Where(a => a.isUnlocked).ToList();
    }

    public List<Achievement> GetLockedAchievements()
    {
        return allAchievements.Where(a => !a.isUnlocked).ToList();
    }

    public void ResetProgress()
    {
        foreach (var key in progressValues.Keys.ToList())
        {
            progressValues[key] = 0f;
            PlayerPrefs.DeleteKey(key.ToString());
        }
        PlayerPrefs.Save();
        UpdateUI();
    }
}