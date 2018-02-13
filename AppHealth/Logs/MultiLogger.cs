using System;
using System.Collections.Generic;
using System.Linq;

namespace AppHealth.Logs
{
  /// <summary>
  /// Реализация <see cref="ILogger"/> для логирования в несколько источников
  /// </summary>
  public sealed class MultiLogger : ILogger
  {
    private readonly ILogger[] _Loggers;

    /// <summary>
    /// Инициализация логгера <see cref="MultiLogger"/>.
    /// </summary>
    /// <param name="loggers">
    /// Коллекция экземпляров логгеров <see cref="ILogger"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <para><paramref name="loggers"/> не задан.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <para><paramref name="loggers"/> пуст.</para>
    /// <para>- или -</para>
    /// <para><paramref name="loggers"/> содержит неозначенные логгеры.</para>
    /// </exception>
    public MultiLogger(IEnumerable<ILogger> loggers)
    {
      if (loggers == null)
        throw new ArgumentNullException("loggers");

      _Loggers = loggers.ToArray();

      if (_Loggers.Length == 0)
        throw new ArgumentException("MultiLogger не содержит установленных логгеров", "loggers");
      if (_Loggers.Any(l => l == null))
        throw new ArgumentException("MultiLogger содержит неозначенные логгеры", "loggers");
    }

    /// <summary>
    /// Логирование сообщения с заданным уровнем. Подробнее: <see cref="ILogger"/>.
    /// </summary>
    /// <param name="level">
    /// Уровень сообщения <see cref="LogLevel"/>
    /// </param>
    /// <param name="message">
    /// Сообщение для логирования
    /// </param>
    public void Log(LogLevel level, string message)
    {
      foreach (ILogger logger in _Loggers)
        logger.Log(level, message);
    }

    /// <summary>
    /// Логирование сообщения с заданным уровнем. Подробнее: <see cref="ILogger"/>.
    /// </summary>
    /// <param name="level">
    /// Уровень сообщения <see cref="LogLevel"/>
    /// </param>
    /// <param name="message">
    /// Сообщение для логирования
    /// </param>
    public void Log(LogLevel level, string message, params object[] arg)
    {
      foreach (ILogger logger in _Loggers)
        logger.Log(level, message, arg);
    }

    public void Flush()
    {
      foreach (ILogger logger in _Loggers) logger.Flush();
    }
  }
}
