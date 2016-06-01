using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace ConvertFRBtoABS
{
    class DBF
    {
        public static void ReadDBF(string filename, DataTable table)
        {
            // http://nansoft.ru/blog/csharp/30.html

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4]; // Кол-во записей: 4 байтa, начиная с 5-го
                fs.Position = 4;
                fs.Read(buffer, 0, buffer.Length);

                int RowsCount = buffer[0] +
                    (buffer[1] * 0x100) +
                    (buffer[2] * 0x10000) +
                    (buffer[3] * 0x1000000);

                buffer = new byte[2]; // Кол-во полей: 2 байтa, начиная с 9-го
                fs.Position = 8;
                fs.Read(buffer, 0, buffer.Length);

                int FieldCount = (((buffer[0] + (buffer[1] * 0x100)) - 1) / 32) - 1;

                string[] FieldName = new string[FieldCount]; // Массив названий полей
                string[] FieldType = new string[FieldCount]; // Массив типов полей

                byte[] FieldSize = new byte[FieldCount]; // Массив размеров полей
                byte[] FieldDigs = new byte[FieldCount]; // Массив размеров дробной части

                buffer = new byte[32 * FieldCount]; // Описание полей: 32 байтa * кол-во, начиная с 33-го
                fs.Position = 32;
                fs.Read(buffer, 0, buffer.Length);
                int FieldsLength = 0;

                for (int col = 0; col < FieldCount; col++)
                {
                    // Заголовки
                    FieldName[col] = Encoding.Default
                        .GetString(buffer, col * 32, 10)
                        .TrimEnd(new char[] { (char)0x00 });
                    FieldType[col] = "" + (char)buffer[col * 32 + 11];
                    FieldSize[col] = buffer[col * 32 + 16];
                    FieldDigs[col] = buffer[col * 32 + 17];
                    FieldsLength = FieldsLength + FieldSize[col];

                    // Создаю колонки
                    switch (FieldType[col])
                    {
                        case "C":
                            table.Columns.Add(FieldName[col], Type.GetType("System.String"));
                            break;

                        case "L":
                            table.Columns.Add(FieldName[col], Type.GetType("System.Boolean"));
                            break;

                        case "D":
                            table.Columns.Add(FieldName[col], Type.GetType("System.DateTime"));
                            break;

                        case "N":
                            if (FieldDigs[col] == 0)
                            {
                                table.Columns.Add(FieldName[col], Type.GetType("System.Int32"));
                            }
                            else
                            {
                                table.Columns.Add(FieldName[col], Type.GetType("System.Decimal"));
                            }
                            break;

                        case "F":
                            table.Columns.Add(FieldName[col], Type.GetType("System.Double"));
                            break;

                        default:
                            //GTable.Columns.Add(FieldName[col], Type.GetType("System.String"));//////////////////?!!!
                            break;
                    }
                }
                fs.ReadByte(); // Пропускаю разделитель схемы и данных

                DateTimeFormatInfo dfi = new CultureInfo("en-US", false).DateTimeFormat;
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

                buffer = new byte[FieldsLength];
                table.BeginLoadData();

                for (int row = 0; row < RowsCount; row++)
                {
                    fs.ReadByte(); // Пропускаю стартовый байт элемента данных
                    fs.Read(buffer, 0, buffer.Length);
                    DataRow R = table.NewRow();
                    int Index = 0;

                    for (int col = 0; col < FieldCount; col++)
                    {
                        //string value = Encoding.GetEncoding(Encoding.UTF8.HeaderName).GetString(buffer, Index, FieldSize[i]).TrimEnd(new char[] { (char)0x00 }).TrimEnd(new char[] { (char)0x20 });
                        string value = Encoding.GetEncoding(866)
                            .GetString(buffer, Index, FieldSize[col])
                            .TrimEnd(new char[] { (char)0x00 })
                            .TrimEnd(new char[] { (char)0x20 });
                        Index += FieldSize[col];

                        if (!string.IsNullOrEmpty(value))
                        {
                            switch (FieldType[col])
                            {
                                case "L":
                                    R[col] = value.Equals("T"); // ? true : false;
                                    break;

                                case "D":
                                    R[col] = DateTime.ParseExact(value, "yyyyMMdd", dfi);
                                    break;

                                case "N":
                                    if (FieldDigs[col] == 0)
                                    {
                                        R[col] = int.Parse(value, nfi);
                                    }
                                    else
                                    {
                                        R[col] = decimal.Parse(value, nfi);
                                    }
                                    break;

                                case "F":
                                    R[col] = double.Parse(value, nfi);
                                    break;

                                default:
                                    R[col] = value;
                                    break;
                            }
                        }
                        else
                        {
                            R[col] = DBNull.Value;
                        }
                    }
                    table.Rows.Add(R);
                    //Application.DoEvents();
                }
                table.EndLoadData();
                fs.Close();
            }
        }
    }
}
