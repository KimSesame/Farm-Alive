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
    public static FirebaseManager Instance { get; private set; } // �̱���

    private FirebaseApp app;
    public static FirebaseApp App { get { return Instance.app; } }

    private Firebase.Auth.FirebaseAuth auth;
    public static Firebase.Auth.FirebaseAuth Auth { get { return Instance.auth; } }

    private FirebaseDatabase dataBase;
    public static FirebaseDatabase DataBase { get { return Instance.dataBase; } }

    private string userId; // Firebase UID

    public event Action OnFirebaseInitialized; // Firebase �ʱ�ȭ �Ϸ� �̺�Ʈ

    /// <summary>
    /// �÷��̾��� ����� UID�� ȣ��.
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
        // �̱��� ����
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
    /// ���̾�̽� �ʱ�ȭ �޼���.
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

                Debug.Log("Firebase �ʱ�ȭ �Ϸ�!");
                CheckAndInitializeUserData();
            }
            else
            {
                Debug.LogError("Firebase ������ �ذ� ����!");
                app = null;
                auth = null;
                dataBase = null;
            }
        });
    }

    /// <summary>
    /// �͸� �α��� ���� �޼���.
    /// </summary>
    private void AnonymouslyLogin()
    {
        ClearLocalUid();

        auth.SignOut();
        Debug.Log("���� ���� �α׾ƿ� �Ϸ�. ���ο� �͸� �α��� �õ�...");

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("�͸� �α��� �۾��� ��ҵǾ����ϴ�.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("�͸� �α��� �۾� �� ���� �߻�: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            if (result?.User == null)
            {
                Debug.LogError("�α��� ������� User ������ ������ �� �����ϴ�.");
                return;
            }

            userId = result.User.UserId; // Firebase UID ����
            SaveUidLocally(userId); // UID�� ���� ����
            Debug.LogFormat($"�͸� �α��� ����! UID: {userId}");

            // uid �� ������ ����.
            SaveUserData();
            // ������ �α��� �ð� ����.
            UpdateLastLogin();
        });
    }

    /// <summary>
    /// ���� UID �˻� �� �ش� �����Ͱ� ���� �� �͸� �α������� ����.
    /// </summary>
    private void CheckAndInitializeUserData()
    {
        string localUid = LoadUidFromLocal();

        if (!string.IsNullOrEmpty(localUid))
        {
            Debug.Log($"���� ����� UID �߰�: {localUid}");
            VerifyUserExistsInFirebase(localUid);
        }
        else
        {
            Debug.Log("���� UID�� �����ϴ�. ���� �α��� ����...");
            AnonymouslyLogin();
        }
    }

    /// <summary>
    /// ���̾�̽� �����Ͱ� ����� �ش� ���̵�� �α���
    /// ������ �͸� �α������� �����ϴ� �޼���.
    /// </summary>
    /// <param name="uid"></param>
    private void VerifyUserExistsInFirebase(string uid)
    {
        DatabaseReference userRef = dataBase.GetReference($"users/{uid}");

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                // Permission Denied ó��
                Debug.LogError("Firebase ���� ����: " + task.Exception);
                Debug.Log("Permission Denied �Ǵ� �ٸ� ���� �߻�. ���� UID �ʱ�ȭ �� �� �α��� ����...");
                AnonymouslyLogin();
                return;
            }

            if (task.IsCompleted && task.Result.Exists)
            {
                Debug.Log($"Firebase���� UID {uid} �߰�!");
                userId = uid;
                UpdateLastLogin();
                NotifyInitializationComplete();
            }
            else
            {
                Debug.Log("Firebase�� �ش� UID ����. ���� UID ���� �� �� �α��� ����...");
                AnonymouslyLogin();
            }
        });
    }

    /// <summary>
    /// ������ ���̾� ���̽��� ���� ������ ���� �޼���.
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
                Debug.Log("����� ������ ���� �Ϸ�!");
                UpdateLastLogin();
                NotifyInitializationComplete();
                return;
            }
            else
            {
                Debug.LogError("����� ������ ���� ����: " + task.Exception);
            }
        });
    }

    /// <summary>
    /// �������� ��� ���̾� ���̽��� ���� �޼���. 
    /// </summary>
    public void SaveStageResult(int stageID, float playedTime, int starCount)
    {

    }

    /// <summary>
    /// �ֱ� �α����� ����� �����ϴ� �޼���.
    /// </summary>
    public void UpdateLastLogin()
    {
        Debug.Log("ĳ���� ������ �α��� ����!");
        Debug.Log("������ �����͸� ��� ���ž��� - (�ڵ� ���Ƶ�!)");

        //DatabaseReference userRef = dataBase.GetReference($"users/{userId}/lastLogin");

        //userRef.SetValueAsync(DateTime.Now.ToString("o")).ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsCompleted && !task.IsFaulted)
        //    {
        //        Debug.Log("������ �α��� �ð� ������Ʈ ����!");
        //    }
        //    else
        //    {
        //        Debug.LogError("������ �α��� �ð� ������Ʈ ����: " + task.Exception);
        //    }
        //});
    }

    /// <summary>
    /// �α��� �Ϸ� ������ �˸��� �޼���.
    /// </summary>
    public void NotifyInitializationComplete()
    {
        OnFirebaseInitialized?.Invoke();
    }

    /// <summary>
    /// ���ÿ� ������ UID�� ����.
    /// </summary>
    /// <param name="uid"></param>
    private void SaveUidLocally(string uid)
    {
        PlayerPrefs.SetString("firebase_uid", uid);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ���ÿ� ����� UID�� �ҷ����� �޼���.
    /// ������ Empty�� ��ȯ.
    /// </summary>
    /// <returns></returns>
    private string LoadUidFromLocal()
    {
        return PlayerPrefs.GetString("firebase_uid", string.Empty);
    }

    /// <summary>
    /// ���ÿ� ����� UID�� �����ϴ� �޼���.
    /// </summary>
    private void ClearLocalUid()
    {
        PlayerPrefs.DeleteKey("firebase_uid");
        PlayerPrefs.Save();
        userId = string.Empty;
    }
}
