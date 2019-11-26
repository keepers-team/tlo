using NLog;
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
using TLO.local.Forms;
using TLO.local.Properties;
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
            this.InitializeComponent();
            this.menuTimerSetting.CheckStateChanged += (sender, args) =>
            {
                if (this.menuTimerSetting.Checked)
                {
                    _LastRunTimer = DateTime.Now;
                    if (!tmr.Enabled) tmr.Start();
                }
                else
                {
                    if (tmr.Enabled) tmr.Stop();
                }
            };
            this._DateRegistration.Value = DateTime.Now.AddDays(-30.0);
            this.Text = this.headText = string.Format("TLO {0}",
                (object) FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
            this._cbCountSeeders.Value = new Decimal(0);
            this._cbCategoryFilters.SelectedItem = (object) "Не скачан торрент и нет хранителя";
            this._CategorySource.Clear();
            this._CategorySource.DataSource = (object) ClientLocalDB.Current.GetCategoriesEnable(true);
            this._CategorySource.CurrentChanged += new EventHandler(this.SelectionChanged);
            this._cbCategory.DataSource = (object) this._CategorySource;
            if (this._CategorySource.Count > 0)
                this._CategorySource.Position = 1;
            this._TopicsSource.CurrentChanged += new EventHandler(this.SelectionChanged);
            this._dataGridTopicsList.AutoGenerateColumns = false;
            this._dataGridTopicsList.ClearSelection();
            this._dataGridTopicsList.DataSource = (object) this._TopicsSource;
            this.Disposed += new EventHandler(this.MainForm_Disposed);
            this.Resize += new EventHandler(this.MainForm_Resize);
            this.tmr = new Timer();
            this.tmr.Tick += new EventHandler(this.tmr_Tick);
            this.tmr.Interval = 1000;
            this.tmr.Start();
            this.IsClose = false;
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.Icon = (Icon) new ComponentResourceManager(typeof(MainForm)).GetObject("$this.Icon");
            this.notifyIcon.MouseDoubleClick += new MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            this.notifyIcon.Visible = true;
            this.WriteReports();
        }

        private void MenuClick(object sender, EventArgs e)
        {
            try
            {
                if (sender == this.menuSettingsToolStripMenuItem)
                {
                    if (new SettingsForm().ShowDialog() == DialogResult.OK)
                    {
                        this._CategorySource.Clear();
                        this._CategorySource.DataSource = (object) null;
                        this._CategorySource.DataSource = (object) ClientLocalDB.Current.GetCategoriesEnable();
                        this._CategorySource.Position = 0;
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
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateTopicsByCategories),
                        "Полное обновление информации о топиках (раздачах) по всем категориям...",
                        (object) ClientLocalDB.Current.GetCategoriesEnable());
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateHashFromAllTorrentClients),
                        "Полное обновление информации из Torrent-клиентов...", (object) null);
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateCountSeedersByAllCategories),
                        "Обновление кол-ва сидов на раздачах...", sender);
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateKeepersByAllCategories),
                        "Обновление данных о хранителях...", sender);
                }
                else if (sender == this.UpdateCountSeedersToolStripMenuItem)
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateCountSeedersByAllCategories),
                        "Обновление кол-ва сидов на раздачах...", sender);
                else if (sender == this.UpdateListTopicsToolStripMenuItem)
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateTopicsByCategories),
                        "Полное обновление информации о топиках (раздачах) по всем категориям...",
                        (object) ClientLocalDB.Current.GetCategoriesEnable());
                else if (sender == this.UpdateKeepTopicsToolStripMenuItem)
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateHashFromAllTorrentClients),
                        "Полное обновление информации из Torrent-клиентов...", (object) null);
                else if (sender == this.ClearDatabaseToolStripMenuItem)
                {
                    if (MessageBox.Show(
                            "Вы пытаетесь очистить базу данны от текущих данных (статистику и информацию о топиках).\r\n Продолжить?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    ClientLocalDB.Current.ClearDatabase();
                }
                else if (sender == this.ClearKeeperListsToolStripMenuItem)
                {
                    if (MessageBox.Show("Вы пытаетесь очистить базу данны от данных других хранителей.\r\n Продолжить?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    ClientLocalDB.Current.ClearKeepers();
                    this.SelectionChanged((object) this._CategorySource, (EventArgs) null);
                }
                else if (sender == this.SendReportsToForumToolStripMenuItem)
                {
                    if (MessageBox.Show(
                            "Отправка отчетов на форум может продолжаться продолжительное время.\r\n Продолжить?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwSendReports),
                        "Отправка отчетов на форум...",
                        (object) this);
                }
                else if (sender == this.CreateReportsToolStripMenuItem)
                {
                    if (MessageBox.Show(
                            "Сборка отчетов может продолжаться продолжительное время и потребуется обновить список раздач и информацию из торрент-клиентов.\r\n Продолжит?",
                            "Запрос подтверждения", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateCountSeedersByAllCategories),
                        "Обновление кол-ва сидов на раздачах...", sender);
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateHashFromAllTorrentClients),
                        "Обновление информации из Torrent-клиентов...", sender);
                }
                else if (sender == this.RuningStopingDistributionToolStripMenuItem)
                {
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateCountSeedersByAllCategories),
                        "Обновление кол-ва сидов на раздачах...", sender);
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwRuningAndStopingDistributions),
                        "Обновление информации из Torrent-клиентов...", sender);
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwCreateReportsTorrentClients),
                        "Построение сводного отчета по торрент-клиентам...", sender);
                }
                else if (sender == this.CreateConsolidatedReportByTorrentClientsToolStripMenuItem)
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwCreateReportsTorrentClients),
                        "Построение сводного отчета по торрент-клиентам...", sender);
                else if (sender == this.LoadListKeepersToolStripMenuItem)
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateKeepersByAllCategories),
                        "Обновление данных о хранителях...", sender);
                else if (sender == this.ExitToolStripMenuItem)
                {
                    this.IsClose = true;
                    this.Close();
                }
                else if (sender == this._btSaveToFile)
                    this.SaveSetingsToFile();
                else if (sender == this._btLoadSettingsFromFile)
                    this.ReadSettingsFromFile();
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
            if (this.backgroundWorkers.Count > 0)
            {
                this.Text = string.Format("{0} ({1})", (object) this.headText, (object) "Выполняются задачи...");
                this.notifyIcon.Text =
                    string.Format("{0} ({1})", (object) this.headText, (object) "Выполняются задачи...");
            }
            else
            {
                DateTime lastRunTimer = this._LastRunTimer;
                DateTime now = DateTime.Now;
                DateTime dateTime1 = now.AddMinutes((double) -Settings.Current.PeriodRunAndStopTorrents);
                TimeSpan timeSpan = lastRunTimer - dateTime1;
                if (timeSpan.TotalSeconds > 0.0)
                {
                    this.Text = string.Format("{0} ({1:hh\\:mm\\:ss})", (object) this.headText, (object) timeSpan);
                    this.notifyIcon.Text =
                        string.Format("{0} ({1:hh\\:mm\\:ss})", (object) this.headText, (object) timeSpan);
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
                            this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateTopicsByCategories),
                                "Полное обновление информации о топиках (раздачах) по всем категориям...",
                                (object) ClientLocalDB.Current.GetCategoriesEnable());
                            this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateKeepersByAllCategories),
                                "Обновление данных о хранителях...", sender);
                            Settings current = Settings.Current;
                            now = DateTime.Now;
                            DateTime date = now.Date;
                            current.LastUpdateTopics = date;
                            Settings.Current.Save();
                        }
                        else
                            this.dwCreateAndRun(
                                new DoWorkEventHandler(WorkerMethods.bwUpdateCountSeedersByAllCategories),
                                "Обновление информации о кол-ве сидов на раздачах...", sender);

                        this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwRuningAndStopingDistributions),
                            "Запуск/Остановка раздач в Torrent-клиентах...", sender);
                        this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwCreateReportsTorrentClients),
                            "Построение сводного отчета по торрент-клиентам...", sender);
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error(ex.Message);
                        this._logger.Debug(ex.Message, ex);
                    }

                    this._LastRunTimer = DateTime.Now;
                }
            }
        }

        private void MainForm_Disposed(object sender, EventArgs e)
        {
            this.tmr.Stop();
            this.tmr.Dispose();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
                return;
            this.Hide();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (sender == this._CategorySource || sender == this._cbCountSeeders ||
                (sender == this._cbBlackList || sender == this._cbCategoryFilters) || sender == this._DateRegistration)
            {
                this._TopicsSource.Clear();
                if (this._CategorySource.Current != null)
                {
                    Category current = this._CategorySource.Current as Category;
                    int num = (int) this._cbCountSeeders.Value;
                    DateTime regTime = this._DateRegistration.Value;
                    bool? isKeep = new bool?();
                    bool? isKeepers = new bool?();
                    bool? isDownload = new bool?();
                    bool? isBlack = new bool?();
                    bool? isPoster = new bool?();
                    string selectedItem = this._cbCategoryFilters.SelectedItem as string;
                    if (!string.IsNullOrWhiteSpace(selectedItem))
                    {
                        switch (selectedItem)
                        {
                            case "Есть хранитель":
                                isKeepers = new bool?(true);
                                break;
                            case "Не скачан торрент":
                                isDownload = new bool?(false);
                                break;
                            case "Не скачан торрент и есть хранитель":
                                isDownload = new bool?(false);
                                isKeepers = new bool?(true);
                                break;
                            case "Не скачан торрент и нет хранителя":
                                isDownload = new bool?(false);
                                isKeepers = new bool?(false);
                                break;
                            case "Не скачано":
                                isDownload = new bool?(false);
                                break;
                            case "Не храню":
                                isKeep = new bool?(false);
                                break;
                            case "Скачиваю раздачу":
                                isDownload = new bool?(true);
                                isKeep = new bool?(false);
                                break;
                            case "Храню":
                                isKeep = new bool?(true);
                                break;
                            case "Храню и есть хранитель":
                                isKeep = new bool?(true);
                                isKeepers = new bool?(true);
                                break;
                            case "Я релизер":
                                isPoster = new bool?(false);
                                break;
                        }
                    }

                    List<TopicInfo> source;

                    if (current.CategoryID != -1)
                    {
                        isBlack = new bool?(this._cbBlackList.Checked);
                        List<TopicInfo> topicInfoList = new List<TopicInfo>();
                        source = !Settings.Current.IsAvgCountSeeders
                            ? ClientLocalDB.Current.GetTopics(regTime, current.CategoryID,
                                num > -1 ? new int?(num) : new int?(), new int?(), isKeep, isKeepers, isDownload,
                                isBlack,
                                isPoster)
                            : ClientLocalDB.Current.GetTopics(regTime, current.CategoryID, new int?(),
                                num > -1 ? new int?(num) : new int?(), isKeep, isKeepers, isDownload, isBlack,
                                isPoster);
                    }
                    else
                    {
                        List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
                        IEnumerable<TopicInfo> inner = ClientLocalDB.Current.GetTopicsByCategory(-1)
                            .Where<TopicInfo>(x => !x.IsBlackList);
                        Dictionary<int, Category> dictionary = ClientLocalDB.Current.GetCategories()
                            .ToDictionary<Category, int, Category>(x => x.CategoryID, x => x);
                        source = new List<TopicInfo>();
                        foreach (TorrentClientInfo torrentClientInfo in torrentClients)
                        {
                            ITorrentClient torrentClient = torrentClientInfo.Create();
                            if (torrentClient != null)
                            {
                                var array1 = torrentClient.GetAllTorrentHash().GroupJoin(inner, t => t.Hash,
                                    b => b.Hash, (t, bt) => new
                                    {
                                        t = t,
                                        bt = bt
                                    }).SelectMany(_param1 => _param1.bt.DefaultIfEmpty<TopicInfo>(), (_param1, b) =>
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
                                    else
                                    {
                                        a = b;
                                    }

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

                    this._lbTotal.Text = string.Format("Кол-во: {0}; Размер: {1}", (object) source.Count<TopicInfo>(),
                        (object) TopicInfo.sizeToString(source.Sum<TopicInfo>((Func<TopicInfo, long>) (x => x.Size))));
                    this._TopicsSource.DataSource = (object) source;
                }
            }

            if (sender != this._CategorySource || this._CategorySource.Current == null)
                return;
            this.tabReports.Controls.Clear();
            Dictionary<Tuple<int, int>, Tuple<string, string>> reports =
                ClientLocalDB.Current.GetReports(new int?((this._CategorySource.Current as Category).CategoryID));
            if (reports.Count<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>() > 0)
            {
                Size size = this.tabReports.Size;
                TabControl tabControl = new TabControl();
                tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                tabControl.Location = new Point(0, 0);
                tabControl.SelectedIndex = 0;
                tabControl.Size = new Size(size.Width, size.Height);
                this.tabReports.Controls.Add((Control) tabControl);
                foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> keyValuePair in (
                    IEnumerable<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>) reports
                    .OrderBy<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int>(
                        (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int>) (x => x.Key.Item2)))
                {
                    if (!(keyValuePair.Value.Item2 == "Резерв") && !(keyValuePair.Value.Item2 == "Удалено"))
                    {
                        TabPage tabPage = new TabPage();
                        TextBox textBox = new TextBox();
                        tabPage.Location = new Point(4, 22);
                        tabPage.Padding = new Padding(3);
                        tabPage.Text = string.Format("Сидируемое: отчет № {0}", (object) keyValuePair.Key.Item2);
                        if (keyValuePair.Key.Item2 == 0)
                            tabPage.Text = string.Format("Шапка сидируемого");
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
                        tabControl.Controls.Add((Control) tabPage);
                        tabPage.Controls.Add((Control) textBox);
                    }
                }
            }
            else
            {
                Size size = this.tabReports.Size;
                TabControl tabControl = new TabControl();
                tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                tabControl.Location = new Point(0, 0);
                tabControl.SelectedIndex = 0;
                tabControl.Size = new Size(size.Width, size.Height);
                this.tabReports.Controls.Add((Control) tabControl);
                TabPage tabPage = new TabPage();
                TextBox textBox = new TextBox();
                tabControl.Controls.Add((Control) tabPage);
                tabPage.Controls.Add((Control) textBox);
                tabPage.Location = new Point(4, 22);
                tabPage.Padding = new Padding(3);
                tabPage.Text = string.Format("Для информации");
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
            if (this.backgroundWorkers.Count != 0 &&
                MessageBox.Show("Выполняются другие задачи. Добавить в очередь новое?", "Внимание",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) !=
                DialogResult.Yes)
                return;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Category current = this._CategorySource.Current as Category;
                if (current == null)
                    return;
                if (sender == this._llUpdateCountSeedersByCategory)
                    UpdaterMethods.UpdateSeedersByCategory(current);
                else if (sender == this._llUpdateTopicsByCategory)
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwUpdateTopicsByCategory),
                        "Обновление списков по разделу...", (object) current);
                else if (sender == this._llUpdateDataDromTorrentClient)
                    UpdaterMethods.UpdateHashFromClients(current.TorrentClientUID);
                else if (sender == this._llDownloadSelectTopics)
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwDownloadTorrentFiles),
                        "Скачиваются выделеные торрент-файлы в каталог...",
                        (object) new Tuple<List<TopicInfo>, MainForm>(
                            (this._TopicsSource.DataSource as List<TopicInfo>)
                            .Where<TopicInfo>((Func<TopicInfo, bool>) (x => x.Checked)).ToList<TopicInfo>(), this));
                else if (sender == this._llSelectedTopicsToTorrentClient)
                {
                    this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwSendTorrentFileToTorrentClient),
                        "Скачиваются и добавляются в торрент-клиент выделенные раздачи...",
                        (object) new Tuple<MainForm, List<TopicInfo>, Category>(this,
                            (this._TopicsSource.DataSource as List<TopicInfo>)
                            .Where<TopicInfo>((Func<TopicInfo, bool>) (x => x.Checked)).ToList<TopicInfo>(), current));
                    this.dwCreateAndRun(
                        new DoWorkEventHandler(WorkerMethods.bwUpdateHashFromTorrentClientsByCategoryUID),
                        "Обновляем список раздач из торрент-клиента...", (object) current);
                }
                else if (sender == this._llSelectedTopicsToBlackList)
                {
                    List<TopicInfo> list = (this._TopicsSource.DataSource as List<TopicInfo>)
                        .Where<TopicInfo>((Func<TopicInfo, bool>) (x => x.Checked)).ToList<TopicInfo>();
                    list.ForEach((Action<TopicInfo>) (x => x.IsBlackList = true));
                    ClientLocalDB.Current.SaveTopicInfo(list, false);
                }
                else if (sender == this._llSelectedTopicsDeleteFromBlackList)
                {
                    List<TopicInfo> list = (this._TopicsSource.DataSource as List<TopicInfo>)
                        .Where<TopicInfo>((Func<TopicInfo, bool>) (x => x.Checked)).ToList<TopicInfo>();
                    list.ForEach((Action<TopicInfo>) (x => x.IsBlackList = false));
                    ClientLocalDB.Current.SaveTopicInfo(list, false);
                }
                else if (sender == this.linkSetNewLabel)
                {
                    if (current == null)
                        return;
                    GetLableName getLableName = new GetLableName();
                    getLableName.Value = string.IsNullOrWhiteSpace(current.Label) ? current.FullName : current.Label;
                    if (getLableName.ShowDialog() == DialogResult.OK)
                        this.dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwSetLabels),
                            "Установка пользовательских меток...",
                            (object) new Tuple<MainForm, List<TopicInfo>, string>(this,
                                (this._TopicsSource.DataSource as List<TopicInfo>)
                                .Where<TopicInfo>((Func<TopicInfo, bool>) (x => x.Checked)).ToList<TopicInfo>(),
                                getLableName.Value));
                }

                this.SelectionChanged((object) this._CategorySource, (EventArgs) null);
            }
            catch (Exception ex)
            {
                this._logger.Error<Exception>(ex);
                int num = (int) MessageBox.Show("Произошла ошибка:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }

            Cursor.Current = Cursors.Default;
        }

        private void ContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (this._dataGridTopicsList.Columns[e.ColumnIndex].DataPropertyName == "Name")
            {
                try
                {
                    int? nullable = this._dataGridTopicsList.Rows[e.RowIndex].Cells[0].Value as int?;
                    if (!nullable.HasValue)
                        return;
                    Process.Start(string.Format("https://{1}/forum/viewtopic.php?t={0}", (object) nullable.Value,
                        Settings.Current.HostRuTrackerOrg));
                }
                catch
                {
                }
            }
            else
            {
                if (!(this._dataGridTopicsList.Columns[e.ColumnIndex].DataPropertyName == "Alternative"))
                    return;
                try
                {
                    int? topicId = this._dataGridTopicsList.Rows[e.RowIndex].Cells[0].Value as int?;
                    if (!topicId.HasValue)
                        return;
                    List<TopicInfo> dataSource = this._TopicsSource.DataSource as List<TopicInfo>;
                    if (dataSource == null)
                        return;
                    TopicInfo topicInfo = dataSource
                        .Where<TopicInfo>((Func<TopicInfo, bool>) (x => x.TopicID == topicId.Value))
                        .FirstOrDefault<TopicInfo>();
                    if (topicInfo == null)
                        return;
                    string empty1 = string.Empty;
                    string empty2 = string.Empty;
                    int num1 = topicInfo.Name.IndexOf('/');
                    string str1;
                    if (topicInfo.Name.IndexOf(']') > num1 && num1 != -1)
                        str1 = ((IEnumerable<string>) topicInfo.Name.Split('/')).FirstOrDefault<string>();
                    else if (topicInfo.Name.IndexOf(']') < num1 && num1 != -1)
                        str1 = ((IEnumerable<string>) topicInfo.Name.Split('/')).FirstOrDefault<string>().Split(']')[1];
                    else if (num1 == -1 && topicInfo.Name.IndexOf('[') < 5 && topicInfo.Name.IndexOf('[') != -1)
                        str1 = ((IEnumerable<string>) topicInfo.Name.Split(']')[1].Split('[')).FirstOrDefault<string>();
                    else if (num1 == -1 && topicInfo.Name.IndexOf('[') != -1)
                        str1 = ((IEnumerable<string>) topicInfo.Name.Split('[')).FirstOrDefault<string>();
                    else
                        str1 = ((IEnumerable<string>) topicInfo.Name.Split('[')).FirstOrDefault<string>();
                    int num2 = topicInfo.Name.IndexOf('[', num1 > -1 ? num1 : 0);
                    if (num2 < 5)
                    {
                        int startIndex = topicInfo.Name.IndexOf(']') + 1;
                        num2 = topicInfo.Name.IndexOf('[', startIndex);
                    }

                    string str2 = topicInfo.Name.Substring(num2 == -1 ? 0 : num2 + 1);
                    if (!string.IsNullOrWhiteSpace(str2))
                        str2 = ((IEnumerable<string>) str2.Split(new char[3]
                        {
                            ',',
                            ' ',
                            ']'
                        }, StringSplitOptions.RemoveEmptyEntries)).FirstOrDefault<string>();
                    if (!string.IsNullOrWhiteSpace(str2))
                        str1 = str1 + " " + str2;
                    Process.Start(string.Format("https://{2}/forum/tracker.php?f={0}&nm={1}",
                        (object) topicInfo.CategoryID, (object) str1, Settings.Current.HostRuTrackerOrg));
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
            key.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            key.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            key.DoWork += e;
            this.backgroundWorkers.Add(key, new Tuple<DateTime, object, string>(DateTime.Now, argument, comment));
            if (this.backgroundWorkers.Count != 1)
                return;
            key.RunWorkerAsync(argument);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.Value = 0;
            this.toolStripProgressBar1.Visible = false;
            this.toolStripStatusLabel1.Text = "";
            this.toolStripStatusLabel1.Visible = false;
            this.statusStrip1.Refresh();
            if (sender != null && sender is BackgroundWorker &&
                this.backgroundWorkers.ContainsKey(sender as BackgroundWorker))
            {
                BackgroundWorker key = sender as BackgroundWorker;
                if (this.backgroundWorkers.ContainsKey(key))
                    this.backgroundWorkers.Remove(key);
                key.Dispose();
            }

            if (e.Result != null)
                this._logger.Info(e.Result);
            if (this.backgroundWorkers.Count > 0)
            {
                // запуск следующей задачи.
                KeyValuePair<BackgroundWorker, Tuple<DateTime, object, string>> keyValuePair = this.backgroundWorkers
                    .OrderBy<KeyValuePair<BackgroundWorker, Tuple<DateTime, object, string>>, DateTime>(
                        (Func<KeyValuePair<BackgroundWorker, Tuple<DateTime, object, string>>, DateTime>) (x =>
                            x.Value.Item1)).First<KeyValuePair<BackgroundWorker, Tuple<DateTime, object, string>>>();
                keyValuePair.Key.RunWorkerAsync(keyValuePair.Value.Item2);
            }
            else
            {
                // записываем окончательные изменения в БД после выполнения последней задачи.
                this.SelectionChanged((object) this._CategorySource, (EventArgs) null);
                this.WriteReports();
                ClientLocalDB.Current.SaveToDatabase();
            }

            GC.Collect();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (sender != null && sender is BackgroundWorker &&
                this.backgroundWorkers.ContainsKey(sender as BackgroundWorker))
            {
                this.toolStripStatusLabel1.Text = this.backgroundWorkers[sender as BackgroundWorker].Item3;
                this.toolStripProgressBar1.Visible = true;
                this.toolStripStatusLabel1.Visible = true;
                this.statusStrip1.Refresh();
            }

            this.toolStripProgressBar1.Value =
                e.ProgressPercentage < 0 || e.ProgressPercentage > 100 ? 100 : e.ProgressPercentage;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!this.IsClose)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
            else
                this.notifyIcon.Visible = false;
        }

        private void _dgvReportDownloads_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewColumn column = this._dataGridTopicsList.Columns[e.ColumnIndex];
            if (column == this.ColumnReport1DgvSelect)
            {
                List<TopicInfo> dataSource = this._TopicsSource.DataSource as List<TopicInfo>;
                if (dataSource == null)
                    return;
                List<TopicInfo> list = dataSource.ToList<TopicInfo>();
                list.ForEach((Action<TopicInfo>) (x =>
                {
                    TopicInfo topicInfo = x;
                    topicInfo.Checked = !topicInfo.Checked;
                }));
                this._TopicsSource.Clear();
                this._TopicsSource.DataSource = (object) list;
            }
            else
            {
                DataGridViewColumn sortedColumn = this._dataGridTopicsList.SortedColumn;
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
                    List<TopicInfo> dataSource = this._TopicsSource.DataSource as List<TopicInfo>;
                    if (dataSource == null)
                        return;
                    List<TopicInfo> list1 = dataSource.ToList<TopicInfo>();
                    List<TopicInfo> list2;
                    if (column == this.ColumnReport1DgvSize)
                        list2 = (sortOrder == SortOrder.Ascending
                            ? (IEnumerable<TopicInfo>) list1.OrderBy<TopicInfo, long>(
                                (Func<TopicInfo, long>) (d => d.Size))
                            : (IEnumerable<TopicInfo>) list1.OrderByDescending<TopicInfo, long>(
                                (Func<TopicInfo, long>) (d => d.Size))).ToList<TopicInfo>();
                    else if (column == this.ColumnReport1DgvName)
                        list2 = (sortOrder == SortOrder.Ascending
                            ? (IEnumerable<TopicInfo>) list1.OrderBy<TopicInfo, string>(
                                (Func<TopicInfo, string>) (d => d.Name))
                            : (IEnumerable<TopicInfo>) list1.OrderByDescending<TopicInfo, string>(
                                (Func<TopicInfo, string>) (d => d.Name))).ToList<TopicInfo>();
                    else if (column == this.ColumnReport1DgvSeeders)
                        list2 = (sortOrder == SortOrder.Ascending
                                ? (IEnumerable<TopicInfo>) list1
                                    .OrderBy<TopicInfo, int>((Func<TopicInfo, int>) (d => d.Seeders))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name))
                                : (IEnumerable<TopicInfo>) list1
                                    .OrderByDescending<TopicInfo, int>((Func<TopicInfo, int>) (d => d.Seeders))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name)))
                            .ToList<TopicInfo>();
                    else if (column == this.ColumnReport1DgvAvgSeeders)
                        list2 = (sortOrder == SortOrder.Ascending
                                ? (IEnumerable<TopicInfo>) list1
                                    .OrderBy<TopicInfo, Decimal?>((Func<TopicInfo, Decimal?>) (d => d.AvgSeeders))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name))
                                : (IEnumerable<TopicInfo>) list1
                                    .OrderByDescending<TopicInfo, Decimal?>(
                                        (Func<TopicInfo, Decimal?>) (d => d.AvgSeeders))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name)))
                            .ToList<TopicInfo>();
                    else if (column == this.ColumnReport1DgvRegTime)
                    {
                        list2 = (sortOrder == SortOrder.Ascending
                                ? (IEnumerable<TopicInfo>) list1
                                    .OrderBy<TopicInfo, DateTime>((Func<TopicInfo, DateTime>) (d => d.RegTime))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name))
                                : (IEnumerable<TopicInfo>) list1
                                    .OrderByDescending<TopicInfo, DateTime>(
                                        (Func<TopicInfo, DateTime>) (d => d.RegTime))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name)))
                            .ToList<TopicInfo>();
                    }
                    else if (column == this.ColumnReport1DgvKeeperCount)
                    {
                        list2 = (sortOrder == SortOrder.Ascending
                                ? (IEnumerable<TopicInfo>) list1
                                    .OrderBy<TopicInfo, int?>((Func<TopicInfo, int?>) (d => d.KeeperCount))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name))
                                : (IEnumerable<TopicInfo>) list1
                                    .OrderByDescending<TopicInfo, int?>((Func<TopicInfo, int?>) (d => d.KeeperCount))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name)))
                            .ToList<TopicInfo>();
                    }
                    else
                    {
                        if (column != this.ColumnReport1DgvStatus)
                            return;
                        list2 = (sortOrder == SortOrder.Ascending
                                ? (IEnumerable<TopicInfo>) list1
                                    .OrderBy<TopicInfo, string>((Func<TopicInfo, string>) (d => d.StatusToString))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name))
                                : (IEnumerable<TopicInfo>) list1
                                    .OrderByDescending<TopicInfo, string>(
                                        (Func<TopicInfo, string>) (d => d.StatusToString))
                                    .ThenBy<TopicInfo, string>((Func<TopicInfo, string>) (d => this.Name)))
                            .ToList<TopicInfo>();
                    }

                    this._TopicsSource.Clear();
                    this._TopicsSource.DataSource = (object) list2;
                    column.HeaderCell.SortGlyphDirection = sortOrder;
                }
            }
        }

        private void _dgvReportDownloads_Click(object sender, EventArgs e)
        {
            if (this._dataGridTopicsList.Columns.GetColumnCount(DataGridViewElementStates.Selected) == 1)
            {
                DataGridViewColumn selectedColumn = this._dataGridTopicsList.SelectedColumns[0];
            }

            Console.WriteLine("");
        }

        private void WriteReports()
        {
            ClientLocalDB.Current.CreateReportByRootCategories();
            this._tcCetegoriesRootReports.Controls.Clear();
            Dictionary<Tuple<int, int>, Tuple<string, string>> reports = ClientLocalDB.Current.GetReports(new int?(0));
            string str1 = reports
                .Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>(
                    (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x => x.Key.Item2 == 0))
                .Select<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, string>(
                    (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, string>) (x => x.Value.Item2))
                .FirstOrDefault<string>();
            this._txtConsolidatedReport.Text = string.IsNullOrWhiteSpace(str1) ? string.Empty : str1;
            string str2 = reports
                .Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>(
                    (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x => x.Key.Item2 == 1))
                .Select<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, string>(
                    (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, string>) (x => x.Value.Item2))
                .FirstOrDefault<string>();
            this._tbConsolidatedTorrentClientsReport.Text = string.IsNullOrWhiteSpace(str2) ? string.Empty : str2;
            IEnumerable<Category> categories = ClientLocalDB.Current.GetCategories()
                .Where<Category>((Func<Category, bool>) (x => x.CategoryID > 100000));
            Size size = this._tcCetegoriesRootReports.Size;
            foreach (Category category in categories)
            {
                string str3 = ClientLocalDB.Current.GetReports(new int?(category.CategoryID))
                    .Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>(
                        (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x => x.Key.Item2 == 0))
                    .Select<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, string>(
                        (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, string>) (x => x.Value.Item2))
                    .FirstOrDefault<string>();
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
                    this._tcCetegoriesRootReports.Controls.Add((Control) tabPage);
                    tabPage.Controls.Add((Control) textBox);
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
                    using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream, Encoding.UTF8))
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
                            .Select<Category, int>((Func<Category, int>) (x => x.CategoryID)).ToArray<int>();
                        Dictionary<Tuple<int, int>, Tuple<string, string>> reports =
                            ClientLocalDB.Current.GetReports(new int?());
                        foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> keyValuePair in reports
                            .Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>(
                                (Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x =>
                                    ((IEnumerable<int>) cats).Contains<int>(x.Key.Item1))))
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
                this._logger.Error(ex.Message);
                this._logger.Trace(ex.StackTrace);
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
                    using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
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
                                    Category category = new Category()
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
                                TorrentClientInfo torrentClientInfo = new TorrentClientInfo()
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

                        ClientLocalDB.Current.SaveTorrentClients((IEnumerable<TorrentClientInfo>) torrentClientInfoList,
                            false);
                        ClientLocalDB.Current.CategoriesSave((IEnumerable<Category>) categoryList, false);
                        ClientLocalDB.Current.SaveSettingsReport(result);
                        ClientLocalDB.Current.SaveToDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(ex.Message);
                this._logger.Trace(ex.StackTrace);
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

            if (loc.X >= SystemInformation.VirtualScreen.Size.Width - this.Size.Width)
            {
                loc.X = SystemInformation.VirtualScreen.Size.Width - this.Size.Width;
            }

            if (loc.Y >= SystemInformation.VirtualScreen.Size.Height - this.Size.Height)
            {
                loc.Y = SystemInformation.VirtualScreen.Size.Height - this.Size.Height;
            }

            this.Location = loc;
        }

        private void FireFormClosing(object sender, FormClosingEventArgs e)
        {
            // Copy window location to app settings
            Properties.Settings.Default.WindowLocation = this.Location;
            Properties.Settings.Default.Save();
        }

        private void ExportUnknown_Click(object sender, EventArgs e)
        {
            dwCreateAndRun(new DoWorkEventHandler(WorkerMethods.bwCreateUnknownTorrentsReport), "Формирование отчета",
                (object) this);
        }
    }
}