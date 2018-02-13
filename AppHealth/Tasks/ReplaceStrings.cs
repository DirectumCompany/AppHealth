using AppHealth.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Замена строк.
  /// </summary>
  class ReplaceStrings : ITask
  {
    /// <summary> Путь до файла с регулярными выражениями </summary>
    private string _regularPath;
    /// <summary> Путь до файла логов </summary>
    private string _source;
    /// <summary>Путь назначения</summary>
    private string _destination;

    public string GetDescription()
    {
      return "Замена строк";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }

    public ITask Parse(XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");

      if (declaration.Attribute("regularPath") != null)
      {
        _regularPath = declaration.Attribute("regularPath").Value;
      }

      if (declaration.Attribute("source") != null)
      {
        _source = declaration.Attribute("source").Value;
      }

      if (declaration.Attribute("destination") != null)
      {
        _destination = declaration.Attribute("destination").Value;
      }
      else
      {
        _destination = _source;
      }

      return this;
    }

    public void Run(ParameterProvider parameters)
    {
      if (!File.Exists(_regularPath)) throw new ArgumentNullException("Отсутствует файл с регулярными выражениями");
      //Получим регулярки
      string[] regulars = File.ReadAllLines(_regularPath);
      //Откроем файл на редактирование
      StreamWriter destinationFile = new StreamWriter(_destination);

      using (StreamReader sourceFile = new StreamReader(_source))
      {
        string logFileContent;
        while (sourceFile.Peek() >= 0)
        {
          logFileContent = sourceFile.ReadLine();

          foreach (var regular in regulars)
          {
            var regex = new Regex(regular);
            logFileContent = regex.Replace(logFileContent, new MatchEvaluator(ReplaceEvaluator));
          }
          destinationFile.WriteLine(logFileContent);
        }
      }
      destinationFile.Close();
    }

    //Метод возвращает хэшкод строки
    public string ReplaceEvaluator(Match match)
    {
      return match.Value.GetHashCode().ToString();
    }
  }
}
