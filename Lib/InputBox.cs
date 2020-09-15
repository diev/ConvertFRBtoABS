// Copyright (c) 2013-2020 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.

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
        private readonly Label label;
        private readonly TextBox textValue;
        private readonly Button buttonOK;
        private readonly Button buttonCancel;

        /// <summary>
        /// Handmade form created in code.
        /// </summary>
        /// <param name="Caption">Caption of the dialog window [null = Application.ProductName].</param>
        /// <param name="Text">Text to show.</param>
        private InputBox(string Caption, string Text)
        {
            label = new Label();
            textValue = new TextBox();
            buttonOK = new Button();
            buttonCancel = new Button();

            SuspendLayout();

            label.AutoSize = true;
            label.Location = new Point(9, 13);
            label.Name = "label";
            label.Size = new Size(31, 13);
            label.TabIndex = 1;
            label.Text = Text;

            textValue.Location = new Point(12, 31);
            textValue.Name = "textValue";
            textValue.Size = new Size(245, 20);
            textValue.TabIndex = 2;
            textValue.WordWrap = false;

            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(57, 67);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 3;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;

            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(138, 67);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Отмена"; //"Cancel";
            buttonCancel.UseVisualStyleBackColor = true;

            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(270, 103);

            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textValue);
            Controls.Add(label);

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
            ib.textValue.Text = s_val;

            if (ib.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            s_val = ib.textValue.Text.Trim();

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