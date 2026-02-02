using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fontes de Áudio")]
    public AudioSource musicaSource; 
    public AudioSource sfxSource;  

    [Header("Clipes de Áudio - Arraste aqui")]
    public AudioClip menuAmbience;  
    public AudioClip voiceOnYourMarks;
    public AudioClip voiceSet;
    public AudioClip gunShot;
    public AudioClip resultsCheer;  
    [Header("Torcida por Prova ")]
    public AudioClip crowd100m;  
    public AudioClip crowd200m;
    public AudioClip crowd400m;
    public AudioClip crowd800m;  
    public AudioClip crowd1500m;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        TocarMenu();
    }
    public void TocarMenu()
    {
        sfxSource.Stop();
        if (musicaSource.clip != menuAmbience)
        {
            musicaSource.clip = menuAmbience;
            musicaSource.loop = true;
            musicaSource.volume = 0.5f; 
            musicaSource.Play();
        }
    }

    public void SilenciarMusica()
    {
        musicaSource.Stop(); 
    }

    public void TocarSFX(AudioClip clip, float volume = 1.0f)
    {
        if(clip != null) sfxSource.PlayOneShot(clip, volume);
    }

    public void TocarAmbienteCorrida(SkeletonRacer.TipoDeProva prova)
    {
        AudioClip clipEscolhido = null;
        switch (prova)
        {
            case SkeletonRacer.TipoDeProva.Sprint_100m: clipEscolhido = crowd100m; break;
            case SkeletonRacer.TipoDeProva.Sprint_200m: clipEscolhido = crowd200m; break;
            case SkeletonRacer.TipoDeProva.SprintLongo_400m: clipEscolhido = crowd400m; break;
            case SkeletonRacer.TipoDeProva.MeioFundo_800m: clipEscolhido = crowd800m; break;
            case SkeletonRacer.TipoDeProva.MeioFundo_1500m: clipEscolhido = crowd1500m; break;
            default: clipEscolhido = crowd100m; break;
        }

        if (clipEscolhido != null)
        {
            musicaSource.clip = clipEscolhido;
            musicaSource.loop = true;
            musicaSource.volume = 0.8f; 
            musicaSource.Play();
        }
    }
    
    public void TocarFinal()
    {
        musicaSource.Stop();
        if(resultsCheer != null) sfxSource.PlayOneShot(resultsCheer, 1.0f);
    }
}