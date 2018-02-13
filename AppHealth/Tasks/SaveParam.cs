using AppHealth.Core;
using System;
using System.IO;
using System.Linq;

namespace AppHealth.Tasks
{

  /// <summary>
  /// Сохранение параметра
  /// </summary>
  class SaveParam : ITask
  {
    /// <summary>Имя параметра</summary>
    private string _key;

    /// <summary>Путь к файлу, для сохранения параметра</summary>
    private string _filePath;
    /// <summary>Кодировка файла</summary>
    public string _fileEncoding;


    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _key = declaration.Attribute("name").Value;

      if (declaration.Attribute("filePath") != null)
      {
        _filePath = declaration.Attribute("filePath").Value;
      }

      if (declaration.Attribute("encoding") != null)
      {
        _fileEncoding = declaration.Attribute("encoding").Value;
      }

      return this;
    }


    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      var enc = String.IsNullOrEmpty(_fileEncoding) ? System.Text.Encoding.UTF8 : System.Text.Encoding.GetEncoding(_fileEncoding);
      _filePath = parameters.Parse(_filePath).First();
      File.WriteAllText(_filePath, parameters.GetParameters()[_key], enc);
    }


    public string GetDescription()
    {
      return "Сохранение параметра в файл с указанной кодировкой.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
