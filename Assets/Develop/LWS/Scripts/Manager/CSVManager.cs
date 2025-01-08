using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GameData;

public class CSVManager : MonoBehaviour
{
    const string cropCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=815678089&format=csv"; // 작물 시트
    const string facilityCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=1924872652&format=csv"; // 시설 시트
    const string correspondentCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=1446733082&format=csv"; // 거래처 시트
    const string corNeedCropCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=1954559638&format=csv"; // 거래처 요구작물 시트
    const string corCropTypeCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=1014353564&format=csv"; // 거래처별 작물종류개수 시트
    const string corCountCropCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=760081684&format=csv"; // 거래처별 요구작물개수 시트
    const string eventCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=1411103725&format=csv"; // 돌발 이벤트 시트
    const string eventSeasonCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=446557917&format=csv"; // 이벤트 계절 시트
    const string stageCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=1766045491&format=csv"; // 스테이지 시트
    const string stageCorCSVPath = "https://docs.google.com/spreadsheets/d/1Sd5x4Mt1fnIVY-X97tvYx1lTE8bh-nQ0/export?gid=169149848&format=csv"; // 스테이지별 거래처 시트

    public List<CropData> cropDatas;

    public List<FACILITY> Facilities;
    
    public List<CORRESPONDENT> Correspondents;
    public List<CORRESPONDENT_REQUIRECROPS> Correspondents_RequireCrops;
    public List<CORRESPONDENT_CROPSTYPECOUNT> Correspondents_CropsType;
    public List<CORRESPONDENT_CROPSCOUNT> Correspondents_CropsCount;

    public List<EVENT> Events;
    public List<EVENT_SEASON> Events_Seasons;

    public List<STAGE> Stages;
    public List<STAGE_CORRESPONDENT> Stages_Correspondents;

    public bool downloadCheck;
    public static CSVManager Instance;

    private void Start()
    {
        downloadCheck = false;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }

