using UnityEngine;
using PrimeTween;
using UnityEngine.Serialization;
using VInspector;

[System.Serializable]
public class BackgroundEffect
{
    public bool enabled;
    [EnableIf("enabled")]
    public float durationOffset;
    public Color startColor = Color.clear;
    public Color endColor = Color.black;
    [EndIf]
        
    public (Color start, Color end, float time) GetValues(float baseDuration) => enabled ? (startColor, endColor, baseDuration + durationOffset) : (Color.clear, Color.clear, baseDuration);
}

[System.Serializable]
public class IconEffect
{
    public bool enabled;
    [EnableIf("enabled")]
    public float durationOffset;
    public Sprite sprite;
    public Color startColor = Color.clear;
    public Color endColor = Color.white;
    public float startScale;
    public float endScale;
    public float startRotation;
    public float endRotation;
    [EndIf]

    public (Color colorStart, Color colorEnd, float scaleStart, float scaleEnd, float rotStart, float rotEnd, float time) GetValues(float baseDuration) => 
        enabled ? (startColor, endColor, startScale, endScale, startRotation, endRotation, baseDuration + durationOffset) 
            : (Color.clear, Color.clear, 0, 0, 0, 0, baseDuration);
}

[System.Serializable]
public class LensDistortEffect
{
    public bool enabled;
    [EnableIf("enabled")]
    public float durationOffset;
    [EndIf]
    
    public (bool distort, float time) GetValues(float baseDuration) => enabled ? (true, baseDuration + durationOffset) : (false, baseDuration);
}

[System.Serializable]
public class ChromaticAberrationEffect
{
    public bool enabled;
    [EnableIf("enabled")]
    public float durationOffset;
    [EndIf]
    
    public (bool aberrate, float time) GetValues(float baseDuration) => enabled ? (true, baseDuration + durationOffset) : (false, baseDuration);
}

[System.Serializable]
public class VignetteEffect
{
    public bool enabled;
    [EnableIf("enabled")]
    public float durationOffset;
    [EndIf]
    
    public (bool vignette, float time) GetValues(float baseDuration) => enabled ? (true, baseDuration + durationOffset) : (false , baseDuration);
}

[System.Serializable]
public class ColorAdjustEffect
{
    public bool enabled;
    [EnableIf("enabled")]
    public float durationOffset;
    public Color startColor = Color.white;
    public Color endColor = Color.black;
    [EndIf]
    
    public (Color start, Color end, float time) GetValues(float baseDuration) => enabled ? (startColor, endColor, baseDuration + durationOffset) : (Color.white, Color.white, baseDuration);
}

public class TransitionSetup
{
    // UI Effect properties
    public Color BgStartColor;
    public Color BgEndColor;
    public float BgDuration;
    public Sprite IconSprite;
    public Color IconStartColor;
    public Color IconEndColor;
    public float IconStartScale;
    public float IconEndScale;
    public float IconStartRotation;
    public float IconEndRotation;
    public float IconDuration;
    
    // Post-process properties
    public float LensDistortDuration;
    public float ChromaticAberrationDuration;
    public float VignetteDuration;
    public Color ColorAdjustStartColor;
    public Color ColorAdjustEndColor;
    public float ColorAdjustDuration;
}

[CreateAssetMenu(fileName = "Transition Full", menuName = "SO Transitions/Full")]
public class SOTransitionEffectFull : SOTransitionEffectBase
{

    [Header("UI Effects")]
    [SerializeField] protected BackgroundEffect backgroundEffect = new();
    [SerializeField] protected IconEffect iconEffect = new();
    

    [Header("Post-Process Effects")]
    [SerializeField] protected LensDistortEffect lensDistortEffect = new();
    [SerializeField] protected ChromaticAberrationEffect chromaticAberrationEffect = new();
    [SerializeField] protected VignetteEffect vignetteEffect = new();
    [SerializeField] protected ColorAdjustEffect colorAdjustEffect = new();

    private TransitionSetup Setup()
    {
        var bgValues = backgroundEffect.GetValues(duration);
        var iconValues = iconEffect.GetValues(duration);
        var lensDistortValues = lensDistortEffect.GetValues(duration);
        var chromaticAberrationValues = chromaticAberrationEffect.GetValues(duration);
        var vignetteValues = vignetteEffect.GetValues(duration);
        var colorAdjustValues = colorAdjustEffect.GetValues(duration);

        return new TransitionSetup
        {
            BgStartColor = bgValues.start,
            BgEndColor = bgValues.end,
            BgDuration = bgValues.time,
            IconSprite = iconEffect.enabled ? iconEffect.sprite : null,
            IconStartColor = iconValues.colorStart,
            IconEndColor = iconValues.colorEnd,
            IconStartScale = iconValues.scaleStart,
            IconEndScale = iconValues.scaleEnd,
            IconStartRotation = iconValues.rotStart,
            IconEndRotation = iconValues.rotEnd,
            IconDuration = iconValues.time,
            
            LensDistortDuration = lensDistortValues.time,
            ChromaticAberrationDuration = chromaticAberrationValues.time,
            VignetteDuration = vignetteValues.time,
            ColorAdjustStartColor = colorAdjustValues.start,
            ColorAdjustEndColor = colorAdjustValues.end,
            ColorAdjustDuration = colorAdjustValues.time
        };
    }

