using AppHealth.Core;
using AppHealth.Tasks;
using System;
using System.Collections.Generic;

namespace AppHealth.Configurations
{
  class Configuration
  {
    /// <summary>
    /// Наименование конфигурации
    /// </summary>
    public string Name { get { return _name; } }
    readonly private string _name;

    /// <summary>
    /// Описание конфигурации
    /// </summary>
    public string Description { get { return _description; } }
    private string _description;

    public ParameterProvider Parameters { get { return _parameters; } }
    readonly private ParameterProvider _parameters;
    readonly private List<ITask> _tasks;

    /// <summary>
    /// Конструктор конфигурации
    /// </summary>
    /// <param name="name">Наименование</param>
    /// <param name="tasks">Список задач</param>
    /// <param name="parameters">Список параметров</param>
    public Configuration(string name, List<ITask> tasks, ParameterProvider parameters, string description)
    {
      _name = name;
      _tasks = tasks;
      _parameters = parameters;
      _description = description;
    }

    /// <summary>
    /// Запуск конфигурации
    /// </summary>
    /// <param name="fromDate">Дата начала анализа</param>
    /// <param name="toDate">Дата завершения анализа</param>
    public void Run(DateTime fromDate, DateTime toDate)
    {
      _parameters.SetPeriod(fromDate, toDate);
      foreach (var task in _tasks)
      {
        Application.Log(LogLevel.Debug, "  Выполнение задачи '{0}' конфигурации '{1}'", task.GetType().Name, _name);
        task.Run(_parameters);
        Application.Log(LogLevel.Debug, "  выполнено");
      }
    }
  }
}
