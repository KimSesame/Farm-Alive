using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameData;

public class StageManager : MonoBehaviour
{
    [SerializeField] int _curStageID;

    [Header("�������� �ð� �Ӽ�")]
    [SerializeField] float _stageTimeLimit = 0;
    [SerializeField] float _curStageTime = 0;
    [SerializeField] bool _isTimerRunning = false;

    public int WeatherID => _curStageData.stage_seasonID;
    private int _weatherID;

    private STAGE _curStageData;

    private int _maxBrokenMachineCount; // TODO : CSVMANAGER���� �о����
    private int _maxDamagedCropCount = 0; // 0���� ���� (���� ������ ���̺����� �������θ� ����)

    // ��� ���峪�� true�� ����
    public int brokenMachineCount = 0;
    // �۹� �ջ�Ǹ� true�� ����
    public int damagedCropCount = 0;


    public static StageManager Instance { get; private set; }

    private void Awake()
    {
        _curStageID = PunManager.Instance.selectedStage;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator Start()
    {
        // CSV �ٿ�ε� ���� ������ ���
        while (!CSVManager.Instance.downloadCheck)
            yield return null;

        List<STAGE> stages = CSVManager.Instance.Stages;
        STAGE foundStage = default;
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].stage_ID == _curStageID)
            {
                foundStage = stages[i];
                break;
            }
        }

        // currentStageData�� ����
        _curStageData = foundStage;
        
        _stageTimeLimit = 360f;
        _weatherID = _curStageData.stage_seasonID;

        // 2) �������� Ÿ�̸� ����
        StartStageTimer();

        Debug.Log($"[StageManager] ���� ��������={foundStage.stage_ID}, ����(seasonID)={foundStage.stage_seasonID}");
    }

    private void Update()
    {
        if (!_isTimerRunning)
            return;

        _curStageTime += Time.deltaTime;
          
        if (_stageTimeLimit > 0 && _curStageTime >= _stageTimeLimit)
        {
            EndStage();
        }
    }

    public void StartStageTimer()
    {
        _curStageTime = 0f;
        _isTimerRunning = true;
    }
    public void EndStage()
    {
        _isTimerRunning=false;

        int star = EvaluateStar();

        FirebaseManager.Instance.SaveStageResult(_curStageID, _curStageTime ,star);
    }

    public int GetWeatherID()
    { 
       return _weatherID;
    }

    private int EvaluateStar()
    {
        int star = 0;

        int successCount = QuestManager.Instance.ClearQuestCount;

        int totalDealer = QuestManager.Instance.TotalQuestCount;

        if (totalDealer == 3)
        {
            if (successCount == 3) star = 3;
            else if (successCount == 2) star = 2;
            else if (successCount == 1) star = 1;
            else star = 0;
        }
        else if (totalDealer == 2)
        {
            if (successCount == 2) star = 3;
            else if (successCount == 1) star = 1;
            else star = 0;
        }
        else if (totalDealer == 1)
        {
            // 1�� �������� => (1=>3star, 0=>0star)
            if (successCount == 1) star = 3;
            else star = 0;
        }

        // TODO
        // if (!isMachineBroken)
        //     star += 1;
        // int ���� (��� ���峭 Ƚ�� ( ������������ �ٸ� ) ) ���� �۰ų� ������ +1�� ����

        if (damagedCropCount <= _maxDamagedCropCount)
            star += 1;

        // �ִ� 5��
        if (star > 5) star = 5;
        return star;
    }
}