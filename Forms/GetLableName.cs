// Decompiled with JetBrains decompiler
// Type: TLO.local.Forms.GetLableName
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TLO.local.Forms
{
  public class GetLableName : Form
  {
    private IContainer components;
    private TextBox _txtLabel;
    private Label label1;
    private Button btOk;
    private Button btCancel;

    internal string Value
    {
      get
      {
        return this._txtLabel.Text;
      }
      set
      {
        this._txtLabel.Text = value;
      }
    }

    public GetLableName()
    {
      this.InitializeComponent();
    }

    private void btClick(object sender, EventArgs e)
    {
      if (sender == this.btCancel)
      {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
      }
      else
      {
        if (sender != this.btOk)
          return;
        this.DialogResult = DialogResult.OK;
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
      this._txtLabel = new TextBox();
      this.label1 = new Label();
      this.btOk = new Button();
      this.btCancel = new Button();
      this.SuspendLayout();
      this._txtLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this._txtLabel.Location = new Point(94, 12);
      this._txtLabel.Name = "_txtLabel";
      this._txtLabel.Size = new Size(408, 20);
      this._txtLabel.TabIndex = 0;
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 15);
      this.label1.Name = "label1";
      this.label1.Size = new Size(76, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Новая метка:";
      this.btOk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.btOk.Location = new Point(427, 38);
      this.btOk.Name = "btOk";
      this.btOk.Size = new Size(75, 23);
      this.btOk.TabIndex = 2;
      this.btOk.Text = "Применить";
      this.btOk.UseVisualStyleBackColor = true;
      this.btOk.Click += new EventHandler(this.btClick);
      this.btCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.btCancel.Location = new Point(346, 38);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new Size(75, 23);
      this.btCancel.TabIndex = 3;
      this.btCancel.Text = "Отмена";
      this.btCancel.UseVisualStyleBackColor = true;
      this.btCancel.Click += new EventHandler(this.btClick);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(514, 72);
      this.Controls.Add((Control) this.btCancel);
      this.Controls.Add((Control) this.btOk);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this._txtLabel);
      this.FormBorderStyle = FormBorderStyle.None;
      this.Name = "GetLableName";
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "GetLableName";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
