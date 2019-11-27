// Decompiled with JetBrains decompiler
// Type: TLO.local.Forms.FolderNameDialog
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TLO.local.Forms
{
  public class FolderNameDialog : Form
  {
    private IContainer components;
    private Button btCancel;
    private Button btOk;
    private Button btAbort;
    private TextBox txtFolderName;

    public string SelectedPath
    {
      get
      {
        return txtFolderName.Text;
      }
      set
      {
        txtFolderName.Text = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
      }
    }

    public FolderNameDialog()
    {
      InitializeComponent();
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
        foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
        {
          if (SelectedPath.Contains(invalidFileNameChar))
          {
            int num = (int) MessageBox.Show("Название каталога содержит недопустимый символ: " + invalidFileNameChar);
            return;
          }
        }
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

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
  }
}
