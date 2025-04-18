using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
    [Header("UI Elements")]
    public Button startButton;
    public Button stopButton;
    public Button resetButton;
    public Text timerText;

    [Header("Timer Settings")]
    public float initialTime = 0f;
    public bool countUp = true;

    private float currentTime;
    private bool isTimerRunning;
    private const string SavedTimeKey = "SavedTimerTime";

    private void Start()
    {
        
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(true);

        
        LoadTimer();

        
        startButton.onClick.AddListener(StartTimer);
        stopButton.onClick.AddListener(StopTimer);
        resetButton.onClick.AddListener(ResetTimer);
    }

    private void Update()
    {
        if (isTimerRunning)
        {
          
            currentTime += countUp ? Time.deltaTime : -Time.deltaTime;
            currentTime = Mathf.Max(0, currentTime); // Не даем уйти ниже 0
            UpdateTimerDisplay();
        }
    }

    private void StartTimer()
    {
        isTimerRunning = true;
        SwitchButtons(true);
    }

    private void StopTimer()
    {
        isTimerRunning = false;
        SaveTimer();
        SwitchButtons(false);
    }

    public void ResetTimer()
    {
      
        isTimerRunning = false;
        currentTime = initialTime;
        UpdateTimerDisplay();
        SaveTimer();

        
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
    }

    private void SwitchButtons(bool timerRunning)
    {
        startButton.gameObject.SetActive(!timerRunning);
        stopButton.gameObject.SetActive(timerRunning);
    }

    private void UpdateTimerDisplay()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                              timeSpan.Hours,
                              timeSpan.Minutes,
                              timeSpan.Seconds);
    }

    private void SaveTimer()
    {
        PlayerPrefs.SetFloat(SavedTimeKey, currentTime);
        PlayerPrefs.Save();
    }

    private void LoadTimer()
    {
        currentTime = PlayerPrefs.GetFloat(SavedTimeKey, initialTime);
        UpdateTimerDisplay();
    }

    [ContextMenu("Полный сброс таймера")]
    private void FullReset()
    {
        ResetTimer();
        PlayerPrefs.DeleteKey(SavedTimeKey);
        PlayerPrefs.Save();
    }
}