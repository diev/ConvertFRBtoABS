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

using System.Data;
using System.Text;

namespace ConvertFRBtoABS
{
    public class PayDoc
    {
        public string DocNo
        { get; set; }

        public string DocDate
        { get; set; }

        public string ValDate //v1.2
        { get; set; }

        public string Sum
        { get; set; }

        public string Queue
        { get; set; }

        public string Details
        { get; set; }

        public string INN
        { get; set; }

        public string KPP
        { get; set; }

        public string LS
        { get; set; }

        public string BIC
        { get; set; }

        public string KS
        { get; set; }

        public string Name2
        { get; set; }

        public string INN2
        { get; set; }

        public string KPP2
        { get; set; }

        public string LS2
        { get; set; }

        public string BIC2
        { get; set; }

        public string KS2
        { get; set; }

        public string SS
        { get; set; }

        public string NAL1
        { get; set; }

        public string NAL2
        { get; set; }

        public string NAL3
        { get; set; }

        public string NAL4
        { get; set; }

        public string NAL5
        { get; set; }

        public string NAL6
        { get; set; }

        public string NAL7
        { get; set; }

        public string OpKind
        { get; set; }

        public string PayCode
        { get; set; }

        public string PurposCode
        { get; set; }

        public PayDoc() // for Tests
        {
            Init();
        }

        public PayDoc(DataRow rec)
        {
            DocNo = rec["NUMBER"].ToString().TrimStart(new char[] { '0' });
            DocDate = rec["DATE"].ToString();

            if (rec.Table.Columns.Contains("ACCEPT_TER")) //v1.2
            {
                ValDate = rec["ACCEPT_TER"].ToString();
            }

            Sum = rec["SUM"].ToString();
            Queue = rec["PAY_QUEUE"].ToString();
            Details = StripSpaces(rec["PAYMENT_AI"].ToString());

            INN = rec["PAYER_INN"].ToString();
            KPP = rec["PAYER_KPP"].ToString();
            LS = rec["PAYER_ACC"].ToString();

            Name2 = StripSpaces(rec["RECIP"].ToString());
            INN2 = rec["RECIP_INN"].ToString();
            KPP2 = rec["RECIP_KPP"].ToString();
            LS2 = rec["RECIP_ACC"].ToString();
            BIC2 = rec["RECIP_BIC"].ToString();
            KS2 = rec["RECIP_KS"].ToString();

            if (rec.Table.Columns.Contains("TAX_STATUS"))
            {
                SS = rec["TAX_STATUS"].ToString();
                NAL1 = rec["KBK"].ToString();
                NAL2 = rec["OKATO"].ToString();
                NAL3 = rec["TAX_REASON"].ToString();
                NAL4 = rec["TAX_PERIOD"].ToString();
                NAL5 = rec["TAX_DOC_N"].ToString();
                NAL6 = rec["TAX_DATE"].ToString();

                if (rec.Table.Columns.Contains("PAYCODE"))
                {
                    PayCode = rec["PAYCODE"].ToString();
                }
            }

            OpKind = rec["OP_KIND"].ToString();
            PurposCode = rec["PurposCode"].ToString();
        }

        private void Init()
        {
            DocNo = string.Empty;
            DocDate = string.Empty;
            ValDate = string.Empty;
            Sum = string.Empty;
            Queue = string.Empty;
            Details = string.Empty;
            INN = string.Empty;
            KPP = string.Empty;
            LS = string.Empty;
            BIC = Program.OurBIC;
            KS = Program.OurKS;
            Name2 = string.Empty;
            INN2 = string.Empty;
            KPP2 = string.Empty;
            LS2 = string.Empty;
            BIC2 = string.Empty;
            KS2 = string.Empty;
            SS = string.Empty;
            NAL1 = string.Empty;
            NAL2 = string.Empty;
            NAL3 = string.Empty;
            NAL4 = string.Empty;
            NAL5 = string.Empty;
            NAL6 = string.Empty;
            NAL7 = string.Empty;
            OpKind = string.Empty;
            PayCode = string.Empty;
            PurposCode = string.Empty;
        }

        //public string ExportToBankier(bool local)
        //{
        //    string[] doc = new string[103];

