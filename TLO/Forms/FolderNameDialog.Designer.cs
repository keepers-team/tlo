﻿using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TLO.Forms
{
    partial class FolderNameDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        
        private void InitializeComponent()
        {
            btCancel = new Button();
            btOk = new Button();
            btAbort = new Button();
            txtFolderName = new TextBox();
            SuspendLayout();
            btCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btCancel.Location = new Point(426, 38);
            btCancel.Name = "btCancel";
            btCancel.Size = new Size(75, 23);
            btCancel.TabIndex = 0;
            btCancel.Text = "Пропустить";
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += ClickButton;
            btOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btOk.Location = new Point(345, 38);
            btOk.Name = "btOk";
            btOk.Size = new Size(75, 23);
            btOk.TabIndex = 1;
            btOk.Text = "Применить";
            btOk.UseVisualStyleBackColor = true;
            btOk.Click += ClickButton;
            btAbort.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btAbort.Location = new Point(12, 38);
            btAbort.Name = "btAbort";
            btAbort.Size = new Size(75, 23);
            btAbort.TabIndex = 2;
            btAbort.Text = "Прервать";
            btAbort.UseVisualStyleBackColor = true;
            btAbort.Click += ClickButton;
            txtFolderName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFolderName.Location = new Point(12, 12);
            txtFolderName.Name = "txtFolderName";
            txtFolderName.Size = new Size(489, 20);
            txtFolderName.TabIndex = 3;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(513, 73);
            ControlBox = false;
            Controls.Add(txtFolderName);
            Controls.Add(btAbort);
            Controls.Add(btOk);
            Controls.Add(btCancel);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FolderNameDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Запрос наименования каталога";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btCancel;
        private Button btOk;
        private Button btAbort;
        private TextBox txtFolderName;
    }
}