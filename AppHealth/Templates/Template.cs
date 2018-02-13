using System.Collections.Generic;

namespace AppHealth.Templates
{
  class Template
  {
    /// <summary>
    /// Наименование конфигурации
    /// </summary>
    public string Name { get { return _name; } }
    readonly private string _name;

    /// <summary>
    /// Описание конфигурации
    /// </summary>
    public string Description { get { return _description; } }
    private string _description;

    /// <summary>
    /// Продукт
    /// </summary>
    public string ProductCodes { get { return _productCodes; } }
    private string _productCodes;

    /// <summary>
    /// Параметры шаблона
    /// </summary>
    public IEnumerable<Parameters.Parameter> Parameters { get { return _parameters; } }
    readonly private IEnumerable<Parameters.Parameter> _parameters;

    /// <summary>
    /// Путь к конфигурации
    /// </summary>
    public string ConfigurationPath { get { return _configurationPath; } }
    private string _configurationPath;

    /// <summary>
    /// Маска файлов для запуска от папки.
    /// </summary>
    public string PathModeMask { get { return _pathModeMask; } }
    private string _pathModeMask;

    /// <summary>
    /// Конструктор конфигурации
    /// </summary>
    /// <param name="name">Наименование</param>
    /// <param name="productCodes">Коды продуктов</param>
    /// <param name="tasks">Список задач</param>
    /// <param name="parameters">Список параметров</param>
    public Template(string name, string productCodes, string description, IEnumerable<Parameters.Parameter> parameters, string configurationPath, string pathModeMask)
    {
      _name = name;
      _description = description;
      _parameters = parameters;
      _productCodes = productCodes;
      _configurationPath = configurationPath;
      _pathModeMask = pathModeMask;
    }
  }
}
