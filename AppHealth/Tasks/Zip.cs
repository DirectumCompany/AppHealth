using AppHealth.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Архивирование файлов
  /// </summary>
  class Zip : ITask
  {

    /// <summary>Источник для архивирования</summary>
    internal string _from;
    /// <summary>Приемник файла архива</summary>
    internal string _to;
    /// <summary>Признак удаления заакхивированных файлов</summary>
    internal bool _deleteAfterArchive = false;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _from = declaration.Attribute("from").Value;
      _to = declaration.Attribute("to").Value;
      if (declaration.Attribute("deleteAfterArchive") != null)
      {
        bool.TryParse(declaration.Attribute("deleteAfterArchive").Value, out _deleteAfterArchive);
      }
      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider paramProvider)
    {
      // ZipFile довольно глупый и если будет создавать в архив в тойже директории которую архивирует, он свалится.
      // Поэтому сперва моздаем временный файл
      var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
      var fromPath = paramProvider.Parse(_from).First();

      ZipFile.CreateFromDirectory(fromPath, tempFile);

      if (_deleteAfterArchive)
      {
        var directory = System.IO.Path.GetDirectoryName(fromPath);
        var wildcard = System.IO.Path.GetFileName(fromPath);
        foreach (var fn in Directory.GetFiles(directory, wildcard, SearchOption.TopDirectoryOnly))
        {
          System.IO.File.Delete(fn);
        }
      }
      var destination = paramProvider.Parse(_to).First();
      if (File.Exists(destination)) File.Delete(destination);
      File.Move(tempFile, paramProvider.Parse(_to).First());
    }


    public string GetDescription()
    {
      return "Архивирование файлов.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
