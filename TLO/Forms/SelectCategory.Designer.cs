﻿using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TLO.Forms {
    partial class SelectCategory {
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
            treeView1 = new TreeView();
            _btCancel = new Button();
            _btSelected = new Button();
            _txtFrom = new TextBox();
            SuspendLayout();
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.Location = new Point(12, 12);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(468, 495);
            treeView1.TabIndex = 0;
            treeView1.DoubleClick += _btSelected_Click;
            _btCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btCancel.Location = new Point(405, 513);
            _btCancel.Name = "_btCancel";
            _btCancel.Size = new Size(75, 23);
            _btCancel.TabIndex = 1;
            _btCancel.Text = "Отмена";
            _btCancel.UseVisualStyleBackColor = true;
            _btCancel.Click += _btCancel_Click;
            _btSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btSelected.Location = new Point(324, 513);
            _btSelected.Name = "_btSelected";
            _btSelected.Size = new Size(75, 23);
            _btSelected.TabIndex = 2;
            _btSelected.Text = "Выбрать";
            _btSelected.UseVisualStyleBackColor = true;
            _btSelected.Click += _btSelected_Click;
            _txtFrom.Location = new Point(12, 513);
            _txtFrom.Name = "_txtFrom";
            _txtFrom.Size = new Size(306, 20);
            _txtFrom.TabIndex = 3;
            _txtFrom.KeyDown += _txtFrom_KeyDown;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(492, 548);
            ControlBox = false;
            Controls.Add(_txtFrom);
            Controls.Add(_btSelected);
            Controls.Add(_btCancel);
            Controls.Add(treeView1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "SelectCategory";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Выбор категории";
            ResumeLayout(false);
            PerformLayout();
        }
        
        #endregion
        
        private TreeView treeView1;
        private Button _btCancel;
        private Button _btSelected;
        private TextBox _txtFrom;
    }
}
