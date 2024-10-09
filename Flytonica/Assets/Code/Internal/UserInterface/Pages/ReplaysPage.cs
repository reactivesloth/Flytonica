using System;
using System.Collections.Generic;
using System.Globalization;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.Replays;
using Code.Internal.UserInterface.Elements.TableElements;
using FishNet;
using TMPro;
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

        public void Init(int id)
        {
            _currentUserId = id;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            delete.onClick.AddListener(Delete);
            view.onClick.AddListener(View);

            replaysRoot.SelectionStateChange += SetButtons;

            SetButtons(replaysRoot.SelectedButton);
            GenerateList();
        }

        protected override void OnClose()
        {
            base.OnClose();

            delete.onClick.RemoveListener(Delete);
            view.onClick.RemoveListener(View);

            replaysRoot.SelectionStateChange += SetButtons;
        }

        private void GenerateList()
        {
            HttpClient.Get(LinkConstants.LogsMultiUrl(new Dictionary<string, string>
                { { "user_id", _currentUserId.ToString() }, { "page", "1" }, { "itemsPerPage", "9999" } }), response =>
            {
                var list = JsonUtility.FromJson<MultiLogDataResponse>(response).data;
                var generateData = new List<TableButtonGenerateData<LogData>>();

                foreach (var replayData in list)
                {
                    print(replayData.created_at);
                    var display = new[]
                    {
                        DateTime.Parse(replayData.created_at).ToString(CultureInfo.InvariantCulture),
                        replayData.user_name, replayData.scenario_name, "-", "-"
                    };
                    var data = new TableButtonGenerateData<LogData>(display, replayData);
                    generateData.Add(data);
                }

                replaysRoot.Generate(generateData);
            }, (error, code) => Debug.LogError(error));
        }

        private void Delete()
        {
            //TODO: Init popup
        }

        private void View()
        {
            //TODO: Init and show ReplayViewPage
            var replayData = replaysRoot.SelectedButton.GetSaveData<LogData>();
            /*HttpClient.GetBinary(LinkConstants.GetFile(replayData.replay_file_path), onSuccess: bytes =>
            {
                var fileName = $"{replayData.user_scenario_id}.replay"; 
                var filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

                System.IO.File.WriteAllBytes(filePath, bytes);
                
                ReplayRecorder.Instance.StartPlayback(replayData.user_scenario_id);
            });*/
            ReplayController.Instance.StartPlayback(replayData.user_scenario_id);
            viewReplayPage?.Open();
        }

        private void SetButtons(bool isSelect)
        {
            delete.gameObject.SetActive(isSelect);
            view.gameObject.SetActive(isSelect);
        }
    }
}