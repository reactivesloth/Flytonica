using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Code.Internal.Scenario
{
    public class ReportBuilder : MonoBehaviour
    {
        private static ReportBuilder _instance;

        public static ReportBuilder Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ReportBuilder>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("ReportBuilder");
                        _instance = singletonObject.AddComponent<ReportBuilder>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return _instance;
            }
        }

        private readonly Dictionary<string, string> _parameters = new();
        private readonly Dictionary<string, float> _numericParameters = new();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public void Clear()
        {
            _parameters.Clear();
            _numericParameters.Clear();
        }

        public void AddParameter(string key, string value)
        {
            if (_parameters.ContainsKey(key))
            {
                _parameters[key] = value;
            }
            else
            {
                _parameters.Add(key, value);
            }
        }

        public void AddParameter(string key, float numericValue)
        {
            if (_numericParameters.ContainsKey(key))
            {
                _numericParameters[key] = numericValue;
            }
            else
            {
                _numericParameters.Add(key, numericValue);
            }
        }

        public void UpdateParameter(string key, string value)
        {
            if (_parameters.ContainsKey(key))
            {
                _parameters[key] = value;
            }
            else
            {
                throw new KeyNotFoundException($"Parameter with key '{key}' not found.");
            }
        }

        public void UpdateParameter(string key, float numericValue)
        {
            if (_numericParameters.ContainsKey(key))
            {
                _numericParameters[key] = numericValue;
            }
            else
            {
                throw new KeyNotFoundException($"Numeric parameter with key '{key}' not found.");
            }
        }

        public void IncrementNumericParameter(string key, float incrementValue)
        {
            if (_numericParameters.ContainsKey(key))
            {
                _numericParameters[key] += incrementValue;
            }
            else
            {
                throw new KeyNotFoundException($"Numeric parameter with key '{key}' not found.");
            }
        }

        public float GetNumericParameter(string key)
        {
            if (_numericParameters.ContainsKey(key))
            {
                return _numericParameters[key];
            }
            else
            {
                throw new KeyNotFoundException($"Numeric parameter with key '{key}' not found.");
            }
        }

        public string GenerateJsonReport()
        {
            var flatDictionary = new Dictionary<string, string>(_parameters);
            foreach (var kvp in _numericParameters)
            {
                flatDictionary[kvp.Key] = kvp.Value.ToString();
            }

            return BuildJsonString(flatDictionary);
        }

        #region JSON Generation
        
        private string BuildJsonString(Dictionary<string, string> flatDictionary)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool first = true;
            foreach (var kvp in flatDictionary)
            {
                if (!first)
                {
                    sb.Append(",");
                }

                sb.Append("\"");
                sb.Append(EscapeString(kvp.Key));
                sb.Append("\":\"");
                sb.Append(EscapeString(kvp.Value));
                sb.Append("\"");
                first = false;
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string EscapeString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                switch (c)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < 32 || c > 126)
                        {
                            sb.AppendFormat("\\u{0:X4}", (int)c);
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            return sb.ToString();
        }
        
        #endregion
    }
}

// КАК использовать
// ReportBuilder.Instance.AddParameter("Количество участников", 5f);
// ReportBuilder.Instance.UpdateParameter("Количество участников", 10f);
// ReportBuilder.Instance.IncrementNumericParameter("Количество участников", 2.5f);
// float value = ReportBuilder.Instance.GetNumericParameter("Количество участников");
// Console.WriteLine(value);
// string jsonReport = ReportBuilder.Instance.GenerateJsonReport();
// Console.WriteLine(jsonReport);