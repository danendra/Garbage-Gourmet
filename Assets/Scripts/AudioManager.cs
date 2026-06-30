using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip resultMusic;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip grabSFX;
    [SerializeField] private AudioClip eatFoodSFX;
    [SerializeField] private AudioClip eatTrashSFX;
    [SerializeField] private AudioClip ratingSFX;
    [SerializeField] private AudioClip buttonClickSFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // MUSIC
    // =====================================================

    public void PlayMenuMusic()    => PlayMusic(menuMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);
    public void PlayResultMusic()  => PlayMusic(resultMusic);

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    // =====================================================
    // SFX
    // =====================================================

    public void PlayGrab()        => sfxSource.PlayOneShot(grabSFX);
    public void PlayEatFood()     => sfxSource.PlayOneShot(eatFoodSFX);
    public void PlayEatTrash()    => sfxSource.PlayOneShot(eatTrashSFX);
    public void PlayRating()      => sfxSource.PlayOneShot(ratingSFX);
    public void PlayButtonClick() => sfxSource.PlayOneShot(buttonClickSFX);
}