using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ConvertFRBtoABS
{
    class Verifier
    {
        private const int CutLength = 20;

        private string prompt;
        private string part;

        public bool Changed = false;

        public Verifier(string prompt, string part)
        {
            this.prompt = prompt;
            this.part = part;
        }

        public void Problem(ref string field, string msg)
        {
            string field0 = field;

            string sfield = (field.Length > CutLength)
                ? field.Substring(0, CutLength) + "..."
                : (field.Length > 0)
                ? field
                : "-пусто-";
            string ask = string.Format("{0} {1}", part, msg);
            string log = string.Format("{0} \"{1}\" - {2}", part, sfield, msg);

            if (InputBox.Query(prompt, ask, ref field))
            {
                if (field.Equals(field0))
                {
                    Console.WriteLine("     Ошибка: {0} ({1}) - не исправлена!", ask, sfield);
                }
                else
                {
                    sfield = (field.Length > CutLength)
                        ? field.Substring(0, CutLength) + "..."
                        : (field.Length > 0)
                        ? field
                        : "-пусто-";

                    Console.WriteLine("     Ошибка: {0} - исправлено на: {1}", ask, sfield);
                    Changed = true;
                }
            }
            else
            {
                File.AppendAllText(Program.LogFile, log + "\n", Program.FileEnc);

                DialogResult result = MessageBox.Show("Вы отказались от исправления!\n\n" +
                    "Выкинуть эту платежку из пакета на загрузку?\n\n" +
                    "Да - выкинуть целиком эту платежку;\n" +
                    "Нет - пропустить эту ошибку и проверять платежку дальше.",
                    prompt, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Stop);

                //if (result == DialogResult.OK)
                //{
                //    Environment.Exit(1);
                //}

                if (result == DialogResult.Yes)
                {
                    Program.AbortDoc = true;
                    throw new Exception(log);
                }
                else if (result == DialogResult.No)
                {
                    Program.AbortDoc = false;
                    throw new Exception(log);
                }
            }
        }

        public bool ProbEx(ref string field, string regexp, string msg = "")
        {
            if (regexp.Contains("~") && msg.Length == 0)
            {
                string[] parts = regexp.Split(new char[] { '~' });
                regexp = parts[0];
                msg = parts[1];
            }
            Regex regex = new Regex(regexp);
            while (!regex.IsMatch(field))
            {
                Problem(ref field, msg);
            }
            return Changed;
        }

        public bool Text(ref string field, int min = 0, int max = 0)
        {
            if (min > 0)
            {
                while (field.Length < min)
                {
                    Problem(ref field, string.Format("короче {0} символов", min));
                }
            }
            if (max > 0)
            {
                while (field.Length > max)
                {
                    Problem(ref field, string.Format("длиннее {0} символов", max));
                }
            }
            //while (field.Contains("^"))
            //{ 
            //    Problem(doc, part, ref field, "содержит ^");
            //}
            while (field.Contains("..."))
            {
                Problem(ref field, "содержит многоточие");
            }
            //while (field.StartsWith("\""))
            //{
            //    Problem(doc, part, ref field, "кавычка в начале");
            //}
            while (field.StartsWith("-"))
            {
                Problem(ref field, "прочерк в начале");
            }
            return Changed;
        }

        public bool Date(ref string field, int before = 0, int after = 0)
        {
            DateTimeFormatInfo dfi = new CultureInfo("ru-RU", false).DateTimeFormat;
            DateTime d;
            while (!DateTime.TryParseExact(field, "dd.MM.yyyy", dfi, DateTimeStyles.None, out d))
            {
                Problem(ref field, "не дата");
            }
            if (before != 0)
            {
                while (DateTime.Compare(d, d.AddDays(-before)) < 0)
                {
                    Problem(ref field, string.Format("старее {0} дней", before));
                }
            }
            if (after != 0)
            {
                while (DateTime.Compare(d, d.AddDays(after)) > 0)
                {
                    Problem(ref field, string.Format("позднее {0} дней", after));
                }
            }
            return Changed;
        }

        public bool INN(ref string field, string LS)
        {
            ProbEx(ref field, Properties.Settings.Default.REGEXP_INN);
            //while (!ValidINNKey(field)) //////////////////////////////////
            //{
            //    Problem(ref field, "неправильный");
            //}
            while (LS.StartsWith("40") && field.Equals(Program.OurINN))
            {
                Problem(ref field, "это ИНН Банка");
            }
            while (field.StartsWith("00"))
            {
                Problem(ref field, "не должен начинаться с 00");
            }
            return Changed;
        }

        public bool KPP(ref string field)
        {
            ProbEx(ref field, Properties.Settings.Default.REGEXP_KPP);
            while (field.StartsWith("00"))
            {
                Problem(ref field, "не должен начинаться с 00");
            }
            return Changed;
        }

        public bool LS(ref string ls, string bic, string ks)
        {
            ProbEx(ref ls, Properties.Settings.Default.REGEXP_LS);
            while (!LSKey(ls, bic, ks))
            {
                Problem(ref ls, "не ключуется");
            }
            return Changed;
        }

        private bool LSKey(string ls, string bic, string ks)
        {
            string bic3 = bic.Substring(bic.Length - 3); //КО
            if (string.IsNullOrEmpty(ks))
            {
                bic3 = "0" + bic.Substring(bic.Length - 5, 2); //РКЦ
            }
            string conto = ls.Substring(0, 8); //40702810*00000000123
            string ls11 = ls.Substring(9);

            string stmp = string.Format(" {0}{1}0{2}", bic3, conto, ls11);
            char[] tmp = stmp.ToCharArray();

            int sum = 0;
            for (int i = 1; i < tmp.Length; i++)
            {
                switch (i % 3)
                {
                    case 0:
                        sum += Convert.ToInt32(tmp[i]) * 3 % 10;
                        break;
                    case 1:
                        sum += Convert.ToInt32(tmp[i]) * 7 % 10;
                        break;
                    case 2:
                        sum += Convert.ToInt32(tmp[i]) % 10;
                        break;
                }
            }
            sum = sum * 3 % 10;

            string ret = string.Format("{0}{1}{2}", conto, sum, ls11);

            return ret.Equals(ls);
        }

        //public string InputBox(string Prompt, string Title = "", string DefaultResponse = "")
        //{
        //    string reply = Microsoft.VisualBasic.Interaction.InputBox(Prompt, Title, DefaultResponse);
        //    return reply;
        //}
    }
}
