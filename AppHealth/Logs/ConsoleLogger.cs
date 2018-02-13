using System;
using System.IO;

namespace AppHealth.Logs
{

  /// <summary>
  /// Консольный логгер сообщений. Реализует <see cref="ILogger"/>.
  /// </summary>
  public class ConsoleLogger : ILogger
  {
    private readonly TextWriter _Console;
    private readonly Predicate<LogEventArgs> _Filter;

    /// <summary>
    /// Инициализация.
    /// </summary>
    public ConsoleLogger(Predicate<LogEventArgs> filter, Func<LogEventArgs, string> formatter, TextWriter outputConsole)
    {
      _Filter = filter ?? (entry => true);
      _Console = outputConsole ?? Console.Out;
    }

    /// <summary>
    /// Логирование сообщение с заданным уровнем. Реализует <see cref="ILogger"/>.
    /// </summary>
    /// <param name="level">
    /// Уровень сообщения.
    /// </param>
    /// <param name="message">
    /// Формат сообщения для логирования.
    /// </param>
    /// <param name="arg">
    /// Параметры дл формата.
    /// </param>
    public void Log(LogLevel level, string message, params object[] arg)
    {
      Log(level, string.Format(message, arg));
    }

    /// <summary>
    /// Логирование сообщение с заданным уровнем. Реализует <see cref="ILogger"/>.
    /// </summary>
    /// <param name="level">
    /// Уровень сообщения.
    /// </param>
    /// <param name="message">
    /// Текст сообщения.
    /// </param>
    public void Log(LogLevel level, string message)
    {
      var entry = new LogEventArgs(DateTime.Now, level, message);
      if (!_Filter(entry)) return;

      //Немного красоты в выводе
      switch (level)
      {
        case LogLevel.Debug:
          Console.ForegroundColor = ConsoleColor.Green;
          break;
        case LogLevel.Informational:
          Console.ForegroundColor = ConsoleColor.Cyan;
          break;
        case LogLevel.Warning:
          Console.ForegroundColor = ConsoleColor.Yellow;
          break;
        case LogLevel.Error:
          Console.ForegroundColor = ConsoleColor.Red;
          break;
        case LogLevel.Important:
          Console.ForegroundColor = ConsoleColor.Magenta;
          break;
      }

      _Console.WriteLine(message);

      Console.ResetColor();
    }

    public void Flush()
    {
      // НИчего не делаем
    }
  }
}
