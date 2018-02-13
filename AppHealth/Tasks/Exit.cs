using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHealth.Tasks {

  /// <summary>
  /// Прерывание выполнение текущей конфигурации
  /// </summary>

  class Exit : ITask {
    
    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration) {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters) {
      throw new OperationCanceledException("Выход из обработки конфигурации.");
    }


    public string GetDescription()
    {
        return "Прерывание выполнения текущей конфигурации.";
    }

    public string GetHelp()
    {
        throw new NotImplementedException();
    }
  }
}
