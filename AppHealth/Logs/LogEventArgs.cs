using System;
using System.Globalization;

namespace AppHealth.Logs
{
  /// <summary>
  /// Класс содержащий описание событие.
  /// </summary>
  public class LogEventArgs : EventArgs
  {
    private readonly DateTime _Timestamp;
    private readonly LogLevel _Level;
    private readonly string _Message;

    /// <summary>
    /// Инициализация.
    /// </summary>
    public LogEventArgs(DateTime timestamp, LogLevel level, string message)
    {
      _Timestamp = timestamp;
      _Level = level;
      _Message = message ?? string.Empty;
    }

    /// <summary>
    /// Время логируемого сообщения.
    /// </summary>
    public DateTime Timestamp
    {
      get
      {
        return _Timestamp;
      }
    }

    /// <summary>
    /// Уровень логируемого сообщения.
    /// </summary>
    public LogLevel Level
    {
      get
      {
        return _Level;
      }
    }

    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public string Message
    {
      get
      {
        return _Message;
      }
    }

    /// <summary>
    /// ToString.
    /// </summary>
    public override string ToString()
    {
      return string.Format(CultureInfo.InvariantCulture, "{0} - {1}: {2}", Timestamp, Level, Message);
    }
  }
}
