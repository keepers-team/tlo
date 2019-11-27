// Decompiled with JetBrains decompiler
// Type: TLO.local.SelectCategory
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NLog;

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
      if (_logger == null)
        _logger = LogManager.GetLogger("SelectCategory");
      InitializeComponent();
      SelectedCategories = new List<Category>();
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
        _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
      }
      Category[] array = ClientLocalDB.Current.GetCategories().OrderBy(x => x.FullName).ToArray();
      foreach (Category category3 in array.Where(x => x.CategoryID > 999999).OrderBy(x => x.FullName).ToArray())
      {
        Category category1 = category3;
        List<TreeNode> source1 = new List<TreeNode>();
        Category[] categoryArray1 = array;
        foreach (Category category4 in categoryArray1.Where(x => x.ParentID == category1.CategoryID).OrderBy(x => x.FullName).ToArray())
        {
          Category category2 = category4;
          List<TreeNode> source2 = new List<TreeNode>();
          Category[] categoryArray2 = array;
          foreach (Category category5 in categoryArray2.Where(x => x.ParentID == category2.CategoryID).OrderBy(x => x.FullName).ToArray())
            source2.Add(new TreeNode(category5.Name)
            {
              Tag = category5
            });
          if (source2.Count() != 0)
            source1.Add(new TreeNode(category2.Name, source2.ToArray())
            {
              Tag = category2
            });
          else
            source1.Add(new TreeNode(category2.Name)
            {
              Tag = category2
            });
        }
        if (source1.Count() != 0)
          treeView1.Nodes.Add(new TreeNode(category1.Name, source1.ToArray())
          {
            Tag = category1
          });
        else
          treeView1.Nodes.Add(new TreeNode(category1.Name)
          {
            Tag = category1
          });
      }
    }

    private void _btCancel_Click(object sender, EventArgs e)
    {
      SelectedCategory = null;
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void _btSelected_Click(object sender, EventArgs e)
    {
      try
      {
        if (treeView1 == null)
          return;
        TreeNode selectedNode = treeView1.SelectedNode;
        if (selectedNode == null)
          return;
        Category tag = selectedNode.Tag as Category;
        if (tag == null || tag.CategoryID > 999999)
        {
          int num = (int) MessageBox.Show("Не выбран раздел или выбран корневой раздел\r\n(Корневой раздел нельзя выбирать)");
        }
        else
        {
          SelectedCategory = tag;
          DialogResult = DialogResult.OK;
          Close();
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
      if (string.IsNullOrWhiteSpace(_txtFrom.Text))
        return;
      try
      {
        if (_txtFrom.Text.Split('=').Length != 2)
          return;
        IEnumerable<Tuple<int, string>> categoriesFromPost = new RuTrackerOrg(Settings.Current.KeeperName, Settings.Current.KeeperPass).GetCategoriesFromPost(_txtFrom.Text);
        SelectedCategories = ClientLocalDB.Current.GetCategories().Join(categoriesFromPost, c => c.CategoryID, t => t.Item1, (c, t) => c).ToList();
        List<Tuple<int, int, string>> result = new List<Tuple<int, int, string>>();
        foreach (Tuple<int, string> tuple in categoriesFromPost)
          result.Add(new Tuple<int, int, string>(tuple.Item1, 0, tuple.Item2));
        ClientLocalDB.Current.SaveSettingsReport(result);
        DialogResult = DialogResult.OK;
        Close();
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message);
        _logger.Error(ex.Message);
        _logger.Debug(ex.StackTrace);
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
  }
}
