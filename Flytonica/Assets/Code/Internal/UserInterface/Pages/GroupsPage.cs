using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.UserInterface.Elements.TableElements;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class GroupsPage : Page
    {
        [SerializeField] private SelectionCollectionManager groupsRoot;
        [SerializeField] private Button showStudents, loadedReplays;
        [SerializeField] private StudentInGroupPage studentsPage;
        [SerializeField] private ReplaysPage replaysPage;

        protected override void OnOpen()
        {
            base.OnOpen();

            showStudents?.onClick.AddListener(ShowStudent);
            loadedReplays?.onClick.AddListener(OnLoadedReplayClick);
            groupsRoot.SelectionStateChange += SetButtons;

            SetButtons(groupsRoot.SelectedButton);
            InitList();
        }

        protected override void OnClose()
        {
            base.OnClose();

            showStudents?.onClick.RemoveListener(ShowStudent);
            loadedReplays?.onClick.RemoveListener(OnLoadedReplayClick);
            groupsRoot.SelectionStateChange -= SetButtons;
        }

        private void InitList()
        {
            HttpClient.Get(
                LinkConstants.GroupsMulti(
                    new Dictionary<string, string> { { "page", "1" }, { "itemsPerPage", "9999" } }), response =>
                {
                    var groups = JsonUtility.FromJson<MultiGroupDataResponse>(response).data;
                    var generateData = new List<TableButtonGenerateData<GroupData>>();

                    foreach (var groupData in groups)
                    {
                        var display = new[] { groupData.name };
                        var data = new TableButtonGenerateData<GroupData>(display, groupData);
                        generateData.Add(data);
                    }

                    groupsRoot.Generate(generateData);
                }, (error, code) => Debug.LogError(error));
        }

        private void ShowStudent()
        {
            studentsPage.Init(groupsRoot.SelectedButton.GetSaveData<GroupData>().id);
            studentsPage.Open();
        }

        private void SetButtons(bool isSelected)
        {
            showStudents?.gameObject.SetActive(isSelected);
        }
        
        private void OnLoadedReplayClick()
        {
            print("Clicked on the loaded replay page");
            replaysPage.InitLocal();
            replaysPage.Open();
        }
    }
}