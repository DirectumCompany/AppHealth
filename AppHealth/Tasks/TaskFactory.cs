using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace AppHealth.Tasks
{
  static class TaskFactory
  {
    /// <summary>
    /// Коллекция типов задач
    /// </summary>
    internal static Dictionary<string, System.Type> TaskHandlerTypes
    {
      get
      {
        if (_taskHandlerTypes == null)
        {
          lock (_lock)
          {
            if (_taskHandlerTypes == null)
            {
              _taskHandlerTypes = ScanAssemblies();
            }
          }
        }
        return _taskHandlerTypes;
      }
    }
    private static Dictionary<string, System.Type> _taskHandlerTypes = null;
    private static Object _lock = new Object();

    /// <summary> 
    /// Поиск реализаций интерфейса ITask
    /// </summary>
    private static Dictionary<string, System.Type> ScanAssemblies()
    {
      return (from type in Assembly.GetExecutingAssembly().GetTypes()
              where !type.IsAbstract
              where typeof(ITask).IsAssignableFrom(type)
              select type).ToDictionary(t => t.Name, t => t);
    }


    /// <summary>
    /// Создание задачи из XML-описания
    /// </summary>
    /// <param name="taskDeclaration">XML-описание задачи</param>
    /// <returns>Экземпляр задачи</returns>
    static public ITask Create(XElement taskDeclaration)
    {
      if (taskDeclaration == null) throw new ArgumentNullException("Определение задачи не означено");

      Type type;

      if (!TaskHandlerTypes.TryGetValue(taskDeclaration.Attribute("type").Value, out type))
        throw new NotImplementedException(String.Format("Не найдена задача с типом {0}", taskDeclaration.Attribute("type").Value));

      return ((ITask)Activator.CreateInstance(type)).Parse(taskDeclaration);
    }
  }
}
