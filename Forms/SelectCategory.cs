using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NLog;
using TLO.Clients;
using TLO.Info;

namespace TLO.Forms
{
    internal partial class SelectCategory : Form
    {
        private static Logger _logger;

        public SelectCategory()
        {
            if (_logger == null)
                _logger = LogManager.GetLogger("SelectCategory");
            InitializeComponent();
            SelectedCategories = new List<Category>();
        }

        public Category SelectedCategory { get; private set; }

        public List<Category> SelectedCategories { get; private set; }

        public void Read()
        {
            try
            {
                ClientLocalDb.Current.CategoriesSave(RuTrackerOrg.Current.GetCategories(), true);
            }
            catch (Exception ex)
            {
                var num = (int) MessageBox.Show("Не удалось загрузить список категорий.\r\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
            }

            var array = ClientLocalDb.Current.GetCategories().OrderBy(x => x.FullName).ToArray();
            foreach (var category3 in array.Where(x => x.CategoryID > 999999).OrderBy(x => x.FullName).ToArray())
            {
                var category1 = category3;
                var source1 = new List<TreeNode>();
                var categoryArray1 = array;
                foreach (var category4 in categoryArray1.Where(x => x.ParentID == category1.CategoryID)
                    .OrderBy(x => x.FullName).ToArray())
                {
                    var category2 = category4;
                    var source2 = new List<TreeNode>();
                    var categoryArray2 = array;
                    foreach (var category5 in categoryArray2.Where(x => x.ParentID == category2.CategoryID)
                        .OrderBy(x => x.FullName).ToArray())
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
                var selectedNode = treeView1.SelectedNode;
                if (selectedNode == null)
                    return;
                var tag = selectedNode.Tag as Category;
                if (tag == null || tag.CategoryID > 999999)
                {
                    var num = (int) MessageBox.Show(
                        "Не выбран раздел или выбран корневой раздел\r\n(Корневой раздел нельзя выбирать)");
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
                var num = (int) MessageBox.Show("Непредвиденное исключение\r\n " + ex.Message);
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
                var categoriesFromPost =
                    new RuTrackerOrg(Settings.Current.KeeperName, Settings.Current.KeeperPass).GetCategoriesFromPost(
                        _txtFrom.Text);
                SelectedCategories = ClientLocalDb.Current.GetCategories()
                    .Join(categoriesFromPost, c => c.CategoryID, t => t.Item1, (c, t) => c).ToList();
                var result = new List<Tuple<int, int, string>>();
                foreach (var tuple in categoriesFromPost)
                    result.Add(new Tuple<int, int, string>(tuple.Item1, 0, tuple.Item2));
                ClientLocalDb.Current.SaveSettingsReport(result);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                var num = (int) MessageBox.Show(ex.Message);
                _logger.Error(ex.Message);
                _logger.Debug(ex.StackTrace);
            }
        }
    }
}