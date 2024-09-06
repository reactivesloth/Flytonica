using System;
using System.Collections;
using Code.Internal.API.Wrappers;
using UnityEngine;
using UnityEngine.Networking;

namespace Code.Internal.API
{
    public class HttpClient : MonoBehaviour
    {
        private static AuthResponseData _authData;
        
        public static UserData UserData { get; private set; }
        public static bool IsAuthorized => _authData != null;

        public static void SetAuthData(AuthResponseData authData) => _authData = authData;
        public static void SetUserData(UserData userData) => UserData = userData;

        
        public static void Get(string url, Action<string> onSuccess = null, Action<string> onError = null)
        {
            var instance = CreateInstance();
            instance.StartCoroutine(instance
                .SendRequestProcess(url, UnityWebRequest.kHttpVerbGET, null, onSuccess, onError));
        }

        public static void Post(string url, string jsonData, Action<string> onSuccess = null,
            Action<string> onError = null)
        {
            var instance = CreateInstance();
            instance.StartCoroutine(instance
                .SendRequestProcess(url, UnityWebRequest.kHttpVerbPOST, jsonData, onSuccess, onError));
        }

        private IEnumerator SendRequestProcess(string url, string method, string jsonData, Action<string> onSuccess,
            Action<string> onError)
        {
            var request = new UnityWebRequest(url, method);

            if (!string.IsNullOrEmpty(jsonData))
            {
                var bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.SetRequestHeader("Authorization", "Bearer " + _authData?.access_token);

            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.downloadHandler.text);

            Destroy(gameObject);
        }

        private static HttpClient CreateInstance()
        {
            var httpClientObject = new GameObject("HttpClient");
            return httpClientObject.AddComponent<HttpClient>();
        }
    }
}