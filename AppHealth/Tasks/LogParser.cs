using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Запуск скрипта LogParser
  /// </summary>
  class LogParser : ITask
  {

    /// <summary>Файл запроса</summary>
    private string _queryFile;
    /// <summary>Параметр: наличие заголовков в файле</summary>
    private bool _headerRow;
    /// <summary>Параметр: формат вывода</summary>
    private string _outputFormat = "TSV";
    /// <summary>Параметр: формат вывода</summary>
    private string _inputFormat = "TSV";
    /// <summary>Дополнительные параметры командной строки LogParser</summary>
    private string _otherParams;

    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _queryFile = declaration.Attribute("queryFile").Value;
      //Наличие заголовков
      if (declaration.Attribute("headerRow") != null) bool.TryParse(declaration.Attribute("headerRow").Value, out _headerRow);
      //Формат вывода
      if (declaration.Attribute("outputFormat") != null && !String.IsNullOrWhiteSpace(declaration.Attribute("outputFormat").Value))
        _outputFormat = declaration.Attribute("outputFormat").Value;

      //Формат ввода
      if (declaration.Attribute("inputFormat") != null && !String.IsNullOrWhiteSpace(declaration.Attribute("inputFormat").Value))
        _inputFormat = declaration.Attribute("inputFormat").Value;

      if (declaration.Attribute("params") != null) _otherParams = declaration.Attribute("params").Value;
      return this;
    }

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public void Run(ParameterProvider paramProvider)
    {
      var param = CreateLogParserParams(paramProvider.GetParameters());
      var queryFile = paramProvider.Parse(_queryFile).First();
      string headerParam = null;

      if ("TSV".Equals(_inputFormat, StringComparison.InvariantCultureIgnoreCase)) headerParam = string.Format("-headerRow:{0}", _headerRow ? "on" : "off");
      var args = String.Format("-i:{3} file:{0}?{1} {2} -o:\"{4}\" {5}", queryFile, param, headerParam, _inputFormat, _outputFormat, _otherParams);
      Application.Log(LogLevel.Informational, args);
      var cmd = new ProcessStartInfo(@"Binaries\LogParser\LogParser.exe", args);
      cmd.UseShellExecute = false;
      Process.Start(cmd).WaitForExit();
    }

    /// <summary>
    /// Формирование строки аргументов, для передачи параметров в командной строке
    /// </summary>
    /// <param name="param">Коллекция параметров</param>
    /// <returns></returns>
    internal static string CreateLogParserParams(IDictionary<string, string> param)
    {
      return string.Join("+", param.Select(x => string.Format("{0}=\"{1}\"", x.Key, x.Value.Replace("\\", "\\\\")))).Replace(@"\\\\", @"\\");
    }



    public string GetDescription()
    {
      return "Выполнение скриптов LogParser.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
