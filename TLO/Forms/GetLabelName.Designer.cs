﻿using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TLO.Forms
{
    partial class GetLabelName
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
            _txtLabel = new TextBox();
            label1 = new Label();
            btOk = new Button();
            btCancel = new Button();
            SuspendLayout();
            _txtLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtLabel.Location = new Point(94, 12);
            _txtLabel.Name = "_txtLabel";
            _txtLabel.Size = new Size(408, 20);
            _txtLabel.TabIndex = 0;
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(76, 13);
            label1.TabIndex = 1;
            label1.Text = "Новая метка:";
            btOk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btOk.Location = new Point(427, 38);
            btOk.Name = "btOk";
            btOk.Size = new Size(75, 23);
            btOk.TabIndex = 2;
            btOk.Text = "Применить";
            btOk.UseVisualStyleBackColor = true;
            btOk.Click += btClick;
            btCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btCancel.Location = new Point(346, 38);
            btCancel.Name = "btCancel";
            btCancel.Size = new Size(75, 23);
            btCancel.TabIndex = 3;
            btCancel.Text = "Отмена";
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btClick;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(514, 72);
            Controls.Add(btCancel);
            Controls.Add(btOk);
            Controls.Add(label1);
            Controls.Add(_txtLabel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "GetLabelName";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "GetLabelName";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox _txtLabel;
        private Label label1;
        private Button btOk;
        private Button btCancel;
    }
}