using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TLO.Clients;
using TLO.Info;

namespace TLO.Forms
{
    internal partial class ForumPages : UserControl
    {
        public ForumPages()
        {
            Urls = new List<Tuple<int, int, TextBox>>();
            InitializeComponent();
        }

        private List<Tuple<int, int, TextBox>> Urls { get; }

        public void LoadSettings()
        {
            panel1.Controls.Clear();
            var reports = ClientLocalDb.Current.GetReports(new int?());
            var categoriesEnable = ClientLocalDb.Current.GetCategoriesEnable();
            categoriesEnable.Add(new Category
            {
                CategoryID = 0,
                Name = " Сводный отчет",
                FullName = " Сводный отчет"
            });
            var num1 = 0;
            var y = 10;
            foreach (var category1 in categoriesEnable.OrderBy(x => x.FullName))
            {
                var category = category1;
                var label = new Label();
                label.AutoSize = true;
                label.Location = new Point(3, y);
                label.Size = new Size(35, 13);
                label.TabIndex = num1;
                label.Text = category.FullName;
                panel1.Controls.Add(label);
                y += 16;
                var array = reports.Where(x => x.Key.Item1 == category.CategoryID).OrderBy(x => x.Key.Item2).ToArray();
                KeyValuePair<Tuple<int, int>, Tuple<string, string>>[] keyValuePairArray;
                if (array.Length != 0)
                    keyValuePairArray = array;
                else
                    keyValuePairArray = new KeyValuePair<Tuple<int, int>, Tuple<string, string>>[1]
                    {
                        new KeyValuePair<Tuple<int, int>, Tuple<string, string>>(
                            new Tuple<int, int>(category.CategoryID, 0), new Tuple<string, string>("", ""))
                    };
                foreach (var keyValuePair in keyValuePairArray)
                    if (category.CategoryID != 0 || keyValuePair.Key.Item2 == 0)
                    {
                        var num2 = num1 + 1;
                        var textBox1 = new TextBox();
                        textBox1.Enabled = false;
                        textBox1.Location = new Point(6, y);
                        textBox1.Size = new Size(123, 20);
                        textBox1.TabIndex = num2;
                        textBox1.Text = "Отчет " + (keyValuePair.Key.Item2 != 0
                                            ? keyValuePair.Key.Item2 +
                                              (keyValuePair.Value.Item2 == "Резерв" ? " (Резерв)" : "")
                                            : " (Шапка)");
                        if (category.CategoryID == 0)
                            textBox1.Text = "Сводный отчет";
                        panel1.Controls.Add(textBox1);
                        num1 = num2 + 1;
                        var textBox2 = new TextBox();
                        textBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        textBox2.Location = new Point(135, y);
                        textBox2.Size = new Size(panel1.Size.Width - 135, 20);
                        textBox2.TabIndex = num1;
                        textBox2.Text = string.IsNullOrWhiteSpace(keyValuePair.Value.Item1)
                            ? ""
                            : keyValuePair.Value.Item1;
                        panel1.Controls.Add(textBox2);
                        Urls.Add(new Tuple<int, int, TextBox>(keyValuePair.Key.Item1, keyValuePair.Key.Item2,
                            textBox2));
                        y += 26;
                    }
            }
        }

        public void Save()
        {
            ClientLocalDb.Current.SaveSettingsReport(Urls
                .Select(x => new Tuple<int, int, string>(x.Item1, x.Item2, x.Item3.Text)).ToList());
        }
    }
}