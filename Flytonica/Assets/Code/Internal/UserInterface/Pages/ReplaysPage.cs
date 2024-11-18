using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.Replays;
using Code.Internal.UserInterface.Elements.TableElements;
using TMPro;
using UltimateReplay;
using UltimateReplay.Storage;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ReplaysPage : Page
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private SelectionCollectionManager replaysRoot;
        [SerializeField] private Button delete, view;
        [SerializeField] private ViewReplayPage viewReplayPage;

        private int _currentUserId;
        private bool _isLocal;

        public void InitUser(int id)
        {
            _isLocal = false;
            _currentUserId = id;
            GenerateListFromUser();
        }

        public void InitLocal()
        {
            _isLocal = true;
            GenerateListFromLocal();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            delete.onClick.AddListener(Delete);
            view.onClick.AddListener(View);

            replaysRoot.SelectionStateChange += SetButtons;

            SetButtons(replaysRoot.SelectedButton);
            
            print(_isLocal);
        }
        
        protected override void OnClose()
        {
            base.OnClose();

            delete.onClick.RemoveListener(Delete);
            view.onClick.RemoveListener(View);

            replaysRoot.SelectionStateChange -= SetButtons;
            print("Replays Page Closed");
        }

        private void GenerateListFromUser()
        {
            HttpClient.Get(LinkConstants.LogsMultiUrl(new Dictionary<string, string>
                { { "user_id", _currentUserId.ToString() }, { "page", "1" }, { "itemsPerPage", "9999" } }), response =>
            {
                var list = JsonUtility.FromJson<MultiLogDataResponse>(response).data;
                var generateData = new List<TableButtonGenerateData<LogData>>();

                foreach (var replayData in list)
                {
                    var display = new[]
                    {
                        DateTime.Parse(replayData.created_at).ToString(CultureInfo.InvariantCulture),
                        replayData.user_name, replayData.scenario_name, "-", "-"
                    };
                    var data = new TableButtonGenerateData<LogData>(display, replayData);
                    generateData.Add(data);
                }

                HttpClient.Get(LinkConstants.LogsIndividualMultiUrl(new Dictionary<string, string>
                        { { "user_id", _currentUserId.ToString() }, { "page", "1" }, { "itemsPerPage", "9999" } }),
                    response2 =>
                    {
                        var list2 = JsonUtility.FromJson<MultiLogDataResponse>(response2).data;

                        foreach (var replayData in list2)
                        {
                            var display = new[]
                            {
                                DateTime.Parse(replayData.created_at).ToString(CultureInfo.InvariantCulture),
                                replayData.user_name, "", "-", "-"
                            };
                            var data = new TableButtonGenerateData<LogData>(display, replayData);
                            generateData.Add(data);
                        }

                        replaysRoot.Generate(generateData);
                    });
            }, (error, code) => Debug.LogError(error));
        }

        private void GenerateListFromLocal()
        {
            var files = Directory.GetFiles(Application.persistentDataPath, "*.replay");

            var generateData = new List<TableButtonGenerateData<LocalLogData>>();

            foreach (var filePath in files)
            {
                try
                {
                    var replayMeta = (CustomMetadata) ReplayFileStorage.ReadMetadataOnly(filePath);
                    var localLogData = new LocalLogData(filePath, replayMeta);
                    var display = new[]
                    {
                        replayMeta?.date, replayMeta?.studentName, replayMeta?.ReplayName, "-", "-"
                    };
                    var data = new TableButtonGenerateData<LocalLogData>(display, localLogData);
                    generateData.Add(data);
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError(e);
                }
                
            }
            replaysRoot.Generate(generateData);
        }

        private void Delete()
        {
            if (_isLocal)
                DeleteLocal();
            else
                DeleteFromServer();
        }

        private void DeleteFromServer()
        {
            PopupPanel.ConfigurePopup("Вы действительно хотите удалить?",
                $"Вы удалите весь отчёт о прохождении этого задания в ЛМС. Продолжить?",
                null, "Удалить", Color.red, Color.white,
                () =>
                {
                    HttpClient.Delete(LinkConstants.LogDeleteUrl(replaysRoot.SelectedButton.GetSaveData<LogData>().id),
                        callback: GenerateListFromUser);
                },
                null, "Отмена", Color.green, Color.black, null);
        }

        private void DeleteLocal()
        {
            //TODO
        }

        private void View()
        {
            if(_isLocal)
                ViewOffline();
            else
                ViewUser();
            viewReplayPage.Open();
        }

        private void ViewUser()
        {
            var replayData = replaysRoot.SelectedButton.GetSaveData<LogData>();
            viewReplayPage.Init(replayData);
        }

        private void ViewOffline()
        {
            var replayPath = replaysRoot.SelectedButton.GetSaveData<LocalLogData>();
            viewReplayPage.Init(replayPath);
        } 

        private void SetButtons(bool isSelect)
        {
            delete.gameObject.SetActive(isSelect);
            view.gameObject.SetActive(isSelect);
        }
    }

    [Serializable]
    public class LocalLogData
    {
        public string path;
        public CustomMetadata metadata;

        public LocalLogData(string path, CustomMetadata metadata)
        {
            this.path = path;
            this.metadata = metadata;
        }
    }
}