using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

namespace TK.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer Routing")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SFXVolumeKey = "SFXVolume";

        private float masterVolume = 1f;
        private float musicVolume = 1f;
        private float sfxVolume = 1f;

        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip resultMusic;

        [Header("SFX System")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private List<SFXData> sfxDataList = new List<SFXData>();
        [SerializeField] private int poolSize = 8;

        private List<AudioSource> sfxPool = new List<AudioSource>();
        private Dictionary<SFXId, SFXData> sfxDictionary = new Dictionary<SFXId, SFXData>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSFXPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSFXPool()
        {
            sfxDictionary.Clear();
            foreach (var data in sfxDataList)
            {
                if (data != null && !sfxDictionary.ContainsKey(data.Id))
                {
                    sfxDictionary.Add(data.Id, data);
                }
            }

            for (int i = 0; i < poolSize; i++)
            {
                AudioSource src;
                if (i == 0 && sfxSource != null)
                {
                    src = sfxSource;
                }
                else
                {
                    GameObject go = new GameObject($"SFX_Source_{i}");
                    go.transform.SetParent(transform);
                    src = go.AddComponent<AudioSource>();
                    if (sfxSource != null)
                    {
                        CopyAudioSourceSettings(sfxSource, src);
                    }
                }
                src.playOnAwake = false;
                src.loop = false;
                sfxPool.Add(src);
            }
        }

        private void CopyAudioSourceSettings(AudioSource source, AudioSource target)
        {
            target.outputAudioMixerGroup = source.outputAudioMixerGroup;
            target.mute = source.mute;
            target.bypassEffects = source.bypassEffects;
            target.bypassListenerEffects = source.bypassListenerEffects;
            target.bypassReverbZones = source.bypassReverbZones;
            target.priority = source.priority;
            target.volume = source.volume;
            target.pitch = source.pitch;
            target.panStereo = source.panStereo;
            target.spatialBlend = source.spatialBlend;
            target.reverbZoneMix = source.reverbZoneMix;
            target.dopplerLevel = source.dopplerLevel;
            target.spread = source.spread;
            target.rolloffMode = source.rolloffMode;
            target.minDistance = source.minDistance;
            target.maxDistance = source.maxDistance;
        }

        public void PlayMenuMusic() => PlayMusic(menuMusic);
        public void PlayGameplayMusic() => PlayMusic(gameplayMusic);
        public void PlayResultMusic() => PlayMusic(resultMusic);

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();

        public void PlaySFX(SFXId id)
        {
            if (!sfxDictionary.TryGetValue(id, out var data) || data == null)
            {
                Debug.LogWarning($"SFX with ID {id} not found or SFXData is null!");
                return;
            }

            AudioResource resource = data.AudioResource;
            if (resource == null)
            {
                Debug.LogWarning($"SFXData for ID {id} has no AudioResource assigned!");
                return;
            }

            AudioSource source = GetAvailableAudioSource();
            if (source == null)
            {
                Debug.LogWarning("No available AudioSource in pool to play SFX!");
                return;
            }

            source.resource = resource;
            source.Play();
            StartCoroutine(IEReturnToPool(source));
        }

        private AudioSource GetAvailableAudioSource()
        {
            foreach (var src in sfxPool)
            {
                if (!src.isPlaying)
                {
                    return src;
                }
            }

            if (sfxPool.Count > 0)
            {
                AudioSource busySource = sfxPool[0];
                busySource.Stop();
                return busySource;
            }

            return null;
        }

        private IEnumerator IEReturnToPool(AudioSource source)
        {
            yield return new WaitForSeconds(0.05f);
            while (source.isPlaying)
            {
                yield return null;
            }
            source.resource = null;
        }

        public void PlayGrab() => PlaySFX(SFXId.Grab);
        public void PlayEatFood() => PlaySFX(SFXId.EatFood);
        public void PlayEatTrash() => PlaySFX(SFXId.EatTrash);
        public void PlayRating() => PlaySFX(SFXId.Rating);
        public void PlayButtonClick() => PlaySFX(SFXId.ButtonClick);
        public void PlayPanelOpen() => PlaySFX(SFXId.PanelOpen);
        public void PlayPanelClose() => PlaySFX(SFXId.PanelClose);
        public void PlayPageTurn() => PlaySFX(SFXId.PageTurn);
        public void PlayBurp() => PlaySFX(SFXId.Burp);

        private void Start()
        {
            LoadVolumeSettings();
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

            ApplyVolume(masterVolumeParam, masterVolume);
            ApplyVolume(musicVolumeParam, musicVolume);
            ApplyVolume(sfxVolumeParam, sfxVolume);

            if (musicSource != null) musicSource.mute = (musicVolume <= 0.01f);
            if (sfxPool != null)
            {
                foreach (var src in sfxPool)
                {
                    if (src != null) src.mute = (sfxVolume <= 0.01f);
                }
            }
        }

        private void ApplyVolume(string parameterName, float linearVolume)
        {
            if (audioMixer == null) return;
            float db = Mathf.Log10(Mathf.Max(linearVolume, 0.0001f)) * 20f;
            audioMixer.SetFloat(parameterName, db);
        }

        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;

        public void SetMasterVolume(float linearVolume)
        {
            masterVolume = Mathf.Clamp01(linearVolume);
            ApplyVolume(masterVolumeParam, masterVolume);
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        }

        public void SetMusicVolume(float linearVolume)
        {
            musicVolume = Mathf.Clamp01(linearVolume);
            ApplyVolume(musicVolumeParam, musicVolume);
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            
            if (musicSource != null) musicSource.mute = (musicVolume <= 0.01f);
        }

        public void SetSFXVolume(float linearVolume)
        {
            sfxVolume = Mathf.Clamp01(linearVolume);
            ApplyVolume(sfxVolumeParam, sfxVolume);
            PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolume);

            if (sfxPool != null)
            {
                foreach (var src in sfxPool)
                {
                    if (src != null) src.mute = (sfxVolume <= 0.01f);
                }
            }
        }
    }
}