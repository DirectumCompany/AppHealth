﻿namespace AppHealth
{
  /// <summary>Уровень сообщения для использования в реализации <see cref="ILogger"/>.
  /// </summary>
  /// <remarks>Выше уровень - выше число</remarks>
  public enum LogLevel
  {
    /// <summary>Уровень сообщения "Отладочный". 
    /// Подробное логирование действий в системе, испольузется во время отладки системы</summary>
    Debug = 0,

    /// <summary>Уровень сообщения "Информационный". 
    /// Для фиксации действий системы</summary>
    Informational = 1,

    /// <summary>Уровень сообщения "Предупреждение". 
    /// Для уведомления о возможных проблемах</summary>
    Warning = 2,

    /// <summary>Уровень сообщения "Ошибка". 
    /// Для фиксации некорретного поведения системы</summary>
    Error = 3,

    /// <summary>Уровень сообщения "Важно". 
    /// Для фиксации сообщений, которые должны обязательно попасть в лог</summary>
    Important = 255
  }
}