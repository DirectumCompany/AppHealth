using AppHealth.Core;
using System;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Заснуть на указанное время
  /// </summary>
  /// <remarks>Сделано для отладки</remarks>
  class Sleep : ITask
  {
    /// <summary>время ожидания</summary>
    private int _timeout;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      int.TryParse(declaration.Attribute("timeout").Value, out _timeout);
      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      if (_timeout > 0) System.Threading.Thread.Sleep(_timeout);
    }


    public string GetDescription()
    {
      return "Приостанавливает выполнение текущей конфигурации на указанный период.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
