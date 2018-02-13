using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace AppHealth.Tasks {
  /// <summary>
  /// Выполнение команды.
  /// </summary>
  class CMD : ITask {
    /// <summary>Команда</summary>
    private string _arguments;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration) {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _arguments = declaration.Attribute("arguments").Value;
    
      return this;
    }


    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters) {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      startInfo.FileName = "cmd.exe";
      startInfo.Arguments = "/c " + parameters.Parse(_arguments).First() + " exit ";
      Process.Start(startInfo).WaitForExit();
      Application.Log(LogLevel.Informational, "{0} {1}", startInfo.FileName, startInfo.Arguments);
    }


    public string GetDescription()
    {
        return "Выполнение команды.";
    }

    public string GetHelp()
    {
        throw new NotImplementedException();
    }
  }
}
