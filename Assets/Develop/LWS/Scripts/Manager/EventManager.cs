using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameData;
using System;

/// <summary>
/// �ý��� ��ȹ�� �� ���� �� ���ÿ� �Ͼ�� �ʴ� ���� �߰� �ʿ�
/// �������� �Ŵ����� ���� ���� �޾ƿͼ� occurPlusPercent ���� �ʿ�
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("�̺�Ʈ ��� �ֱ�")]
    [SerializeField] float _checkInterval = 1.0f;

    // �̺�Ʈ�� ���� ���̸� �� �̺�Ʈ �߻� �Ұ�
    private bool _isEventPlaying = false;

    public event Action<EVENT> onEventStarted;
    public event Action<EVENT> onEventEnded;

    [SerializeField] EVENT _currentEvent; // ���� ���� ���� �̺�Ʈ


    private void Start()
    {
        StartCoroutine(EventRoutine());
    }

    private IEnumerator EventRoutine()
    {
        // CSV �ٿ�ε尡 ���� ������ ���
        while (!CSVManager.Instance.downloadCheck)
            yield return null;

        // 2) �̺�Ʈ ����Ʈ
        List<EVENT> eventList = CSVManager.Instance.Events;
        List<EVENT_SEASON> seasonList = CSVManager.Instance.Events_Seasons;

        while (true)
        {
            yield return new WaitForSeconds(_checkInterval);

            // �̺�Ʈ �������̸� ��� �н�
            if (_isEventPlaying)
                continue;

            // ��� �̺�Ʈ ���� Ȯ�� üũ ��,
            // �ΰ� �̻� �߻� �� 1���� ����
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
                // �� �̻� �߻��ϸ� 1���� ����
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

        // ���ӽð� ������ �ڵ� ����
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
    /// �̺�Ʈ �ذ� �� ȣ�� ��Ź�帳�ϴ�
    /// </summary>
    public void EventResolve()
    {
        if (!_isEventPlaying) return;

        _isEventPlaying = false;

        onEventEnded?.Invoke(_currentEvent);
    }
}