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

        private int _prefixNumber = 1;
        
        // Единый список для всех параметров
        private readonly List<Parameter<string>> _parameters = new();

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

        // Очистка всех параметров
        public void Clear()
        {
            _prefixNumber = 1;
            _parameters.Clear();
        }

        public void AddPrefix() => _prefixNumber++;

        #region Методы для параметров

        public void AddParameter(string key, string value, bool isPrefix = true)
        {
            key = isPrefix ? $"({_prefixNumber}) {key}" : key;
            _parameters.Add(new Parameter<string>(key, value));
        }

        public void AddParameter(string key, float value)
        {
            AddParameter(key, value.ToString());
        }

        public void AddParameter(string key, int value)
        {
            AddParameter(key, value.ToString());
        }

        public void UpdateParameter(string key, string value)
        {
            var param = FindLastParameter(key);
            if (param != null)
            {
                param.Value = value;
            }
            else
            {
                throw new KeyNotFoundException($"Parameter with key '{key}' not found.");
            }
        }

        public void UpdateParameter(string key, float value)
        {
            var param = FindLastParameter(key);
            if (param != null)
            {
                param.Value = value.ToString();
            }
            else
            {
                throw new KeyNotFoundException($"Parameter with key '{key}' not found.");
            }
        }

        public void UpdateParameter(string key, int value)
        {
            var param = FindLastParameter(key);
            if (param != null)
            {
                param.Value = value.ToString();
            }
            else
            {
                throw new KeyNotFoundException($"Parameter with key '{key}' not found.");
            }
        }

        public void IncrementNumericParameter(string key, float incrementValue)
        {
            var param = FindLastParameter(key);
            if (param != null)
            {
                if (float.TryParse(param.Value, out float currentValue))
                {
                    currentValue += incrementValue;
                    param.Value = currentValue.ToString();
                }
                else
                {
                    throw new InvalidOperationException($"Parameter with key '{key}' is not a numeric value.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"Parameter with key '{key}' not found.");
            }
        }

        public float GetNumericParameter(string key)
        {
            var param = FindLastParameter(key);
            if (param != null)
            {
                if (float.TryParse(param.Value, out float value))
                {
                    return value;
                }
                else
                {
                    throw new InvalidOperationException($"Parameter with key '{key}' is not a numeric value.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"Parameter with key '{key}' not found.");
            }
        }

        #endregion

        // Генерация JSON отчёта
        public string GenerateJsonReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool first = true;

            // Группируем параметры по ключам
            var groupedParameters = GroupParametersByKey(_parameters);
            foreach (var group in groupedParameters)
            {
                foreach (var value in group.Value)
                {
                    if (!first) sb.Append(",");
                    sb.Append($"\"{EscapeString(group.Key)}\":");

                    // Проверяем, является ли значение числовым
                    if (float.TryParse(value, out float numericValue))
                    {
                        sb.Append(numericValue);
                    }
                    else
                    {
                        sb.Append($"\"{EscapeString(value)}\"");
                    }

                    first = false;
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        #region Вспомогательные методы

        // Группировка параметров по ключу
        private Dictionary<string, List<string>> GroupParametersByKey(List<Parameter<string>> parameters)
        {
            var grouped = new Dictionary<string, List<string>>();
            foreach (var param in parameters)
            {
                if (grouped.ContainsKey(param.Key))
                {
                    grouped[param.Key].Add(param.Value);
                }
                else
                {
                    grouped[param.Key] = new List<string> { param.Value };
                }
            }
            return grouped;
        }

        // Экранирование строк для JSON
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

        // Поиск последнего параметра с заданным ключом
        private Parameter<string> FindLastParameter(string key)
        {
            for (int i = _parameters.Count - 1; i >= 0; i--)
            {
                if (_parameters[i].Key.Equals(key, StringComparison.Ordinal))
                {
                    return _parameters[i];
                }
            }
            return null;
        }

        #endregion

        // Внутренний класс для представления параметра
        private class Parameter<T>
        {
            public string Key { get; }
            public T Value { get; set; }

            public Parameter(string key, T value)
            {
                Key = key;
                Value = value;
            }
        }
    }

    // Пример использования
    /*
    // Добавление строковых параметров
    ReportBuilder.Instance.AddParameter("Имя игрока", "Алексей");
    ReportBuilder.Instance.AddParameter("Имя игрока", "Ирина"); // Будет добавлено как "Имя игрока_2"

    // Добавление числовых параметров
    ReportBuilder.Instance.AddParameter("Время игры", 15.5f);
    ReportBuilder.Instance.IncrementNumericParameter("Время игры", 2.5f); // Обновит последнее значение до 18.0f

    ReportBuilder.Instance.AddParameter("Уровень", 3);
    ReportBuilder.Instance.AddParameter("Уровень", 4); // Будет добавлено как "Уровень_2"

    // Получение значения числового параметра
    float времяИгры = ReportBuilder.Instance.GetNumericParameter("Время игры");
    Console.WriteLine(времяИгры); // Выведет 18.0

    // Генерация JSON отчёта
    string jsonReport = ReportBuilder.Instance.GenerateJsonReport();
    Console.WriteLine(jsonReport);
    // Выведет:
    // {
    //   "Имя игрока_1":"Алексей",
    //   "Имя игрока_2":"Ирина",
    //   "Время игры_1":15.5,
    //   "Время игры_2":18.0,
    //   "Уровень_1":3,
    //   "Уровень_2":4
    // }
    */
}
