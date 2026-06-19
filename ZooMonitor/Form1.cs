using System.Drawing.Drawing2D;

namespace ZooMonitor;

public partial class Form1 : Form
{
    private readonly Color C_Bg = Color.FromArgb(240, 245, 235);
    private readonly Color C_Panel = Color.FromArgb(255, 255, 252);
    private readonly Color C_Header = Color.FromArgb(45, 85, 65);
    private readonly Color C_Green = Color.FromArgb(90, 180, 120);
    private readonly Color C_Yellow = Color.FromArgb(240, 195, 60);
    private readonly Color C_Red = Color.FromArgb(220, 85, 75);
    private readonly Color C_Blue = Color.FromArgb(70, 145, 215);
    private readonly Color C_Dark = Color.FromArgb(35, 50, 42);
    private readonly Color C_Text = Color.FromArgb(60, 65, 58);
    private readonly Color C_Muted = Color.FromArgb(145, 155, 145);

    public Form1()
    {
        InitializeComponent();
        this.Text = "动物园健康监测系统";
        this.Size = new Size(1200, 750);
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
        // ===== 顶部标题栏 =====
        var header = new Panel { Size = new Size(1200, 70), Location = new Point(0, 0), BackColor = C_Header };
        header.Paint += (s, e) =>
        {
            using var b = new LinearGradientBrush(header.ClientRectangle, Color.FromArgb(40, 95, 68), Color.FromArgb(50, 75, 62), LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(b, header.ClientRectangle);
        };
        var logo = new Label { Text = "ZooMonitor", Font = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(25, 14), AutoSize = true };
        var sub = new Label { Text = "动物健康监测管理系统", Font = new Font("Microsoft YaHei UI", 10F), ForeColor = Color.FromArgb(160, 210, 180), Location = new Point(220, 25), AutoSize = true };
        var timeLabel = new Label { Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Font = new Font("Microsoft YaHei UI", 11F), ForeColor = Color.FromArgb(180, 220, 195), Location = new Point(980, 25), AutoSize = true };
        var timer = new System.Windows.Forms.Timer { Interval = 30000 };
        timer.Tick += (_, _) => timeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        timer.Start();
        header.Controls.AddRange(new Control[] { logo, sub, timeLabel });
        this.Controls.Add(header);

        // ===== 统计卡片 =====
        int cardW = 260, cardH = 100, gap = 20, startX = 30;
        string[][] stats = new[] {
            new[]{"动物总数", "48", ColorToHex(C_Blue)},
            new[]{"健康", "42", ColorToHex(C_Green)},
            new[]{"观察中", "4", ColorToHex(C_Yellow)},
            new[]{"需关注", "2", ColorToHex(C_Red)}
        };
        for (int i = 0; i < stats.Length; i++)
        {
            int x = startX + i * (cardW + gap);
            var card = new Panel { Size = new Size(cardW, cardH), Location = new Point(x, 90), BackColor = C_Panel };
            int idx = i;
            var col = ColorTranslator.FromHtml(stats[i][2]);
            var colorLabel = new Label { Text = "", Size = new Size(6, cardH), Location = new Point(0, 0), BackColor = col };
            var numLabel = new Label { Text = stats[i][1], Font = new Font("Microsoft YaHei UI", 28F, FontStyle.Bold), ForeColor = col, Location = new Point(22, 18), AutoSize = true };
            var nameLabel = new Label { Text = stats[i][0], Font = new Font("Microsoft YaHei UI", 10F), ForeColor = C_Muted, Location = new Point(22, 58), AutoSize = true };
            card.Controls.AddRange(new Control[] { colorLabel, numLabel, nameLabel });
            this.Controls.Add(card);
        }

        // ===== 左侧 - 动物列表 =====
        var leftPanel = new Panel { Size = new Size(280, 440), Location = new Point(30, 210), BackColor = C_Panel };
        var leftTitle = new Label { Text = "动物列表", Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold), ForeColor = C_Dark, Location = new Point(15, 12), AutoSize = true };
        var searchBox = new TextBox { Size = new Size(250, 30), Location = new Point(15, 35), Font = new Font("Microsoft YaHei UI", 9F), BackColor = Color.FromArgb(245, 248, 243), BorderStyle = BorderStyle.FixedSingle };

        var animalGrid = new DataGridView
        {
            Size = new Size(250, 350), Location = new Point(15, 72),
            BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            GridColor = Color.FromArgb(235, 240, 232), Font = new Font("Microsoft YaHei UI", 9F),
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            RowTemplate = { Height = 32 }
        };
        animalGrid.Columns.Add("cName", "姓名"); animalGrid.Columns.Add("cSpecies", "种类"); animalGrid.Columns.Add("cStatus", "状态");
        animalGrid.Columns["cName"]!.Width = 80; animalGrid.Columns["cSpecies"]!.Width = 70; animalGrid.Columns["cStatus"]!.Width = 70;
        animalGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = C_Header, ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold) };
        animalGrid.RowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = C_Text, SelectionBackColor = Color.FromArgb(90, 180, 120), SelectionForeColor = Color.White };

        string[][] animals = new[] {
            new[]{"小雷", "狮子", "健康"}, new[]{"小象", "大象", "健康"},
            new[]{"阿虎", "老虎", "观察"}, new[]{"高高", "长颈鹿", "健康"},
            new[]{"可可", "鹦鹉", "健康"}, new[]{"团团", "熊猫", "关注"},
            new[]{"露娜", "企鹅", "健康"}, new[]{"辛巴", "狮子", "健康"},
        };
        foreach (var a in animals)
        {
            int idx2 = animalGrid.Rows.Add(a);
            var status = a[2];
            if (status == "健康") animalGrid.Rows[idx2].Cells["cStatus"]!.Style.ForeColor = C_Green;
            else if (status == "观察") animalGrid.Rows[idx2].Cells["cStatus"]!.Style.ForeColor = C_Yellow;
            else animalGrid.Rows[idx2].Cells["cStatus"]!.Style.ForeColor = C_Red;
        }

        leftPanel.Controls.AddRange(new Control[] { leftTitle, searchBox, animalGrid });
        this.Controls.Add(leftPanel);

        // ===== 中间 - 健康详情 =====
        var midPanel = new Panel { Size = new Size(380, 440), Location = new Point(325, 210), BackColor = C_Panel };
        var midTitle = new Label { Text = "健康详情", Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold), ForeColor = C_Dark, Location = new Point(15, 12), AutoSize = true };

        // 健康指标可视化
        var healthPanel = new Panel { Size = new Size(350, 150), Location = new Point(15, 38), BackColor = Color.FromArgb(248, 252, 248) };
        healthPanel.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            string[] labels = { "心率", "体温", "活动量", "食欲", "体重" };
            int[] vals = { 85, 92, 78, 88, 95 };
            Color[] colors = { C_Red, C_Yellow, C_Blue, C_Green, C_Blue };
            for (int i = 0; i < 5; i++)
            {
                int bx = 15 + i * 68, by = 30, bw = 45, bh = 80;
                using var bgBrush = new SolidBrush(Color.FromArgb(230, 238, 228));
                e.Graphics.FillRectangle(bgBrush, bx, by + (100 - bh), bw, bh);
                int fh = vals[i] * bh / 100;
                using var fb = new SolidBrush(colors[i]);
                e.Graphics.FillRectangle(fb, bx, by + (100 - fh), bw, fh);
                using var lf = new Font("Microsoft YaHei UI", 7F);
                using var lb = new SolidBrush(C_Muted);
                e.Graphics.DrawString(labels[i], lf, lb, bx, by + 105);
                e.Graphics.DrawString(vals[i] + "%", lf, lb, bx + 5, by + 118);
            }
            using var p = new Pen(Color.FromArgb(225, 233, 222));
            e.Graphics.DrawRectangle(p, 0, 0, 349, 149);
        };

        // 观察记录
        var noteLabel = new Label { Text = "观察记录", Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold), ForeColor = C_Dark, Location = new Point(15, 195), AutoSize = true };
        var noteBox = new TextBox
        {
            Size = new Size(350, 100), Location = new Point(15, 215),
            Font = new Font("Microsoft YaHei UI", 9.5F), Multiline = true,
            BorderStyle = BorderStyle.FixedSingle, ForeColor = C_Text,
            BackColor = Color.FromArgb(248, 252, 248),
            Text = "所有动物状态正常。\n小雷活动积极，进食良好。\n团团略有倦怠，持续观察中。",
            ScrollBars = ScrollBars.Vertical
        };
        var updateBtn = new Button { Text = "更新记录", Size = new Size(100, 32), Location = new Point(265, 320), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, BackColor = C_Green, ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 9F), Cursor = Cursors.Hand };

        // 警报
        var alertLabel = new Label { Text = "警报信息", Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold), ForeColor = C_Dark, Location = new Point(15, 360), AutoSize = true };
        var alertBox = new ListBox
        {
            Size = new Size(350, 60), Location = new Point(15, 380),
            BackColor = Color.FromArgb(255, 245, 240), BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei UI", 8.5F), ForeColor = C_Red, ItemHeight = 18
        };
        alertBox.Items.Add("⚠ 团团（熊猫）体温偏高，持续监测中");
        alertBox.Items.Add("⚠ 阿虎（老虎）连续两天活动量下降");

        midPanel.Controls.AddRange(new Control[] { midTitle, healthPanel, noteLabel, noteBox, updateBtn, alertLabel, alertBox });
        this.Controls.Add(midPanel);

        // ===== 右侧 - 活动日志 =====
        var rightPanel = new Panel { Size = new Size(260, 440), Location = new Point(720, 210), BackColor = C_Panel };
        var rightTitle = new Label { Text = "最近动态", Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold), ForeColor = C_Dark, Location = new Point(15, 12), AutoSize = true };

        string[] activities = new[]{
            "08:00  喂食 — 全部完成",
            "08:45  小雷 — 健康检查通过",
            "09:30  团团 — 体温检测38.5℃",
            "10:15  小象 — 沐浴完成",
            "11:00  露娜 — 游泳活动正常",
            "11:30  喂药 — 阿虎已服药",
            "13:00  喂食 — 下午轮次",
            "14:00  高高 — 伸展运动完成",
            "14:45  可可 — 叫声频率正常",
        };
        var actList = new ListBox
        {
            Size = new Size(230, 300), Location = new Point(15, 38),
            BackColor = Color.FromArgb(248, 252, 248), BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei UI", 8.5F), ForeColor = C_Text, ItemHeight = 20
        };
        foreach (var a in activities) actList.Items.Add(a);

        var addBtn = new Button { Text = "+ 添加记录", Size = new Size(120, 32), Location = new Point(15, 350), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(200, 210, 200) }, BackColor = Color.White, ForeColor = C_Dark, Font = new Font("Microsoft YaHei UI", 9F), Cursor = Cursors.Hand };
        var exportBtn = new Button { Text = "导出报告", Size = new Size(100, 32), Location = new Point(145, 395), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, BackColor = C_Blue, ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 9F), Cursor = Cursors.Hand };

        rightPanel.Controls.AddRange(new Control[] { rightTitle, actList, addBtn, exportBtn });
        this.Controls.Add(rightPanel);
    }

    private static string ColorToHex(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
}
