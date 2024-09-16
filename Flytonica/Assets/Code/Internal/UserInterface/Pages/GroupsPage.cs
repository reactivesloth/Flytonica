using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.UserInterface.Elements.TableElements;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class GroupsPage: Page
    {
        [SerializeField] private SelectionCollectionManager groupsRoot;
        [SerializeField] private Button delete, create, edit, showStudents, update;

        protected override void OnOpen()
        {
            base.OnOpen();
            
            delete?.onClick.AddListener(Delete);
            create?.onClick.AddListener(Create);
            edit?.onClick.AddListener(Edit);
            showStudents?.onClick.AddListener(ShowStudent);
            update?.onClick.AddListener(UpdateList);
            
            InitList();
        }

        protected override void OnClose()
        {
            base.OnClose();
            
            delete?.onClick.RemoveListener(Delete);
            create?.onClick.RemoveListener(Create);
            edit?.onClick.RemoveListener(Edit);
            showStudents?.onClick.RemoveListener(ShowStudent);
            update?.onClick.RemoveListener(UpdateList);
        }

        private void InitList()
        {
            HttpClient.Get(LinkConstants.GroupsMulti(), response =>
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
            }, Debug.LogError);
        }

        private void Delete()
        {
            
        }

        private void Create()
        {
            
        }

        private void Edit()
        {
            
        }

        private void ShowStudent()
        {
            
        }

        private void UpdateList()
        {
            
        }
    }
}