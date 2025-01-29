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
        Deaths,
        WaveTimer,
        WaveScore,
        Custom
    }
    
    [SerializeField] private List<TextConfig> textConfigs = new List<TextConfig>();
    private Dictionary<string, Action<string>> customEvents = new Dictionary<string, Action<string>>();
    private Dictionary<TextConfig, Sequence> activeAnimations = new Dictionary<TextConfig, Sequence>();

    private void Awake()
    {
        GameManager.OnGameStateChanged.AddListener(HandleGameStateChange);
    }

    private void OnEnable()
    {
        RegisterEventListeners();
        UpdateGameModeText();
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
                GameManager.OnScoreUpdate.AddListener((score) => 
                    UpdateText(config, FormatNumber(score, config)));
                break;
                
            case TextType.BubblesLeft:
                GameManager.OnBubbleLeftUpdate.AddListener((bubbles) => 
                    UpdateText(config, FormatNumber(bubbles, config)));
                break;
                
            case TextType.Time:
                GameManager.OnTimeUpdate.AddListener((time) =>
                {
                    UpdateText(config, FormatTime(time, config));
                    // Update color separately for timer texts
                    if (GameManager.CurrentGameMode != null && 
                        GameManager.CurrentGameMode.TimerMode != TimerMode.Off)
                    {
                        foreach (TextMeshPro text in config.texts)
                        {
                            if (text != null)
                            {
                                text.color = GameManager.CurrentGameMode.GetTimerColor(time);
                            }
                        }
                    }
                });
                break;
                
            case TextType.Wave:
                GameManager.OnWaveUpdate.AddListener((wave) => 
                    UpdateText(config, FormatNumber(wave, config)));
                break;
                
            case TextType.GameModeName:
                GameManager.OnGameModeChanged.AddListener((gameMode) => 
                    UpdateText(config, gameMode.ModeName));
                break;

            case TextType.Deaths:
                GameManager.OnDeathUpdate.AddListener((deaths) =>
                    UpdateText(config, FormatNumber(deaths, config)));
                break;

            case TextType.WaveTimer:
                GameManager.OnWaveTimerUpdate.AddListener((timer) =>
                {
                    UpdateText(config, FormatTime(timer, config));
                    // Update color separately for timer texts
                    if (GameManager.CurrentGameMode != null && 
                        GameManager.CurrentGameMode.TimerMode != TimerMode.Off)
                    {
                        foreach (TextMeshPro text in config.texts)
                        {
                            if (text != null)
                            {
                                text.color = GameManager.CurrentGameMode.GetTimerColor(timer);
                            }
                        }
                    }
                });
                break;

            case TextType.WaveScore:
                GameManager.OnWaveScoreUpdate.AddListener((waveScore) =>
                    UpdateText(config, FormatNumber(waveScore, config)));
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
        if (GameManager.CurrentGameMode != null)
        {
            foreach (var config in textConfigs)
            {
                if (config.type == TextType.GameModeName)
                {
                    UpdateText(config, GameManager.CurrentGameMode.ModeName);
                }
            }
        }
    }

    private void UnregisterEventListeners()
    {
        GameManager.OnScoreUpdate.RemoveAllListeners();
        GameManager.OnBubbleLeftUpdate.RemoveAllListeners();
        GameManager.OnTimeUpdate.RemoveAllListeners();
        GameManager.OnWaveUpdate.RemoveAllListeners();
        GameManager.OnGameStateChanged.RemoveListener(HandleGameStateChange);
        GameManager.OnGameModeChanged.RemoveAllListeners();
        GameManager.OnDeathUpdate.RemoveAllListeners();
        GameManager.OnWaveTimerUpdate.RemoveAllListeners();
        GameManager.OnWaveScoreUpdate.RemoveAllListeners();
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

    private void HandleGameStateChange()
    {
        if (GameManager.CurrentGameState == GameState.WaitingToStart)
        {
            foreach (var config in textConfigs)
            {
                switch (config.type)
                {
                    case TextType.Score:
                    case TextType.Wave:
                    case TextType.Deaths:
                    case TextType.WaveScore:
                        UpdateText(config, "0");
                        break;
                    case TextType.BubblesLeft:
                        UpdateText(config, "-");
                        break;
                    case TextType.Time:
                    case TextType.WaveTimer:
                        if (GameManager.CurrentGameMode != null)
                        {
                            float initialTime = config.type == TextType.Time ?
                                (GameManager.CurrentGameMode.TimerMode == TimerMode.CountUp ? 0 : GameManager.CurrentGameMode.TargetTime) :
                                GameManager.CurrentGameMode.TimeToCompleteWave;
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
        
            // Update timer color if this is a timer text
            if ((config.type == TextType.Time || config.type == TextType.WaveTimer) && 
                GameManager.CurrentGameMode != null && 
                GameManager.CurrentGameMode.TimerMode != TimerMode.Off)
            {
                float currentTime = config.type == TextType.Time ? 
                    GameManager.CurrentTime : 
                    GameManager.CurrentWaveTimer;
                
                text.color = GameManager.CurrentGameMode.GetTimerColor(currentTime);
            }
        
            if (config.enableAnimation)
            {
                if (activeAnimations.TryGetValue(config, out var activeSequence))
                {
                    activeSequence.Complete();
                }

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var config in textConfigs)
        {
            if (config.texts == null || config.texts.Length == 0) continue;

            string placeholderText = GetPlaceholderText(config.type);
            string formattedText = $"{config.prefix}{placeholderText}{config.suffix}";

            foreach (var text in config.texts)
            {
                if (text != null)
                {
                    text.text = formattedText;
                }
            }
        }
    }

    private string GetPlaceholderText(TextType type)
    {
        return type switch
        {
            TextType.Score => "0",
            TextType.BubblesLeft => "-",
            TextType.Time => "0:00",
            TextType.Wave => "0",
            TextType.GameModeName => "Game Mode",
            TextType.Deaths => "0",
            TextType.WaveTimer => "0:00",
            TextType.WaveScore => "0",
            TextType.Custom => "Custom Text",
            _ => "Text"
        };
    }
#endif
}