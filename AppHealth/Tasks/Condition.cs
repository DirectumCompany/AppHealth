using ExpressionEvaluator;
using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Условный оператор.
  /// </summary>
  class Condition : ITask
  {
    /// <summary>
    /// Выражение для проверки условия
    /// </summary>
    private string _expression;
    /// <summary>
    /// Задачи которые будут отработаны, при выполнении условия
    /// </summary>
    private List<ITask> _tasks;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _expression = declaration.Attribute("expression").Value;

      _tasks = new List<ITask>();
      var taskNodes = declaration.XPathSelectElements(@"Task");
      foreach (var taskNode in taskNodes)
      {
        _tasks.Add(Tasks.TaskFactory.Create(taskNode));
      }

      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      var types = new TypeRegistry();
      types.RegisterDefaultTypes();
      types.RegisterType("CultureInfo", typeof(CultureInfo));
      types.RegisterSymbol("fromDate", parameters._fromDate);
      types.RegisterSymbol("toDate", parameters._toDate);

      var expression = new CompiledExpression(parameters.Parse(_expression).First());
      expression.TypeRegistry = types;

      if ((bool)expression.Eval())
      {
        foreach (var task in _tasks)
        {
          Application.Log(LogLevel.Debug, "  Выполнение задачи '{0}'", task.GetType().Name);
          task.Run(parameters);
          Application.Log(LogLevel.Debug, "  выполнено");
        }
      }
    }


    public string GetDescription()
    {
        return "Условный оператор";
    }

    public string GetHelp()
    {
        throw new NotImplementedException();
    }
  }
}
