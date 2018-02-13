using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHealth.Tasks {

  /// <summary>
  /// Вывод в лог значения
  /// </summary>
  /// <remarks>Предназначено для отладки, вывода в лог каких-то значений</remarks>
  class Echo : ITask {
    /// <summary>Значение для вывода в лог</summary>
    private string _value;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration) {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _value = declaration.Attribute("value").Value;
      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters) {
      Application.Log(LogLevel.Informational, parameters.Parse(_value).First());
    }


    public string GetDescription()
    {
        return "Вывод значения в лог.";
    }

    public string GetHelp()
    {
        throw new NotImplementedException();
    }
  }
}
