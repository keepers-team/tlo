// Decompiled with JetBrains decompiler
// Type: TLO.local.SettingsForm
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;
using System.Collections.Generic;
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
            InitializeComponent();
            _tbTorrentClientName.Enabled = false;
            _cbTorrentClientType.Enabled = false;
            _tbTorrentClientHostIP.Enabled = false;
            _tbTorrentClientPort.Enabled = false;
            _tbTorrentClientUserName.Enabled = false;
            _tbTorrentClientUserPassword.Enabled = false;
            dgwTorrentClients.AutoGenerateColumns = false;
            dgwTorrentClients.ClearSelection();
            dgwTorrentClients.DataSource = null;
            _TorrentClientsSource = new BindingSource();
            _TorrentClientsSource.DataSource = ClientLocalDB.Current.GetTorrentClients();
            dgwTorrentClients.DataSource = _TorrentClientsSource;
            dgwCategories.AutoGenerateColumns = false;
            dgwCategories.ClearSelection();
            dgwCategories.DataSource = null;
            _CategoriesSource = new BindingSource();
            _CategoriesSource.DataSource = ClientLocalDB.Current.GetCategoriesEnable();
            dgwCategories.DataSource = _CategoriesSource;
            if (_CategoriesSource.Count > 0)
                _CategoriesSource.Position = 0;
            if (_TorrentClientsSource.Count > 0)
                _TorrentClientsSource.Position = 0;
            forumPages1.LoadSettings();
            CreatePageAllCategories();
            Settings current = Settings.Current;
            _appKeeperName.Text = current.KeeperName;
            _appKeeperPass.Text = current.KeeperPass;
            _appIsUpdateStatistics.Checked = current.IsUpdateStatistics;
            _appCountDaysKeepHistory.Value = current.CountDaysKeepHistory;
            _appPeriodRunAndStopTorrents.Value = current.PeriodRunAndStopTorrents;
            _appCountSeedersReport.Value = current.CountSeedersReport;
            _appIsAvgCountSeeders.Checked = current.IsAvgCountSeeders;
            _appSelectLessOrEqual.Checked = current.IsSelectLessOrEqual;
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

                _dbLoadInMemoryCheckbox.AutoSize = true;
                _dbLoadInMemoryCheckbox.Checked = current.LoadDBInMemory.GetValueOrDefault(false);
                _dbLoadInMemoryCheckbox.CheckState = checkState;
            }
            proxyInput.Text = current.Proxy;
            DisableCertVerifyCheck.Checked = current.DisableServerCertVerify.GetValueOrDefault(false);
            DisableCertVerifyCheck.CheckState = current.DisableServerCertVerify.GetValueOrDefault(false) ?
                CheckState.Checked : CheckState.Unchecked;
            if (current.ApiHost != "")
            {
                foreach (String item in apiHosts.Items)
                {
                    if (item == current.ApiHost)
                    {
                        apiHosts.SelectedItem = item;
                    }
                }
            }
            NumericUpDown appLogLevel = _appLogLevel;
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

            Decimal num2 = num1;
            appLogLevel.Value = num2;
            _appIsNotSaveStatistics.Checked = current.IsNotSaveStatistics;
            _appReportTop1.Text = current.ReportTop1;
            _appReportTop2.Text = current.ReportTop2;
            _appReportLine.Text = current.ReportLine;
            _appReportBottom.Text = current.ReportBottom;
            summaryReportTemplate.Text = current.ReportSummaryTemplate;
            categoryReportTemplate.Text = current.ReportCategoriesTemplate;
            reportHeaderTemplate.Text = current.ReportCategoryHeaderTemplate;
        }

        private void _Focus_Enter(object sender, EventArgs e)
        {
            if (_TorrentClientsSource.Current != null)
            {
                TorrentClientInfo current = _TorrentClientsSource.Current as TorrentClientInfo;
                if (sender == _tbTorrentClientName)
                    current.Name = _tbTorrentClientName.Text;
                else if (sender == _cbTorrentClientType)
                    current.Type = _cbTorrentClientType.Text;
                else if (sender == _tbTorrentClientHostIP)
                    current.ServerName = _tbTorrentClientHostIP.Text;
                else if (sender == _tbTorrentClientPort)
                {
                    int result = 0;
                    if (int.TryParse(_tbTorrentClientPort.Text, out result))
                        current.ServerPort = result;
                    else
                        _tbTorrentClientPort.Text = "0";
                }
                else if (sender == _tbTorrentClientUserName)
                    current.UserName = _tbTorrentClientUserName.Text;
                else if (sender == _tbTorrentClientUserPassword)
                    current.UserPassword = _tbTorrentClientUserPassword.Text;
                else if (sender == _tcrbCurrent && _tcrbCurrent.Checked)
                {
                    current.ServerName = "127.0.0.1";
                    _tbTorrentClientHostIP.Enabled = false;
                }
                else if (sender == _tcrbRemote && _tcrbRemote.Checked)
                {
                    current.ServerName = _tbTorrentClientHostIP.Text;
                    _tbTorrentClientHostIP.Enabled = true;
                }
            }

            if (_CategoriesSource.Current == null)
                return;
            Category current1 = _CategoriesSource.Current as Category;
            if (sender == _CategoriesCbTorrentClient)
            {
                TorrentClientInfo selectedItem = _CategoriesCbTorrentClient.SelectedItem as TorrentClientInfo;
                if (selectedItem == null)
                    return;
                current1.TorrentClientUID = selectedItem.UID;
            }
            else if (sender == _CategoriesCbStartCountSeeders)
            {
                int result = 0;
                if (!int.TryParse(_CategoriesCbStartCountSeeders.SelectedItem as string, out result))
                    return;
                current1.CountSeeders = result;
            }
            else if (sender == _CategoriesTbFolderDownloads)
                current1.Folder = _CategoriesTbFolderDownloads.Text;
            else if (sender == _cbIsSaveTorrentFile)
                current1.IsSaveTorrentFiles = _cbIsSaveTorrentFile.Checked;
            else if (sender == _cbIsSaveWebPage)
                current1.IsSaveWebPage = _cbIsSaveWebPage.Checked;
            else if (sender == _cbSubFolder)
            {
                string selectedItem = _cbSubFolder.SelectedItem as string;
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
                if (sender != _CategoriesTbLabel)
                    return;
                current1.Label = string.IsNullOrWhiteSpace(_CategoriesTbLabel.Text)
                    ? current1.FullName
                    : _CategoriesTbLabel.Text.Trim();
            }
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (sender == dgwTorrentClients)
            {
                if (_TorrentClientsSource.Current == null)
                {
                    _tbTorrentClientName.Enabled = false;
                    _cbTorrentClientType.Enabled = false;
                    _tbTorrentClientHostIP.Enabled = false;
                    _tbTorrentClientPort.Enabled = false;
                    _tbTorrentClientUserName.Enabled = false;
                    _tbTorrentClientUserPassword.Enabled = false;
                    _tbTorrentClientName.Text = string.Empty;
                    _cbTorrentClientType.Text = string.Empty;
                    _tbTorrentClientHostIP.Text = string.Empty;
                    _tbTorrentClientPort.Text = string.Empty;
                    _tbTorrentClientUserName.Text = string.Empty;
                    _tbTorrentClientUserPassword.Text = string.Empty;
                    _tcrbRemote.Checked = false;
                    _tcrbCurrent.Checked = true;
                    _tbTorrentClientHostIP.Enabled = false;
                }
                else
                {
                    TorrentClientInfo current = _TorrentClientsSource.Current as TorrentClientInfo;
                    _tbTorrentClientName.Enabled = true;
                    _cbTorrentClientType.Enabled = true;
                    _tbTorrentClientHostIP.Enabled = true;
                    _tbTorrentClientPort.Enabled = true;
                    _tbTorrentClientUserName.Enabled = true;
                    _tbTorrentClientUserPassword.Enabled = true;
                    _tbTorrentClientName.Text = current.Name;
                    _cbTorrentClientType.Text = current.Type;
                    if (current.ServerName == "127.0.0.1")
                    {
                        _tcrbRemote.Checked = false;
                        _tcrbCurrent.Checked = true;
                    }
                    else
                    {
                        _tbTorrentClientHostIP.Text = current.ServerName;
                        _tcrbCurrent.Checked = false;
                        _tcrbRemote.Checked = true;
                    }

                    _tbTorrentClientPort.Text = current.ServerPort.ToString();
                    _tbTorrentClientUserName.Text = current.UserName;
                    _tbTorrentClientUserPassword.Text = current.UserPassword;
                }
            }

            if (sender == dgwCategories)
            {
                if (_CategoriesSource.Current == null)
                {
                    _CategoriesTbCategoryID.Text = string.Empty;
                    _CategoriesTbFullName.Text = string.Empty;
                    _CategoriesCbStartCountSeeders.Enabled = false;
                    _CategoriesTbLabel.Text = string.Empty;
                }
                else
                {
                    Category obj = _CategoriesSource.Current as Category;
                    _CategoriesTbCategoryID.Text = obj.CategoryID.ToString();
                    _CategoriesTbFullName.Text = obj.FullName;
                    _CategoriesCbStartCountSeeders.Enabled = true;
                    ComboBox startCountSeeders = _CategoriesCbStartCountSeeders;
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

                    startCountSeeders.SelectedItem = str;
                    _CategoriesTbFolderDownloads.Text = obj.Folder;
                    _CategoriesCbTorrentClient.DataSource = null;
                    _CategoriesCbTorrentClient.DataSource = _TorrentClientsSource.DataSource;
                    _CategoriesCbTorrentClient.SelectedItem =
                        (_CategoriesCbTorrentClient.DataSource as List<TorrentClientInfo>)
                        .Where(x => x.UID == obj.TorrentClientUID)
                        .FirstOrDefault();
                    num = obj.CreateSubFolder;
                    switch (num)
                    {
                        case 0:
                            _cbSubFolder.SelectedItem = "Не нужен";
                            break;
                        case 1:
                            _cbSubFolder.SelectedItem = "С ID топика";
                            break;
                        case 2:
                            _cbSubFolder.SelectedItem = "Запрашивать";
                            break;
                    }

                    _cbIsSaveWebPage.Checked = obj.IsSaveWebPage;
                    _cbIsSaveTorrentFile.Checked = obj.IsSaveTorrentFiles;
                    _CategoriesTbLabel.Text = string.IsNullOrWhiteSpace(obj.Label) ? obj.FullName : obj.Label;
                }
            }

            if (sender != _appIsNotSaveStatistics)
                return;
            if (_appIsNotSaveStatistics.Checked)
            {
                _appIsUpdateStatistics.Checked = false;
                _appIsUpdateStatistics.Enabled = false;
            }
            else
                _appIsUpdateStatistics.Enabled = true;
        }

        private bool hasChanges;

        private void ClickButtons(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (sender == _btTorrentClientAdd)
                {
                    _TorrentClientsSource.Add(new TorrentClientInfo());
                    _TorrentClientsSource.Position = _TorrentClientsSource.Count;
                }
                else if (sender == _btTorrentClientDelete)
                {
                    if (_TorrentClientsSource.Current == null)
                        return;
                    TorrentClientInfo current = _TorrentClientsSource.Current as TorrentClientInfo;
                    if (MessageBox.Show("Вы хотите удалить из списка torrent-клиент \"" + current.Name + "\"?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) ==
                        DialogResult.Yes)
                        _TorrentClientsSource.Remove(current);
                }
            }
            catch
            {
            }

            if (sender == _dbLoadInMemoryCheckbox)
            {
                hasChanges = true;
            }

            try
            {
                if (sender == _btCategoryAdd)
                {
                    SelectCategory dialog = new SelectCategory();
                    dialog.Read();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.SelectedCategories.Count() > 0)
                        {
                            dialog.SelectedCategories.ForEach(x =>
                            {
                                x.IsEnable = true;
                                _CategoriesSource.Add(x);
                            });
                            _CategoriesSource.Position = _CategoriesSource.Count;
                        }

                        if (dialog.SelectedCategory == null)
                            return;
                        if ((_CategoriesSource.DataSource as List<Category>).Any(
                            x => x.CategoryID == dialog.SelectedCategory.CategoryID))
                        {
                            int num = (int) MessageBox.Show("Выбранная категория уже присутствует");
                        }
                        else
                        {
                            dialog.SelectedCategory.IsEnable = true;
                            _CategoriesSource.Add(dialog.SelectedCategory);
                            _CategoriesSource.Position = _CategoriesSource.Count;
                        }
                    }
                }
                else if (sender == _btCategoryRemove)
                {
                    if (_CategoriesSource.Current == null)
                        return;
                    Category current = _CategoriesSource.Current as Category;
                    if (MessageBox.Show("Удалить из обработки раздел \"" + current.Name + "\"?", "Подтверждение",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                        _CategoriesSource.Remove(current);
                }
                else if (sender == _CategoriesBtSelectFolder)
                {
                    if (_CategoriesSource.Current == null)
                        return;
                    Category current = _CategoriesSource.Current as Category;
                    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                    folderBrowserDialog.SelectedPath =
                        string.IsNullOrWhiteSpace(current.Folder) ? "c:\\" : current.Folder;
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        current.Folder = folderBrowserDialog.SelectedPath;
                        _CategoriesTbFolderDownloads.Text = current.Folder;
                    }
                }
            }
            catch
            {
            }

            try
            {
                if (sender == _btSave)
                {
                    ClientLocalDB.Current.SaveTorrentClients(
                        _TorrentClientsSource.DataSource as List<TorrentClientInfo>, true);
                    ClientLocalDB.Current.CategoriesSave(
                        _CategoriesSource.DataSource as List<Category>);
                    forumPages1.Save();
                    DialogResult = DialogResult.OK;
                    Settings current = Settings.Current;
                    current.KeeperName = _appKeeperName.Text;
                    current.KeeperPass = _appKeeperPass.Text;
                    current.IsUpdateStatistics = _appIsUpdateStatistics.Checked;
                    current.CountDaysKeepHistory = (int) _appCountDaysKeepHistory.Value;
                    current.PeriodRunAndStopTorrents = (int) _appPeriodRunAndStopTorrents.Value;
                    current.CountSeedersReport = (int) _appCountSeedersReport.Value;
                    current.IsAvgCountSeeders = _appIsAvgCountSeeders.Checked;
                    current.IsSelectLessOrEqual = _appSelectLessOrEqual.Checked;
                    current.LogLevel = (int) _appLogLevel.Value;
                    current.IsNotSaveStatistics = _appIsNotSaveStatistics.Checked;
                    current.ReportTop1 = _appReportTop1.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportTop2 = _appReportTop2.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportLine = _appReportLine.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportBottom = _appReportBottom.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportCategoryHeaderTemplate = reportHeaderTemplate.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportCategoriesTemplate = categoryReportTemplate.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    current.ReportSummaryTemplate = summaryReportTemplate.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
                    if (_dbLoadInMemoryCheckbox.CheckState != CheckState.Indeterminate)
                    {
                        current.LoadDBInMemory = _dbLoadInMemoryCheckbox.Checked;
                    }
                    current.Proxy = proxyInput.Text;
                    current.DisableServerCertVerify = DisableCertVerifyCheck.Checked;
                    current.ApiHost = apiHosts.SelectedItem.ToString();
                    current.Save();
                    ClientLocalDB.Current.SaveToDatabase();
                    Close();
                    if (hasChanges)
                    {
                        MessageBox.Show("Для вступления изменений в силу может потребоваться перезапустить программу.",
                            "Внимание");
                    }
                }
                else if (sender == _btCancel)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
                else
                {
                    if (_btCheck != sender)
                        return;
                    List<string> stringList = new List<string>();
                    foreach (TorrentClientInfo torrentClientInfo in
                        _TorrentClientsSource.DataSource as List<TorrentClientInfo>)
                    {
                        try
                        {
                            ITorrentClient torrentClient = torrentClientInfo.Create();
                            if (torrentClient == null)
                                stringList.Add(string.Format(
                                    "Торрент-клиент \"{0}\": Не удалось определить тип torrent-клиента",
                                    torrentClientInfo.Name));
                            else
                                torrentClient.Ping();
                        }
                        catch
                        {
                            stringList.Add(string.Format("Не удалось подключиться к торрент-клиенту \"{0}\"",
                                torrentClientInfo.Name));
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
            Control control = panel2;
            Dictionary<int, Category> dictionary1 = ClientLocalDB.Current.GetCategories()
                .ToDictionary(x => x.CategoryID,
                    x => x);
            Dictionary<int, Category> categoriesEnable = ClientLocalDB.Current.GetCategoriesEnable()
                .ToDictionary(x => x.CategoryID,
                    x => x);
            categoriesEnable.ToDictionary(
                x => x.Key,
                x => x.Value.ParentID);
            for (int index = 0; index < 3; ++index)
            {
                foreach (Category category in categoriesEnable.Values.ToArray())
                {
                    if (!categoriesEnable.ContainsKey(category.ParentID) && dictionary1.ContainsKey(category.ParentID))
                        categoriesEnable.Add(dictionary1[category.ParentID].CategoryID, dictionary1[category.ParentID]);
                }
            }

            for (int index = 0; index < 3; ++index)
            {
                List<Category> list = dictionary1.Values.ToList();
                foreach (Category category1 in categoriesEnable.Values.ToList())
                {
                    Category c = category1;
                    foreach (Category category2 in list.Where(x =>
                    {
                        if (!categoriesEnable.ContainsKey(x.CategoryID))
                            return x.ParentID == c.CategoryID;
                        return false;
                    }).ToArray())
                    {
                        if (!categoriesEnable.ContainsKey(category2.CategoryID) &&
                            dictionary1.ContainsKey(category2.CategoryID))
                            categoriesEnable.Add(category2.CategoryID, category2);
                    }
                }
            }

            Dictionary<int, string> dictionary2 = ClientLocalDB.Current.GetReports(new int?())
                .Where(
                    x =>
                    {
                        if (x.Key.Item2 == 0)
                            return (uint) x.Key.Item1 > 0U;
                        return false;
                    }).ToDictionary(
                    x => x.Key.Item1,
                    x => x.Value.Item1);
            int num = 0;
            int y1 = 10;
            foreach (Category category in categoriesEnable.Values.OrderBy(
                x => x.FullName))
            {
                Label label1 = new Label();
                label1.AutoSize = true;
                label1.Location = new Point(3, y1);
                label1.Size = new Size(35, 13);
                label1.TabIndex = num;
                label1.Text = category.FullName;
                control.Controls.Add(label1);
                int y2 = y1 + 16;
                Label label2 = new Label();
                label2.Location = new Point(6, y2);
                label2.Size = new Size(123, 20);
                label2.Text = "Списки хранимого";
                control.Controls.Add(label2);
                ++num;
                TextBox textBox = new TextBox();
                textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                textBox.Location = new Point(135, y2);
                textBox.Size = new Size(panel1.Size.Width - 135, 20);
                textBox.TabIndex = num;
                textBox.Text =
                    !string.IsNullOrWhiteSpace(category.ReportList) || !dictionary2.ContainsKey(category.CategoryID)
                        ? category.ReportList
                        : dictionary2[category.CategoryID];
                control.Controls.Add(textBox);
                y1 = y2 + 26;
            }
        }
    }
}