using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TLO.Forms
{
    internal partial class FolderNameDialog : Form
    {
        public FolderNameDialog()
        {
            InitializeComponent();
        }

        public string SelectedPath
        {
            get => txtFolderName.Text;
            set => txtFolderName.Text = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private void ClickButton(object sender, EventArgs e)
        {
            if (sender == btAbort)
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
            else if (sender == btCancel)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                if (sender != btOk)
                    return;
                foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
                    if (SelectedPath.Contains(invalidFileNameChar))
                    {
                        var num = (int) MessageBox.Show("Название каталога содержит недопустимый символ: " +
                                                        invalidFileNameChar);
                        return;
                    }

                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}