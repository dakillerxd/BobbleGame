using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class UpdateGameText : MonoBehaviour
{
    [Serializable]
    public class TextConfig
    {
        public TextType type;
        public TextMeshPro[] texts;
        public string prefix = "<sketchy>";
        public string suffix = "</>";
        
        [Header("Animation")]
        public bool enableAnimation = true;
        public float animationDuration = 0.5f;
        public float animationStrength = 1.5f;
        public int animationFrequency = 5;
        
        [Header("Format Settings")]
        public bool useThousandsSeparator;
        public bool showDecimalPlaces;
        public int decimalPlaces = 1;
        
        [Header("Custom")]
        public string customEventName;
    }
    
    public enum TextType
    {
        Score,
        BubblesLeft,
        Time,
        Wave,
        GameModeName,
        Custom
    }
    
    [SerializeField] private List<TextConfig> textConfigs = new List<TextConfig>();
    private Dictionary<string, Action<string>> customEvents = new Dictionary<string, Action<string>>();
    private Dictionary<TextConfig, Sequence> activeAnimations = new Dictionary<TextConfig, Sequence>();

    private void Awake()
    {
        SessionManager.OnGameStateChanged.AddListener(HandleGameStateChange);
    }

    private void OnEnable()
    {
        RegisterEventListeners();
        UpdateGameModeText(); // Update game mode text when enabled
    }

    private void OnDisable()
    {
        UnregisterEventListeners();
        ClearActiveAnimations();
    }

    private void RegisterEventListeners()
    {
        foreach (var config in textConfigs)
        {
            switch (config.type)
            {
                case TextType.Score:
                    SessionManager.OnScoreUpdate.AddListener((score) => 
                        UpdateText(config, FormatNumber(score, config)));
                    break;
                    
                case TextType.BubblesLeft:
                    SessionManager.OnBubbleLeftUpdate.AddListener((bubbles) => 
                        UpdateText(config, FormatNumber(bubbles, config)));
                    break;
                    
                case TextType.Time:
                    SessionManager.OnTimeUpdate.AddListener((time) => 
                        UpdateText(config, FormatTime(time, config)));
                    break;
                    
                case TextType.Wave:
                    SessionManager.OnWaveUpdate.AddListener((wave) => 
                        UpdateText(config, FormatNumber(wave, config)));
                    break;
                    
                case TextType.GameModeName:
                    SessionManager.OnSessionStart.AddListener(UpdateGameModeText);
                    break;
                    
                case TextType.Custom:
                    if (!string.IsNullOrEmpty(config.customEventName))
                    {
                        customEvents[config.customEventName] = (value) => UpdateText(config, value);
                    }
                    break;
            }
        }
    }

    private void UpdateGameModeText()
    {
        if (SessionManager.CurrentGameMode != null)
        {
            foreach (var config in textConfigs)
            {
                if (config.type == TextType.GameModeName)
                {
                    UpdateText(config, SessionManager.CurrentGameMode.ModeName);
                }
            }
        }
    }

    private void UnregisterEventListeners()
    {
        SessionManager.OnScoreUpdate.RemoveAllListeners();
        SessionManager.OnBubbleLeftUpdate.RemoveAllListeners();
        SessionManager.OnTimeUpdate.RemoveAllListeners();
        SessionManager.OnWaveUpdate.RemoveAllListeners();
        SessionManager.OnGameStateChanged.RemoveListener(HandleGameStateChange);
        SessionManager.OnSessionStart.RemoveAllListeners();
        customEvents.Clear();
    }

    private void ClearActiveAnimations()
    {
        foreach (var animation in activeAnimations.Values)
        {
            animation.Stop();
        }
        activeAnimations.Clear();
    }

    private void HandleGameStateChange(GameState newState)
    {
        if (newState == GameState.WaitingToStart)
        {
            foreach (var config in textConfigs)
            {
                switch (config.type)
                {
                    case TextType.Score:
                    case TextType.Wave:
                        UpdateText(config, "0");
                        break;
                    case TextType.BubblesLeft:
                        UpdateText(config, "-");
                        break;
                    case TextType.Time:
                        if (SessionManager.CurrentGameMode != null)
                        {
                            float initialTime = SessionManager.CurrentGameMode.IsCountUp ? 0 : 
                                              SessionManager.CurrentGameMode.TargetTime;
                            UpdateText(config, FormatTime(initialTime, config));
                        }
                        break;
                    case TextType.GameModeName:
                        UpdateGameModeText();
                        break;
                }
            }
        }
    }

    private void UpdateText(TextConfig config, string value)
    {
        if (config.texts == null || config.texts.Length <= 0) return;

        string formattedText = $"{config.prefix}{value}{config.suffix}";

        foreach (TextMeshPro text in config.texts)
        {
            if (text == null) continue;
            
            text.text = formattedText;
            
            if (config.enableAnimation)
            {
                // Cancel any active animation for this config
                if (activeAnimations.TryGetValue(config, out var activeSequence))
                {
                    activeSequence.Complete();
                }

                // Create new animation
                var sequence = Sequence.Create()
                    .Group(Tween.PunchScale(
                        text.transform, 
                        strength: text.transform.localScale * config.animationStrength, 
                        duration: config.animationDuration, 
                        frequency: config.animationFrequency
                    ));
                
                activeAnimations[config] = sequence;
            }
        }
    }

    private string FormatTime(float time, TextConfig config)
    {
        if (time <= 0) return "0";
        
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        
        if (config.showDecimalPlaces)
        {
            float fraction = time % 1;
            string decimalPart = fraction.ToString($"F{config.decimalPlaces}").Substring(2);
            return time >= 60 
                ? $"{minutes}:{seconds:D2}.{decimalPart}" 
                : $"{seconds}.{decimalPart}";
        }
        
        return time >= 60 
            ? $"{minutes}:{seconds:D2}" 
            : seconds.ToString();
    }

    private string FormatNumber(int number, TextConfig config)
    {
        if (config.useThousandsSeparator)
        {
            return string.Format("{0:N0}", number);
        }
        return number.ToString();
    }

    public void UpdateCustomText(string eventName, string value)
    {
        if (customEvents.TryGetValue(eventName, out var action))
        {
            action.Invoke(value);
        }
    }
}