    public override Sequence OutTransition()
    {
        var setup = Setup();
        
        State = TransitionState.PlayingOut;
        var sequence = Sequence.Create(cycles, cycleMode, easeType, useUnscaledTime, useFixedUpdate);

        if (backgroundEffect.enabled)
        {
            sequence.Group(EffectFullScreenColor(setup.BgStartColor, setup.BgEndColor, setup.BgDuration));
        }

        if (iconEffect.enabled)
        {
            sequence.Group(EffectIconColor(setup.IconSprite, setup.IconStartColor, setup.IconEndColor, setup.IconDuration));
            sequence.Group(EffectIconScale(setup.IconSprite, setup.IconStartScale, setup.IconEndScale, setup.IconDuration));
            sequence.Group(EffectIconRotate(setup.IconSprite, setup.IconStartRotation, setup.IconEndRotation, setup.IconDuration));
        }

        if (lensDistortEffect.enabled)
        {
            sequence.Group(EffectDistortLens(true, setup.LensDistortDuration));
        }

        if (chromaticAberrationEffect.enabled)
        {
            sequence.Group(EffectChromaticAberrate(true, setup.ChromaticAberrationDuration));
        }

        if (vignetteEffect.enabled)
        {
            sequence.Group(EffectFadeVignette(true, setup.VignetteDuration));
        }

        if (colorAdjustEffect.enabled)
        {
            sequence.Group(EffectColorAdjust(setup.ColorAdjustStartColor, setup.ColorAdjustEndColor, setup.ColorAdjustDuration));
        }

        return sequence;
    }

    public override Sequence InTransition()
    {
        var setup = Setup();
        
        State = TransitionState.PlayingIn;
        var sequence = Sequence.Create(cycles, cycleMode, easeType, useUnscaledTime, useFixedUpdate);

        if (iconEffect.enabled)
        {
            sequence.Group(EffectIconScale(setup.IconSprite, setup.IconEndScale, setup.IconStartScale, setup.IconDuration));
            sequence.Group(EffectIconColor(setup.IconSprite, setup.IconEndColor, setup.IconStartColor, setup.IconDuration));
            sequence.Group(EffectIconRotate(setup.IconSprite, setup.IconEndRotation, -setup.IconEndRotation, setup.IconDuration));
        }

        if (lensDistortEffect.enabled)
        {
            sequence.Group(EffectDistortLens(false, setup.LensDistortDuration));
        }

        if (chromaticAberrationEffect.enabled)
        {
            sequence.Group(EffectChromaticAberrate(false, setup.ChromaticAberrationDuration));
        }

        if (vignetteEffect.enabled)
        {
            sequence.Group(EffectFadeVignette(false, setup.VignetteDuration));
        }

        if (colorAdjustEffect.enabled)
        {
            sequence.Group(EffectColorAdjust(setup.ColorAdjustEndColor, setup.ColorAdjustStartColor, setup.ColorAdjustDuration));
        }

        if (backgroundEffect.enabled)
        {
            sequence.Chain(EffectFullScreenColor(setup.BgEndColor, setup.BgStartColor, setup.BgDuration));
        }

        return sequence.ChainCallback(() => State = TransitionState.NotPlaying);
    }

    public override Sequence StartTransition()
    {
        var setup = Setup();
        
        State = TransitionState.PlayingStart;
        var sequence = Sequence.Create(cycles, cycleMode, easeType, useUnscaledTime, useFixedUpdate);

        if (backgroundEffect.enabled)
        {
            sequence.Group(EffectFullScreenColor(setup.BgEndColor, setup.BgStartColor, setup.BgDuration + startTransitionDurationOffset));
        }

        if (lensDistortEffect.enabled)
        {
            sequence.Group(EffectDistortLens(false, setup.LensDistortDuration + startTransitionDurationOffset));
        }

        if (chromaticAberrationEffect.enabled)
        {
            sequence.Group(EffectChromaticAberrate(false, setup.ChromaticAberrationDuration + startTransitionDurationOffset));
        }

        if (vignetteEffect.enabled)
        {
            sequence.Group(EffectFadeVignette(false, setup.VignetteDuration + startTransitionDurationOffset));
        }

        if (colorAdjustEffect.enabled)
        {
            sequence.Group(EffectColorAdjust(setup.ColorAdjustEndColor, setup.ColorAdjustStartColor, setup.ColorAdjustDuration + startTransitionDurationOffset));
        }

        return sequence.ChainCallback(() => State = TransitionState.NotPlaying);
    }
}