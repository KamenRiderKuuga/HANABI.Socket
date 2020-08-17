using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public class ListTextWriter : TextWriter
    {
        private ListBox _listBox;
        private delegate void VoidAction();

        public ListTextWriter(ListBox box, long lastCount = 3000)
        {
            _listBox = box;
            System.Threading.Tasks.Task.Run(() =>
            {
                while (true)
                {
                    if (_listBox.Items.Count >= lastCount)
                    {
                        VoidAction action = delegate
                        {
                            _listBox.Items.Clear();
                        };
                        _listBox.BeginInvoke(action);
                    }

                    System.Threading.Thread.Sleep(3000);
                }
            });
        }

        public override void Write(string value)
        {
            VoidAction action = delegate
            {
                _listBox.Items.Insert(0, string.Format("[{0:HH:mm:ss}]{1}", DateTime.Now, value));
            };
            _listBox.BeginInvoke(action);
        }

        public override void WriteLine(string value)
        {
            VoidAction action = delegate
            {
                _listBox.Items.Insert(0, string.Format("[{0:HH:mm:ss}]{1}", DateTime.Now, value));
            };
            _listBox.BeginInvoke(action);
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
