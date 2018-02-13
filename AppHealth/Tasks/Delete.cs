using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHealth.Tasks {
  /// <summary>
  /// Удаление файлов
  /// </summary>
  class Delete : ITask {
    /// <summary>Маска для удаления файлов</summary>
    private string _filter;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration) {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _filter = declaration.Attribute("filter").Value;
      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters) {
      _filter = parameters.Parse(_filter).First();
      Application.Log(LogLevel.Informational, "Обработка удаления: {0}", _filter);
      var directory = Path.GetDirectoryName(_filter);
      var wildcard = Path.GetFileName(_filter);
      if (string.IsNullOrEmpty(wildcard)) wildcard = "*.*";
      foreach (var fn in Directory.GetFiles(directory, wildcard, SearchOption.TopDirectoryOnly)) {
          Application.Log(LogLevel.Informational, "Удаление файла {0}", fn);
          File.Delete(fn);
      }
      if (string.IsNullOrEmpty(Path.GetFileName(_filter)))
      {
          Application.Log(LogLevel.Informational, "Удаление папки {0}", directory);
          Directory.Delete(directory);
      }
    }


    public string GetDescription()
    {
        return "Удаление файлов по маске или папки.";
    }

    public string GetHelp()
    {
        throw new NotImplementedException();
    }
  }
}
