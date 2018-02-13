using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionEvaluator;
using System.Globalization;

namespace AppHealth.Tasks {
  /// <summary>
  /// Парсит выражения в параметр.
  /// </summary>
  /// <remarks>Выражение и имя параметра могут содержать параметры</remarks>
  /// <see cref="https://csharpeval.codeplex.com/"/>
  /// <example>"1 + 2" // returns (int) 3</example>
  /// <example>"'\\\\Foo' + 'bar' + '\\'s'" // returns \Foobar's</example>
  /// <example>"'Foobar'.Substring(0,3)" // returns "Foo"</example>
  /// <example>'One Two'.Replace(' ','/') // returns "One/Two"</example>
  /// <example>DateTime.Now.ToString('yyyy/MM/dd hh:mm:ss')// returns "2015.04.11 10:57:46"</example>
  class Expression : ITask {
    /// <summary>Выражение</summary>
    private string _expression;
    /// <summary>Результат выражения</summary>
    private string _outputParam;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration) {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _expression = declaration.Attribute("expression").Value;
      _outputParam = declaration.Attribute("outputParam").Value;
      return this;
    }
    
    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters) {
      var types = new TypeRegistry();
      types.RegisterDefaultTypes();
      types.RegisterType("CultureInfo", typeof(CultureInfo));
      types.RegisterSymbol("fromDate", parameters._fromDate);
      types.RegisterSymbol("toDate", parameters._toDate);
      var expression = new CompiledExpression(parameters.Parse(_expression).First());

      expression.TypeRegistry = types;
      parameters.AddParameter(parameters.Parse(_outputParam).First(), expression.Eval().ToString());
    }


    public string GetDescription()
    {
        return "Вычисление выражения.";
    }

    public string GetHelp()
    {
        return "Выражение и имя параметра могут содержать параметры.\n" +
            "\"1 + 2\" // returns (int) 3 \n" +
            "\"'Foobar'.Substring(0,3)\" // returns \"Foo\" \n" +
            "'One Two'.Replace(' ','/') // returns \"One/Two\" \n" +
            "DateTime.Now.ToString('yyyy/MM/dd hh:mm:ss')// returns \"2015.04.11 10:57:46\" \n" +
            "Подробнее см. https://csharpeval.codeplex.com/";
    }
  }
}
