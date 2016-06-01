using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ConvertFRBtoABS
{
    class Program
    {
        #region Init
        public static string SourcePath = Properties.Settings.Default.SourcePath;
        public static string SourceMask = Properties.Settings.Default.SourceMask;
        public static string LogFile = Properties.Settings.Default.LogFile;
        public static string FileInversionMO = Properties.Settings.Default.FileInversionMO;
        public static string FileInversionOut = Properties.Settings.Default.FileInversionOut;
        public static string FileInversionLoc = Properties.Settings.Default.FileInversionLoc;
        public static string InversionFormat = Properties.Settings.Default.InversionFormat + "\n";
        //string static string FileBankier = Properties.Settings.Default.FileBankier;

        public const string OurBIC = "044030702";
        public const string OurKS = "30101810600000000702";
        public const string OurINN = "7831001422";
        public const string OurName = "АО \"Сити Инвест Банк\"";

        public static Encoding FileEnc = Encoding.GetEncoding(866);

        public static bool AbortDoc = false;
        #endregion

        static void Main(string[] args)
        {
            int cnt = 0;
            foreach (string filename in Directory.GetFiles(SourcePath, SourceMask))
            {
                if (AbortDoc)
                {
                    DialogResult result = MessageBox.Show("Вы хотите завершить работу?\n\n" +
                        "Да - выйти из программы;\n" +
                        "Нет - продолжить проверку.",
                        "Вы отказались от исправления", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                    if (result == DialogResult.Yes)
                    {
                        break;
                    }
                    else
                    {
                        AbortDoc = false;
                    }
                }

                DataTable DBFTable = new DataTable();

                Console.WriteLine("Reading {0}", filename);
                DBF.ReadDBF(filename, DBFTable);
                foreach (DataRow Rec in DBFTable.Rows)
                {
                    #region Test
                    //foreach (object Value in Rec.ItemArray)
                    //{
                    //    Console.WriteLine(Value.ToString());
                    //}
                    //Console.ReadLine();

                    //for (int col = 0; col < GTable.Columns.Count; col++)
                    //{
                    //    string s = GTable.Columns[col].Caption;
                    //    Console.WriteLine("{0} = {1}", s, Rec.ItemArray[col].ToString());
                    //}
                    //Console.WriteLine("!!! Payer = {0}", Rec["PAYER"].ToString());
                    //Console.ReadLine();
                    #endregion

                    if (AbortDoc)
                    {
                        DialogResult result = MessageBox.Show("Вы хотите завершить проверку на этом?\n\n" +
                            "Да - выкинуть целиком остаток пачки;\n" +
                            "Нет - продолжить проверку.",
                            "Вы отказались от исправления", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                        if (result == DialogResult.Yes)
                        {
                            break;
                        }
                        else
                        {
                            AbortDoc = false;
                        }
                    }

                    #region ReadDoc
                    PayDoc doc = new PayDoc(Rec);
                    bool local = doc.BIC2.Equals(doc.BIC);
                    #endregion

                    Console.WriteLine("{0,4} N{1,-6} {2,18} {3}", ++cnt, doc.DocNo, doc.Sum, doc.Name2);

                    DateTimeFormatInfo dfi = new CultureInfo("ru-RU", false).DateTimeFormat;
                    NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

                    string prompt = string.Format("N{0} на {1}, поле ", doc.DocNo, doc.Sum);
                    AbortDoc = false;

                    #region Rules

                    #region Rule3
                    try
                    {
                        Verifier ver = new Verifier(prompt + "3", "Номер док-та");
                        string test = doc.DocNo;

                        //uint i;
                        //while (!UInt32.TryParse(test, NumberStyles.None, nfi, out i))
                        //{
                        //    ver.Problem(ref test, "не число");
                        //}
                        if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_N))
                        {
                            doc.DocNo = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule4
                    try
                    {
                        Verifier ver = new Verifier(prompt + "4", "Дата док-та");
                        string test = doc.DocDate;

                        //Платежное поручение и только
                        if (doc.OpKind.Equals("01") && ver.Date(ref test, 10, 5))
                        {
                            doc.DocDate = test;
                        }
                        if (ver.Date(ref test))
                        {
                            doc.DocDate = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule7
                    try
                    {
                        Verifier ver = new Verifier(prompt + "7", "Сумма");
                        string test = doc.Sum;

                        decimal n;
                        while (!Decimal.TryParse(test, NumberStyles.AllowDecimalPoint, nfi, out n))
                        {
                            ver.Problem(ref test, "не сумма");
                        }
                        if (ver.Changed)
                        {
                            doc.Sum = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule21
                    try
                    {
                        Verifier ver = new Verifier(prompt + "21", "Очер. плат.");
                        string test = doc.Queue;

                        //uint i;
                        //while (!UInt32.TryParse(test, out i))
                        //{
                        //    ver.Problem(ref test, "не число");
                        //}
                        if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_QUE))
                        {
                            doc.Queue = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule60
                    try
                    {
                        Verifier ver = new Verifier(prompt + "60", "ИНН плат.");
                        string test = doc.INN;

                        if (ver.INN(ref test, doc.LS))
                        {
                            doc.INN = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule9
                    try
                    {
                        Verifier ver = new Verifier(prompt + "9", "Счет плат.");
                        string test = doc.LS;

                        if (ver.LS(ref test, doc.BIC, doc.KS))
                        {
                            doc.LS = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule61
                    try
                    {
                        Verifier ver = new Verifier(prompt + "61", "ИНН получ.");
                        string test = doc.INN2;

                        if (ver.INN(ref test, doc.LS2))
                        {
                            doc.INN2 = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule17
                    try
                    {
                        Verifier ver = new Verifier(prompt + "17", "Счет получ.");
                        string test = doc.LS2;

                        if (ver.LS(ref test, doc.BIC2, doc.KS2))
                        {
                            doc.LS2 = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule8
                    //try
                    //{
                    //    Verifier ver = new Verifier(prompt + "8", "Плательщик");
                    //    string test = doc.Name;

                    //    if (ver.Text(ref test, 3, 160))
                    //    {
                    //        doc.Name = test;
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    if (AbortDoc)
                    //    {
                    //        continue;
                    //    }
                    //}
                    #endregion

                    #region Rule16
                    try
                    {
                        Verifier ver = new Verifier(prompt + "16", "Получатель");
                        string test = doc.Name2;

                        if (ver.Text(ref test, 3, 160))
                        {
                            doc.Name2 = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Rule24
                    try
                    {
                        Verifier ver = new Verifier(prompt + "24", "Назначение");
                        string test = doc.Details;

                        if (ver.Text(ref test, 3, 210))
                        {
                            doc.Details = test;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (AbortDoc)
                        {
                            continue;
                        }
                    }
                    #endregion

                    if (doc.SS.Length > 0)
                    {
                        #region Rule101
                        try
                        {
                            Verifier ver = new Verifier(prompt + "101", "Статус плат.");
                            string test = doc.SS;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_SS))
                            {
                                doc.PayCode = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule102
                        try
                        {
                            Verifier ver = new Verifier(prompt + "102", "КПП плат.");
                            string test = doc.KPP;

                            if (ver.KPP(ref test))
                            {
                                doc.KPP = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule103
                        try
                        {
                            Verifier ver = new Verifier(prompt + "103", "КПП получ.");
                            string test = doc.KPP2;

                            if (ver.KPP(ref test))
                            {
                                doc.KPP2 = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule22
                        try
                        {
                            Verifier ver = new Verifier(prompt + "22", "Код УИН");
                            string test = doc.PayCode;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_UIN))
                            {
                                doc.PayCode = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule104
                        try
                        {
                            Verifier ver = new Verifier(prompt + "104", "КБК");
                            string test = doc.NAL1;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_NAL1))
                            {
                                doc.NAL1 = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule105
                        try
                        {
                            Verifier ver = new Verifier(prompt + "105", "ОКТМО");
                            string test = doc.NAL2;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_NAL2))
                            {
                                doc.NAL2 = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule106
                        try
                        {
                            Verifier ver = new Verifier(prompt + "106", "Основание");
                            string test = doc.NAL3;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_NAL3))
                            {
                                doc.NAL3 = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule107
                        try
                        {
                            Verifier ver = new Verifier(prompt + "107", "Период");
                            string test = doc.NAL4;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_NAL4))
                            {
                                doc.NAL4 = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule108
                        try
                        {
                            Verifier ver = new Verifier(prompt + "108", "Номер док-та");
                            string test = doc.NAL5;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_NAL5))
                            {
                                doc.NAL5 = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion

                        #region Rule109
                        try
                        {
                            Verifier ver = new Verifier(prompt + "109", "Дата док-та");
                            string test = doc.NAL6;

                            if (ver.ProbEx(ref test, Properties.Settings.Default.REGEXP_NAL6))
                            {
                                doc.NAL6 = test;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (AbortDoc)
                            {
                                continue;
                            }
                        }
                        #endregion
                    }
                    #endregion Rules

                    #region Bankier
                    //Банкир
                    //string bankier = doc.ExportToBankier(local) + '\n';
                    //File.AppendAllText(FileBankier, bankier, FileEnc);
                    #endregion

                    #region Inversia
                    //Инверсия
                    string inv = doc.ExportToInversion(local);
                    File.AppendAllText(local ? FileInversionLoc : FileInversionOut, inv, FileEnc);
                    #endregion
                }

                //string bak = Path.Combine(SourcePath, string.Format(@"BAK\{0:yyyy}\{0:MM}\{0:dd}\{0:HHmm}{1}", DateTime.Now, Path.GetFileName(filename)));
                string bak = Path.Combine(SourcePath, string.Format(@"BAK\{0:yyyy-MM-dd}\{0:HHmm}{1}", DateTime.Now, Path.GetFileName(filename)));
                string path = Path.GetDirectoryName(bak);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.Move(filename, bak);
                string msg;
                if (File.Exists(bak))
                {
                    msg = filename + string.Format(" {0} ok\n", DBFTable.Rows.Count);
                }
                else
                {
                    msg = filename + " не создать\n";
                }
                File.AppendAllText(LogFile, msg, FileEnc);
                Console.WriteLine();
            }
            Console.WriteLine("Press Enter");
            Console.ReadLine();

            Environment.ExitCode = 0;
        }
    }
}
