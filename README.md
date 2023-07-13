# [ConvertFRBtoABS]

[![Build status]][appveyor]
[![GitHub Release]][releases]

Конвертирование сканированных платежных документов из *FineReader BANK* 
в формате DBF в формат импорта АБС *Инверсия XXI Век* с попутной коррекцией 
неправильно распознанного и сменой кодировки.

![Рабочее окно приложения]

## Requirements

- .Net Framework 3.5 для ConvertFRBtoABS для работы на Windows XP+,
поддержка до 2029 года.

- или последний .Net Framework 4.0 для работы на Windows XP+,
если для компиляции в целевой ОС использовать прилагаемый scs_v4.cmd.

- .Net Framework 4.8 для ConvertFRBtoABS.Tests для отладки и компиляции.

## License

Licensed under the [Apache License, Version 2.0].

[ConvertFRBtoABS]: http://diev.github.io/ConvertFRBtoABS/
[Apache License, Version 2.0]: LICENSE

[appveyor]: https://ci.appveyor.com/project/diev/convertfrbtoabs
[releases]: https://github.com/diev/ConvertFRBtoABS/releases/latest

[Build status]: https://ci.appveyor.com/api/projects/status/tjajducaps0g7wsd?svg=true
[GitHub Release]: https://img.shields.io/github/release/diev/ConvertFRBtoABS.svg

[Рабочее окно приложения]: docs/assets/images/plategka.gif
