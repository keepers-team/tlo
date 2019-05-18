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
        return this.txtFolderName.Text;
      }
      set
      {
        this.txtFolderName.Text = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
      }
    }

    public FolderNameDialog()
    {
      this.InitializeComponent();
    }

    private void ClickButton(object sender, EventArgs e)
    {
      if (sender == this.btAbort)
      {
        this.DialogResult = DialogResult.Abort;
        this.Close();
      }
      else if (sender == this.btCancel)
      {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
      }
      else
      {
        if (sender != this.btOk)
          return;
        foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
        {
          if (this.SelectedPath.Contains<char>(invalidFileNameChar))
          {
            int num = (int) MessageBox.Show("Название каталога содержит недопустимый символ: " + invalidFileNameChar.ToString());
            return;
          }
        }
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.btCancel = new Button();
      this.btOk = new Button();
      this.btAbort = new Button();
      this.txtFolderName = new TextBox();
      this.SuspendLayout();
      this.btCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.btCancel.Location = new Point(426, 38);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new Size(75, 23);
      this.btCancel.TabIndex = 0;
      this.btCancel.Text = "Пропустить";
      this.btCancel.UseVisualStyleBackColor = true;
      this.btCancel.Click += new EventHandler(this.ClickButton);
      this.btOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.btOk.Location = new Point(345, 38);
      this.btOk.Name = "btOk";
      this.btOk.Size = new Size(75, 23);
      this.btOk.TabIndex = 1;
      this.btOk.Text = "Применить";
      this.btOk.UseVisualStyleBackColor = true;
      this.btOk.Click += new EventHandler(this.ClickButton);
      this.btAbort.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      this.btAbort.Location = new Point(12, 38);
      this.btAbort.Name = "btAbort";
      this.btAbort.Size = new Size(75, 23);
      this.btAbort.TabIndex = 2;
      this.btAbort.Text = "Прервать";
      this.btAbort.UseVisualStyleBackColor = true;
      this.btAbort.Click += new EventHandler(this.ClickButton);
      this.txtFolderName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.txtFolderName.Location = new Point(12, 12);
      this.txtFolderName.Name = "txtFolderName";
      this.txtFolderName.Size = new Size(489, 20);
      this.txtFolderName.TabIndex = 3;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(513, 73);
      this.ControlBox = false;
      this.Controls.Add((Control) this.txtFolderName);
      this.Controls.Add((Control) this.btAbort);
      this.Controls.Add((Control) this.btOk);
      this.Controls.Add((Control) this.btCancel);
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FolderNameDialog";
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Запрос наименования каталога";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