        //    doc[1] = DMY2YMD(DocDate);
        //    doc[2] = DocNo;
        //    doc[3] = OpKind;
        //    doc[4] = "A";
        //    doc[9] = local ? "801" : "802";
        //    doc[10] = "4";
        //    doc[11] = "810";
        //    doc[12] = Sum;
        //    doc[13] = Sum;
        //    doc[15] = Queue;
        //    doc[19] = LS;
        //    doc[23] = BIC;
        //    doc[25] = LS2;
        //    doc[27] = KS2;
        //    doc[29] = BIC2;
        //    doc[31] = Details;
        //    doc[35] = Name2;
        //    doc[48] = "e";
        //    doc[49] = LS;
        //    doc[51] = local ? string.Empty : "SM1";
        //    doc[53] = OpKind.Equals("01") ? "26" : "5";
        //    doc[54] = "e";
        //    doc[55] = local ? LS2 : KS;
        //    doc[73] = "243";
        //    doc[75] = INN;
        //    doc[76] = INN2;
        //    doc[92] = KPP;
        //    doc[93] = KPP2;
        //    doc[94] = SS;
        //    doc[95] = NAL1;
        //    doc[96] = NAL2;
        //    doc[97] = NAL3;
        //    doc[98] = NAL4;
        //    doc[99] = NAL5;
        //    doc[100] = NAL6;
        //    doc[101] = NAL7;
        //    doc[102] = DateTime.Today.ToString("yyyyMMdd");

        //    return string.Join("^", doc, 1, 102);
        //}

