using System;
using System.Collections;
using Code.Internal.API.Wrappers;
using Code.Internal.API.Wrappers.ReceiveModels;
using UnityEngine;
using UnityEngine.Networking;

namespace Code.Internal.API
{
    public class HttpClient : MonoBehaviour
    {
        public static AuthResponseData AuthData { get; private set; }
        public static UserData UserData { get; private set; }
        public static bool IsAuthorized => AuthData != null;

        public static void SetAuthData(AuthResponseData authData) => AuthData = authData;
        public static void SetUserData(UserData userData) => UserData = userData;

        public static void Logout()
        {
            AuthData = null;
            UserData = null;
        }

        public static void Get(string url, Action<string> onSuccess = null, Action<string, long> onError = null, Action callback= null)
        {
            var instance = CreateInstance(url);
            instance.StartCoroutine(instance
                .SendRequestProcess(url, UnityWebRequest.kHttpVerbGET, null, onSuccess, onError, callback));
        }
        
        public static void GetBinary(string url, Action<byte[]> onSuccess = null, Action<string, long> onError = null, Action callback = null)
        {
            var instance = CreateInstance(url);
            instance.StartCoroutine(instance
                .SendBinaryRequestProcess(url, UnityWebRequest.kHttpVerbGET, onSuccess, onError, callback));
        }

        public static void Post(string url, string jsonData, Action<string> onSuccess = null,
            Action<string, long> onError = null, Action callback= null)
        {
            var instance = CreateInstance(url);
            instance.StartCoroutine(instance
                .SendRequestProcess(url, UnityWebRequest.kHttpVerbPOST, jsonData, onSuccess, onError, callback));
        }

        public static void PostFormData(string url, WWWForm formData, Action<string> onSuccess = null,
            Action<string, long> onError = null)
        {
            var instance = CreateInstance(url);
            instance.StartCoroutine(instance
                .SendFormDataRequestProcess(url, formData, onSuccess, onError));
        }

        public static void Delete(string url, Action<string> onSuccess = null, Action<string, long> onError = null, Action callback= null)
        {
            var instance = CreateInstance(url);
            instance.StartCoroutine(instance
                .SendRequestProcess(url, UnityWebRequest.kHttpVerbDELETE, null, onSuccess, onError, callback));
        }

        private IEnumerator SendRequestProcess(string url, string method, string jsonData, Action<string> onSuccess,
            Action<string, long> onError, Action callback)
        {
            var request = new UnityWebRequest(url, method);

            print(url);
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                var bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.SetRequestHeader("Authorization", "Bearer " + AuthData?.access_token);

            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.downloadHandler.text, request.responseCode);
            
            callback?.Invoke();

            Destroy(gameObject);
        }

        private IEnumerator SendFormDataRequestProcess(string url, WWWForm formData, Action<string> onSuccess,
            Action<string, long> onError)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(url, formData))
            {
                request.SetRequestHeader("Authorization", "Bearer " + AuthData?.access_token);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(request.downloadHandler.text);
                else
                    onError?.Invoke(request.downloadHandler.text, request.responseCode);

                Destroy(gameObject);
            }
        }
        
        private IEnumerator SendBinaryRequestProcess(string url, string method, Action<byte[]> onSuccess,
            Action<string, long> onError, Action callback)
        {
            var request = new UnityWebRequest(url, method);

            print(url);

            request.SetRequestHeader("Authorization", "Bearer " + AuthData?.access_token);

            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.data);
            else
                onError?.Invoke(request.downloadHandler.error, request.responseCode);

            callback?.Invoke();

            Destroy(gameObject);
        }

        private static HttpClient CreateInstance(string url)
        {
            var httpClientObject = new GameObject(url);
            return httpClientObject.AddComponent<HttpClient>();
        }
    }
}