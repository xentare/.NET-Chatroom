using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace Client
{
    public class TextBoxOutputter : TextWriter
    {
        TextBox textBox;

        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
            }));
        }

        public override Encoding Encoding => System.Text.Encoding.UTF8;
    }
}