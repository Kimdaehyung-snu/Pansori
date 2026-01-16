using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Serialization;

/// <summary>
/// BGM: 씬 네임과 똑같은 네임의 브금이 재생되는 방식.
/// SFX: 효과음 나야하는 타이밍에서 호출하기.
/// </summary>
public class SoundManager : PansoriSingleton<SoundManager>
{
    
    [SerializeField] private AudioMixer mixer;
    [Header("each bg source have to be same as scene name")]
    [SerializeField]private AudioSource[] bgList;

    
    private AudioSource bgSound;
    
 
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        for (int i = 0; i < bgList.Length; i++)
        {
            if (scene.name == bgList[i].clip.name)
            {
                bgSound = bgList[i];
                BgSoundPlay(bgList[i].clip);
            }   
        }
    }
    

    
    /// <summary>
    /// 효과음이 달린 오브젝트에서, 해당 함수를 호출해야함.
    /// </summary>
    /// <param name="SFXName"></param>
    /// <param name="clip"></param>
    public void SFXPlay(string SFXName, AudioClip clip)
    {
        GameObject go = new GameObject(SFXName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        audioSource.clip = clip;
        audioSource.volume = 0.3f;
        audioSource.Play();

        Destroy(go, clip.length);
    }

    /// <summary>
    /// 환경설정에서 브금 사운드 조절용
    /// 슬라이드 UI의 onValueChanged에 해당 함수를 추가하세요
    /// </summary>
    /// <param name="val"></param>
    public void BGSoundVolume(float val)
    {
        mixer.SetFloat("BGMVolume", Mathf.Log10(val)*20);
    }
    
    /// <summary>
    /// 환경설정에서 효과음 사운드 조절용
    /// 슬라이드 UI의 onValueChanged에 해당 함수를 추가하세요
    /// </summary>
    /// <param name="val"></param>
    public void SFXSoundVolume(float val)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(val)*20);
    }

    /// <summary>
    /// 브금이 달린 오브젝트(this)에서, 해당 함수를 호출해야함.
    /// </summary>
    /// <param name="clip"></param>
    private void BgSoundPlay(AudioClip clip)
    {
        foreach (var sound in bgList)
        {
            sound.Stop();
        }
        
        bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
        bgSound.clip = clip;
        bgSound.loop = true;
        bgSound.volume = 0.3f;
        bgSound.Play();     
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (bgSound != null && bgSound.isPlaying)
        {
            bgSound.Stop();
        }
        
        foreach (var sound in bgList)
        {
            if (sound != null && sound.isPlaying)
            {
                sound.Stop();
            }
        }
    }
}
