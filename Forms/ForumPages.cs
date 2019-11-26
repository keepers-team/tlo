// Decompiled with JetBrains decompiler
// Type: TLO.local.ForumPages
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TLO.local
{
  partial class ForumPages : UserControl
  {
    private List<Tuple<int, int, TextBox>> Urls { get; set; }

    public ForumPages()
    {
      this.Urls = new List<Tuple<int, int, TextBox>>();
      this.InitializeComponent();
    }

    public void LoadSettings()
    {
      this.panel1.Controls.Clear();
      Dictionary<Tuple<int, int>, Tuple<string, string>> reports = ClientLocalDB.Current.GetReports(new int?());
      List<Category> categoriesEnable = ClientLocalDB.Current.GetCategoriesEnable();
      categoriesEnable.Add(new Category()
      {
        CategoryID = 0,
        Name = " Сводный отчет",
        FullName = " Сводный отчет"
      });
      int num1 = 0;
      int y = 10;
      foreach (Category category1 in (IEnumerable<Category>) categoriesEnable.OrderBy<Category, string>((Func<Category, string>) (x => x.FullName)))
      {
        Category category = category1;
        Label label = new Label();
        label.AutoSize = true;
        label.Location = new Point(3, y);
        label.Size = new Size(35, 13);
        label.TabIndex = num1;
        label.Text = category.FullName;
        this.panel1.Controls.Add((Control) label);
        y += 16;
        KeyValuePair<Tuple<int, int>, Tuple<string, string>>[] array = reports.Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>((Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x => x.Key.Item1 == category.CategoryID)).OrderBy<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int>((Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int>) (x => x.Key.Item2)).ToArray<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>();
        KeyValuePair<Tuple<int, int>, Tuple<string, string>>[] keyValuePairArray;
        if (array.Length != 0)
          keyValuePairArray = array;
        else
          keyValuePairArray = new KeyValuePair<Tuple<int, int>, Tuple<string, string>>[1]
          {
            new KeyValuePair<Tuple<int, int>, Tuple<string, string>>(new Tuple<int, int>(category.CategoryID, 0), new Tuple<string, string>("", ""))
          };
        foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> keyValuePair in keyValuePairArray)
        {
          if (category.CategoryID != 0 || keyValuePair.Key.Item2 == 0)
          {
            int num2 = num1 + 1;
            TextBox textBox1 = new TextBox();
            textBox1.Enabled = false;
            textBox1.Location = new Point(6, y);
            textBox1.Size = new Size(123, 20);
            textBox1.TabIndex = num2;
            textBox1.Text = "Отчет " + (keyValuePair.Key.Item2 != 0 ? keyValuePair.Key.Item2.ToString() + (keyValuePair.Value.Item2 == "Резерв" ? " (Резерв)" : "") : " (Шапка)");
            if (category.CategoryID == 0)
              textBox1.Text = "Сводный отчет";
            this.panel1.Controls.Add((Control) textBox1);
            num1 = num2 + 1;
            TextBox textBox2 = new TextBox();
            textBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox2.Location = new Point(135, y);
            textBox2.Size = new Size(this.panel1.Size.Width - 135, 20);
            textBox2.TabIndex = num1;
            textBox2.Text = string.IsNullOrWhiteSpace(keyValuePair.Value.Item1) ? "" : keyValuePair.Value.Item1;
            this.panel1.Controls.Add((Control) textBox2);
            this.Urls.Add(new Tuple<int, int, TextBox>(keyValuePair.Key.Item1, keyValuePair.Key.Item2, textBox2));
            y += 26;
          }
        }
      }
    }

    public void Save()
    {
      ClientLocalDB.Current.SaveSettingsReport(this.Urls.Select<Tuple<int, int, TextBox>, Tuple<int, int, string>>((Func<Tuple<int, int, TextBox>, Tuple<int, int, string>>) (x => new Tuple<int, int, string>(x.Item1, x.Item2, x.Item3.Text))).ToList<Tuple<int, int, string>>());
    }
  }
}
