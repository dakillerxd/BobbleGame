using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using PrimeTween;
using VInspector;
using Random = UnityEngine.Random;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Volume _postProcessingVolume;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _fullScreenImage;
    
    [Header("Transitions")]
    [SerializeField] private SOTransitionEffectBase defaultTransition;
    [SerializeField] private SOTransitionEffectBase[] transitionEffects;
    
    private SOTransitionEffectBase _currentSoTransition;
    private bool _pendingTransition;
    private static Sequence activeSequence;
    private static Sequence outSequence;
    private static string sceneToLoad;
    
    private void Awake()
    {
        if (Instance && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupComponents();
        Tween.Delay(1f, PlayStartTransition, useUnscaledTime: true);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        SetupComponents();
        PlayInTransition();
    }
    

#region Setup // ------------------------------------------------------------------------------------

     private void SetupComponents()
    {
        if (!_postProcessingVolume)
        {
            _postProcessingVolume = FindAnyObjectByType<Volume>();
            if (!_postProcessingVolume)
            {
                Debug.LogWarning("No Post Processing Volume found in the scene!");
            }
        }
        
        if (!_iconImage)
        {
            _iconImage = transform.Find("IconImage").GetComponent<Image>();
            if (!_iconImage)
            {
                Debug.LogWarning("No Icon Image found in children!");
            }
        }
        
        if (!_fullScreenImage)
        {
            _fullScreenImage = transform.Find("FullScreenImage").GetComponent<Image>();
            if (!_fullScreenImage)
            {
                Debug.LogWarning("No Full Screen Image found in children!");
            }
        }
        
        Mask maskComponent = null;
        Image maskImg = null;
        
        var maskTransform = transform.Find("Mask");
        if (maskTransform)
        {
            maskComponent = maskTransform.GetComponent<Mask>();
            maskImg = maskTransform.GetComponent<Image>();
        }
        
        if (!maskComponent) Debug.LogWarning("No Mask found in children");
        if (!maskImg) Debug.LogWarning("No Mask Image found in children");

        // Initialize components in SOTransitionEffectBase
        SOTransitionEffectBase.InitializeComponents(
            _postProcessingVolume,
            _iconImage,
            _fullScreenImage,
            maskComponent,
            maskImg
        );
    }

    private void ResetComponents()
    {
        if (_iconImage)
        {
            _iconImage.sprite = null;
            _iconImage.color = new Color(1, 1, 1, 0);
        }

        if (_fullScreenImage)
        {
            _fullScreenImage.sprite = null;
            _fullScreenImage.color = new Color(1, 1, 1, 0);
        }
        
        var maskComponent = transform.Find("Mask")?.GetComponent<Mask>();
        if (maskComponent)
        {
            maskComponent.showMaskGraphic = false;
        }
        
        var maskImage = transform.Find("Mask")?.GetComponent<Image>();
        if (maskImage)
        {
            maskImage.sprite = null;
            maskImage.color = new Color(1, 1, 1, 0);
        }
    }
    
    public static string ActiveSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
            
    private static void LoadScene(string sceneName, float delay = 0.01f)
    {
        sceneToLoad = sceneName;
        Tween.Delay(delay, () => SceneManager.LoadScene(sceneToLoad), useUnscaledTime: true);
    }


#endregion Setup // ------------------------------------------------------------------------------------


#region Transitions // ----------------------------------------------------------------------------

 private void PlayInTransition()
    {
        if (_pendingTransition && _currentSoTransition)
        {
            Sequence.Create(useUnscaledTime: true)
                .Group(_currentSoTransition.InTransition())
                .OnComplete(ResetComponents);
            _pendingTransition = false;
            _currentSoTransition = null;
        }
    }

    private void PlayStartTransition()
    {
        if (defaultTransition)
        {
            _pendingTransition = false; // Ensure we don't play transition in
            Sequence.Create(useUnscaledTime: true)
                .Group(defaultTransition.StartTransition())
                .OnComplete(ResetComponents);
        }
    }
    
    public static void PlayTransition(SOTransitionEffectBase soTransition, string sceneName, bool transitionBackIn = true)
    {
        if (!Instance || !soTransition) return;
        
        activeSequence.Stop();
        outSequence.Stop();

        Instance._currentSoTransition = soTransition;
        
        Sequence.Create(useUnscaledTime: true)
            .Group(soTransition.OutTransition())
            .ChainCallback(() => {
                Instance._pendingTransition = transitionBackIn;
                LoadScene(sceneName);
            });
    }
    
    public static void PlayTransition(string sceneName, bool transitionBackIn = true)
    {
        if (!Instance || !Instance.defaultTransition) return;
        PlayTransition(Instance.defaultTransition, sceneName, transitionBackIn);
    }
    
    public static void PlayTransition(string transitionName, string sceneName, bool transitionBackIn = true)
    {
        if (!Instance || Instance.transitionEffects == null || Instance.transitionEffects.Length == 0) 
            return;

        SOTransitionEffectBase transition = Array.Find(Instance.transitionEffects, t => t && t.name == transitionName);

        if (transition)
        {
            PlayTransition(transition, sceneName, transitionBackIn);
        }
        else
        {
            Debug.LogWarning($"Transition '{transitionName}' not found in transition effects list!");
        }
    }
    
    public static void PlayRandomTransition(string sceneName, bool transitionBackIn = true)
    {
        if (!Instance || Instance.transitionEffects.Length == 0) return;
        PlayTransition(Instance.transitionEffects[Random.Range(0, Instance.transitionEffects.Length)], sceneName, transitionBackIn);
    }

    public static bool IsTransitionPlaying()
    {
        return Instance._currentSoTransition.State != TransitionState.NotPlaying;
    }
    

#endregion Transitions // ----------------------------------------------------------------------------
   
}