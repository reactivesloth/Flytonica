using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.UserInterface.Elements.TableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class UserTaskControlPage : Page
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private SelectionCollectionManager currentUserTasksRoot, availableTasksRoot;
        [SerializeField] private Button deleteTask, setTask;

        private int _currentUserId;

        public void Init(int userId)
        {
            _currentUserId = userId;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            deleteTask?.onClick.AddListener(Delete);
            setTask?.onClick.AddListener(Add);

            InitCurrentTasks();
            InitAvailableTasks();

            currentUserTasksRoot.SelectionStateChange += OnSelectStateCurrentTask;
            availableTasksRoot.SelectionStateChange += OnSelectStateAvailableTask;

            OnSelectStateCurrentTask(currentUserTasksRoot.SelectedButton);
            OnSelectStateAvailableTask(availableTasksRoot.SelectedButton);
        }

        protected override void OnClose()
        {
            base.OnClose();
            deleteTask?.onClick.RemoveListener(Delete);
            setTask?.onClick.RemoveListener(Add);

            currentUserTasksRoot.SelectionStateChange -= OnSelectStateCurrentTask;
            availableTasksRoot.SelectionStateChange -= OnSelectStateAvailableTask;
        }

        private void InitCurrentTasks()
        {
            HttpClient.Get(
                LinkConstants.UserScenarioUrl(_currentUserId,
                    new Dictionary<string, string> { { "page", "1" }, { "itemsPerPage", "9999" } }), response =>
                {
                    var tasks = JsonUtility.FromJson<MultiAssignedScenarioDataResponse>(response).data;
                    var generateData = new List<TableButtonGenerateData<AssignedScenarioData>>();

                    foreach (var taskData in tasks)
                    {
                        var display = new[] { taskData.scenario_name };
                        var data = new TableButtonGenerateData<AssignedScenarioData>(display, taskData);
                        generateData.Add(data);
                    }

                    currentUserTasksRoot.Generate(generateData);
                }, Debug.LogError);
        }

        private void InitAvailableTasks()
        {
            HttpClient.Get(LinkConstants.ScenarioMultiUrl(new Dictionary<string, string> { { "page", "1" }, { "itemsPerPage", "9999" } }), response =>
            {
                var tasks = JsonUtility.FromJson<MultiScenarioDataResponse>(response).data;
                var generateData = new List<TableButtonGenerateData<ScenarioData>>();

                foreach (var taskData in tasks)
                {
                    var display = new[] { taskData.name };
                    var data = new TableButtonGenerateData<ScenarioData>(display, taskData);
                    generateData.Add(data);
                }

                availableTasksRoot.Generate(generateData);
            }, Debug.LogError);
        }

        private void Delete()
        {
            var currentData = currentUserTasksRoot.SelectedButton.GetSaveData<AssignedScenarioData>();
            HttpClient.Delete(LinkConstants.UserScenarioUrl(currentData.id), response =>
            {
                Debug.Log(response);
                InitCurrentTasks();
            }, Debug.LogError);
        }

        private void Add()
        {
            var currentData = availableTasksRoot.SelectedButton.GetSaveData<ScenarioData>();

            var formData = new WWWForm();
            formData.AddField("scenario_id", currentData.id);
            formData.AddField("user_id", _currentUserId);

            HttpClient.PostFormData(LinkConstants.UserScenarioUrl(), formData,
                response =>
                {
                    Debug.Log(response);
                    InitCurrentTasks();
                }, Debug.LogError);
        }

        private void OnSelectStateCurrentTask(bool isSelect)
        {
            deleteTask.gameObject.SetActive(isSelect);
            if (isSelect)
                availableTasksRoot.Unselect();
        }

        private void OnSelectStateAvailableTask(bool isSelect)
        {
            setTask.gameObject.SetActive(isSelect);
            if (isSelect)
                currentUserTasksRoot.Unselect();
        }
    }
}