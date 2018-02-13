Утилита для мониторинга и анализа стабильности работы приложений. Включает в себя подготовленные конфигурации для продуктов "Сервер NOMAD" и "Веб-доступ DIRECTUM".

Сам по себе AppHealth – это заменитель bat-файла c XML-описанием. Он умеет выполнять [ряд задач](https://github.com/DirectumCompany/AppHealth/wiki/Задачи), последовательность и параметры выполнения которых задаются в [файле конфигурации](https://github.com/DirectumCompany/AppHealth/wiki/Конфигурации) в виде XML-тегов.

Утилиту можно найти в поставке сервера NOMAD, в папке по умолчанию C:\inetpub\wwwroot\NOMAD\App_Data\AppHealth.zip
Актуальный список доступных параметров запуска можно получить выполнив команду
> AppHealth help

Пример:

![](https://club.directum.ru/images/4/43/8899.jpeg)

Варианты использования см. [варианты запуска](https://github.com/DirectumCompany/AppHealth/wiki/Режимы-запуска)