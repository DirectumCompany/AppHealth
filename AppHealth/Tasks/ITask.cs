using AppHealth.Core;

namespace AppHealth.Tasks
{
  public interface ITask
  {
    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    ITask Parse(System.Xml.Linq.XElement declaration);
    /// <summary>Получение информации по задаче</summary>
    /// <returns></returns>
    string GetDescription();

    /// <summary>Получение справки по задача</summary>
    /// <returns></returns>
    string GetHelp();

    /// <summary>
    /// Запуск задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    void Run(ParameterProvider parameters);
  }
}
