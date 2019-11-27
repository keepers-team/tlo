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
        return _txtLabel.Text;
      }
      set
      {
        _txtLabel.Text = value;
      }
    }

    public GetLableName()
    {
      InitializeComponent();
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

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

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
      Name = "GetLableName";
      StartPosition = FormStartPosition.CenterScreen;
      Text = "GetLableName";
      ResumeLayout(false);
      PerformLayout();
    }
  }
}
