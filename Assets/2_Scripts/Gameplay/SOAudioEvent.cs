using UnityEngine;
using UnityEditor;
using CustomAttribute;
using UnityEngine.Audio;


[CreateAssetMenu(fileName = "AudioEvent", menuName = "SO Audio/Audio Event")]
public class SOAudioEvent : ScriptableObject
{
    public AudioClip[] clips;
    public AudioMixerGroup mixerGroup;
    [MinMaxRange(0f, 1f)] public RangedFloat volume;
    [MinMaxRange(-3f, 3f)] public RangedFloat pitch;
    [Range(-1f, 1f)] public float stereoPan;
    [Range(0f, 1f)] public float spatialBlend; 
    [Range(0f, 1.1f)] public float reverbZoneMix = 1f;
    public bool bypassEffects;
    public bool bypassListenerEffects;
    public bool bypassReverbZones;
    public bool loop;


    public void Play(AudioSource source)
    {
        if (clips.Length == 0) // Make sure there are clips
        {
            #if UNITY_EDITOR
            // Debug.Log("No clips found");
            #endif
            return;
        }
        
        // Set settings to audio source and play
        SetAudioSourceSettings(source);
        source.Play();
    }
    
    public void PlayUsingDummyAs()
    {
        if (clips.Length == 0) // Make sure there are clips
        {
            #if UNITY_EDITOR
            // Debug.Log("No clips found");
            #endif
            return;
        }
        
        AudioSource source = new GameObject("DummyAudioSource").AddComponent<AudioSource>();

        // Set settings to audio source and play
        SetAudioSourceSettings(source);
        source.Play();
        Destroy(source.gameObject, source.clip.length);
    }

    public void PlayDelayed(AudioSource source, float delay)
    {
        if (clips.Length == 0) // Make sure there are clips
        {
            #if UNITY_EDITOR
            Debug.Log("No clips found");
            #endif
            return;
        }
        
        SetAudioSourceSettings(source);
        source.PlayDelayed(delay);
    }
    
    public void Stop(AudioSource source)
    {
        source.Stop();
    }

    public void Pause(AudioSource source)
    {
        source.Pause();
    }

    public void Continue(AudioSource source)
    {
        source.UnPause();
    }

    public void SetAudioSourceSettings(AudioSource source)
    {
        source.clip = clips[Random.Range(0, clips.Length)];
        source.outputAudioMixerGroup = mixerGroup;
        source.volume = Random.Range(volume.minValue, volume.maxValue);
        source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
        source.panStereo = stereoPan;
        source.spatialBlend = spatialBlend;
        source.reverbZoneMix = reverbZoneMix;
        source.bypassEffects = bypassEffects;
        source.bypassListenerEffects = bypassListenerEffects;
        source.bypassReverbZones = bypassReverbZones;
        source.loop = loop;
    }
    
    

    
    
}


#if UNITY_EDITOR
// Preview button
[CustomEditor(typeof(SOAudioEvent), true)]
public class AudioEventEditor : Editor
{

    [SerializeField] private AudioSource previewer;

    public void OnEnable()
    {
        previewer = EditorUtility
            .CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource))
            .GetComponent<AudioSource>();
    }

    public void OnDisable()
    {
        DestroyImmediate(previewer.gameObject);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
        if (GUILayout.Button("Preview Sound"))
        {
            ((SOAudioEvent)target).Play(previewer);
        }
        
        if (GUILayout.Button("Stop Sound"))
        {
            ((SOAudioEvent)target).Stop(previewer);
        }

        EditorGUI.EndDisabledGroup();
    }
}
#endif