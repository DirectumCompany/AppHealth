using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AppHealth.Core
{
  public class ParameterProvider
  {
    readonly private Dictionary<string, string> _params;
    internal DateTime _fromDate;
    internal DateTime _toDate;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="parameters">Список параметров</param>
    public ParameterProvider(Dictionary<string, string> parameters)
    {
      _params = parameters;
    }

    public ReadOnlyDictionary<string, string> GetParameters()
    {
      return new ReadOnlyDictionary<string, string>(_params);
    }

    /// <summary>
    /// Добавление нового параметра
    /// </summary>
    /// <param name="Key">Имя параметра</param>
    /// <param name="Value">Значение параметра</param>
    /// <remarks>Если параметр уже существует, он будет перезаписан</remarks>
    /// <remarks>Не хотел делать, но пришлось</remarks>
    internal void AddParameter(string Key, string Value)
    {
      _params[Key] = Value;
    }

    internal void SetPeriod(DateTime fromDate, DateTime toDate)
    {
      _fromDate = fromDate;
      _toDate = toDate;
      _params["FromDate"] = _fromDate.ToString("yyyy-MM-dd");
      _params["ToDate"] = _toDate.ToString("yyyy-MM-dd");
    }

    /// <summary>Подстановка параметров в строку</summary>
    /// <param name="value">Строка формата</param>
    /// <returns>Строка с заменеными параметрами</returns>
    public IEnumerable<string> Parse(string value)
    {
      if (string.IsNullOrWhiteSpace(value)) yield return string.Empty;
      var regex = new Regex("{day:(?<format>.*)}");

      foreach (var param in _params)
        value = value.Replace(string.Format("%{0}%", param.Key), param.Value);

      var match = regex.Match(value);
      if (match.Success)
      {
        var start = _fromDate;
        while (start < _toDate)
        {
          yield return value.Replace(match.Value, start.ToString(match.Groups["format"].Value));
          start = start.AddDays(1);
        }
      }
      else
        yield return value;
    }
  }
}
