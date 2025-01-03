using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("로딩 화면 UI")]
    public GameObject loadingUI;          
    [Tooltip("프로그레스 바")]
    public Slider progressBar;
    [Tooltip("진행도")]
    public TMP_Text progressText;

    private bool _isSceneReady = false;    // 씬 로드 완료 상태 확인

    private void Start()
    {
        loadingUI.SetActive(true);
        string targetSceneName = SceneLoader.TargetScene;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("TargetScene이 설정되지 않았습니다.");
            return;
        }
        StartCoroutine(LoadSceneAsync(targetSceneName));
    }

    private IEnumerator LoadSceneAsync(string targetSceneName)
    {
        // FusionLobby 씬을 비동기로 로드
        var asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);

        // 씬 자동 활성화 비활성화
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // 씬 로드가 완료되었을 때
            if (asyncLoad.progress >= 0.9f && !_isSceneReady)
            {
                Debug.Log("씬 로딩 완료. 프로그레스 바 준비.");
                _isSceneReady = true;

                // 3초 동안 프로그레스 바를 자연스럽게 진행
                yield return StartCoroutine(SimulateProgressBar(3f));

                // 씬 활성화
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private IEnumerator SimulateProgressBar(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // 프로그레스 바 및 텍스트 업데이트
            progressBar.value = progress;
            progressText.text = $"{(progress * 100f):0}%";

            yield return null;
        }

        // 최종 완료 상태 설정
        progressBar.value = 1f;
        progressText.text = "100%";
    }
}