        StartCoroutine(DownloadRoutine());
    }

    IEnumerator DownloadRoutine()
    {
        // 작물 다운로드

        UnityWebRequest request = UnityWebRequest.Get(cropCSVPath); // 링크를 통해서 웹사이트에 다운로드 요청
        yield return request.SendWebRequest();                  // 링크를 진행하고 완료될 때까지 대기

        string receiveText = request.downloadHandler.text;      // 다운로드 완료한 파일을 텍스트로 읽기

        string[] lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            string[] values = lines[y].Split(',', '\t');

            cropDatas[y-2].ID = int.Parse(values[0]);
            cropDatas[y-2].cropName = values[1];
            cropDatas[y-2].digCount = int.Parse(values[2]);
            cropDatas[y-2].maxMoisture = int.Parse(values[3]);
            cropDatas[y-2].maxNutrient = int.Parse(values[4]);
            cropDatas[y-2].growthTime = float.Parse(values[5]);
            cropDatas[y-2].droughtMaxMoisture = int.Parse(values[6]);
            cropDatas[y-2].droughtMaxNutrient = int.Parse(values[7]);
            cropDatas[y-2].damageRate = float.Parse(values[8]);
            cropDatas[y-2].damageLimitTime = float.Parse(values[9]);
            cropDatas[y-2].temperatureDecreaseLimitTime = float.Parse(values[10]);
            cropDatas[y-2].temperatureIncreaseLimitTime = float.Parse(values[11]);
        }
        Debug.Log("Download crops completed");

        // 시설 다운로드

        request = UnityWebRequest.Get(facilityCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            FACILITY facility = new FACILITY();

            string[] values = lines[y].Split(',', '\t');

            facility.idx = y - 2;
            facility.facility_ID = int.Parse(values[0]);
            facility.facility_name = values[1];
            facility.facility_symptomPercent = float.Parse(values[2]);
            facility.facility_stormSymptomPercent = float.Parse(values[3]);
            facility.facility_timeLimit = float.Parse(values[4]);
            facility.facility_maxHammeringCount = int.Parse(values[5]);
            int.TryParse(values[6], out facility.facility_maxBootCount);

            Facilities.Add(facility);
        }

        // 거래처 다운로드

        request = UnityWebRequest.Get(correspondentCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            CORRESPONDENT correspondent = new CORRESPONDENT();

            string[] values = lines[y].Split(',', '\t');

            correspondent.idx = y - 2;
            correspondent.correspondent_ID = int.Parse(values[0]);
            correspondent.correspondent_name = values[1];
            correspondent.correspondent_timeLimit = float.Parse(values[2]);

            Correspondents.Add(correspondent);
        }

        // 거래처-요구작물 다운로드

        request = UnityWebRequest.Get(corNeedCropCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            CORRESPONDENT_REQUIRECROPS correspondent_requireCrops = new CORRESPONDENT_REQUIRECROPS();

            string[] values = lines[y].Split(',', '\t');

            correspondent_requireCrops.idx = y - 2;
            correspondent_requireCrops.correspondent_corID = int.Parse(values[0]);
            correspondent_requireCrops.correspondent_cropID = new int[3];

            for (int i = 0; i < 3; i++)
            {
                int.TryParse(values[i + 1], out int value);
                correspondent_requireCrops.correspondent_cropID[i] = value;
            }

            Correspondents_RequireCrops.Add(correspondent_requireCrops);
        }

        // 거래처-작물종류개수 다운로드

        request = UnityWebRequest.Get(corCropTypeCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            CORRESPONDENT_CROPSTYPECOUNT correspondent_cropsTypeCount = new CORRESPONDENT_CROPSTYPECOUNT();

            string[] values = lines[y].Split(',', '\t');

            correspondent_cropsTypeCount.idx = y - 2;
            correspondent_cropsTypeCount.correspondent_corID = int.Parse(values[0]);
            correspondent_cropsTypeCount.correspondent_stage = new int[10];

            for (int i = 0; i < 10; i++)
            {
                int.TryParse(values[i + 1], out int value);
                correspondent_cropsTypeCount.correspondent_stage[i] = value;
            }

            Correspondents_CropsType.Add(correspondent_cropsTypeCount);
        }

        // 거래처-요구 작물 개수 다운로드

        request = UnityWebRequest.Get(corCountCropCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            CORRESPONDENT_CROPSCOUNT correspondent_cropsCount = new CORRESPONDENT_CROPSCOUNT();

            string[] values = lines[y].Split(',', '\t');

            correspondent_cropsCount.idx = y - 2;
            correspondent_cropsCount.correspondent_corID = int.Parse(values[0]);
            correspondent_cropsCount.correspondent_stage = new int[10];

            for (int i = 0; i < 10; i++)
            {
                int.TryParse(values[i + 1], out int value);
                correspondent_cropsCount.correspondent_stage[i] = value;
            }

            Correspondents_CropsCount.Add(correspondent_cropsCount);
        }

        // 이벤트 개수 다운로드

        request = UnityWebRequest.Get(eventCSVPath); 
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            EVENT events = new EVENT();

            string[] values = lines[y].Split(',', '\t');

            events.idx = y - 2;
            events.event_ID = int.Parse(values[0]);
            events.event_name = values[1];
            events.event_occurPercent = float.Parse(values[2]);
            events.event_occurPlusPercent = float.Parse(values[3]);
            events.event_continueTime = float.Parse(values[4]);

            Events.Add(events);
        }

        // 이벤트 확률 증가 계절 다운로드

        request = UnityWebRequest.Get(eventSeasonCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            EVENT_SEASON events_seasons = new EVENT_SEASON();

            string[] values = lines[y].Split(',', '\t');

            events_seasons.idx = y - 2;
            events_seasons.event_ID = int.Parse(values[0]);
            events_seasons.event_seasonID = new int[4];

            for (int i = 0; i < 4; i++)
            {
                int.TryParse(values[i + 1], out int value);
                events_seasons.event_seasonID[i] = value;
            }

            Events_Seasons.Add(events_seasons);
        }

        // 스테이지 다운로드

        request = UnityWebRequest.Get(stageCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for (int y = 2; y < lines.Length; y++)
        {
            STAGE stage = new STAGE();

            string[] values = lines[y].Split(',', '\t');

            stage.idx = y - 2;
            stage.stage_ID = int.Parse(values[0]);
            stage.stage_seasonID = int.Parse(values[1]);
            stage.stage_allowSymptomFacilityCount = int.Parse(values[2]);

            Stages.Add(stage);
        }

        // 스테이지 등장 가능 거래처 다운로드

        request = UnityWebRequest.Get(stageCorCSVPath);
        yield return request.SendWebRequest();

        receiveText = request.downloadHandler.text;

        lines = receiveText.Split('\n');
        for(int y = 2;y < lines.Length; y++)
        {
            STAGE_CORRESPONDENT stage_correspondent = new STAGE_CORRESPONDENT();
           
            string[] values = lines[y].Split(',', '\t');

            stage_correspondent.idx = y - 2;
            stage_correspondent.stage_ID = int.Parse(values[0]);
            stage_correspondent.stage_corCount = int.Parse(values[1]);
            stage_correspondent.stage_corList = new int[3];

            for (int i = 0; i < 3; i++)
            {
                int.TryParse(values[i + 2], out int value);
                stage_correspondent.stage_corList[i] = value;
            }

            Stages_Correspondents.Add(stage_correspondent);
        }

        downloadCheck = true;
    }
}