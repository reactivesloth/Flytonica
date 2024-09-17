namespace Code.Internal.API.Wrappers.SendModels
{
    
    [System.Serializable]
    public class AssigneScenarioData
    {
        public int scenario_id;
        public int user_id;

        public AssigneScenarioData(int scenarioID, int userID)
        {
            scenario_id = scenarioID;
            user_id = userID;
        }
    }
}