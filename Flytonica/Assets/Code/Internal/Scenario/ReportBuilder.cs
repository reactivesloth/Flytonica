using System;
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

        private readonly List<Parameter<string>> _parameters = new();
        private readonly List<Parameter<float>> _floatParameters = new();
        private readonly List<Parameter<int>> _intParameters = new();

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
            _floatParameters.Clear();
            _intParameters.Clear();
        }

        // Методы для строковых параметров
        public void AddParameter(string key, string value)
        {
            var param = _parameters.Find(p => p.Key == key);
            if (param != null)
            {
                param.Values.Add(value);
            }
            else
            {
                _parameters.Add(new Parameter<string>(key, value));
            }
        }

        public void UpdateParameter(string key, string value)
        {
            var param = _parameters.Find(p => p.Key == key);
            if (param != null)
            {
                if (param.Values.Count > 0)
                {
                    param.Values[param.Values.Count - 1] = value;
                }
                else
                {
                    param.Values.Add(value);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Parameter with key '{key}' not found.");
            }
        }

        // Методы для параметров типа float
        public void AddParameter(string key, float value)
        {
            var param = _floatParameters.Find(p => p.Key == key);
            if (param != null)
            {
                param.Values.Add(value);
            }
            else
            {
                _floatParameters.Add(new Parameter<float>(key, value));
            }
        }

        public void UpdateParameter(string key, float value)
        {
            var param = _floatParameters.Find(p => p.Key == key);
            if (param != null)
            {
                if (param.Values.Count > 0)
                {
                    param.Values[param.Values.Count - 1] = value;
                }
                else
                {
                    param.Values.Add(value);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Numeric parameter with key '{key}' not found.");
            }
        }

        public void IncrementNumericParameter(string key, float incrementValue)
        {
            var param = _floatParameters.Find(p => p.Key == key);
            if (param != null)
            {
                if (param.Values.Count > 0)
                {
                    param.Values[param.Values.Count - 1] += incrementValue;
                }
                else
                {
                    throw new InvalidOperationException($"No value to increment for key '{key}'.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"Numeric parameter with key '{key}' not found.");
            }
        }

        public float GetNumericParameter(string key)
        {
            var param = _floatParameters.Find(p => p.Key == key);
            if (param != null)
            {
                if (param.Values.Count > 0)
                {
                    return param.Values[param.Values.Count - 1];
                }
                else
                {
                    throw new InvalidOperationException($"No value to get for key '{key}'.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"Numeric parameter with key '{key}' not found.");
            }
        }

        // Методы для параметров типа int
        public void AddParameter(string key, int value)
        {
            var param = _intParameters.Find(p => p.Key == key);
            if (param != null)
            {
                param.Values.Add(value);
            }
            else
            {
                _intParameters.Add(new Parameter<int>(key, value));
            }
        }

        public string GenerateJsonReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool first = true;

            foreach (var param in _parameters)
            {
                if (!first) sb.Append(",");
                sb.Append($"\"{EscapeString(param.Key)}\":");
                sb.Append(BuildJsonArray(param.Values));
                first = false;
            }

            foreach (var param in _floatParameters)
            {
                if (!first) sb.Append(",");
                sb.Append($"\"{EscapeString(param.Key)}\":");
                sb.Append(BuildJsonArray(param.Values));
                first = false;
            }

            foreach (var param in _intParameters)
            {
                if (!first) sb.Append(",");
                sb.Append($"\"{EscapeString(param.Key)}\":");
                sb.Append(BuildJsonArray(param.Values));
                first = false;
            }

            sb.Append("}");
            return sb.ToString();
        }

        #region Вспомогательные методы

        private string BuildJsonArray<T>(List<T> values)
        {
            if (values.Count == 1)
            {
                return FormatJsonValue(values[0]);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                for (int i = 0; i < values.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(FormatJsonValue(values[i]));
                }
                sb.Append("]");
                return sb.ToString();
            }
        }

        private string FormatJsonValue<T>(T value)
        {
            if (value is string)
            {
                return $"\"{EscapeString(value.ToString())}\"";
            }
            else if (value is float || value is double || value is int || value is long || value is decimal)
            {
                return value.ToString();
            }
            else
            {
                // Для других типов данных сериализуем как строку
                return $"\"{EscapeString(value.ToString())}\"";
            }
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

        // Класс Parameter
        private class Parameter<T>
        {
            public string Key { get; }
            public List<T> Values { get; }

            public Parameter(string key, T value)
            {
                Key = key;
                Values = new List<T> { value };
            }
        }
    }
}

// КАК использовать
// ReportBuilder.Instance.AddParameter("Количество участников", 5f);
// ReportBuilder.Instance.AddParameter("Количество участников", 10f);
// ReportBuilder.Instance.IncrementNumericParameter("Количество участников", 2.5f);
// float value = ReportBuilder.Instance.GetNumericParameter("Количество участников");
// Console.WriteLine(value);
// string jsonReport = ReportBuilder.Instance.GenerateJsonReport();
// Console.WriteLine(jsonReport);