using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using PrimeTween;
using VInspector;

public enum TransitionState
{
    PlayingIn,
    PlayingOut,
    PlayingStart,
    NotPlaying,
}

public abstract class SOTransitionEffectBase : ScriptableObject
{
    [Header("Transition")]
    [SerializeField] protected float duration = 3f;
    [SerializeField] protected int cycles = 1;
    [SerializeField] protected CycleMode cycleMode = CycleMode.Restart;
    [SerializeField] protected bool useUnscaledTime = true;
    [SerializeField] protected bool useFixedUpdate = false;
    [SerializeField] protected Ease easeType = Ease.InOutSine;
    [SerializeField] protected float startTransitionDurationOffset = 2f;
    
    public TransitionState State { get; protected set; }
    
    protected static Volume PostProcessingVolume;
    protected static Image IconImage;
    protected static Image FullScreenImage;
    protected static Mask Mask;
    protected static Image MaskImage;
    protected static ChromaticAberration ChromaticAberration;
    protected static LensDistortion LensDistortion;
    protected static Vignette Vignette;
    protected static ColorAdjustments ColorAdjustments;
    
    public static void InitializeComponents(Volume volume, Image icon, Image fullScreen, Mask maskComponent, Image maskImg)
    {
        PostProcessingVolume = volume;
        IconImage = icon;
        FullScreenImage = fullScreen;
        Mask = maskComponent;
        MaskImage = maskImg;

        if (PostProcessingVolume)
        {
            PostProcessingVolume.profile.TryGet(out ChromaticAberration);
            PostProcessingVolume.profile.TryGet(out LensDistortion);
            PostProcessingVolume.profile.TryGet(out Vignette);
            PostProcessingVolume.profile.TryGet(out ColorAdjustments);
        }
    }


    public abstract Sequence OutTransition();
    public abstract Sequence InTransition();
    public abstract Sequence StartTransition();
    
    

#region UI Effects // ------------------------------------------------------------------------------

    protected Tween EffectIconColor(Sprite image, Color startColor, Color endColor, float time = 1, Ease ease = Ease.InOutSine)
    {
        if (!IconImage) return Tween.Delay(0);
        
        IconImage.sprite = image;
        
        return Tween.Custom(startColor, endColor, time, ease: ease,
            onValueChange: val => IconImage.color = val);
    }

    protected Tween EffectIconAlpha(Sprite image, float startAlpha, float endAlpha, float time = 1, Ease ease = Ease.InOutSine)
    {
        if (!IconImage) return Tween.Delay(0);
        
        IconImage.sprite = image;
        
        return Tween.Custom(startAlpha, endAlpha, time, ease: ease,
            onValueChange: val => IconImage.color = new Color(IconImage.color.r, IconImage.color.g, IconImage.color.b, val));
    }

    protected Tween EffectIconScale(Sprite image, float startScale, float endScale, float time = 1, Ease ease = Ease.InOutSine)
    {
        if (!IconImage) return Tween.Delay(0);
        
        IconImage.sprite = image;
        
        return Tween.Custom(Vector3.one * startScale, Vector3.one * endScale, time, ease: ease,
            onValueChange: val => IconImage.transform.localScale = val);
    }

    protected Tween EffectIconRotate(Sprite image, float startDegrees, float endDegrees, float time = 1, Ease ease = Ease.InOutSine)
    {
        if (!IconImage) return Tween.Delay(0);
        
        IconImage.sprite = image;
        
        return Tween.Custom(startDegrees, endDegrees, time, ease: ease,
            onValueChange: val => IconImage.transform.rotation = Quaternion.Euler(0, 0, val));
    }

    protected Tween EffectFullScreenColor(Color startColor, Color endColor, float time = 1, Ease ease = Ease.InOutSine)
    {
        if (!FullScreenImage) return Tween.Delay(0);
        
        return Tween.Custom(startColor, endColor, time, ease: ease,
            onValueChange: val => FullScreenImage.color = val);
    }

    protected Tween EffectFullScreenAlpha(float startAlpha, float endAlpha, float time = 1, Ease ease = Ease.InOutSine)
    {
        if (!FullScreenImage) return Tween.Delay(0);
        
        return Tween.Custom(startAlpha, endAlpha, time, ease: ease,
            onValueChange: val => FullScreenImage.color = new Color(FullScreenImage.color.r, FullScreenImage.color.g, FullScreenImage.color.b, val));
    }


#endregion UI Effects UI Effects // ------------------------------------------------------------------------------
    
    
#region Post Process Effects UI Effects // ------------------------------------------------------------------------------

    protected Tween EffectFadeVignette(bool fadeIn = true, float time = 1)
    {
        if (!PostProcessingVolume || !Vignette) return Tween.Delay(0);
        
        Vignette.active = true;
        Vignette.intensity.overrideState = true;
        float startIntensity = fadeIn ? 0 : 1;
        float endIntensity = fadeIn ? 1 : 0;
       
        return Tween.Custom(startIntensity, endIntensity, time,
            onValueChange: val => Vignette.intensity.value = val);
    }

    protected Tween EffectChromaticAberrate(bool fadeIn = true, float time = 1)
    {
        if (!PostProcessingVolume || !ChromaticAberration) return Tween.Delay(0);
        
        ChromaticAberration.active = true;
        ChromaticAberration.intensity.overrideState = true;
        float startIntensity = fadeIn ? 0 : 1;
        float endIntensity = fadeIn ? 1 : 0;
       
        return Tween.Custom(startIntensity, endIntensity, time,
            onValueChange: val => ChromaticAberration.intensity.value = val);
    }

    protected Tween EffectDistortLens(bool fadeIn = true, float time = 1)
    {
        if (!PostProcessingVolume || !LensDistortion) return Tween.Delay(0);
        
        LensDistortion.active = true;
        LensDistortion.intensity.overrideState = true;
        float startIntensity = fadeIn ? 0 : 1;
        float endIntensity = fadeIn ? 1 : 0;
       
        return Tween.Custom(startIntensity, endIntensity, time,
            onValueChange: val => LensDistortion.intensity.value = val);
    }

    protected Tween EffectColorAdjust(Color startColor, Color endColor, float time = 1, Ease ease = Ease.InOutSine)
    {
        if (!PostProcessingVolume || !ColorAdjustments) return Tween.Delay(0);
        
        ColorAdjustments.active = true;
        ColorAdjustments.colorFilter.overrideState = true;

        return Tween.Custom(startColor, endColor, time, ease: ease,
            onValueChange: val => ColorAdjustments.colorFilter.value = val);
    }

#endregion Post Process Effects UI Effects // ------------------------------------------------------------------------------


#region Editor Only  // ------------------------------------------------------------------------------
#if UNITY_EDITOR
    
    [Button]
    public void PlayTransition()
    {
        TransitionManager.PlayTransition(this, TransitionManager.ActiveSceneName(), true);
    }
    
#endif
#endregion Editor Only  // ------------------------------------------------------------------------------
    


    
    
}