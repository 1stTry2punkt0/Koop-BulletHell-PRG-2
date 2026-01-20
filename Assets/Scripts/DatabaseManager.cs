using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;

    [Header("API Base URL (no trailing slash)")]
    [SerializeField] private string baseUrl = "http://localhost/bulletHell";

    [Serializable]
    private class SaveScoreRequest { public string name; public int score; }

    [Serializable]
    public class ScoreRow { public string name; public int score; }

    [Serializable]
    private class GetScoresResponse { public bool ok; public ScoreRow[] scores; public string error; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //SaveScore("TestPlayer1", 11111);
        //SaveScore("TestPlayer2", 22222);
        //SaveScore("TestPlayer3", 33333);
        //SaveScore("TestPlayer4", 44444);
        FetchTopScores(4, scores =>
        {
            foreach (var score in scores)
            {
                Debug.Log($"{score.name}: {score.score}");
            }
        });
    }


    public void SaveScore(string name, int score)
    {
        StartCoroutine(SaveScoreCoroutine(name, score));
    }

    private IEnumerator SaveScoreCoroutine(string name, int score)
    {
        var url = $"{baseUrl}/save_score.php";

        var reqObj = new SaveScoreRequest { name = name, score = score };
        string json = JsonUtility.ToJson(reqObj);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"SaveScore failed: {req.error}");
            yield break;
        }

        Debug.Log("Score saved: " + req.downloadHandler.text);
    }


    public void FetchTopScores(int limit, Action<ScoreRow[]> onResult)
    {
        StartCoroutine(GetScoresCoroutine(limit, onResult));
    }

    private IEnumerator GetScoresCoroutine(int limit, Action<ScoreRow[]> onResult)
    {
        var url = $"{baseUrl}/get_scores.php?limit={limit}";

        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"FetchScores failed: {req.error}");
            onResult?.Invoke(Array.Empty<ScoreRow>());
            yield break;
        }

        var res = JsonUtility.FromJson<GetScoresResponse>(req.downloadHandler.text);

        if (res == null || !res.ok)
        {
            Debug.LogError("Server error: " + (res?.error ?? "Invalid JSON"));
            onResult?.Invoke(Array.Empty<ScoreRow>());
            yield break;
        }

        onResult?.Invoke(res.scores);
    }

}