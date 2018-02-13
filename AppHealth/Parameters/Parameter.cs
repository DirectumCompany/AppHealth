using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AppHealth.Parameters
{
  /// <summary>
  /// Параметр конфигурации
  /// </summary>
  [Serializable()]
  public class Parameter
  {
    /// <summary>
    /// Наименование параметра
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; }

    /// <summary>
    /// Значение параметра
    /// </summary>
    [XmlAttribute("value")]
    public string Value { get; set; }

    /// <summary>
    /// Описание параметра
    /// </summary>
    [XmlAttribute("description")]
    public string Description { get; set; }

    /// <summary>
    /// Обязательность параметра
    /// </summary>
    [XmlAttribute("required")]
    public bool Required { get; set; }
  }

  [Serializable(), XmlRoot("Parameters", Namespace = "", IsNullable = true)]
  public class ParameterCollection
  {
    [XmlElement("Parameter")]
    public List<Parameter> Parameters { get; set; }
  }
}
