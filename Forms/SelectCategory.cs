// Decompiled with JetBrains decompiler
// Type: TLO.local.SelectCategory
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TLO.local
{
  internal class SelectCategory : Form
  {
    private static Logger _logger;
    private IContainer components;
    private TreeView treeView1;
    private Button _btCancel;
    private Button _btSelected;
    private TextBox _txtFrom;

    public Category SelectedCategory { get; private set; }

    public List<Category> SelectedCategories { get; private set; }

    public SelectCategory()
    {
      if (SelectCategory._logger == null)
        SelectCategory._logger = LogManager.GetLogger("SelectCategory");
      this.InitializeComponent();
      this.SelectedCategories = new List<Category>();
    }

    public void Read()
    {
      try
      {
        ClientLocalDB.Current.CategoriesSave(RuTrackerOrg.Current.GetCategories(), true);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Не удалось загрузить список категорий.\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
        SelectCategory._logger.Error(ex.Message + "\r\n" + ex.StackTrace);
      }
      Category[] array = ClientLocalDB.Current.GetCategories().OrderBy<Category, string>((Func<Category, string>) (x => x.FullName)).ToArray<Category>();
      foreach (Category category3 in ((IEnumerable<Category>) array).Where<Category>((Func<Category, bool>) (x => x.CategoryID > 999999)).OrderBy<Category, string>((Func<Category, string>) (x => x.FullName)).ToArray<Category>())
      {
        Category category1 = category3;
        List<TreeNode> source1 = new List<TreeNode>();
        Category[] categoryArray1 = array;
        foreach (Category category4 in ((IEnumerable<Category>) categoryArray1).Where<Category>((Func<Category, bool>) (x => x.ParentID == category1.CategoryID)).OrderBy<Category, string>((Func<Category, string>) (x => x.FullName)).ToArray<Category>())
        {
          Category category2 = category4;
          List<TreeNode> source2 = new List<TreeNode>();
          Category[] categoryArray2 = array;
          foreach (Category category5 in ((IEnumerable<Category>) categoryArray2).Where<Category>((Func<Category, bool>) (x => x.ParentID == category2.CategoryID)).OrderBy<Category, string>((Func<Category, string>) (x => x.FullName)).ToArray<Category>())
            source2.Add(new TreeNode(category5.Name)
            {
              Tag = (object) category5
            });
          if (source2.Count<TreeNode>() != 0)
            source1.Add(new TreeNode(category2.Name, source2.ToArray())
            {
              Tag = (object) category2
            });
          else
            source1.Add(new TreeNode(category2.Name)
            {
              Tag = (object) category2
            });
        }
        if (source1.Count<TreeNode>() != 0)
          this.treeView1.Nodes.Add(new TreeNode(category1.Name, source1.ToArray())
          {
            Tag = (object) category1
          });
        else
          this.treeView1.Nodes.Add(new TreeNode(category1.Name)
          {
            Tag = (object) category1
          });
      }
    }

    private void _btCancel_Click(object sender, EventArgs e)
    {
      this.SelectedCategory = (Category) null;
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void _btSelected_Click(object sender, EventArgs e)
    {
      try
      {
        if (this.treeView1 == null)
          return;
        TreeNode selectedNode = this.treeView1.SelectedNode;
        if (selectedNode == null)
          return;
        Category tag = selectedNode.Tag as Category;
        if (tag == null || tag.CategoryID > 999999)
        {
          int num = (int) MessageBox.Show("Не выбран раздел или выбран корневой раздел\r\n(Корневой раздел нельзя выбирать)");
        }
        else
        {
          this.SelectedCategory = tag;
          this.DialogResult = DialogResult.OK;
          this.Close();
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Непредвиденное исключение\r\n " + ex.Message);
      }
    }

    private void _txtFrom_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Return)
        return;
      if (string.IsNullOrWhiteSpace(this._txtFrom.Text))
        return;
      try
      {
        if (this._txtFrom.Text.Split('=').Length != 2)
          return;
        IEnumerable<Tuple<int, string>> categoriesFromPost = new RuTrackerOrg(Settings.Current.KeeperName, Settings.Current.KeeperPass).GetCategoriesFromPost(this._txtFrom.Text);
        this.SelectedCategories = ClientLocalDB.Current.GetCategories().Join<Category, Tuple<int, string>, int, Category>(categoriesFromPost, (Func<Category, int>) (c => c.CategoryID), (Func<Tuple<int, string>, int>) (t => t.Item1), (Func<Category, Tuple<int, string>, Category>) ((c, t) => c)).ToList<Category>();
        List<Tuple<int, int, string>> result = new List<Tuple<int, int, string>>();
        foreach (Tuple<int, string> tuple in categoriesFromPost)
          result.Add(new Tuple<int, int, string>(tuple.Item1, 0, tuple.Item2));
        ClientLocalDB.Current.SaveSettingsReport(result);
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message);
        SelectCategory._logger.Error(ex.Message);
        SelectCategory._logger.Debug(ex.StackTrace);
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
      this.treeView1 = new TreeView();
      this._btCancel = new Button();
      this._btSelected = new Button();
      this._txtFrom = new TextBox();
      this.SuspendLayout();
      this.treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.treeView1.Location = new Point(12, 12);
      this.treeView1.Name = "treeView1";
      this.treeView1.Size = new Size(468, 495);
      this.treeView1.TabIndex = 0;
      this.treeView1.DoubleClick += new EventHandler(this._btSelected_Click);
      this._btCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this._btCancel.Location = new Point(405, 513);
      this._btCancel.Name = "_btCancel";
      this._btCancel.Size = new Size(75, 23);
      this._btCancel.TabIndex = 1;
      this._btCancel.Text = "Отмена";
      this._btCancel.UseVisualStyleBackColor = true;
      this._btCancel.Click += new EventHandler(this._btCancel_Click);
      this._btSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this._btSelected.Location = new Point(324, 513);
      this._btSelected.Name = "_btSelected";
      this._btSelected.Size = new Size(75, 23);
      this._btSelected.TabIndex = 2;
      this._btSelected.Text = "Выбрать";
      this._btSelected.UseVisualStyleBackColor = true;
      this._btSelected.Click += new EventHandler(this._btSelected_Click);
      this._txtFrom.Location = new Point(12, 513);
      this._txtFrom.Name = "_txtFrom";
      this._txtFrom.Size = new Size(306, 20);
      this._txtFrom.TabIndex = 3;
      this._txtFrom.KeyDown += new KeyEventHandler(this._txtFrom_KeyDown);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(492, 548);
      this.ControlBox = false;
      this.Controls.Add((Control) this._txtFrom);
      this.Controls.Add((Control) this._btSelected);
      this.Controls.Add((Control) this._btCancel);
      this.Controls.Add((Control) this.treeView1);
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.Name = "SelectCategory";
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Выбор категории";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
