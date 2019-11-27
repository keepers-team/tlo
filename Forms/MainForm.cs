using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using NLog;
using TLO.local.Forms;
using TLO.local.Tools;

namespace TLO.local
{
    public partial class MainForm : Form
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private DateTime _LastRunTimer = DateTime.Now;
        private BindingSource _CategorySource = new BindingSource();
        private BindingSource _TopicsSource = new BindingSource();

        private Dictionary<BackgroundWorker, Tuple<DateTime, object, string>> backgroundWorkers =
            new Dictionary<BackgroundWorker, Tuple<DateTime, object, string>>();

        private string headText;
        private Timer tmr;
        private NotifyIcon notifyIcon;

        private bool IsClose { get; set; }

        public MainForm()
        {
            InitializeComponent();
            menuTimerSetting.CheckStateChanged += (sender, args) =>
            {
                if (menuTimerSetting.Checked)
                {
                    _LastRunTimer = DateTime.Now;
                    if (!tmr.Enabled) tmr.Start();
                }
                else
                {
                    if (tmr.Enabled) tmr.Stop();
                }
            };
            _DateRegistration.Value = DateTime.Now.AddDays(-30.0);
            Text = headText = string.Format("TLO {0}",
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
            _cbCountSeeders.Value = new Decimal(0);
            _cbCategoryFilters.SelectedItem = "Не скачан торрент и нет хранителя";
            _CategorySource.Clear();
            _CategorySource.DataSource = ClientLocalDB.Current.GetCategoriesEnable(true);
            _CategorySource.CurrentChanged += SelectionChanged;
            _cbCategory.DataSource = _CategorySource;
            if (_CategorySource.Count > 0)
                _CategorySource.Position = 1;
            _TopicsSource.CurrentChanged += SelectionChanged;
            _dataGridTopicsList.AutoGenerateColumns = false;
            _dataGridTopicsList.ClearSelection();
            _dataGridTopicsList.DataSource = _TopicsSource;
            Disposed += MainForm_Disposed;
            Resize += MainForm_Resize;
            tmr = new Timer();
            tmr.Tick += tmr_Tick;
            tmr.Interval = 1000;
            tmr.Start();
            IsClose = false;
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = (Icon) new ComponentResourceManager(typeof(MainForm)).GetObject("$this.Icon");
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            notifyIcon.Visible = true;
            WriteReports();
        }

        private void MenuClick(object sender, EventArgs e)
        {
            try
            {
                if (sender == menuSettingsToolStripMenuItem)
                {
                    if (new SettingsForm().ShowDialog() == DialogResult.OK)
                    {
                        _CategorySource.Clear();
                        _CategorySource.DataSource = null;
                        _CategorySource.DataSource = ClientLocalDB.Current.GetCategoriesEnable();
                        _CategorySource.Position = 0;
//                        if (MessageBox.Show(
//                                "Запустить загрузку/обновление информации о топиках (раздачах) по всем категориям?",
//                                "Обновление данных", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
//                                MessageBoxDefaultButton.Button1) == DialogResult.OK)
//                            this.dwCreateAndRun(new DoWorkEventHandler(Logic.bwUpdateTopicsByCategories),
//                                "Полное обновление информации о топиках (раздачах) по всем категориям...",
//                                (object) ClientLocalDB.Current.GetCategoriesEnable());
                    }
                }
                else if (sender == UpdateAll)
                {
                    dwCreateAndRun(WorkerMethods.bwUpdateTopicsByCategories,
                        "Полное обновление информации о топиках (раздачах) по всем категориям...",
                        ClientLocalDB.Current.GetCategoriesEnable());
                    dwCreateAndRun(WorkerMethods.bwUpdateHashFromAllTorrentClients,
                        "Полное обновление информации из Torrent-клиентов...");
                    dwCreateAndRun(WorkerMethods.bwUpdateCountSeedersByAllCategories,
                        "Обновление кол-ва сидов на раздачах...", sender);
                    dwCreateAndRun(WorkerMethods.bwUpdateKeepersByAllCategories,
                        "Обновление данных о хранителях...", sender);
                }
                else if (sender == UpdateCountSeedersToolStripMenuItem)
                    dwCreateAndRun(WorkerMethods.bwUpdateCountSeedersByAllCategories,
                        "Обновление кол-ва сидов на раздачах...", sender);
                else if (sender == UpdateListTopicsToolStripMenuItem)
                    dwCreateAndRun(WorkerMethods.bwUpdateTopicsByCategories,
                        "Полное обновление информации о топиках (раздачах) по всем категориям...",
                        ClientLocalDB.Current.GetCategoriesEnable());
                else if (sender == UpdateKeepTopicsToolStripMenuItem)
                    dwCreateAndRun(WorkerMethods.bwUpdateHashFromAllTorrentClients,
                        "Полное обновление информации из Torrent-клиентов...");
                else if (sender == ClearDatabaseToolStripMenuItem)
                {
                    if (MessageBox.Show(
                            "Вы пытаетесь очистить базу данны от текущих данных (статистику и информацию о топиках).\r\n Продолжить?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    ClientLocalDB.Current.ClearDatabase();
                }
                else if (sender == ClearKeeperListsToolStripMenuItem)
                {
                    if (MessageBox.Show("Вы пытаетесь очистить базу данны от данных других хранителей.\r\n Продолжить?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    ClientLocalDB.Current.ClearKeepers();
                    SelectionChanged(_CategorySource, null);
                }
                else if (sender == SendReportsToForumToolStripMenuItem)
                {
                    if (MessageBox.Show(
                            "Отправка отчетов на форум может продолжаться продолжительное время.\r\n Продолжить?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    dwCreateAndRun(WorkerMethods.bwSendReports,
                        "Отправка отчетов на форум...",
                        this);
                }
                else if (sender == CreateReportsToolStripMenuItem)
                {
                    if (MessageBox.Show(
                            "Сборка отчетов может продолжаться продолжительное время и потребуется обновить список раздач и информацию из торрент-клиентов.\r\n Продолжит?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    dwCreateAndRun(WorkerMethods.bwUpdateCountSeedersByAllCategories,
                        "Обновление кол-ва сидов на раздачах...", sender);
                    dwCreateAndRun(WorkerMethods.bwUpdateHashFromAllTorrentClients,
                        "Обновление информации из Torrent-клиентов...", sender);
                }
                else if (sender == RuningStopingDistributionToolStripMenuItem)
                {
                    dwCreateAndRun(WorkerMethods.bwUpdateCountSeedersByAllCategories,
                        "Обновление кол-ва сидов на раздачах...", sender);
                    dwCreateAndRun(WorkerMethods.bwRuningAndStopingDistributions,
                        "Обновление информации из Torrent-клиентов...", sender);
                    dwCreateAndRun(WorkerMethods.bwCreateReportsTorrentClients,
                        "Построение сводного отчета по торрент-клиентам...", sender);
                }
                else if (sender == CreateConsolidatedReportByTorrentClientsToolStripMenuItem)
                    dwCreateAndRun(WorkerMethods.bwCreateReportsTorrentClients,
                        "Построение сводного отчета по торрент-клиентам...", sender);
                else if (sender == LoadListKeepersToolStripMenuItem)
                    dwCreateAndRun(WorkerMethods.bwUpdateKeepersByAllCategories,
                        "Обновление данных о хранителях...", sender);
                else if (sender == ExitToolStripMenuItem)
                {
                    IsClose = true;
                    Close();
                }
                else if (sender == _btSaveToFile)
                    SaveSetingsToFile();
                else if (sender == _btLoadSettingsFromFile)
                    ReadSettingsFromFile();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                int num = (int) MessageBox.Show(ex.Message);
            }

            Cursor.Current = Cursors.Default;
        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            if (backgroundWorkers.Count > 0)
            {
                Text = string.Format("{0} ({1})", headText, "Выполняются задачи...");
                notifyIcon.Text =
                    string.Format("{0} ({1})", headText, "Выполняются задачи...");
            }
            else
            {
                DateTime lastRunTimer = _LastRunTimer;
                DateTime now = DateTime.Now;
                DateTime dateTime1 = now.AddMinutes(-Settings.Current.PeriodRunAndStopTorrents);
                TimeSpan timeSpan = lastRunTimer - dateTime1;
                if (timeSpan.TotalSeconds > 0.0)
                {
                    Text = string.Format("{0} ({1:hh\\:mm\\:ss})", headText, timeSpan);
                    notifyIcon.Text =
                        string.Format("{0} ({1:hh\\:mm\\:ss})", headText, timeSpan);
                }
                else
                {
                    try
                    {
                        DateTime lastUpdateTopics = Settings.Current.LastUpdateTopics;
                        now = DateTime.Now;
                        DateTime dateTime2 = now.AddDays(-1.0);
                        if (lastUpdateTopics < dateTime2)
                        {
                            dwCreateAndRun(WorkerMethods.bwUpdateTopicsByCategories,
                                "Полное обновление информации о топиках (раздачах) по всем категориям...",
                                ClientLocalDB.Current.GetCategoriesEnable());
                            dwCreateAndRun(WorkerMethods.bwUpdateKeepersByAllCategories,
                                "Обновление данных о хранителях...", sender);
                            Settings current = Settings.Current;
                            now = DateTime.Now;
                            DateTime date = now.Date;
                            current.LastUpdateTopics = date;
                            Settings.Current.Save();
                        }
                        else
                            dwCreateAndRun(
                                WorkerMethods.bwUpdateCountSeedersByAllCategories,
                                "Обновление информации о кол-ве сидов на раздачах...", sender);

                        dwCreateAndRun(WorkerMethods.bwRuningAndStopingDistributions,
                            "Запуск/Остановка раздач в Torrent-клиентах...", sender);
                        dwCreateAndRun(WorkerMethods.bwCreateReportsTorrentClients,
                            "Построение сводного отчета по торрент-клиентам...", sender);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message);
                        _logger.Debug(ex.Message, ex);
                    }

                    _LastRunTimer = DateTime.Now;
                }
            }
        }

        private void MainForm_Disposed(object sender, EventArgs e)
        {
            tmr.Stop();
            tmr.Dispose();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
                return;
            Hide();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (sender == _CategorySource || sender == _cbCountSeeders ||
                (sender == _cbBlackList || sender == _cbCategoryFilters) || sender == _DateRegistration)
            {
                _TopicsSource.Clear();
                if (_CategorySource.Current != null)
                {
                    Category current = _CategorySource.Current as Category;
                    int num = (int) _cbCountSeeders.Value;
                    DateTime regTime = _DateRegistration.Value;
                    bool? isKeep = new bool?();
                    bool? isKeepers = new bool?();
                    bool? isDownload = new bool?();
                    bool? isBlack = new bool?();
                    bool? isPoster = new bool?();
                    string selectedItem = _cbCategoryFilters.SelectedItem as string;
                    if (!string.IsNullOrWhiteSpace(selectedItem))
                    {
                        switch (selectedItem)
                        {
                            case "Есть хранитель":
                                isKeepers = true;
                                break;
                            case "Не скачан торрент":
                                isDownload = false;
                                break;
                            case "Не скачан торрент и есть хранитель":
                                isDownload = false;
                                isKeepers = true;
                                break;
                            case "Не скачан торрент и нет хранителя":
                                isDownload = false;
                                isKeepers = false;
                                break;
                            case "Не скачано":
                                isDownload = false;
                                break;
                            case "Не храню":
                                isKeep = false;
                                break;
                            case "Скачиваю раздачу":
                                isDownload = true;
                                isKeep = false;
                                break;
                            case "Храню":
                                isKeep = true;
                                break;
                            case "Храню и есть хранитель":
                                isKeep = true;
                                isKeepers = true;
                                break;
                            case "Я релизер":
                                isPoster = false;
                                break;
                        }
                    }

                    List<TopicInfo> source;

                    if (current.CategoryID != -1)
                    {
                        isBlack = _cbBlackList.Checked;
                        List<TopicInfo> topicInfoList = new List<TopicInfo>();
                        source = !Settings.Current.IsAvgCountSeeders
                            ? ClientLocalDB.Current.GetTopics(regTime, current.CategoryID,
                                num > -1 ? num : new int?(), new int?(), isKeep, isKeepers, isDownload,
                                isBlack,
                                isPoster)
                            : ClientLocalDB.Current.GetTopics(regTime, current.CategoryID, new int?(),
                                num > -1 ? num : new int?(), isKeep, isKeepers, isDownload, isBlack,
                                isPoster);
                    }
                    else
                    {
                        List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
                        IEnumerable<TopicInfo> inner = ClientLocalDB.Current.GetTopicsByCategory(-1)
                            .Where(x => !x.IsBlackList);
                        Dictionary<int, Category> dictionary = ClientLocalDB.Current.GetCategories()
                            .ToDictionary(x => x.CategoryID, x => x);
                        source = new List<TopicInfo>();
                        foreach (TorrentClientInfo torrentClientInfo in torrentClients)
                        {
                            ITorrentClient torrentClient = torrentClientInfo.Create();
                            if (torrentClient != null)
                            {
                                var array1 = torrentClient.GetAllTorrentHash().GroupJoin(inner, t => t.Hash,
                                    b => b.Hash, (t, bt) => new
                                    {
                                        t, bt
                                    }).SelectMany(_param1 => _param1.bt.DefaultIfEmpty(), (_param1, b) =>
                                {
                                    int num3 = b != null ? b.CategoryID : -1;
                                    long size = _param1.t.Size;
                                    bool? isRun = _param1.t.IsRun;
                                    int num4;
                                    if (!isRun.HasValue)
                                    {
                                        num4 = -1;
                                    }
                                    else
                                    {
                                        isRun = _param1.t.IsRun;
                                        num4 = isRun.Value ? 1 : 0;
                                    }

                                    int num5 = _param1.t.IsPause ? 1 : 0;
                                    int num6 = b == null ? -1 : b.Seeders;
                                    TopicInfo a;
                                    if (b == null)
                                    {
                                        a = (TopicInfo) _param1.t.Clone();
                                        a.CategoryID = num3;
                                        a.Name2 = _param1.t.TorrentName;
                                        a.Size = size;
                                        a.IsRun = isRun;
                                        a.IsPause = num5 != 0;
                                        a.Seeders = num6;
                                        a.Label = _param1.t.Label;
                                        return a;
                                    }

                                    a = b;

                                    return a;
                                    /*{
                                        CategoryID = num3,
                                        Name = _param1.t.TorrentName,
                                        Size = size,
                                        IsRun = num4,
                                        IsPause = num5 != 0,
                                        Seeders = num6,
                                        Label = _param1.t.Label
                                    };*/
                                });
                                source.AddRange(array1.Where(x => x.CategoryID == -1).ToArray());
                            }
                        }
                    }

                    _lbTotal.Text = string.Format("Кол-во: {0}; Размер: {1}", source.Count(),
                        TopicInfo.sizeToString(source.Sum(x => x.Size)));
                    _TopicsSource.DataSource = source;
                }
            }

            if (sender != _CategorySource || _CategorySource.Current == null)
                return;
            tabReports.Controls.Clear();
            Dictionary<Tuple<int, int>, Tuple<string, string>> reports =
                ClientLocalDB.Current.GetReports((_CategorySource.Current as Category).CategoryID);
            if (reports.Count() > 0)
            {
                Size size = tabReports.Size;
                TabControl tabControl = new TabControl();
                tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                tabControl.Location = new Point(0, 0);
                tabControl.SelectedIndex = 0;
                tabControl.Size = new Size(size.Width, size.Height);
                tabReports.Controls.Add(tabControl);
                foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> keyValuePair in reports
                    .OrderBy(
                        x => x.Key.Item2))
                {
                    if (!(keyValuePair.Value.Item2 == "Резерв") && !(keyValuePair.Value.Item2 == "Удалено"))
                    {
                        TabPage tabPage = new TabPage();
                        TextBox textBox = new TextBox();
                        tabPage.Location = new Point(4, 22);
                        tabPage.Padding = new Padding(3);
                        tabPage.Text = string.Format("Сидируемое: отчет № {0}", keyValuePair.Key.Item2);
                        if (keyValuePair.Key.Item2 == 0)
                            tabPage.Text = "Шапка сидируемого";
                        tabPage.UseVisualStyleBackColor = true;
                        tabPage.AutoScroll = true;
                        textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left |
                                         AnchorStyles.Right;
                        textBox.Location = new Point(0, 0);
                        textBox.Multiline = true;
                        textBox.ReadOnly = true;
                        textBox.ScrollBars = ScrollBars.Both;
                        textBox.Size = new Size(size.Width - 8, size.Height - 20);
                        textBox.Text = keyValuePair.Value.Item2;
                        tabControl.Controls.Add(tabPage);
                        tabPage.Controls.Add(textBox);
                    }
                }
            }
            else
            {
                Size size = tabReports.Size;
                TabControl tabControl = new TabControl();
                tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                tabControl.Location = new Point(0, 0);
                tabControl.SelectedIndex = 0;
                tabControl.Size = new Size(size.Width, size.Height);
                tabReports.Controls.Add(tabControl);
                TabPage tabPage = new TabPage();
                TextBox textBox = new TextBox();
                tabControl.Controls.Add(tabPage);
                tabPage.Controls.Add(textBox);
                tabPage.Location = new Point(4, 22);
                tabPage.Padding = new Padding(3);
                tabPage.Text = "Для информации";
                tabPage.UseVisualStyleBackColor = true;
                textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                textBox.Location = new Point(0, 0);
                textBox.Multiline = true;
                textBox.ReadOnly = true;
                textBox.ScrollBars = ScrollBars.Both;
                textBox.Size = new Size(size.Width - 8, size.Height - 20);
                textBox.Text =
                    "Здесь должен быть отчет о сидируемом, но его нет.\r\nВозможные причины: сервис не успел обработать задачи сервера на скачивание страниц, прочитать torrent-файлы или не смог подключиться к torrent-клиенту\r\n\r\nЕсли на вкладке \"Не скачано\" есть раздачи которые Вы храните, то попробуйте сформировать отчет принудительно из пункта меню";
            }
        }

        private void LinkClick(object sender, EventArgs e)
        {
            if (backgroundWorkers.Count != 0 &&
                MessageBox.Show("Выполняются другие задачи. Добавить в очередь новое?", "Внимание",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) !=
                DialogResult.Yes)
                return;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Category current = _CategorySource.Current as Category;
                if (current == null)
                    return;
                if (sender == _llUpdateCountSeedersByCategory)
                    UpdaterMethods.UpdateSeedersByCategory(current);
                else if (sender == _llUpdateTopicsByCategory)
                    dwCreateAndRun(WorkerMethods.bwUpdateTopicsByCategory,
                        "Обновление списков по разделу...", current);
                else if (sender == _llUpdateDataDromTorrentClient)
                    UpdaterMethods.UpdateHashFromClients(current.TorrentClientUID);
                else if (sender == _llDownloadSelectTopics)
                    dwCreateAndRun(WorkerMethods.bwDownloadTorrentFiles,
                        "Скачиваются выделеные торрент-файлы в каталог...",
                        new Tuple<List<TopicInfo>, MainForm>(
                            (_TopicsSource.DataSource as List<TopicInfo>)
                            .Where(x => x.Checked).ToList(), this));
                else if (sender == _llSelectedTopicsToTorrentClient)
                {
                    dwCreateAndRun(WorkerMethods.bwSendTorrentFileToTorrentClient,
                        "Скачиваются и добавляются в торрент-клиент выделенные раздачи...",
                        new Tuple<MainForm, List<TopicInfo>, Category>(this,
                            (_TopicsSource.DataSource as List<TopicInfo>)
                            .Where(x => x.Checked).ToList(), current));
                    dwCreateAndRun(
                        WorkerMethods.bwUpdateHashFromTorrentClientsByCategoryUID,
                        "Обновляем список раздач из торрент-клиента...", current);
                }
                else if (sender == _llSelectedTopicsToBlackList)
                {
                    List<TopicInfo> list = (_TopicsSource.DataSource as List<TopicInfo>)
                        .Where(x => x.Checked).ToList();
                    list.ForEach(x => x.IsBlackList = true);
                    ClientLocalDB.Current.SaveTopicInfo(list);
                }
                else if (sender == _llSelectedTopicsDeleteFromBlackList)
                {
                    List<TopicInfo> list = (_TopicsSource.DataSource as List<TopicInfo>)
                        .Where(x => x.Checked).ToList();
                    list.ForEach(x => x.IsBlackList = false);
                    ClientLocalDB.Current.SaveTopicInfo(list);
                }
                else if (sender == linkSetNewLabel)
                {
                    if (current == null)
                        return;
                    GetLableName getLableName = new GetLableName();
                    getLableName.Value = string.IsNullOrWhiteSpace(current.Label) ? current.FullName : current.Label;
                    if (getLableName.ShowDialog() == DialogResult.OK)
                        dwCreateAndRun(WorkerMethods.bwSetLabels,
                            "Установка пользовательских меток...",
                            new Tuple<MainForm, List<TopicInfo>, string>(this,
                                (_TopicsSource.DataSource as List<TopicInfo>)
                                .Where(x => x.Checked).ToList(),
                                getLableName.Value));
                }

                SelectionChanged(_CategorySource, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                int num = (int) MessageBox.Show("Произошла ошибка:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }

            Cursor.Current = Cursors.Default;
        }

        private void ContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridTopicsList.Columns[e.ColumnIndex].DataPropertyName == "Name")
            {
                try
                {
                    int? nullable = _dataGridTopicsList.Rows[e.RowIndex].Cells[0].Value as int?;
                    if (!nullable.HasValue)
                        return;
                    Process.Start(string.Format("https://{1}/forum/viewtopic.php?t={0}", nullable.Value,
                        Settings.Current.HostRuTrackerOrg));
                }
                catch
                {
                }
            }
            else
            {
                if (!(_dataGridTopicsList.Columns[e.ColumnIndex].DataPropertyName == "Alternative"))
                    return;
                try
                {
                    int? topicId = _dataGridTopicsList.Rows[e.RowIndex].Cells[0].Value as int?;
                    if (!topicId.HasValue)
                        return;
                    List<TopicInfo> dataSource = _TopicsSource.DataSource as List<TopicInfo>;
                    if (dataSource == null)
                        return;
                    TopicInfo topicInfo = dataSource
                        .Where(x => x.TopicID == topicId.Value)
                        .FirstOrDefault();
                    if (topicInfo == null)
                        return;
                    string empty1 = string.Empty;
                    string empty2 = string.Empty;
                    int num1 = topicInfo.Name.IndexOf('/');
                    string str1;
                    if (topicInfo.Name.IndexOf(']') > num1 && num1 != -1)
                        str1 = topicInfo.Name.Split('/').FirstOrDefault();
                    else if (topicInfo.Name.IndexOf(']') < num1 && num1 != -1)
                        str1 = topicInfo.Name.Split('/').FirstOrDefault().Split(']')[1];
                    else if (num1 == -1 && topicInfo.Name.IndexOf('[') < 5 && topicInfo.Name.IndexOf('[') != -1)
                        str1 = topicInfo.Name.Split(']')[1].Split('[').FirstOrDefault();
                    else if (num1 == -1 && topicInfo.Name.IndexOf('[') != -1)
                        str1 = topicInfo.Name.Split('[').FirstOrDefault();
                    else
                        str1 = topicInfo.Name.Split('[').FirstOrDefault();
                    int num2 = topicInfo.Name.IndexOf('[', num1 > -1 ? num1 : 0);
                    if (num2 < 5)
                    {
                        int startIndex = topicInfo.Name.IndexOf(']') + 1;
                        num2 = topicInfo.Name.IndexOf('[', startIndex);
                    }

                    string str2 = topicInfo.Name.Substring(num2 == -1 ? 0 : num2 + 1);
                    if (!string.IsNullOrWhiteSpace(str2))
                        str2 = str2.Split(new char[3]
                        {
                            ',',
                            ' ',
                            ']'
                        }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(str2))
                        str1 = str1 + " " + str2;
                    Process.Start(string.Format("https://{2}/forum/tracker.php?f={0}&nm={1}",
                        topicInfo.CategoryID, str1, Settings.Current.HostRuTrackerOrg));
                }
                catch
                {
                }
            }
        }

        private void dwCreateAndRun(DoWorkEventHandler e, string comment = "...", object argument = null)
        {
            BackgroundWorker key = new BackgroundWorker();
            key.WorkerReportsProgress = true;
            key.WorkerSupportsCancellation = true;
            key.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            key.ProgressChanged += backgroundWorker1_ProgressChanged;
            key.DoWork += e;
            backgroundWorkers.Add(key, new Tuple<DateTime, object, string>(DateTime.Now, argument, comment));
            if (backgroundWorkers.Count != 1)
                return;
            key.RunWorkerAsync(argument);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel1.Visible = false;
            statusStrip1.Refresh();
            if (sender != null && sender is BackgroundWorker &&
                backgroundWorkers.ContainsKey(sender as BackgroundWorker))
            {
                BackgroundWorker key = sender as BackgroundWorker;
                if (backgroundWorkers.ContainsKey(key))
                    backgroundWorkers.Remove(key);
                key.Dispose();
            }

            if (e.Result != null)
                _logger.Info(e.Result);
            if (backgroundWorkers.Count > 0)
            {
                // запуск следующей задачи.
                KeyValuePair<BackgroundWorker, Tuple<DateTime, object, string>> keyValuePair = backgroundWorkers
                    .OrderBy(
                        x =>
                            x.Value.Item1).First();
                keyValuePair.Key.RunWorkerAsync(keyValuePair.Value.Item2);
            }
            else
            {
                // записываем окончательные изменения в БД после выполнения последней задачи.
                SelectionChanged(_CategorySource, null);
                WriteReports();
                ClientLocalDB.Current.SaveToDatabase();
            }

            GC.Collect();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (sender != null && sender is BackgroundWorker &&
                backgroundWorkers.ContainsKey(sender as BackgroundWorker))
            {
                toolStripStatusLabel1.Text = backgroundWorkers[sender as BackgroundWorker].Item3;
                toolStripProgressBar1.Visible = true;
                toolStripStatusLabel1.Visible = true;
                statusStrip1.Refresh();
            }

            toolStripProgressBar1.Value =
                e.ProgressPercentage < 0 || e.ProgressPercentage > 100 ? 100 : e.ProgressPercentage;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!IsClose)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }
            else
                notifyIcon.Visible = false;
        }

        private void _dgvReportDownloads_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewColumn column = _dataGridTopicsList.Columns[e.ColumnIndex];
            if (column == ColumnReport1DgvSelect)
            {
                List<TopicInfo> dataSource = _TopicsSource.DataSource as List<TopicInfo>;
                if (dataSource == null)
                    return;
                List<TopicInfo> list = dataSource.ToList();
                list.ForEach(x =>
                {
                    TopicInfo topicInfo = x;
                    topicInfo.Checked = !topicInfo.Checked;
                });
                _TopicsSource.Clear();
                _TopicsSource.DataSource = list;
            }
            else
            {
                DataGridViewColumn sortedColumn = _dataGridTopicsList.SortedColumn;
                SortOrder sortOrder =
                    column.HeaderCell.SortGlyphDirection == SortOrder.None ||
                    column.HeaderCell.SortGlyphDirection == SortOrder.Descending
                        ? SortOrder.Ascending
                        : SortOrder.Descending;
                if (column == null)
                {
                    int num = (int) MessageBox.Show("Select a single column and try again.", "Error: Invalid Selection",
                        MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    List<TopicInfo> dataSource = _TopicsSource.DataSource as List<TopicInfo>;
                    if (dataSource == null)
                        return;
                    List<TopicInfo> list1 = dataSource.ToList();
                    List<TopicInfo> list2;
                    if (column == ColumnReport1DgvSize)
                        list2 = (sortOrder == SortOrder.Ascending
                            ? list1.OrderBy(
                                d => d.Size)
                            : list1.OrderByDescending(
                                d => d.Size)).ToList();
                    else if (column == ColumnReport1DgvName)
                        list2 = (sortOrder == SortOrder.Ascending
                            ? list1.OrderBy(
                                d => d.Name)
                            : list1.OrderByDescending(
                                d => d.Name)).ToList();
                    else if (column == ColumnReport1DgvSeeders)
                        list2 = (sortOrder == SortOrder.Ascending
                                ? list1
                                    .OrderBy(d => d.Seeders)
                                    .ThenBy(d => Name)
                                : list1
                                    .OrderByDescending(d => d.Seeders)
                                    .ThenBy(d => Name))
                            .ToList();
                    else if (column == ColumnReport1DgvAvgSeeders)
                        list2 = (sortOrder == SortOrder.Ascending
                                ? list1
                                    .OrderBy(d => d.AvgSeeders)
                                    .ThenBy(d => Name)
                                : list1
                                    .OrderByDescending(
                                        d => d.AvgSeeders)
                                    .ThenBy(d => Name))
                            .ToList();
                    else if (column == ColumnReport1DgvRegTime)
                    {
                        list2 = (sortOrder == SortOrder.Ascending
                                ? list1
                                    .OrderBy(d => d.RegTime)
                                    .ThenBy(d => Name)
                                : list1
                                    .OrderByDescending(
                                        d => d.RegTime)
                                    .ThenBy(d => Name))
                            .ToList();
                    }
                    else if (column == ColumnReport1DgvKeeperCount)
                    {
                        list2 = (sortOrder == SortOrder.Ascending
                                ? list1
                                    .OrderBy(d => d.KeeperCount)
                                    .ThenBy(d => Name)
                                : list1
                                    .OrderByDescending(d => d.KeeperCount)
                                    .ThenBy(d => Name))
                            .ToList();
                    }
                    else
                    {
                        if (column != ColumnReport1DgvStatus)
                            return;
                        list2 = (sortOrder == SortOrder.Ascending
                                ? list1
                                    .OrderBy(d => d.StatusToString)
                                    .ThenBy(d => Name)
                                : list1
                                    .OrderByDescending(
                                        d => d.StatusToString)
                                    .ThenBy(d => Name))
                            .ToList();
                    }

                    _TopicsSource.Clear();
                    _TopicsSource.DataSource = list2;
                    column.HeaderCell.SortGlyphDirection = sortOrder;
                }
            }
        }

        private void _dgvReportDownloads_Click(object sender, EventArgs e)
        {
            if (_dataGridTopicsList.Columns.GetColumnCount(DataGridViewElementStates.Selected) == 1)
            {
                DataGridViewColumn selectedColumn = _dataGridTopicsList.SelectedColumns[0];
            }

            Console.WriteLine("");
        }

        private void WriteReports()
        {
            Reports.CreateReportByRootCategories();
            _tcCetegoriesRootReports.Controls.Clear();
            Dictionary<Tuple<int, int>, Tuple<string, string>> reports = ClientLocalDB.Current.GetReports(0);
            string str1 = reports
                .Where(
                    x => x.Key.Item2 == 0)
                .Select(
                    x => x.Value.Item2)
                .FirstOrDefault();
            _txtConsolidatedReport.Text = string.IsNullOrWhiteSpace(str1) ? string.Empty : str1;
            string str2 = reports
                .Where(
                    x => x.Key.Item2 == 1)
                .Select(
                    x => x.Value.Item2)
                .FirstOrDefault();
            _tbConsolidatedTorrentClientsReport.Text = string.IsNullOrWhiteSpace(str2) ? string.Empty : str2;
            IEnumerable<Category> categories = ClientLocalDB.Current.GetCategories()
                .Where(x => x.CategoryID > 100000);
            Size size = _tcCetegoriesRootReports.Size;
            foreach (Category category in categories)
            {
                string str3 = ClientLocalDB.Current.GetReports(category.CategoryID)
                    .Where(
                        x => x.Key.Item2 == 0)
                    .Select(
                        x => x.Value.Item2)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(str3))
                {
                    TabPage tabPage = new TabPage();
                    TextBox textBox = new TextBox();
                    tabPage.Location = new Point(4, 22);
                    tabPage.Padding = new Padding(3);
                    tabPage.Text = category.Name;
                    tabPage.UseVisualStyleBackColor = true;
                    tabPage.AutoScroll = true;
                    tabPage.Size = size;
                    textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    textBox.Location = new Point(0, 0);
                    textBox.Multiline = true;
                    textBox.ReadOnly = true;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Size = new Size(size.Width - 8, size.Height - 20);
                    textBox.Text = str3;
                    _tcCetegoriesRootReports.Controls.Add(tabPage);
                    tabPage.Controls.Add(textBox);
                }
            }
        }

        private void SaveSetingsToFile()
        {
            try
            {
                string empty = string.Empty;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = "tloback";
                saveFileDialog.Filter = "Файл архивных настроек|*.tloback";
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                string fileName = saveFileDialog.FileName;
                if (string.IsNullOrWhiteSpace(fileName))
                    return;
                using (FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                    {
                        foreach (TorrentClientInfo torrentClient in ClientLocalDB.Current.GetTorrentClients())
                        {
                            binaryWriter.Write("TorrentClientInfo");
                            binaryWriter.Write(torrentClient.UID.ToString());
                            binaryWriter.Write(torrentClient.Name);
                            binaryWriter.Write(torrentClient.Type);
                            binaryWriter.Write(torrentClient.ServerName);
                            binaryWriter.Write(torrentClient.ServerPort);
                            binaryWriter.Write(torrentClient.UserName);
                            binaryWriter.Write(torrentClient.UserPassword);
                        }

                        foreach (Category category in ClientLocalDB.Current.GetCategoriesEnable())
                        {
                            binaryWriter.Write("Category");
                            binaryWriter.Write(category.CategoryID);
                            binaryWriter.Write(category.CountSeeders);
                            binaryWriter.Write(category.TorrentClientUID.ToString());
                            binaryWriter.Write(category.Folder);
                            binaryWriter.Write(category.CreateSubFolder);
                            binaryWriter.Write(category.IsSaveTorrentFiles);
                            binaryWriter.Write(category.IsSaveWebPage);
                            binaryWriter.Write(category.Label);
                        }

                        int[] cats = ClientLocalDB.Current.GetCategoriesEnable()
                            .Select(x => x.CategoryID).ToArray();
                        Dictionary<Tuple<int, int>, Tuple<string, string>> reports =
                            ClientLocalDB.Current.GetReports(new int?());
                        foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> keyValuePair in reports
                            .Where(
                                x =>
                                    cats.Contains(x.Key.Item1)))
                        {
                            binaryWriter.Write("Report");
                            binaryWriter.Write(keyValuePair.Key.Item1);
                            binaryWriter.Write(keyValuePair.Key.Item2);
                            binaryWriter.Write(keyValuePair.Value.Item1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Trace(ex.StackTrace);
            }
        }

        private void ReadSettingsFromFile()
        {
            try
            {
                string empty = string.Empty;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "tloback";
                openFileDialog.Filter = "Файл архивных настроек|*.tloback";
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                string fileName = openFileDialog.FileName;
                if (string.IsNullOrWhiteSpace(fileName))
                    return;
                List<TorrentClientInfo> torrentClientInfoList = new List<TorrentClientInfo>();
                List<Category> categoryList = new List<Category>();
                List<Tuple<int, int, string>> result = new List<Tuple<int, int, string>>();
                using (FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        while (binaryReader.BaseStream.Length != binaryReader.BaseStream.Position)
                        {
                            string str = binaryReader.ReadString();
                            if (!(str == "TorrentClientInfo"))
                            {
                                if (!(str == "Category"))
                                {
                                    if (str == "Report")
                                        result.Add(new Tuple<int, int, string>(binaryReader.ReadInt32(),
                                            binaryReader.ReadInt32(), binaryReader.ReadString()));
                                }
                                else
                                {
                                    Category category = new Category
                                    {
                                        CategoryID = binaryReader.ReadInt32(),
                                        IsEnable = true,
                                        CountSeeders = binaryReader.ReadInt32(),
                                        TorrentClientUID = Guid.Parse(binaryReader.ReadString()),
                                        Folder = binaryReader.ReadString(),
                                        CreateSubFolder = binaryReader.ReadInt32(),
                                        IsSaveTorrentFiles = binaryReader.ReadBoolean(),
                                        IsSaveWebPage = binaryReader.ReadBoolean(),
                                        Label = binaryReader.ReadString()
                                    };
                                    categoryList.Add(category);
                                }
                            }
                            else
                            {
                                TorrentClientInfo torrentClientInfo = new TorrentClientInfo
                                {
                                    UID = Guid.Parse(binaryReader.ReadString()),
                                    Name = binaryReader.ReadString(),
                                    Type = binaryReader.ReadString(),
                                    ServerName = binaryReader.ReadString(),
                                    ServerPort = binaryReader.ReadInt32(),
                                    UserName = binaryReader.ReadString(),
                                    UserPassword = binaryReader.ReadString()
                                };
                                torrentClientInfoList.Add(torrentClientInfo);
                            }
                        }

                        ClientLocalDB.Current.SaveTorrentClients(torrentClientInfoList);
                        ClientLocalDB.Current.CategoriesSave(categoryList);
                        ClientLocalDB.Current.SaveSettingsReport(result);
                        ClientLocalDB.Current.SaveToDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Trace(ex.StackTrace);
            }
        }

        private void FormLoad(object sender, EventArgs e)
        {
            var loc = Properties.Settings.Default.WindowLocation;
            if (loc.X < 0)
            {
                loc.X = 0;
            }

            if (loc.Y < 0)
            {
                loc.Y = 0;
            }

            if (loc.X >= SystemInformation.VirtualScreen.Size.Width - Size.Width)
            {
                loc.X = SystemInformation.VirtualScreen.Size.Width - Size.Width;
            }

            if (loc.Y >= SystemInformation.VirtualScreen.Size.Height - Size.Height)
            {
                loc.Y = SystemInformation.VirtualScreen.Size.Height - Size.Height;
            }

            Location = loc;
        }

        private void FireFormClosing(object sender, FormClosingEventArgs e)
        {
            // Copy window location to app settings
            Properties.Settings.Default.WindowLocation = Location;
            Properties.Settings.Default.Save();
        }

        private void ExportUnknown_Click(object sender, EventArgs e)
        {
            dwCreateAndRun(WorkerMethods.bwCreateUnknownTorrentsReport, "Формирование отчета",
                this);
        }
    }
}