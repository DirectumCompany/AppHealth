using AppHealth.Core;
using System;
using System.IO;
using System.Linq;

namespace AppHealth.Tasks
{

  /// <summary>
  /// Установка значения параметра
  /// </summary>
  class SetParam : ITask
  {
    /// <summary>Имя параметра</summary>
    private string _key;
    /// <summary>Значение параметра</summary>
    /// <remarks>В качестве значения можно использовать другой параметр</remarks>
    private string _value;

    /// <summary>Путь к файлу, для установки параметра</summary>
    private string _filePath;
    /// <summary>Кодировка файла</summary>
    public string _fileEncoding;
    /// <summary>Количество пропущенных строк</summary>
    private int _skipLines;
    /// <summary>Количество прочтенных строк</summary>
    private int _takeLines;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _key = declaration.Attribute("name").Value;

      if (declaration.Attribute("value") != null)
      {
        _value = declaration.Attribute("value").Value;
      }

      // Для установке параметра из файла
      if (declaration.Attribute("filePath") != null)
      {
        _filePath = declaration.Attribute("filePath").Value;

        if (declaration.Attribute("encoding") != null)
        {
          _fileEncoding = declaration.Attribute("encoding").Value;
        }

        if (declaration.Attribute("skip") != null)
        {
          int.TryParse(declaration.Attribute("skip").Value, out _skipLines);
        }

        if (declaration.Attribute("take") != null)
        {
          int.TryParse(declaration.Attribute("take").Value, out _takeLines);
        }
      }

      return this;
    }


    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      if (!string.IsNullOrEmpty(_value)) parameters.AddParameter(_key, parameters.Parse(_value).First());
      if (!string.IsNullOrEmpty(_filePath))
      {
        var filePath = parameters.Parse(_filePath).First();

        if (File.Exists(filePath))
        {
          if (_takeLines == 0) _value = string.Join("\n", File.ReadAllLines(filePath).Skip(_skipLines));
          else _value = string.Join("\n", File.ReadAllLines(filePath).Skip(_skipLines).Take(_takeLines));

          parameters.AddParameter(_key, _value);
        }
        else
        {
          Application.Log(LogLevel.Error, string.Format("File {0} not found.", filePath));
        }
      }
    }


    public string GetDescription()
    {
      return "Установка значения параметра";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
