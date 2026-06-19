using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace MusicPlayer;

public partial class Form1 : Form
{
    // 控件引用
    private Label _songTitleLabel = null!;
    private Label _artistLabel = null!;
    private Button _playBtn = null!;
    private Panel _albumArt = null!;
    private ListView _songList = null!;

    // 颜色方案 — 深色优雅风
    private readonly Color C_Bg = Color.FromArgb(18, 18, 35);
    private readonly Color C_Panel = Color.FromArgb(22, 22, 45);
    private readonly Color C_Accent = Color.FromArgb(255, 107, 107);
    private readonly Color C_Gold = Color.FromArgb(255, 215, 100);
    private readonly Color C_Teal = Color.FromArgb(69, 183, 209);
    private readonly Color C_Text = Color.FromArgb(235, 235, 250);
    private readonly Color C_Muted = Color.FromArgb(145, 145, 180);

    public Form1()
    {
        InitializeComponent();
        this.Text = "音乐播放器";
        this.Size = new Size(1080, 680);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = C_Bg;
        this.Font = new Font("Microsoft YaHei UI", 10F);
        this.DoubleBuffered = true;

        BuildUI();
    }

    private void BuildUI()
    {
        // ========== 顶部标题栏 ==========
        var header = CreatePanel(0, 0, 1080, 60, Color.FromArgb(22, 20, 45));
        header.Paint += (s, e) =>
        {
            using var b = new LinearGradientBrush(header.ClientRectangle,
                Color.FromArgb(28, 20, 50), Color.FromArgb(20, 25, 50), LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(b, header.ClientRectangle);
            // 底部细线
            using var p = new Pen(Color.FromArgb(50, 50, 80));
            e.Graphics.DrawLine(p, 0, 59, 1080, 59);
        };

        var logo = CreateLabel("♪ 音乐播放器", "Segoe UI", 20F, FontStyle.Bold, C_Gold, 25, 14);
        var sub = CreateLabel("优雅聆听", "Microsoft YaHei UI", 9F, FontStyle.Regular, C_Muted, 205, 22);

        var searchBox = new TextBox
        {
            Size = new Size(190, 30),
            Location = new Point(800, 15),
            Font = new Font("Microsoft YaHei UI", 10F),
            BackColor = Color.FromArgb(40, 40, 68),
            ForeColor = C_Text,
            BorderStyle = BorderStyle.FixedSingle
        };
        var searchIcon = CreateLabel("🔍", "Segoe UI", 11F, FontStyle.Regular, C_Muted, 775, 18);

        header.Controls.AddRange(new Control[] { logo, sub, searchBox, searchIcon });
        this.Controls.Add(header);

        // ========== 左侧导航 ==========
        var sidebar = CreatePanel(0, 60, 195, 500, Color.FromArgb(16, 16, 35));
        string[] navs = { "🏠 发现音乐", "🎵 本地音乐", "📋 播放列表", "❤️ 收藏歌单" };
        int ny = 15;
        foreach (var n in navs)
        {
            var btn = new Button
            {
                Text = n, Size = new Size(180, 40), Location = new Point(8, ny),
                FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent, ForeColor = C_Muted,
                Font = new Font("Microsoft YaHei UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.MouseEnter += (_, _) => { if (btn.BackColor != Color.FromArgb(255, 70, 70, 120)) btn.BackColor = Color.FromArgb(35, 35, 65); };
            btn.MouseLeave += (_, _) => { if (btn.BackColor != Color.FromArgb(255, 70, 70, 120)) btn.BackColor = Color.Transparent; };
            sidebar.Controls.Add(btn);
            ny += 46;
        }

        var sep = new Panel { Size = new Size(160, 1), Location = new Point(18, ny + 5), BackColor = Color.FromArgb(45, 45, 75) };
        sidebar.Controls.Add(sep);

        var myPlaylist = CreateLabel("我的歌单", "Microsoft YaHei UI", 9F, FontStyle.Bold, C_Muted, 18, ny + 20);
        sidebar.Controls.Add(myPlaylist);

        string[] pls = { "🎧 最近播放", "🌙 夜晚的歌", "🔥 我的最爱" };
        ny += 45;
        foreach (var pl in pls)
        {
            var l = CreateLabel(pl, "Microsoft YaHei UI", 9F, FontStyle.Regular, Color.FromArgb(125, 125, 160), 25, ny);
            l.Cursor = Cursors.Hand;
            l.MouseEnter += (_, _) => l.ForeColor = C_Text;
            l.MouseLeave += (_, _) => l.ForeColor = Color.FromArgb(125, 125, 160);
            sidebar.Controls.Add(l);
            ny += 28;
        }
        this.Controls.Add(sidebar);

        // ========== 主区域 — 歌曲列表 ==========
        var mainArea = CreatePanel(205, 70, 520, 360, C_Bg);
        var listTitle = CreateLabel("推荐歌曲", "Microsoft YaHei UI", 13F, FontStyle.Bold, C_Text, 5, 5);
        mainArea.Controls.Add(listTitle);

        _songList = new ListView
        {
            Size = new Size(510, 310), Location = new Point(5, 35),
            View = View.Details, BackColor = C_Panel, ForeColor = C_Text,
            BorderStyle = BorderStyle.None, FullRowSelect = true,
            HeaderStyle = ColumnHeaderStyle.None,
            Font = new Font("Microsoft YaHei UI", 10F),
            OwnerDraw = true
        };
        _songList.Columns.Add("#", 35);
        _songList.Columns.Add("歌曲", 230);
        _songList.Columns.Add("歌手", 130);
        _songList.Columns.Add("时长", 65);

        typeof(ListView).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.SetProperty, null, _songList, new object[] { true });

        _songList.DrawColumnHeader += (_, _) => { };
        _songList.DrawItem += (s, e) =>
        {
            var item = e.Item;
            int idx = item.Index;
            bool alt = idx % 2 == 1;
            bool sel = item.Selected;

            Color bg = alt ? Color.FromArgb(25, 25, 52) : Color.FromArgb(22, 22, 48);
            if (sel) bg = Color.FromArgb(75, 75, 140);

            using var bb = new SolidBrush(bg);
            e.Graphics.FillRectangle(bb, e.Bounds);

            using var sf = new StringFormat { LineAlignment = StringAlignment.Center };
            using var idxF = new Font("Segoe UI", 9F, FontStyle.Regular);
            using var idxB = new SolidBrush(C_Muted);
            e.Graphics.DrawString(item.Text, idxF, idxB, new RectangleF(8, e.Bounds.Y, 30, e.Bounds.Height), sf);

            using var nF = new Font("Microsoft YaHei UI", 10F, sel ? FontStyle.Bold : FontStyle.Regular);
            using var nB = new SolidBrush(sel ? C_Gold : C_Text);
            e.Graphics.DrawString(item.SubItems[1].Text, nF, nB, new RectangleF(42, e.Bounds.Y, 225, e.Bounds.Height), sf);

            using var sF = new Font("Microsoft YaHei UI", 9.5F);
            using var sB = new SolidBrush(C_Muted);
            e.Graphics.DrawString(item.SubItems[2].Text, sF, sB, new RectangleF(272, e.Bounds.Y, 125, e.Bounds.Height), sf);

            e.Graphics.DrawString(item.SubItems[3].Text, sF, sB, new RectangleF(420, e.Bounds.Y, 60, e.Bounds.Height), sf);
        };
        _songList.DrawSubItem += (_, _) => { };
        _songList.SelectedIndexChanged += (_, _) =>
        {
            if (_songList.SelectedItems.Count > 0)
            {
                var it = _songList.SelectedItems[0];
                _songTitleLabel.Text = it.SubItems[1].Text;
                _artistLabel.Text = it.SubItems[2].Text;
                _albumArt.Invalidate();
            }
        };
        _songList.DoubleClick += (_, _) => { if (_songList.SelectedItems.Count > 0) _playBtn.Text = "⏸"; };

        mainArea.Controls.Add(_songList);
        this.Controls.Add(mainArea);

        // ========== 右侧 — 正在播放 ==========
        var rightArea = CreatePanel(735, 70, 330, 360, C_Bg);

        _albumArt = new Panel
        {
            Size = new Size(200, 200), Location = new Point(65, 10),
            BackColor = Color.FromArgb(30, 30, 58)
        };
        _albumArt.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(5, 5, 190, 190);
            using var bg = new LinearGradientBrush(r, Color.FromArgb(65, 40, 85), Color.FromArgb(30, 40, 75), LinearGradientMode.ForwardDiagonal);
            e.Graphics.FillEllipse(bg, r);
            using var p = new Pen(Color.FromArgb(80, C_Gold), 2.5f);
            e.Graphics.DrawEllipse(p, r);
            using var inner = new SolidBrush(Color.FromArgb(50, 20, 20, 50));
            e.Graphics.FillEllipse(inner, 65, 65, 70, 70);
            using var cf = new Font("Segoe UI", 30F, FontStyle.Bold);
            using var cb = new SolidBrush(Color.FromArgb(120, C_Gold));
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("♪", cf, cb, new PointF(100, 100), sf);
        };

        var npLabel = CreateLabel("— 正在播放 —", "Microsoft YaHei UI", 8F, FontStyle.Bold, Color.FromArgb(110, 110, 155), 20, 220);
        _songTitleLabel = CreateLabel("未选择歌曲", "Microsoft YaHei UI", 15F, FontStyle.Bold, C_Text, 20, 240);
        _songTitleLabel.Size = new Size(290, 28);
        _artistLabel = CreateLabel("双击歌曲开始播放", "Microsoft YaHei UI", 9.5F, FontStyle.Regular, C_Muted, 20, 270);
        _artistLabel.Size = new Size(290, 22);

        rightArea.Controls.AddRange(new Control[] { _albumArt, npLabel, _songTitleLabel, _artistLabel });
        this.Controls.Add(rightArea);

        // ========== 底部控制栏 ==========
        var bar = CreatePanel(0, 440, 1080, 120, Color.FromArgb(20, 18, 40));
        bar.Paint += (s, e) =>
        {
            using var b = new LinearGradientBrush(bar.ClientRectangle,
                Color.FromArgb(24, 18, 42), Color.FromArgb(18, 22, 45), LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(b, bar.ClientRectangle);
            using var p = new Pen(Color.FromArgb(35, 35, 65));
            e.Graphics.DrawLine(p, 0, 0, 1080, 0);
        };

        // 进度条
        var curTime = CreateLabel("00:00", "Segoe UI", 9F, FontStyle.Regular, C_Muted, 75, 15);
        var totalTime = CreateLabel("00:00", "Segoe UI", 9F, FontStyle.Regular, C_Muted, 945, 15);
        var progress = new TrackBar
        {
            Size = new Size(810, 30), Location = new Point(130, 12),
            Minimum = 0, Maximum = 100, Value = 0,
            TickStyle = TickStyle.None, BackColor = Color.FromArgb(20, 18, 40), ForeColor = C_Teal
        };

        // 控制按钮
        var prevBtn = MakeCtrlBtn("⏮", 350, 50);
        _playBtn = MakeCtrlBtn("▶", 425, 50, 50, 44);
        var nextBtn = MakeCtrlBtn("⏭", 495, 50);
        var stopBtn = MakeCtrlBtn("⏹", 560, 50);
        _playBtn.Click += (_, _) => _playBtn.Text = _playBtn.Text == "▶" ? "⏸" : "▶";

        // 音量
        var volIcon = CreateLabel("🔊", "Segoe UI", 11F, FontStyle.Regular, C_Muted, 720, 57);
        var volume = new TrackBar
        {
            Size = new Size(170, 30), Location = new Point(755, 54),
            Minimum = 0, Maximum = 100, Value = 70,
            TickStyle = TickStyle.None, BackColor = Color.FromArgb(20, 18, 40), ForeColor = C_Accent
        };

        // 底部歌词区
        var lyricLine = new Label
        {
            Text = "✦ 欢迎使用音乐播放器 — 聆听每一刻",
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Italic),
            ForeColor = Color.FromArgb(95, 95, 140), Location = new Point(75, 95), AutoSize = true
        };

        bar.Controls.AddRange(new Control[] { curTime, totalTime, progress, prevBtn, _playBtn, nextBtn, stopBtn, volIcon, volume, lyricLine });
        this.Controls.Add(bar);

        // ========== 加载示例数据 ==========
        var songs = new[] {
            ("1", "起风了", "买辣椒也用券", "5:15"), ("2", "孤勇者", "陈奕迅", "4:20"),
            ("3", "光年之外", "邓紫棋", "3:55"), ("4", "海底", "一支榴莲", "4:38"),
            ("5", "错位时空", "艾辰", "3:52"), ("6", "踏山河", "七叔", "3:20"),
            ("7", "星辰大海", "黄霄雲", "3:56"), ("8", "赤伶", "HITA", "4:20"),
            ("9", "半生雪", "七叔", "3:45"), ("10", "目及皆是你", "小蓝背心", "3:42")
        };
        foreach (var s in songs)
            _songList.Items.Add(new ListViewItem(new[] { s.Item1, s.Item2, s.Item3, s.Item4 }));
    }

    private Panel CreatePanel(int x, int y, int w, int h, Color c)
    {
        return new Panel { Size = new Size(w, h), Location = new Point(x, y), BackColor = c };
    }

    private Label CreateLabel(string text, string font, float size, FontStyle style, Color color, int x, int y)
    {
        return new Label
        {
            Text = text, Font = new Font(font, size, style), ForeColor = color,
            Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent
        };
    }

    private Button MakeCtrlBtn(string text, int x, int y, int w = 52, int h = 38)
    {
        return new Button
        {
            Text = text, Size = new Size(w, h), Location = new Point(x, y),
            FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 },
            BackColor = Color.Transparent, ForeColor = C_Text,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold), Cursor = Cursors.Hand
        };
    }
}
