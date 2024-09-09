using Code.Internal.UserInterface.Elements;
using UnityEngine;

namespace Code.Internal.UserInterface.Pages
{
    public class ScenariosPage: Page
    {
        [SerializeField] private Table scenariosTable;

        protected override void OnOpen()
        {
            base.OnOpen();
            
            //TODO: Запрос и вызов GenerateTable
        }

        private void GenerateTable()
        {
            //TODO: Генерация таблицы 
        }
    }
}