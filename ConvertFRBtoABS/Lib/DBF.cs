#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov 2013-2023
// Source https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------
#endregion

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace Lib
{
    /// <summary>
    /// DBF class contains methods to handle DBF files.
    /// </summary>
    /// <remarks>
    /// Code is based on http://nansoft.ru/blog/csharp/30.html
    /// </remarks>
    /// <example>
    /// This sample shows how to read datarows and process them.
    /// <code>
    ///     DataTable DBFTable = new DataTable();
    ///     ReadDBF(filename, DBFTable);
    ///     foreach (DataRow Rec in DBFTable.Rows) {}
    /// </code>
    /// </example>
    internal class DBF
    {
        // Типы полей DBF
        private const string _C = "C";
        private const string _L = "L";
        private const string _D = "D";
        private const string _N = "N";
        private const string _F = "F";

        private readonly static Type _typeC = Type.GetType("System.String");
        private readonly static Type _typeL = Type.GetType("System.Boolean");
        private readonly static Type _typeD = Type.GetType("System.DateTime");
        private readonly static Type _typeN0 = Type.GetType("System.Int32");
        private readonly static Type _typeN = Type.GetType("System.Decimal");
        private readonly static Type _typeF = Type.GetType("System.Double");

        private readonly static char[] _trim = new char[] { (char)0x00, (char)0x20 };

        private readonly static DateTimeFormatInfo _dfi = new CultureInfo("en-US", false).DateTimeFormat;
        private readonly static NumberFormatInfo _nfi = new CultureInfo("en-US", false).NumberFormat;

        private readonly static Encoding _enc = Encoding.Default;
        private readonly static Encoding _oem = Encoding.GetEncoding(866);

        /// <summary>
        /// Read DBF file.
        /// </summary>
        /// <param name="filename">DBF file name to read.</param>
        /// <param name="table">Table to store data.</param>
        public static void ReadDBF(string filename, DataTable table)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4]; // Кол-во записей: 4 байтa, начиная с 5-го
                fs.Position = 4;
                fs.Read(buffer, 0, buffer.Length);

                int rowsCount = buffer[0] +
                    (buffer[1] * 0x100) +
                    (buffer[2] * 0x10000) +
                    (buffer[3] * 0x1000000);

                buffer = new byte[2]; // Кол-во полей: 2 байтa, начиная с 9-го
                fs.Position = 8;
                fs.Read(buffer, 0, buffer.Length);

                int fieldCount = ((buffer[0] + (buffer[1] * 0x100) - 1) / 32) - 1;

                string[] fieldName = new string[fieldCount]; // Массив названий полей
                string[] fieldType = new string[fieldCount]; // Массив типов полей

                byte[] fieldSize = new byte[fieldCount]; // Массив размеров полей
                byte[] fieldDigs = new byte[fieldCount]; // Массив размеров дробной части

                buffer = new byte[32 * fieldCount]; // Описание полей: 32 байтa * кол-во, начиная с 33-го
                fs.Position = 32;
                fs.Read(buffer, 0, buffer.Length);

                int fieldsLength = 0;

                for (int col = 0; col < fieldCount; col++)
                {
                    int pos = col * 32;

                    // Заголовки
                    fieldName[col] = _enc.GetString(buffer, pos, 10).TrimEnd(_trim);
                    fieldType[col] = string.Empty + (char)buffer[pos + 11];
                    fieldSize[col] = buffer[pos + 16];
                    fieldDigs[col] = buffer[pos + 17];

                    fieldsLength += fieldSize[col];

                    // Создаю колонки
                    Type type;
                    switch (fieldType[col])
                    {
                        case _C:
                            type = _typeC;
                            break;

                        case _L:
                            type = _typeL;
                            break;

                        case _D:
                            type = _typeD;
                            break;

                        case _N:
                            type = fieldDigs[col] == 0
                                ? _typeN0
                                : _typeN;
                            break;

                        case _F:
                            type = _typeF;
                            break;

                        default:
                            throw new Exception("Неизвестный тип поля DBF");
                    }

                    table.Columns.Add(fieldName[col], type);
                }

                fs.ReadByte(); // Пропускаю разделитель схемы и данных
                buffer = new byte[fieldsLength];
                table.BeginLoadData();

                for (int row = 0; row < rowsCount; row++)
                {
                    fs.ReadByte(); // Пропускаю стартовый байт элемента данных
                    fs.Read(buffer, 0, buffer.Length);
                    DataRow dataRow = table.NewRow();
                    int index = 0;

                    for (int col = 0; col < fieldCount; col++)
                    {
                        string value = _oem.GetString(buffer, index, fieldSize[col]).TrimEnd(_trim);
                        index += fieldSize[col];

                        if (string.IsNullOrEmpty(value))
                        {
                            dataRow[col] = DBNull.Value;
                            continue;
                        }

                        switch (fieldType[col])
                        {
                            case _L:
                                dataRow[col] = value == "T"; // ? true : false;
                                break;

                            case _D:
                                dataRow[col] = DateTime.ParseExact(value, "yyyyMMdd", _dfi);
                                break;

                            case _N:
                                if (fieldDigs[col] == 0)
                                {
                                    dataRow[col] = int.Parse(value, _nfi);
                                }
                                else
                                {
                                    dataRow[col] = decimal.Parse(value, _nfi);
                                }
                                break;

                            case _F:
                                dataRow[col] = double.Parse(value, _nfi);
                                break;

                            default:
                                dataRow[col] = value;
                                break;
                        }
                    }

                    table.Rows.Add(dataRow);
                    //Application.DoEvents();
                }

                table.EndLoadData();
                fs.Close();
            }
        }
    }
}
