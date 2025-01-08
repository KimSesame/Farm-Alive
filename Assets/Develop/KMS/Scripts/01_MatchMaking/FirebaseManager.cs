using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; } // 싱글톤

    private FirebaseApp app;
    public static FirebaseApp App { get { return Instance.app; } }

    private Firebase.Auth.FirebaseAuth auth;
    public static Firebase.Auth.FirebaseAuth Auth { get { return Instance.auth; } }

    private FirebaseDatabase dataBase;
    public static FirebaseDatabase DataBase { get { return Instance.dataBase; } }

    private string userId; // Firebase UID

    public event Action OnFirebaseInitialized; // Firebase 초기화 완료 이벤트

    /// <summary>
    /// 플레이어의 저장된 UID를 호출.
    /// </summary>
    /// <returns></returns>
    public string GetUserId()
    {
        if (string.IsNullOrEmpty(userId))
        {
            userId = PlayerPrefs.GetString("firebase_uid", string.Empty);
        }
        return userId;
    }

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeFirebase();
    }

    /// <summary>
    /// 파이어베이스 초기화 메서드.
    /// </summary>
    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                dataBase = FirebaseDatabase.DefaultInstance;

                Debug.Log("Firebase 초기화 완료!");
                CheckAndInitializeUserData();
            }
            else
            {
                Debug.LogError("Firebase 의존성 해결 실패!");
                app = null;
                auth = null;
                dataBase = null;
            }
        });
    }

    /// <summary>
    /// 익명 로그인 생성 메서드.
    /// </summary>
    private void AnonymouslyLogin()
    {
        ClearLocalUid();

        auth.SignOut();
        Debug.Log("기존 인증 로그아웃 완료. 새로운 익명 로그인 시도...");

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("익명 로그인 작업이 취소되었습니다.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("익명 로그인 작업 중 오류 발생: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            if (result?.User == null)
            {
                Debug.LogError("로그인 결과에서 User 정보를 가져올 수 없습니다.");
                return;
            }

            userId = result.User.UserId; // Firebase UID 저장
            SaveUidLocally(userId); // UID를 로컬 저장
            Debug.LogFormat($"익명 로그인 성공! UID: {userId}");

            // uid 및 데이터 저장.
            SaveUserData();
            // 마지막 로그인 시간 저장.
            UpdateLastLogin();
        });
    }

    /// <summary>
    /// 유저 UID 검색 후 해당 데이터가 없을 시 익명 로그인으로 생성.
    /// </summary>
    private void CheckAndInitializeUserData()
    {
        string localUid = LoadUidFromLocal();

        if (!string.IsNullOrEmpty(localUid))
        {
            Debug.Log($"로컬 저장된 UID 발견: {localUid}");
            VerifyUserExistsInFirebase(localUid);
        }
        else
        {
            Debug.Log("로컬 UID가 없습니다. 새로 로그인 진행...");
            AnonymouslyLogin();
        }
    }

    /// <summary>
    /// 파이어베이스 데이터가 존재시 해당 아이디로 로그인
    /// 없을시 익명 로그인으로 실행하는 메서드.
    /// </summary>
    /// <param name="uid"></param>
    private void VerifyUserExistsInFirebase(string uid)
    {
        DatabaseReference userRef = dataBase.GetReference($"users/{uid}");

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                // Permission Denied 처리
                Debug.LogError("Firebase 접근 실패: " + task.Exception);
                Debug.Log("Permission Denied 또는 다른 오류 발생. 로컬 UID 초기화 후 새 로그인 진행...");
                AnonymouslyLogin();
                return;
            }

            if (task.IsCompleted && task.Result.Exists)
            {
                Debug.Log($"Firebase에서 UID {uid} 발견!");
                userId = uid;
                UpdateLastLogin();
                NotifyInitializationComplete();
            }
            else
            {
                Debug.Log("Firebase에 해당 UID 없음. 로컬 UID 삭제 후 새 로그인 진행...");
                AnonymouslyLogin();
            }
        });
    }

    /// <summary>
    /// 생성시 파이어 베이스에 유저 데이터 저장 메서드.
    /// </summary>
    private void SaveUserData()
    {
        DatabaseReference userRef = dataBase.GetReference($"users/{userId}");

        Dictionary<string, object> userData = new Dictionary<string, object>()
        {
            { "uid", userId },
            { "nickname", "" },
            { "createdAt", DateTime.Now.ToString("o") },
            { "lastLogin", DateTime.Now.ToString("o") },
            { "settings", new Dictionary<string, object>()
                {
                    { "level", 1 },
                    { "score", 0 }
                }
            },
            { "achievements", new List<string>() { "first_login" } }
        };

        userRef.SetValueAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("사용자 데이터 저장 완료!");
                UpdateLastLogin();
                NotifyInitializationComplete();
                return;
            }
            else
            {
                Debug.LogError("사용자 데이터 저장 실패: " + task.Exception);
            }
        });
    }

    /// <summary>
    /// 스테이지 결과 파이어 베이스에 저장 메서드. 
    /// </summary>
    public void SaveStageResult(int stageID, float playedTime, int starCount)
    {

    }

    /// <summary>
    /// 최근 로그인한 기록을 저장하는 메서드.
    /// </summary>
    public void UpdateLastLogin()
    {
        Debug.Log("캐릭터 마지막 로그인 갱신!");
        Debug.Log("지금은 데이터를 잠시 갱신안함 - (코드 막아둠!)");

        //DatabaseReference userRef = dataBase.GetReference($"users/{userId}/lastLogin");

        //userRef.SetValueAsync(DateTime.Now.ToString("o")).ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsCompleted && !task.IsFaulted)
        //    {
        //        Debug.Log("마지막 로그인 시간 업데이트 성공!");
        //    }
        //    else
        //    {
        //        Debug.LogError("마지막 로그인 시간 업데이트 실패: " + task.Exception);
        //    }
        //});
    }

    /// <summary>
    /// 로그인 완료 시작을 알리는 메서드.
    /// </summary>
    public void NotifyInitializationComplete()
    {
        OnFirebaseInitialized?.Invoke();
    }

    /// <summary>
    /// 로컬에 유저의 UID를 저장.
    /// </summary>
    /// <param name="uid"></param>
    private void SaveUidLocally(string uid)
    {
        PlayerPrefs.SetString("firebase_uid", uid);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 로컬에 저장된 UID를 불러오는 메서드.
    /// 없을시 Empty로 반환.
    /// </summary>
    /// <returns></returns>
    private string LoadUidFromLocal()
    {
        return PlayerPrefs.GetString("firebase_uid", string.Empty);
    }

    /// <summary>
    /// 로컬에 저장된 UID를 삭제하는 메서드.
    /// </summary>
    private void ClearLocalUid()
    {
        PlayerPrefs.DeleteKey("firebase_uid");
        PlayerPrefs.Save();
        userId = string.Empty;
    }
}
