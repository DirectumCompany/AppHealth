using AppHealth.Core;
using Mustache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.XPath;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Формирование итогового отчета (сообщения) по ошибкам
  /// </summary>
  /// <remarks>Очень узкий для использования и костыльный</remarks>
  class Message : ITask
  {

    /// <summary>Шаблон</summary>
    private string _template;
    /// <summary>Путь до файла-шаблона</summary>
    private string _templateFile;
    /// <summary>Параметр для вывода итогового результата</summary>
    private string _outputParam;
    /// <summary>
    /// Описания параметров, для использования в шаблоне
    /// </summary>
    private List<ParameterDescription> _paramsDescription;


    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");

      if (declaration.Attribute("template") != null)
      {
        _template = declaration.Attribute("template").Value;
      }

      if (declaration.Attribute("templateFile") != null)
      {
        _templateFile = declaration.Attribute("templateFile").Value;
      }

      _outputParam = declaration.Attribute("outputParam").Value;

      var tmpParams = declaration.XPathSelectElements("//Param");
      if (tmpParams.Any())
      {
        _paramsDescription = new List<ParameterDescription>();
        foreach (var param in tmpParams)
        {
          _paramsDescription.Add(new ParameterDescription()
          {
            Name = param.Attribute("name").Value,
            FilePath = param.Attribute("file").Value,
            HasHeaders = bool.Parse(param.Attribute("headerRow").Value),
            Mode = param.Attribute("mode") == null ? "text" : param.Attribute("mode").Value.ToLower(CultureInfo.InvariantCulture),
            Encoding = param.Attribute("encoding") == null ? null : param.Attribute("encoding").Value.ToLower(CultureInfo.InvariantCulture),
          });
        }
      }


      return this;
    }

    /// <summary>
    /// Запуск задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      System.Threading.Thread.Sleep(5000);
      FormatCompiler compiler = new FormatCompiler();

      if (string.IsNullOrWhiteSpace(_template) && !string.IsNullOrWhiteSpace(_templateFile))
      {
        _template = File.ReadAllText(parameters.Parse(_templateFile).First());
      }

      //Компилция шаблона
      Generator generator = compiler.Compile(_template);

      //В шаблоне можно использовать все параметры, заданные в конфигурации.
      var _templateParams = new Dictionary<string, object>();
      foreach (var param in parameters.GetParameters())
      {
        _templateParams.Add(param.Key, param.Value);
      }


      foreach (var param in _paramsDescription)
      {
        object value = null;
        var filePath = parameters.Parse(param.FilePath).First();
        var enc = String.IsNullOrEmpty(param.Encoding) ? System.Text.Encoding.UTF8 : System.Text.Encoding.GetEncoding(param.Encoding);

        if (File.Exists(filePath))
        {
          switch (param.Mode)
          {
            case "text":
              if (param.HasHeaders) value = string.Join("\n", File.ReadAllLines(filePath).Skip(1));
              else value = File.ReadAllText(filePath, enc);
              break;
            case "csv":
              value = File.ReadAllLines(filePath, enc).Skip(param.HasHeaders ? 1 : 0).Select(x => x.Split(';')).ToList();
              break;
            case "tsv":
              value = File.ReadAllLines(filePath, enc).Skip(param.HasHeaders ? 1 : 0).Select(x => x.Split('\t')).ToList();
              break;
            default:
              value = File.ReadAllText(filePath, enc);
              break;
          }
        }



        _templateParams.Add(param.Name, value);
      }

      parameters.AddParameter(_outputParam, generator.Render(_templateParams));
    }


    /// <summary>
    /// Описание параметра
    /// </summary>
    /// <remarks>Надо продумать как использовать</remarks>
    private class ParameterDescription
    {
      public string Name { get; set; }
      public string FilePath { get; set; }
      public bool HasHeaders { get; set; }
      public string Mode { get; set; }
      public string Encoding { get; set; }
    }


    public string GetDescription()
    {
      return "Формирование отчета.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }

}
