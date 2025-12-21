using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;

        [Range(0f, 1f)] public float volume;

        [HideInInspector] public AudioSource source;
        public bool loop;
    }

    public Sound[] sounds;
    public Sound[] CreatedSounds;

    public static AudioManager instance;

    bool MusicStatus = true;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (var S in sounds)
        {
            S.source = gameObject.AddComponent<AudioSource>();
            S.source.clip = S.clip;
            S.source.volume = S.volume;
            S.source.loop = S.loop;
        }

        string MusicStatus = PlayerPrefs.GetString("MusicStatus", "ON");
        if (MusicStatus == "OFF")
        {
            SetMusicStatus(false);
        }
        else if(MusicStatus == "ON")
        {
            SetMusicStatus(true);
        }
    }

    public void SetMusicStatus(bool status) => MusicStatus = status;

    /*private void Start()
    {
        Play("theme");
    }*/

    public void Play(string name)
    {
        if (!MusicStatus)
            return;
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.Play();
    }

    public void Stop(string name)
    {
        if (!MusicStatus)
            return;
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.Stop();
    }

    public void PlayNewSound(string name, float destroyTime)
    {
        if (!MusicStatus)
            return;
        Sound s = Array.Find(CreatedSounds, sound => sound.name == name);
        if (s == null)
            return;
        s = AddAudioSource(s);
        s.source.Play();
        Destroy(s.source, destroyTime);
    }

    public void PlayNewSound(string name)
    {
        if (!MusicStatus)
            return;
        Sound s = Array.Find(CreatedSounds, sound => sound.name == name);
        if (s == null)
            return;
        s = AddAudioSource(s);
        s.source.Play();
        Destroy(s.source, s.source.clip.length);
    }

    Sound AddAudioSource(Sound s)
    {
        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.loop = s.loop;
        return s;
    }

    public void FullVolume(string name)
    {
        if (!MusicStatus)
            return;
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.volume = s.volume;
    }

    public void ZeroVolume(string name)
    {
        if (!MusicStatus)
            return;
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.volume = 0f;
    }
}