        public string ExportToInversion(bool local)
        {
            string fmt = Program.InversionFormat;

            StringBuilder doc = new StringBuilder(2048);
            doc.Append("# Doc Begin\n");

            if (local)
            {
                string batch = OpKind.Equals("01") ? "26" : "5"; //Batch_Num

                doc.AppendFormat(fmt, "Address", "0");
                doc.AppendFormat(fmt, "BO1", "2");
                doc.AppendFormat(fmt, "BO2", string.Empty);
                doc.AppendFormat(fmt, "Date_Reg", DocDate);
                doc.AppendFormat(fmt, "Date_Doc", DocDate);
                doc.AppendFormat(fmt, "Date_Val", DocDate);
                doc.AppendFormat(fmt, "Doc_Num", DocNo);
                doc.AppendFormat(fmt, "Batch_Num", batch);
                doc.AppendFormat(fmt, "Priority", Queue);
                doc.AppendFormat(fmt, "Purpose", Details);
                doc.AppendFormat(fmt, "Summa", Sum);
                doc.AppendFormat(fmt, "Currency", "RUR");
                doc.AppendFormat(fmt, "Date_Shadow", DocDate);
                doc.AppendFormat(fmt, "DWay_Type", "E");
                doc.AppendFormat(fmt, "VO", OpKind);
                doc.AppendFormat(fmt, "Cus_SB", string.Empty);
                doc.AppendFormat(fmt, "Payer_Acc", LS);
                doc.AppendFormat(fmt, "Recipient_Acc", LS2);
                doc.AppendFormat(fmt, "Client_Name", string.Empty);
                doc.AppendFormat(fmt, "Client_INN", INN);
                doc.AppendFormat(fmt, "Client_CorAcc", string.Empty /*doc.KS*/);
                doc.AppendFormat(fmt, "Corr_RBIC", BIC2);
                doc.AppendFormat(fmt, "Corr_CorAcc", KS2);
                doc.AppendFormat(fmt, "Corr_Bank_Name", string.Empty);
                doc.AppendFormat(fmt, "Corr_Name", Name2);
                doc.AppendFormat(fmt, "Corr_INN", INN2);
                doc.AppendFormat(fmt, "User", string.Empty /*"XXI"*/);
                doc.AppendFormat(fmt, "KPP", KPP);
                doc.AppendFormat(fmt, "KPPPOL", KPP2);
                doc.AppendFormat(fmt, "STATUSSOSTAVIT", SS);
                doc.AppendFormat(fmt, "KBK_F", NAL1);
                doc.AppendFormat(fmt, "OKATO", NAL2);
                doc.AppendFormat(fmt, "POKOSNPLAT", NAL3);
                doc.AppendFormat(fmt, "POKNALPERIOD", NAL4);
                doc.AppendFormat(fmt, "POKNUMDOC", NAL5);
                doc.AppendFormat(fmt, "POKDATEDOC", NAL6);
                doc.AppendFormat(fmt, "POKTYPEPLAT", NAL7);
                doc.AppendFormat(fmt, "Doc_Index", PayCode);
                doc.AppendFormat(fmt, "PurposCode", PurposCode);
            }
            else //out
            {
                string batch = "26";
                string bo1 = "4";

                switch (OpKind)
                {
                    case "02":
                        batch = "5";
                        bo1 = "15";
                        break;

                    case "06":
                        batch = "5";
                        bo1 = "23";
                        break;

                    default:
                        break;
                }

                doc.AppendFormat(fmt, "Address", "0");
                doc.AppendFormat(fmt, "BO1", bo1);
                doc.AppendFormat(fmt, "BO2", string.Empty);
                doc.AppendFormat(fmt, "Date_Reg", DocDate);
                doc.AppendFormat(fmt, "Date_Doc", DocDate);
                doc.AppendFormat(fmt, "Date_Val", bo1.Equals("15") ? ValDate : DocDate);
                doc.AppendFormat(fmt, "Doc_Num", DocNo);
                doc.AppendFormat(fmt, "Batch_Num", batch);
                doc.AppendFormat(fmt, "Priority", Queue);
                doc.AppendFormat(fmt, "Purpose", Details);
                doc.AppendFormat(fmt, "Summa", Sum);
                doc.AppendFormat(fmt, "Currency", "RUR");
                doc.AppendFormat(fmt, "Date_Shadow", DocDate);
                doc.AppendFormat(fmt, "DWay_Type", "E");
                doc.AppendFormat(fmt, "VO", OpKind);
                doc.AppendFormat(fmt, "Cus_SB", string.Empty);
                doc.AppendFormat(fmt, "Payer_Acc", LS);
                doc.AppendFormat(fmt, "Recipient_Acc", LS2);
                doc.AppendFormat(fmt, "Client_Name", string.Empty);
                doc.AppendFormat(fmt, "Client_INN", INN);
                doc.AppendFormat(fmt, "Client_CorAcc", string.Empty /*doc.KS*/);
                doc.AppendFormat(fmt, "Corr_RBIC", BIC2);
                doc.AppendFormat(fmt, "Corr_CorAcc", KS2);
                doc.AppendFormat(fmt, "Corr_Bank_Name", string.Empty);
                doc.AppendFormat(fmt, "Corr_Name", Name2);
                doc.AppendFormat(fmt, "Corr_INN", INN2);
                doc.AppendFormat(fmt, "User", string.Empty /*"XXI"*/);
                doc.AppendFormat(fmt, "KPP", KPP);
                doc.AppendFormat(fmt, "KPPPOL", KPP2);
                doc.AppendFormat(fmt, "STATUSSOSTAVIT", SS);
                doc.AppendFormat(fmt, "KBK_F", NAL1);
                doc.AppendFormat(fmt, "OKATO", NAL2);
                doc.AppendFormat(fmt, "POKOSNPLAT", NAL3);
                doc.AppendFormat(fmt, "POKNALPERIOD", NAL4);
                doc.AppendFormat(fmt, "POKNUMDOC", NAL5);
                doc.AppendFormat(fmt, "POKDATEDOC", NAL6);
                doc.AppendFormat(fmt, "POKTYPEPLAT", NAL7);
                doc.AppendFormat(fmt, "Doc_Index", PayCode);
                doc.AppendFormat(fmt, "PurposCode", PurposCode);
            }

            doc.Append("# Doc End\n");

            return doc.ToString();
        }

        private static string StripSpaces(string s)
        {
            while (s.Contains("  "))
            {
                s = s.Replace("  ", " ");
            }

            return s;
        }

        //private static string YMD2DMY(string s) //yyyymmdd -> dd.mm.yyyy
        //{
        //    string ret = string.Empty;

        //    if (s.Length == 8)
        //    {
        //        ret = string.Join(".", s.Substring(6, 2), s.Substring(4, 2), s.Substring(0, 4));
        //    }

        //    return ret;
        //}

        //private static string DMY2YMD(string s) //dd.mm.yyyy -> yyyymmdd
        //{
        //    string ret = string.Empty;

        //    if (s.Length == 10)
        //    {
        //        ret = s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2);
        //    }

        //    return ret;
        //}
    }
}
