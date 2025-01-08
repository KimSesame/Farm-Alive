using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameData;
using System;

/// <summary>
/// 시스템 기획서 상 같은 색 동시에 일어나지 않는 로직 추가 필요
/// 스테이지 매니저의 현재 날씨 받아와서 occurPlusPercent 설정 필요
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("이벤트 계산 주기")]
    [SerializeField] float _checkInterval = 1.0f;

    // 이벤트가 진행 중이면 새 이벤트 발생 불가
    private bool _isEventPlaying = false;

    public event Action<EVENT> onEventStarted;
    public event Action<EVENT> onEventEnded;

    [SerializeField] EVENT _currentEvent; // 현재 진행 중인 이벤트


    private void Start()
    {
        StartCoroutine(EventRoutine());
    }

    private IEnumerator EventRoutine()
    {
        // CSV 다운로드가 끝날 때까지 대기
        while (!CSVManager.Instance.downloadCheck)
            yield return null;

        // 2) 이벤트 리스트
        List<EVENT> eventList = CSVManager.Instance.Events;
        List<EVENT_SEASON> seasonList = CSVManager.Instance.Events_Seasons;

        while (true)
        {
            yield return new WaitForSeconds(_checkInterval);

            // 이벤트 진행중이면 계산 패스
            if (_isEventPlaying)
                continue;

            // 모든 이벤트 각각 확률 체크 후,
            // 두개 이상 발생 시 1개만 랜덤
            List<EVENT> triggered = new List<EVENT>();
            foreach (var ev in eventList)
            {
                float finalRate = ev.event_occurPercent + ev.event_occurPlusPercent;

                if (ProbabilityHelper.Draw(finalRate))
                {
                    triggered.Add(ev);
                }
            }
            if (triggered.Count > 0)
            {
                // 둘 이상 발생하면 1개만 랜덤
                var chosenEvent = ProbabilityHelper.Draw(triggered);
                StartEvent(chosenEvent);
            }
        }
    }

    private void StartEvent(EVENT evData)
    {
        _isEventPlaying = true;
        _currentEvent = evData;

        onEventStarted?.Invoke(evData);

        // 지속시간 있으면 자동 종료
        if (evData.event_continueTime > 0)
        {
            StartCoroutine(EndRoutine(evData.event_continueTime));
        }
    }

    private IEnumerator EndRoutine(float dur)
    {
        yield return new WaitForSeconds(dur);
        EventResolve();
    }

    /// <summary>
    /// 이벤트 해결 시 호출 부탁드립니다
    /// </summary>
    public void EventResolve()
    {
        if (!_isEventPlaying) return;

        _isEventPlaying = false;

        onEventEnded?.Invoke(_currentEvent);
    }
}