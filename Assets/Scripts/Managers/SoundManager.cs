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
/// 미니게임 BGM: 미니게임 프리팹 이름과 일치하는 BGM 재생 (속도 반영)
/// </summary>
public class SoundManager : PansoriSingleton<SoundManager>
{
    
    [SerializeField] private AudioMixer mixer;
    [Header("each bg source have to be same as scene name")]
    [SerializeField]private AudioSource[] bgList;
    
    [Header("Microgame BGM List (clip name = prefab name)")]
    [SerializeField] private AudioSource[] microgameBgList;
    
    [Header("Microgame Result Sounds")]
    [SerializeField] private AudioClip microgameSuccessClip;  // 성공 효과음
    [SerializeField] private AudioClip microgameFailClip;     // 실패 효과음

    
    private AudioSource bgSound;
    private AudioSource currentMicrogameBgSound;
    
 
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
    
    #region Microgame BGM
    
    /// <summary>
    /// 미니게임 BGM 재생 (프리팹 이름으로 매칭, 속도에 따른 pitch 조절)
    /// </summary>
    /// <param name="microgameName">미니게임 프리팹 이름</param>
    /// <param name="speed">게임 속도 (pitch에 반영)</param>
    public void PlayMicrogameBGM(string microgameName, float speed = 1.0f)
    {
        if (microgameBgList == null || microgameBgList.Length == 0)
        {
            Debug.LogWarning("[SoundManager] microgameBgList가 비어있습니다.");
            return;
        }
        
        // 기존 미니게임 BGM 정지
        StopMicrogameBGM();
        
        // microgameBgList에서 프리팹 이름과 일치하는 클립 찾기
        for (int i = 0; i < microgameBgList.Length; i++)
        {
            if (microgameBgList[i] != null && 
                microgameBgList[i].clip != null && 
                microgameBgList[i].clip.name == microgameName)
            {
                currentMicrogameBgSound = microgameBgList[i];
                
                // pitch 설정 (속도 반영, 0.5f ~ 3.0f 범위 제한)
                currentMicrogameBgSound.pitch = Mathf.Clamp(speed, 0.5f, 3.0f);
                currentMicrogameBgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
                currentMicrogameBgSound.loop = true;
                currentMicrogameBgSound.volume = 0.3f;
                currentMicrogameBgSound.Play();
                
                Debug.Log($"[SoundManager] 미니게임 BGM 재생: {microgameName} (속도: {speed})");
                return;
            }
        }
        
        Debug.LogWarning($"[SoundManager] 미니게임 BGM을 찾을 수 없습니다: {microgameName}");
    }
    
    /// <summary>
    /// 미니게임 BGM 정지
    /// </summary>
    public void StopMicrogameBGM()
    {
        if (currentMicrogameBgSound != null && currentMicrogameBgSound.isPlaying)
        {
            currentMicrogameBgSound.Stop();
            currentMicrogameBgSound.pitch = 1.0f; // pitch 초기화
        }
        
        // 모든 미니게임 BGM 정지 (안전을 위해)
        if (microgameBgList != null)
        {
            foreach (var sound in microgameBgList)
            {
                if (sound != null && sound.isPlaying)
                {
                    sound.Stop();
                    sound.pitch = 1.0f;
                }
            }
        }
        
        currentMicrogameBgSound = null;
    }
    
    /// <summary>
    /// 현재 재생 중인 미니게임 BGM의 속도(pitch) 변경
    /// </summary>
    /// <param name="speed">새로운 속도</param>
    public void SetMicrogameBGMSpeed(float speed)
    {
        if (currentMicrogameBgSound != null && currentMicrogameBgSound.isPlaying)
        {
            currentMicrogameBgSound.pitch = Mathf.Clamp(speed, 0.5f, 3.0f);
            Debug.Log($"[SoundManager] 미니게임 BGM 속도 변경: {speed}");
        }
    }
    
    /// <summary>
    /// 미니게임 BGM이 현재 재생 중인지 확인
    /// </summary>
    public bool IsMicrogameBGMPlaying => currentMicrogameBgSound != null && currentMicrogameBgSound.isPlaying;
    
    #endregion
    
    #region Microgame Result Sounds
    
    /// <summary>
    /// 미니게임 결과 사운드 재생 (성공/실패)
    /// </summary>
    /// <param name="success">성공 여부</param>
    public void PlayMicrogameResultSound(bool success)
    {
        AudioClip clip = success ? microgameSuccessClip : microgameFailClip;
        if (clip != null)
        {
            SFXPlay(success ? "MicrogameSuccess" : "MicrogameFail", clip);
            Debug.Log($"[SoundManager] 미니게임 결과 사운드 재생: {(success ? "성공" : "실패")}");
        }
        else
        {
            Debug.LogWarning($"[SoundManager] 미니게임 {(success ? "성공" : "실패")} 사운드가 설정되지 않았습니다.");
        }
    }
    
    #endregion
}
