namespace AppHealth
{
  /// <summary>Интерфейс логгера</summary>
  public interface ILogger
  {
    /// <summary>Логирование сообщение с определенным уровнем</summary>
    /// <param name="level">Уровень сообщения</param>
    /// <param name="message">Сообщение для записи в лог</param>
    void Log(LogLevel level, string message);

    /// <summary>Логирование сообщение с определенным уровнем</summary>
    /// <param name="level">Уровень сообщения</param>
    /// <param name="message">Формат сообщения для записи в лог</param>
    /// <param name="arg">Параметры формата</param>
    void Log(LogLevel level, string message, params object[] arg); //Убрать

    /// <summary>
    /// Отправка данных
    /// </summary>
    void Flush();
  }
}