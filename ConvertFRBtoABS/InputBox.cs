using System;
using System.Windows.Forms;
using System.Drawing;
 
// http://www.rsdn.ru/forum/src/1898705.flat

public class InputBox : Form
{
    private Label label;
    private TextBox textValue;
    private Button buttonOK;
    private Button buttonCancel;

    private InputBox(string Caption, string Text)
    {
        this.label = new Label();
        this.textValue = new TextBox();
        this.buttonOK = new Button();
        this.buttonCancel = new Button();

        this.SuspendLayout();

        this.label.AutoSize = true;
        this.label.Location = new Point(9, 13);
        this.label.Name = "label";
        this.label.Size = new Size(31, 13);
        this.label.TabIndex = 1;
        this.label.Text = Text;

        this.textValue.Location = new Point(12, 31);
        this.textValue.Name = "textValue";
        this.textValue.Size = new Size(245, 20);
        this.textValue.TabIndex = 2;
        this.textValue.WordWrap = false;

        this.buttonOK.DialogResult = DialogResult.OK;
        this.buttonOK.Location = new Point(57, 67);
        this.buttonOK.Name = "buttonOK";
        this.buttonOK.Size = new Size(75, 23);
        this.buttonOK.TabIndex = 3;
        this.buttonOK.Text = "OK";
        this.buttonOK.UseVisualStyleBackColor = true;

        this.buttonCancel.DialogResult = DialogResult.Cancel;
        this.buttonCancel.Location = new Point(138, 67);
        this.buttonCancel.Name = "buttonCancel";
        this.buttonCancel.Size = new Size(75, 23);
        this.buttonCancel.TabIndex = 4;
        this.buttonCancel.Text = "Отмена"; //"Cancel";
        this.buttonCancel.UseVisualStyleBackColor = true;

        this.AcceptButton = this.buttonOK;
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.CancelButton = this.buttonCancel;
        this.ClientSize = new Size(270, 103);

        this.Controls.Add(this.buttonCancel);
        this.Controls.Add(this.buttonOK);
        this.Controls.Add(this.textValue);
        this.Controls.Add(this.label);

        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "InputBox";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = Caption;

        this.ResumeLayout(false);
        this.PerformLayout();
    }

    public static bool Query(string Caption, string Text, ref string s_val)
    {
        InputBox ib = new InputBox(Caption, Text);
        ib.textValue.Text = s_val;
        if (ib.ShowDialog() != DialogResult.OK)
        {
            return false;
        }
        s_val = ib.textValue.Text;
        return true;
    }

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

                if ((sTr.Length > 0) && (sTr[0] == '#'))
                {
                    sTr = sTr.Remove(0, 1);
                    val = Convert.ToInt32(sTr, 16);
                }
                else if ((sTr.Length > 1) && ((sTr[1] == 'x') && (sTr[0] == '0')))
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

            if ((val < min) || (val > max))
            {
                MessageBox.Show("Требуется число в диапазоне " + min.ToString() + "..." + max.ToString() + " !");
                OKVal = false;
            }
        }
        while (!OKVal);

        value = val;
        return true;
    }
}
