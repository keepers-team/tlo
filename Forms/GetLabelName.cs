using System;
using System.Windows.Forms;

namespace TLO.Forms
{
    internal partial class GetLabelName : Form
    {
        public GetLabelName()
        {
            InitializeComponent();
        }

        internal string Value
        {
            get => _txtLabel.Text;
            set => _txtLabel.Text = value;
        }

        private void btClick(object sender, EventArgs e)
        {
            if (sender == btCancel)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                if (sender != btOk)
                    return;
                DialogResult = DialogResult.OK;
            }
        }
    }
}