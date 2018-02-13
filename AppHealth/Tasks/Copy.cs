using AppHealth.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace AppHealth.Tasks
{
  /// <summary>
  /// Копирование файлов с возможностью агрегации.
  /// </summary>
  class Copy : ITask
  {
    /// <summary>Путь источник</summary>
    private string _source;
    /// <summary>Путь назначения</summary>
    private string _destination;
    /// <summary>Поиск с подпапками</summary>
    private bool _withSubFolders = false;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _source = declaration.Attribute("source").Value;
      _destination = declaration.Attribute("destination").Value;
      if (declaration.Attribute("withSubFolders") != null)
      {
        bool.TryParse(declaration.Attribute("withSubFolders").Value, out _withSubFolders);
      }
      return this;
    }


    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      // TODO: Итерировать источник вместе
      var destination = parameters.Parse(_destination).First();

      var files = new List<string>();
      foreach (var _path in parameters.Parse(_source))
      {
        if (File.Exists(_path))
          files.Add(_path);
        else if 
          (Directory.Exists(_path)) files.AddRange(Directory.GetFiles(_path));
        else
        {
          var dir = Path.GetDirectoryName(_path);
          var dirMask = "";
          var fileMask = Path.GetFileName(_path);

          Application.Log(LogLevel.Debug, "dir: {0} dirMask: {1} fileMask:{2}", dir, dirMask, fileMask);

          if (dir.Contains('*') || dir.Contains('?'))
          {
            dir = Path.GetPathRoot(_path);
            dirMask = _path.Replace(dir + '\\', "");
            dirMask = dirMask.Replace(fileMask, "");

            if (dirMask.EndsWith("\\")) dirMask = dirMask.Substring(0, dirMask.Length - 1);
            if (dirMask.EndsWith("//")) dirMask = dirMask.Substring(0, dirMask.Length - 1);
          }

          if (string.IsNullOrEmpty(dirMask))
            files.AddRange(Directory.GetFiles(dir, fileMask, _withSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
          else
            foreach (var directory in Directory.GetDirectories(dir, dirMask))
              files.AddRange(Directory.GetFiles(directory, fileMask, _withSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        }
      }

      //Если указана папка, то копируем пофайлово
      if (string.IsNullOrEmpty(Path.GetExtension(destination)))
      {
        if (!Directory.Exists(destination))
          Directory.CreateDirectory(destination);

        foreach (var file in files)
        {
          Application.Log(LogLevel.Debug, "Копирование файла: {0}", file);
          File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        }
      }
      else
      {
        Application.Log(LogLevel.Debug, "GetDirectoryName: {0}", Path.GetDirectoryName(destination));

        if (!Directory.Exists(Path.GetDirectoryName(destination)))
          Directory.CreateDirectory(Path.GetDirectoryName(destination));

        using (var destStream = File.Create(destination))
        {
          foreach (var file in files)
          {
            Application.Log(LogLevel.Debug, "Копирование файла: {0}", file);
            // TODO: Указать размер буфера
            using (var sourceStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) sourceStream.CopyTo(destStream);
          }
        }
      }
    }


    public string GetDescription()
    {
      return "Копирование файлов.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
