using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType { Pop, Move, Drop, Stick}

[System.Serializable]
public struct Sound
{
    public SoundType soundType;
    public AudioClip audioClip;
}

public class SoundManager : MonoBehaviour {

    static public SoundManager S;
    private AudioSource[] _audioSources;
    [SerializeField] private Sound[] _sounds;
    private Dictionary<SoundType, AudioClip> soundMap = new Dictionary<SoundType, AudioClip>();

    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        foreach (Sound s in _sounds)
        {
            soundMap.Add(s.soundType, s.audioClip);
        }
        _audioSources = GetComponents<AudioSource>();
        foreach (AudioSource aus in _audioSources)
        {
            aus.volume = 0.25f;
        }
    }

    public void PlaySound(SoundType st)
    {
        for (int i = 0; i < _audioSources.Length; i++)
        {
            if (!_audioSources[i].isPlaying)
            {
                _audioSources[i].clip = soundMap[st];
                _audioSources[i].Play();
                break;
            }
        }
        
          
    }

}
