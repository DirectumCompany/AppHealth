using AppHealth.Logs;
using AppHealth.Properties;
using System;
using System.IO;
using System.IO.Compression;

namespace AppHealth.Core
{
  /// <summary>
  /// Класс-описание приложения
  /// </summary>
  class Application
  {

    /// <summary>
    /// Рабочая папка
    /// </summary>
    public static string WorkingFolder { get { return _workingFolder; } }
    private static string _workingFolder;

    /// <summary>
    /// Уровень логирования
    /// </summary>
    protected static LogLevel LogLevel
    {
      get
      {
        if (!_logLevel.HasValue)
        {
          // Если не задан, то минимальный
          if (string.IsNullOrEmpty(Settings.Default.LogLevel)) _logLevel = ((LogLevel)0);
          else _logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), Settings.Default.LogLevel, true);

          Console.ForegroundColor = ConsoleColor.DarkGray;
          Console.WriteLine("Установлен режим логирования: {0}", _logLevel);
          Console.ResetColor();
        }
        return _logLevel.Value;
      }
    }
    private static LogLevel? _logLevel;


    /// <summary>
    /// Логгеры приложения
    /// </summary>
    private static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() =>
      new MultiLogger(new ILogger[]
            {
                    new ConsoleLogger((entry => entry.Level >= LogLevel) , null, Console.Out), //Минимальный уровень читаем из параметров
                    new CrashNotifier(),
            })
      );

    /// <summary>
    /// Логирование сообщения с указанным уровнем и форматом
    /// </summary>
    /// <param name="level">Уровень сообщения</param>
    /// <param name="message">Сообщение/формат сообщения</param>
    /// <param name="arg">Аргументы</param>
    internal static void Log(LogLevel level, string message, params object[] arg)
    {
      //TODO: загружать логгер и уровень из параметров запуска приложения
      _logger.Value.Log(level, string.Format(message, arg));
    }

    private Application()
    {
      // Скрываем
    }

    /// <summary>
    /// Инициализация приложения
    /// </summary>
    /// <param name="workingFolder"></param>
    static public void Initialization(string workingFolder)
    {
      _workingFolder = workingFolder;
      PrepareFolders(workingFolder);

      if (!Directory.Exists(Path.Combine(workingFolder, "Templates")))
        ExtractTemplates(workingFolder);

      if (!Directory.Exists(Path.Combine(workingFolder, "Binaries")))
        ExtractBinaries(workingFolder);
    }

    /// <summary>
    /// Подготовка папок
    /// </summary>
    /// <param name="workingFolder"></param>
    static void PrepareFolders(string workingFolder)
    {
      Environment.CurrentDirectory = workingFolder;
      Directory.CreateDirectory(Path.Combine(workingFolder, "Reports"));
      Directory.CreateDirectory(Path.Combine(workingFolder, "Configurations"));
    }

    /// <summary>
    /// Распаковка конфигураций
    /// </summary>
    /// <param name="workingFolder">Папка приложения</param>
    static void ExtractTemplates(string workingFolder)
    {
      Log(LogLevel.Informational, "Создание шаблонов конфигураций");

      var tempZip = Path.GetTempFileName();
      File.WriteAllBytes(tempZip, Resources.Templates);
      ZipFile.ExtractToDirectory(tempZip, workingFolder);
      File.Delete(tempZip);
    }

    /// <summary>
    /// Распаковка бинарников
    /// </summary>
    /// <param name="workingFolder">Папка приложения</param>
    static void ExtractBinaries(string workingFolder)
    {
      Log(LogLevel.Informational, "Создание папки зависимых компонент");

      var tempZip = Path.GetTempFileName();
      File.WriteAllBytes(tempZip, Resources.Binaries);
      ZipFile.ExtractToDirectory(tempZip, workingFolder);
      File.Delete(tempZip);
    }

    /// <summary>
    /// Завершение приложения
    /// </summary>
    internal static void Exit()
    {
      _logger.Value.Flush();
    }
  }
}
