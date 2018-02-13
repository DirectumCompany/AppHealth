using AppHealth.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Разархивирование файлов
  /// </summary>
  class UnZip : ITask
  {

    /// <summary>Источник для архивирования</summary>
    internal string _from;
    /// <summary>Приемник файла архива</summary>
    internal string _to;
    /// <summary>Признак удаления архива после распаковки</summary>
    internal bool _deleteAfterExtract = false;

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
      if (declaration.Attribute("deleteAfterExtract") != null)
      {
        bool.TryParse(declaration.Attribute("deleteAfterExtract").Value, out _deleteAfterExtract);
      }
      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider paramProvider)
    {
      //TODO: Проверки.
      var from = paramProvider.Parse(_from).First();
      var to = paramProvider.Parse(_to).First();
      ZipFile.ExtractToDirectory(from, to);
      if (_deleteAfterExtract) File.Delete(from);
    }


    public string GetDescription()
    {
      return "Разархивирование zip-файлов.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
