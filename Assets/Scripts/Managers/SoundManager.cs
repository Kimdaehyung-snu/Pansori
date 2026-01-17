using System;
using System.Collections;
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
    [SerializeField] private AudioClip[] microgameBgClipList;
    
    [Header("Microgame Result Sounds")]
    [SerializeField] private AudioClip microgameSuccessClip;  // 성공 효과음
    [SerializeField] private AudioClip microgameFailClip;     // 실패 효과음
    
    [Header("Main BGM (자진모리)")]
    [SerializeField] private AudioClip mainBGMClip; // 자진모리 브금
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float defaultVolume = 0.3f;

    
    private AudioSource bgSound;
    private AudioSource microgameBGMSource; // 미니게임 BGM 전용 소스
    private AudioSource mainBGMSource; // 메인 BGM 소스
    
    // 코루틴 관리 변수 (중복 실행 방지)
    private Coroutine mainBGMFadeCoroutine;
    private Coroutine microgameBGMFadeCoroutine;
    
 
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
    
    #region Fade Utilities
    
    /// <summary>
    /// AudioSource 볼륨을 페이드인합니다.
    /// </summary>
    /// <param name="source">대상 AudioSource</param>
    /// <param name="targetVolume">목표 볼륨</param>
    /// <param name="duration">페이드 시간</param>
    private IEnumerator FadeInCoroutine(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;
        
        source.volume = 0f;
        source.Play();
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
    }
    
    /// <summary>
    /// AudioSource 볼륨을 페이드아웃하고 정지합니다.
    /// </summary>
    /// <param name="source">대상 AudioSource</param>
    /// <param name="duration">페이드 시간</param>
    /// <param name="onComplete">완료 후 콜백</param>
    private IEnumerator FadeOutCoroutine(AudioSource source, float duration, Action onComplete = null)
    {
        if (source == null || !source.isPlaying)
        {
            onComplete?.Invoke();
            yield break;
        }
        
        float startVolume = source.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        source.volume = 0f;
        source.Stop();
        source.volume = startVolume; // 원래 볼륨 복원
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 코루틴을 안전하게 정지하고 새 코루틴을 시작합니다.
    /// </summary>
    private Coroutine SafeStartCoroutine(ref Coroutine existingCoroutine, IEnumerator newCoroutine)
    {
        if (existingCoroutine != null)
        {
            StopCoroutine(existingCoroutine);
        }
        existingCoroutine = StartCoroutine(newCoroutine);
        return existingCoroutine;
    }
    
    #endregion
    
    #region Microgame BGM
    
    /// <summary>
    /// 미니게임 BGM AudioSource 초기화
    /// </summary>
    private void InitializeMicrogameBGMSource()
    {
        if (microgameBGMSource == null)
        {
            GameObject microgameBGMObj = new GameObject("MicrogameBGMSource");
            microgameBGMObj.transform.SetParent(transform);
            microgameBGMSource = microgameBGMObj.AddComponent<AudioSource>();
            microgameBGMSource.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
            microgameBGMSource.loop = true;
            microgameBGMSource.volume = 0.3f;
            microgameBGMSource.playOnAwake = false;
        }
    }
    
    /// <summary>
    /// 미니게임 BGM 재생 (프리팹 이름으로 매칭, 속도에 따른 pitch 조절, 자동 페이드인)
    /// </summary>
    /// <param name="microgameName">미니게임 프리팹 이름</param>
    /// <param name="speed">게임 속도 (pitch에 반영)</param>
    public void PlayMicrogameBGM(string microgameName, float speed = 1.0f)
    {
        if (microgameBgClipList == null || microgameBgClipList.Length == 0)
        {
            Debug.LogWarning("[SoundManager] microgameBgClipList가 비어있습니다.");
            return;
        }
        
        // 기존 미니게임 BGM 즉시 정지 (페이드 없이)
        StopMicrogameBGMImmediate();
        
        // AudioSource 초기화
        InitializeMicrogameBGMSource();
        
        // microgameBgClipList에서 프리팹 이름과 일치하는 클립 찾기
        for (int i = 0; i < microgameBgClipList.Length; i++)
        {
            if (microgameBgClipList[i] != null && 
                microgameBgClipList[i].name == microgameName)
            {
                microgameBGMSource.clip = microgameBgClipList[i];
                
                // pitch 설정 (속도 반영, 0.5f ~ 3.0f 범위 제한)
                microgameBGMSource.pitch = Mathf.Clamp(speed, 0.5f, 3.0f);
                
                // 자동 페이드인 적용
                SafeStartCoroutine(ref microgameBGMFadeCoroutine, FadeInCoroutine(microgameBGMSource, defaultVolume, fadeInDuration));
                
                Debug.Log($"[SoundManager] 미니게임 BGM 재생: {microgameName} (속도: {speed})");
                return;
            }
        }
        
        Debug.LogWarning($"[SoundManager] 미니게임 BGM을 찾을 수 없습니다: {microgameName}");
    }
    
    /// <summary>
    /// 미니게임 BGM 정지 (자동 페이드아웃)
    /// </summary>
    public void StopMicrogameBGM()
    {
        if (microgameBGMSource == null || !microgameBGMSource.isPlaying)
        {
            return;
        }
        
        // 자동 페이드아웃 적용
        SafeStartCoroutine(ref microgameBGMFadeCoroutine, FadeOutCoroutine(microgameBGMSource, fadeOutDuration, () =>
        {
            microgameBGMSource.pitch = 1.0f; // pitch 초기화
            microgameBGMSource.clip = null;
            Debug.Log("[SoundManager] 미니게임 BGM 정지 (페이드아웃 완료)");
        }));
    }
    
    /// <summary>
    /// 미니게임 BGM 즉시 정지 (페이드 없이)
    /// </summary>
    private void StopMicrogameBGMImmediate()
    {
        if (microgameBGMFadeCoroutine != null)
        {
            StopCoroutine(microgameBGMFadeCoroutine);
            microgameBGMFadeCoroutine = null;
        }
        
        if (microgameBGMSource != null && microgameBGMSource.isPlaying)
        {
            microgameBGMSource.Stop();
            microgameBGMSource.pitch = 1.0f; // pitch 초기화
            microgameBGMSource.clip = null;
        }
    }
    
    /// <summary>
    /// 현재 재생 중인 미니게임 BGM의 속도(pitch) 변경
    /// </summary>
    /// <param name="speed">새로운 속도</param>
    public void SetMicrogameBGMSpeed(float speed)
    {
        if (microgameBGMSource != null && microgameBGMSource.isPlaying)
        {
            microgameBGMSource.pitch = Mathf.Clamp(speed, 0.5f, 3.0f);
            Debug.Log($"[SoundManager] 미니게임 BGM 속도 변경: {speed}");
        }
    }
    
    /// <summary>
    /// 미니게임 BGM이 현재 재생 중인지 확인
    /// </summary>
    public bool IsMicrogameBGMPlaying => microgameBGMSource != null && microgameBGMSource.isPlaying;
    
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
    
    #region Main BGM (자진모리)
    
    /// <summary>
    /// 메인 BGM AudioSource 초기화
    /// </summary>
    private void InitializeMainBGMSource()
    {
        if (mainBGMSource == null)
        {
            GameObject mainBGMObj = new GameObject("MainBGMSource");
            mainBGMObj.transform.SetParent(transform);
            mainBGMSource = mainBGMObj.AddComponent<AudioSource>();
            mainBGMSource.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
            mainBGMSource.loop = true;
            mainBGMSource.volume = 0.3f;
            mainBGMSource.playOnAwake = false;
        }
    }
    
    /// <summary>
    /// 메인 BGM 재생 (자진모리 브금, 속도 반영, 자동 페이드인)
    /// </summary>
    /// <param name="speed">게임 속도 (pitch에 반영)</param>
    public void PlayMainBGM(float speed = 1.0f)
    {
        if (mainBGMClip == null)
        {
            Debug.LogWarning("[SoundManager] mainBGMClip(자진모리 브금)이 설정되지 않았습니다.");
            return;
        }
        
        InitializeMainBGMSource();
        
        // 이미 재생 중이면 속도만 업데이트
        if (mainBGMSource.isPlaying && mainBGMSource.clip == mainBGMClip)
        {
            SetMainBGMSpeed(speed);
            return;
        }
        
        mainBGMSource.clip = mainBGMClip;
        mainBGMSource.pitch = Mathf.Clamp(speed, 0.5f, 3.0f);
        
        // 자동 페이드인 적용
        SafeStartCoroutine(ref mainBGMFadeCoroutine, FadeInCoroutine(mainBGMSource, defaultVolume, fadeInDuration));
        
        Debug.Log($"[SoundManager] 메인 BGM 재생: {mainBGMClip.name} (속도: {speed})");
    }
    
    /// <summary>
    /// 메인 BGM 정지 (자동 페이드아웃)
    /// </summary>
    public void StopMainBGM()
    {
        if (mainBGMSource == null || !mainBGMSource.isPlaying)
        {
            return;
        }
        
        // 자동 페이드아웃 적용
        SafeStartCoroutine(ref mainBGMFadeCoroutine, FadeOutCoroutine(mainBGMSource, fadeOutDuration, () =>
        {
            Debug.Log("[SoundManager] 메인 BGM 정지 (페이드아웃 완료)");
        }));
    }
    
    /// <summary>
    /// 메인 BGM 속도(pitch) 변경
    /// </summary>
    /// <param name="speed">새로운 속도</param>
    public void SetMainBGMSpeed(float speed)
    {
        if (mainBGMSource != null)
        {
            mainBGMSource.pitch = Mathf.Clamp(speed, 0.5f, 3.0f);
            Debug.Log($"[SoundManager] 메인 BGM 속도 변경: {speed}");
        }
    }
    
    /// <summary>
    /// 메인 BGM이 현재 재생 중인지 확인
    /// </summary>
    public bool IsMainBGMPlaying => mainBGMSource != null && mainBGMSource.isPlaying;
    
    #endregion
}
