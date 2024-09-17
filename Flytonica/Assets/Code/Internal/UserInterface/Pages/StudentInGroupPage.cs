using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.UserInterface.Elements.TableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class StudentInGroupPage : Page
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private SelectionCollectionManager studentsRoot;
        [SerializeField] private Button showReplays, setTask;
        [SerializeField] private UserTaskControlPage userTaskControlPage;

        private GroupData _currentGroupData;

        protected override void OnOpen()
        {
            base.OnOpen();
            
            showReplays.onClick.AddListener(ShowReplays);
            setTask.onClick.AddListener(SetTask);
            studentsRoot.SelectionStateChange += SetButtons;
            
            SetButtons(studentsRoot.SelectedButton);
        }

        protected override void OnClose()
        {
            base.OnClose();
            
            showReplays.onClick.RemoveListener(ShowReplays);
            setTask.onClick.RemoveListener(SetTask);
            studentsRoot.SelectionStateChange -= SetButtons;
        }

        public void Init(int groupId)
        {
            HttpClient.Get(LinkConstants.GetGroup(groupId), response =>
            {
                _currentGroupData = JsonUtility.FromJson<GroupData>(response);
                var generateData = new List<TableButtonGenerateData<StudentData>>();

                foreach (var studentData in _currentGroupData.members.data)
                {
                    var display = new[] { studentData.user_name };
                    var data = new TableButtonGenerateData<StudentData>(display, studentData);
                    generateData.Add(data);
                }

                studentsRoot.Generate(generateData);
                titleText.text = _currentGroupData.name;
            }, Debug.LogError);
        }

        private void ShowReplays()
        {
            
        }

        private void SetTask()
        {
            userTaskControlPage.Init(studentsRoot.SelectedButton.GetSaveData<StudentData>().user_id);
            userTaskControlPage.Open();
        }

        private void SetButtons(bool isSelected)
        {
            showReplays?.gameObject.SetActive(isSelected);
            setTask?.gameObject.SetActive(isSelected);
        }
    }
}