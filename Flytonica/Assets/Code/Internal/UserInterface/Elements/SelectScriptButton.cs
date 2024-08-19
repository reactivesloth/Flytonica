using System.Collections.Generic;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements
{
    public class SelectScriptButton : MonoBehaviour
    {
        [SerializeField] private GameObject listPrefab;
        
        private bool _isOpenList = false;
        private List<SelectScriptButton> _childButtonsList;

        public void Init(List<SelectScriptButton> buttons)
        {
            
        }
        
        public void Init(/*структура сценария*/)
        {
            
        }
    }
}