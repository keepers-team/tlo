// Decompiled with JetBrains decompiler
// Type: TLO.local.SettingsForm
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
    partial class SettingsForm : Form
    {
        private BindingSource _TorrentClientsSource = new BindingSource();
        private BindingSource _CategoriesSource = new BindingSource();

        public SettingsForm()
        {
            this.InitializeComponent();
            this._tbTorrentClientName.Enabled = false;
            this._cbTorrentClientType.Enabled = false;
            this._tbTorrentClientHostIP.Enabled = false;
            this._tbTorrentClientPort.Enabled = false;
            this._tbTorrentClientUserName.Enabled = false;
            this._tbTorrentClientUserPassword.Enabled = false;
            this.dgwTorrentClients.AutoGenerateColumns = false;
            this.dgwTorrentClients.ClearSelection();
            this.dgwTorrentClients.DataSource = (object) null;
            this._TorrentClientsSource = new BindingSource();
            this._TorrentClientsSource.DataSource = (object) ClientLocalDB.Current.GetTorrentClients();
            this.dgwTorrentClients.DataSource = (object) this._TorrentClientsSource;
            this.dgwCategories.AutoGenerateColumns = false;
            this.dgwCategories.ClearSelection();
            this.dgwCategories.DataSource = (object) null;
            this._CategoriesSource = new BindingSource();
            this._CategoriesSource.DataSource = (object) ClientLocalDB.Current.GetCategoriesEnable();
            this.dgwCategories.DataSource = (object) this._CategoriesSource;
            if (this._CategoriesSource.Count > 0)
                this._CategoriesSource.Position = 0;
            if (this._TorrentClientsSource.Count > 0)
                this._TorrentClientsSource.Position = 0;
            this.forumPages1.LoadSettings();
            CreatePageAllCategories();
            Settings current = Settings.Current;
            this._appKeeperName.Text = current.KeeperName;
            this._appKeeperPass.Text = current.KeeperPass;
            this._appIsUpdateStatistics.Checked = current.IsUpdateStatistics;
            this._appCountDaysKeepHistory.Value = (Decimal) current.CountDaysKeepHistory;
            this._appPeriodRunAndStopTorrents.Value = (Decimal) current.PeriodRunAndStopTorrents;
            this._appCountSeedersReport.Value = (Decimal) current.CountSeedersReport;
            this._appIsAvgCountSeeders.Checked = current.IsAvgCountSeeders;
            this._appSelectLessOrEqual.Checked = current.IsSelectLessOrEqual;
            {
                CheckState checkState;
                if (Settings.Current.LoadDBInMemory == null)
                {
                    checkState = CheckState.Indeterminate;
                }
                else if ((bool)current.LoadDBInMemory)
                {
                    checkState = CheckState.Checked;
                }
                else
                {
                    checkState = CheckState.Unchecked;
                }

                this._dbLoadInMemoryCheckbox.AutoSize = true;
                this._dbLoadInMemoryCheckbox.Checked = current.LoadDBInMemory.GetValueOrDefault(false);
                this._dbLoadInMemoryCheckbox.CheckState = checkState;
            }
            this.proxyInput.Text = current.Proxy;
            this.DisableCertVerifyCheck.Checked = current.DisableServerCertVerify.GetValueOrDefault(false);
            this.DisableCertVerifyCheck.CheckState = current.DisableServerCertVerify.GetValueOrDefault(false) ?
                CheckState.Checked : CheckState.Unchecked;
            if (current.ApiHost != "")
            {
                foreach (String item in this.apiHosts.Items)
                {
                    if (item == current.ApiHost)
                    {
                        this.apiHosts.SelectedItem = item;
                    }
                }
            }
            NumericUpDown appLogLevel = this._appLogLevel;
            int? logLevel = current.LogLevel;
            int num1;
            if (!logLevel.HasValue)
            {
                num1 = 0;
            }
            else
            {
                logLevel = current.LogLevel;
                num1 = logLevel.Value;
            }

            Decimal num2 = (Decimal) num1;
            appLogLevel.Value = num2;
            this._appIsNotSaveStatistics.Checked = current.IsNotSaveStatistics;
            this._appReportTop1.Text = current.ReportTop1;
            this._appReportTop2.Text = current.ReportTop2;
            this._appReportLine.Text = current.ReportLine;
            this._appReportBottom.Text = current.ReportBottom;
        }

        private void _Focus_Enter(object sender, EventArgs e)
        {
            if (this._TorrentClientsSource.Current != null)
            {
                TorrentClientInfo current = this._TorrentClientsSource.Current as TorrentClientInfo;
                if (sender == this._tbTorrentClientName)
                    current.Name = this._tbTorrentClientName.Text;
                else if (sender == this._cbTorrentClientType)
                    current.Type = this._cbTorrentClientType.Text;
                else if (sender == this._tbTorrentClientHostIP)
                    current.ServerName = this._tbTorrentClientHostIP.Text;
                else if (sender == this._tbTorrentClientPort)
                {
                    int result = 0;
                    if (int.TryParse(this._tbTorrentClientPort.Text, out result))
                        current.ServerPort = result;
                    else
                        this._tbTorrentClientPort.Text = "0";
                }
                else if (sender == this._tbTorrentClientUserName)
                    current.UserName = this._tbTorrentClientUserName.Text;
                else if (sender == this._tbTorrentClientUserPassword)
                    current.UserPassword = this._tbTorrentClientUserPassword.Text;
                else if (sender == this._tcrbCurrent && this._tcrbCurrent.Checked)
                {
                    current.ServerName = "127.0.0.1";
                    this._tbTorrentClientHostIP.Enabled = false;
                }
                else if (sender == this._tcrbRemote && this._tcrbRemote.Checked)
                {
                    current.ServerName = this._tbTorrentClientHostIP.Text;
                    this._tbTorrentClientHostIP.Enabled = true;
                }
            }

            if (this._CategoriesSource.Current == null)
                return;
            Category current1 = this._CategoriesSource.Current as Category;
            if (sender == this._CategoriesCbTorrentClient)
            {
                TorrentClientInfo selectedItem = this._CategoriesCbTorrentClient.SelectedItem as TorrentClientInfo;
                if (selectedItem == null)
                    return;
                current1.TorrentClientUID = selectedItem.UID;
            }
            else if (sender == this._CategoriesCbStartCountSeeders)
            {
                int result = 0;
                if (!int.TryParse(this._CategoriesCbStartCountSeeders.SelectedItem as string, out result))
                    return;
                current1.CountSeeders = result;
            }
            else if (sender == this._CategoriesTbFolderDownloads)
                current1.Folder = this._CategoriesTbFolderDownloads.Text;
            else if (sender == this._cbIsSaveTorrentFile)
                current1.IsSaveTorrentFiles = this._cbIsSaveTorrentFile.Checked;
            else if (sender == this._cbIsSaveWebPage)
                current1.IsSaveWebPage = this._cbIsSaveWebPage.Checked;
            else if (sender == this._cbSubFolder)
            {
                string selectedItem = this._cbSubFolder.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(selectedItem))
                    return;
                if (!(selectedItem == "Не нужен"))
                {
                    if (!(selectedItem == "С ID топика"))
                    {
                        if (!(selectedItem == "Запрашивать"))
                            return;
                        current1.CreateSubFolder = 2;
                    }
                    else
                        current1.CreateSubFolder = 1;
                }
                else
                    current1.CreateSubFolder = 0;
            }
            else
            {
                if (sender != this._CategoriesTbLabel)
                    return;
                current1.Label = string.IsNullOrWhiteSpace(this._CategoriesTbLabel.Text)
                    ? current1.FullName
                    : this._CategoriesTbLabel.Text.Trim();
            }
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (sender == this.dgwTorrentClients)
            {
                if (this._TorrentClientsSource.Current == null)
                {
                    this._tbTorrentClientName.Enabled = false;
                    this._cbTorrentClientType.Enabled = false;
                    this._tbTorrentClientHostIP.Enabled = false;
                    this._tbTorrentClientPort.Enabled = false;
                    this._tbTorrentClientUserName.Enabled = false;
                    this._tbTorrentClientUserPassword.Enabled = false;
                    this._tbTorrentClientName.Text = string.Empty;
                    this._cbTorrentClientType.Text = string.Empty;
                    this._tbTorrentClientHostIP.Text = string.Empty;
                    this._tbTorrentClientPort.Text = string.Empty;
                    this._tbTorrentClientUserName.Text = string.Empty;
                    this._tbTorrentClientUserPassword.Text = string.Empty;
                    this._tcrbRemote.Checked = false;
                    this._tcrbCurrent.Checked = true;
                    this._tbTorrentClientHostIP.Enabled = false;
                }
                else
                {
                    TorrentClientInfo current = this._TorrentClientsSource.Current as TorrentClientInfo;
                    this._tbTorrentClientName.Enabled = true;
                    this._cbTorrentClientType.Enabled = true;
                    this._tbTorrentClientHostIP.Enabled = true;
                    this._tbTorrentClientPort.Enabled = true;
                    this._tbTorrentClientUserName.Enabled = true;
                    this._tbTorrentClientUserPassword.Enabled = true;
                    this._tbTorrentClientName.Text = current.Name;
                    this._cbTorrentClientType.Text = current.Type;
                    if (current.ServerName == "127.0.0.1")
                    {
                        this._tcrbRemote.Checked = false;
                        this._tcrbCurrent.Checked = true;
                    }
                    else
                    {
                        this._tbTorrentClientHostIP.Text = current.ServerName;
                        this._tcrbCurrent.Checked = false;
                        this._tcrbRemote.Checked = true;
                    }

                    this._tbTorrentClientPort.Text = current.ServerPort.ToString();
                    this._tbTorrentClientUserName.Text = current.UserName;
                    this._tbTorrentClientUserPassword.Text = current.UserPassword;
                }
            }

            if (sender == this.dgwCategories)
            {
                if (this._CategoriesSource.Current == null)
                {
                    this._CategoriesTbCategoryID.Text = string.Empty;
                    this._CategoriesTbFullName.Text = string.Empty;
                    this._CategoriesCbStartCountSeeders.Enabled = false;
                    this._CategoriesTbLabel.Text = string.Empty;
                }
                else
                {
                    Category obj = this._CategoriesSource.Current as Category;
                    this._CategoriesTbCategoryID.Text = obj.CategoryID.ToString();
                    this._CategoriesTbFullName.Text = obj.FullName;
                    this._CategoriesCbStartCountSeeders.Enabled = true;
                    ComboBox startCountSeeders = this._CategoriesCbStartCountSeeders;
                    int num;
                    string str;
                    if (obj.CountSeeders < 0)
                    {
                        str = "-";
                    }
                    else
                    {
                        num = obj.CountSeeders;
                        str = num.ToString();
                    }

                    startCountSeeders.SelectedItem = (object) str;
                    this._CategoriesTbFolderDownloads.Text = obj.Folder;
                    this._CategoriesCbTorrentClient.DataSource = (object) null;
                    this._CategoriesCbTorrentClient.DataSource = this._TorrentClientsSource.DataSource;
                    this._CategoriesCbTorrentClient.SelectedItem =
                        (object) (this._CategoriesCbTorrentClient.DataSource as List<TorrentClientInfo>)
                        .Where<TorrentClientInfo>((Func<TorrentClientInfo, bool>) (x => x.UID == obj.TorrentClientUID))
                        .FirstOrDefault<TorrentClientInfo>();
                    num = obj.CreateSubFolder;
                    switch (num)
                    {
                        case 0:
                            this._cbSubFolder.SelectedItem = (object) "Не нужен";
                            break;
                        case 1:
                            this._cbSubFolder.SelectedItem = (object) "С ID топика";
                            break;
                        case 2:
                            this._cbSubFolder.SelectedItem = (object) "Запрашивать";
                            break;
                    }

                    this._cbIsSaveWebPage.Checked = obj.IsSaveWebPage;
                    this._cbIsSaveTorrentFile.Checked = obj.IsSaveTorrentFiles;
                    this._CategoriesTbLabel.Text = string.IsNullOrWhiteSpace(obj.Label) ? obj.FullName : obj.Label;
                }
            }

            if (sender != this._appIsNotSaveStatistics)
                return;
            if (this._appIsNotSaveStatistics.Checked)
            {
                this._appIsUpdateStatistics.Checked = false;
                this._appIsUpdateStatistics.Enabled = false;
            }
            else
                this._appIsUpdateStatistics.Enabled = true;
        }

        private bool hasChanges = false;

        private void ClickButtons(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (sender == this._btTorrentClientAdd)
                {
                    this._TorrentClientsSource.Add((object) new TorrentClientInfo());
                    this._TorrentClientsSource.Position = this._TorrentClientsSource.Count;
                }
                else if (sender == this._btTorrentClientDelete)
                {
                    if (this._TorrentClientsSource.Current == null)
                        return;
                    TorrentClientInfo current = this._TorrentClientsSource.Current as TorrentClientInfo;
                    if (MessageBox.Show("Вы хотите удалить из списка torrent-клиент \"" + current.Name + "\"?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) ==
                        DialogResult.Yes)
                        this._TorrentClientsSource.Remove((object) current);
                }
            }
            catch
            {
            }

            if (sender == this._dbLoadInMemoryCheckbox)
            {
                hasChanges = true;
            }

            try
            {
                if (sender == this._btCategoryAdd)
                {
                    SelectCategory dialog = new SelectCategory();
                    dialog.Read();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.SelectedCategories.Count<Category>() > 0)
                        {
                            dialog.SelectedCategories.ForEach((Action<Category>) (x =>
                            {
                                x.IsEnable = true;
                                this._CategoriesSource.Add((object) x);
                            }));
                            this._CategoriesSource.Position = this._CategoriesSource.Count;
                        }

                        if (dialog.SelectedCategory == null)
                            return;
                        if ((this._CategoriesSource.DataSource as List<Category>).Any<Category>(
                            (Func<Category, bool>) (x => x.CategoryID == dialog.SelectedCategory.CategoryID)))
                        {
                            int num = (int) MessageBox.Show("Выбранная категория уже присутствует");
                        }
                        else
                        {
                            dialog.SelectedCategory.IsEnable = true;
                            this._CategoriesSource.Add((object) dialog.SelectedCategory);
                            this._CategoriesSource.Position = this._CategoriesSource.Count;
                        }
                    }
                }
                else if (sender == this._btCategoryRemove)
                {
                    if (this._CategoriesSource.Current == null)
                        return;
                    Category current = this._CategoriesSource.Current as Category;
                    if (MessageBox.Show("Удалить из обработки раздел \"" + current.Name + "\"?", "Подтверждение",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                        this._CategoriesSource.Remove((object) current);
                }
                else if (sender == this._CategoriesBtSelectFolder)
                {
                    if (this._CategoriesSource.Current == null)
                        return;
                    Category current = this._CategoriesSource.Current as Category;
                    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                    folderBrowserDialog.SelectedPath =
                        string.IsNullOrWhiteSpace(current.Folder) ? "c:\\" : current.Folder;
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        current.Folder = folderBrowserDialog.SelectedPath;
                        this._CategoriesTbFolderDownloads.Text = current.Folder;
                    }
                }
            }
            catch
            {
            }

            try
            {
                if (sender == this._btSave)
                {
                    ClientLocalDB.Current.SaveTorrentClients(
                        (IEnumerable<TorrentClientInfo>)
                        (this._TorrentClientsSource.DataSource as List<TorrentClientInfo>), true);
                    ClientLocalDB.Current.CategoriesSave(
                        (IEnumerable<Category>) (this._CategoriesSource.DataSource as List<Category>), false);
                    this.forumPages1.Save();
                    this.DialogResult = DialogResult.OK;
                    Settings current = Settings.Current;
                    current.KeeperName = this._appKeeperName.Text;
                    current.KeeperPass = this._appKeeperPass.Text;
                    current.IsUpdateStatistics = this._appIsUpdateStatistics.Checked;
                    current.CountDaysKeepHistory = (int) this._appCountDaysKeepHistory.Value;
                    current.PeriodRunAndStopTorrents = (int) this._appPeriodRunAndStopTorrents.Value;
                    current.CountSeedersReport = (int) this._appCountSeedersReport.Value;
                    current.IsAvgCountSeeders = this._appIsAvgCountSeeders.Checked;
                    current.IsSelectLessOrEqual = this._appSelectLessOrEqual.Checked;
                    current.LogLevel = new int?((int) this._appLogLevel.Value);
                    current.IsNotSaveStatistics = this._appIsNotSaveStatistics.Checked;
                    current.ReportTop1 = this._appReportTop1.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportTop2 = this._appReportTop2.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportLine = this._appReportLine.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportBottom = this._appReportBottom.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    if (this._dbLoadInMemoryCheckbox.CheckState != CheckState.Indeterminate)
                    {
                        current.LoadDBInMemory = this._dbLoadInMemoryCheckbox.Checked;
                    }
                    current.Proxy = this.proxyInput.Text;
                    current.DisableServerCertVerify = this.DisableCertVerifyCheck.Checked;
                    current.ApiHost = this.apiHosts.SelectedItem.ToString();
                    current.Save();
                    ClientLocalDB.Current.SaveToDatabase();
                    this.Close();
                    if (hasChanges)
                    {
                        MessageBox.Show("Для вступления изменений в силу может потребоваться перезапустить программу.",
                            "Внимание");
                    }
                }
                else if (sender == this._btCancel)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
                else
                {
                    if (this._btCheck != sender)
                        return;
                    List<string> stringList = new List<string>();
                    foreach (TorrentClientInfo torrentClientInfo in
                        this._TorrentClientsSource.DataSource as List<TorrentClientInfo>)
                    {
                        try
                        {
                            ITorrentClient torrentClient = torrentClientInfo.Create();
                            if (torrentClient == null)
                                stringList.Add(string.Format(
                                    "Торрент-клиент \"{0}\": Не удалось определить тип torrent-клиента",
                                    (object) torrentClientInfo.Name));
                            else
                                torrentClient.Ping();
                        }
                        catch
                        {
                            stringList.Add(string.Format("Не удалось подключиться к торрент-клиенту \"{0}\"",
                                (object) torrentClientInfo.Name));
                        }
                    }

                    foreach (string text in stringList)
                    {
                        int num = (int) MessageBox.Show(text, "Проверка");
                    }

                    int num1 = (int) MessageBox.Show("Подключение к torrent-клиентам проверено.", "Проверка");
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                int num = (int) MessageBox.Show(ex.Message);
            }
        }

        private void CreatePageAllCategories()
        {
            Control control = this.panel2;
            Dictionary<int, Category> dictionary1 = ClientLocalDB.Current.GetCategories()
                .ToDictionary<Category, int, Category>((Func<Category, int>) (x => x.CategoryID),
                    (Func<Category, Category>) (x => x));
            Dictionary<int, Category> categoriesEnable = ClientLocalDB.Current.GetCategoriesEnable()
                .ToDictionary<Category, int, Category>((Func<Category, int>) (x => x.CategoryID),
                    (Func<Category, Category>) (x => x));
            categoriesEnable.ToDictionary<KeyValuePair<int, Category>, int, int>(
                (Func<KeyValuePair<int, Category>, int>) (x => x.Key),
                (Func<KeyValuePair<int, Category>, int>) (x => x.Value.ParentID));
            for (int index = 0; index < 3; ++index)
            {
                foreach (Category category in categoriesEnable.Values.ToArray<Category>())
                {
                    if (!categoriesEnable.ContainsKey(category.ParentID) && dictionary1.ContainsKey(category.ParentID))
                        categoriesEnable.Add(dictionary1[category.ParentID].CategoryID, dictionary1[category.ParentID]);
                }
            }

            for (int index = 0; index < 3; ++index)
            {
                List<Category> list = dictionary1.Values.ToList<Category>();
                foreach (Category category1 in categoriesEnable.Values.ToList<Category>())
                {
                    Category c = category1;
                    foreach (Category category2 in list.Where<Category>((Func<Category, bool>) (x =>
                    {
                        if (!categoriesEnable.ContainsKey(x.CategoryID))
                            return x.ParentID == c.CategoryID;
                        return false;
                    })).ToArray<Category>())
                    {
                        if (!categoriesEnable.ContainsKey(category2.CategoryID) &&
                            dictionary1.ContainsKey(category2.CategoryID))
                            categoriesEnable.Add(category2.CategoryID, category2);
                    }
                }
            }

            Dictionary<int, string> dictionary2 = ClientLocalDB.Current.GetReports(new int?())
                .Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>(
                    (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x =>
                    {
                        if (x.Key.Item2 == 0)
                            return (uint) x.Key.Item1 > 0U;
                        return false;
                    })).ToDictionary<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int, string>(
                    (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int>) (x => x.Key.Item1),
                    (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, string>) (x => x.Value.Item1));
            int num = 0;
            int y1 = 10;
            foreach (Category category in (IEnumerable<Category>) categoriesEnable.Values.OrderBy<Category, string>(
                (Func<Category, string>) (x => x.FullName)))
            {
                Label label1 = new Label();
                label1.AutoSize = true;
                label1.Location = new Point(3, y1);
                label1.Size = new Size(35, 13);
                label1.TabIndex = num;
                label1.Text = category.FullName;
                control.Controls.Add((Control) label1);
                int y2 = y1 + 16;
                Label label2 = new Label();
                label2.Location = new Point(6, y2);
                label2.Size = new Size(123, 20);
                label2.Text = "Списки хранимого";
                control.Controls.Add((Control) label2);
                ++num;
                TextBox textBox = new TextBox();
                textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                textBox.Location = new Point(135, y2);
                textBox.Size = new Size(this.panel1.Size.Width - 135, 20);
                textBox.TabIndex = num;
                textBox.Text =
                    !string.IsNullOrWhiteSpace(category.ReportList) || !dictionary2.ContainsKey(category.CategoryID)
                        ? category.ReportList
                        : dictionary2[category.CategoryID];
                control.Controls.Add((Control) textBox);
                y1 = y2 + 26;
            }
        }
    }
}