#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov 2013-2021
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
using System.Windows.Forms;
using System.Drawing;

namespace Lib
{
    /// <summary>
    /// Input a value using GUI from console applications.
    /// </summary>
    /// <remarks>
    /// Code is based on http://www.rsdn.ru/forum/src/1898705.flat
    /// </remarks>
    public class InputBox : Form
    {
        private readonly Label _label;
        private readonly TextBox _textValue;
        private readonly Button _buttonOK;
        private readonly Button _buttonCancel;

        /// <summary>
        /// Handmade form created in code.
        /// </summary>
        /// <param name="Caption">Caption of the dialog window [null = Application.ProductName].</param>
        /// <param name="Text">Text to show.</param>
        private InputBox(string Caption, string Text)
        {
            _label = new Label();
            _textValue = new TextBox();
            _buttonOK = new Button();
            _buttonCancel = new Button();

            SuspendLayout();

            _label.AutoSize = true;
            _label.Location = new Point(9, 13);
            _label.Name = "label";
            _label.Size = new Size(31, 13);
            _label.TabIndex = 1;
            _label.Text = Text;

            _textValue.Location = new Point(12, 31);
            _textValue.Name = "textValue";
            _textValue.Size = new Size(245, 20);
            _textValue.TabIndex = 2;
            _textValue.WordWrap = false;

            _buttonOK.DialogResult = DialogResult.OK;
            _buttonOK.Location = new Point(57, 67);
            _buttonOK.Name = "buttonOK";
            _buttonOK.Size = new Size(75, 23);
            _buttonOK.TabIndex = 3;
            _buttonOK.Text = "OK";
            _buttonOK.UseVisualStyleBackColor = true;

            _buttonCancel.DialogResult = DialogResult.Cancel;
            _buttonCancel.Location = new Point(138, 67);
            _buttonCancel.Name = "buttonCancel";
            _buttonCancel.Size = new Size(75, 23);
            _buttonCancel.TabIndex = 4;
            _buttonCancel.Text = "Отмена"; //"Cancel";
            _buttonCancel.UseVisualStyleBackColor = true;

            AcceptButton = _buttonOK;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _buttonCancel;
            ClientSize = new Size(270, 103);

            Controls.Add(_buttonCancel);
            Controls.Add(_buttonOK);
            Controls.Add(_textValue);
            Controls.Add(_label);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputBox";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            this.Text = Caption ?? Application.ProductName;

            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// Input a string value like an InputQuery().
        /// </summary>
        /// <example>
        /// <code>
        ///     string value = "abcd";
        ///     if(!InputBox.Query("Ввод строки", "Строка:", ref value))
        ///         MessageBox.Show("Cancel");
        ///     else
        ///         MessageBox.Show(value);
        /// </code>
        /// </example>
        /// <param name="Caption">Caption of the dialog window [null = Application.ProductName].</param>
        /// <param name="Text">Prompt to user.</param>
        /// <param name="s_val">Value to show and return.</param>
        /// <returns>User pressed OK.</returns>
        public static bool Query(string Caption, string Text, ref string s_val)
        {
            InputBox ib = new InputBox(Caption, Text);
            ib._textValue.Text = s_val;

            if (ib.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            s_val = ib._textValue.Text.Trim();

            return true;
        }

        /// <summary>
        /// Input a numeric (optionaly hex) value like an InputQuery().
        /// </summary>
        /// <example>
        /// <code>
        ///     int value = 0;
        ///     if (!InputBox.InputValue("Ввод числа X", "Значение X:", "0x", "X4", ref value, 0, 0xFFFF)) return;
        ///     MessageBox.Show("Введено число X = " + value.ToString());
        /// </code>
        /// </example>
        /// <param name="Caption">Caption of the dialog window [null = Application.ProductName].</param>
        /// <param name="Text">Prompt to user.</param>
        /// <param name="prefix">Hex.</param>
        /// <param name="format">Format to String.</param>
        /// <param name="value">Value to show and return.</param>
        /// <param name="min">Aloowed minimum for value.</param>
        /// <param name="max">Allowed maximum for value.</param>
        /// <returns></returns>
        public static bool InputValue(string Caption, string Text, string prefix, string format, ref int value, int min, int max)
        {
            int val = value;
            string s_val = prefix + value.ToString(format);

            bool OKVal;

            do
            {
                OKVal = true;

                if (!Query(Caption, Text, ref s_val))
                {
                    return false;
                }

                try
                {
                    string sTr = s_val.Trim();

                    if (sTr.Length > 0 && sTr[0] == '#')
                    {
                        sTr = sTr.Remove(0, 1);
                        val = Convert.ToInt32(sTr, 16);
                    }
                    else if (sTr.Length > 1 && sTr[1] == 'x' && sTr[0] == '0')
                    {
                        sTr = sTr.Remove(0, 2);
                        val = Convert.ToInt32(sTr, 16);
                    }
                    else
                    {
                        val = Convert.ToInt32(sTr, 10);
                    }
                }
                catch
                {
                    MessageBox.Show("Требуется ввести число!");
                    OKVal = false;
                }

                if (val < min || val > max)
                {
                    MessageBox.Show("Требуется число в диапазоне " + min.ToString() + ".." + max.ToString() + " !");
                    OKVal = false;
                }
            }
            while (!OKVal);

            value = val;

            return true;
        }
    }
}
