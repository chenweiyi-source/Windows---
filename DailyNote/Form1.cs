using System.Text.Json;

namespace DailyNote;

public class DiaryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string Mood { get; set; } = "开心";
    public DateTime Date { get; set; } = DateTime.Today;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public partial class Form1 : Form
{
    private readonly Color C_Bg = Color.FromArgb(248, 243, 235);
    private readonly Color C_Sidebar = Color.FromArgb(50, 55, 75);
    private readonly Color C_Accent = Color.FromArgb(235, 145, 80);
    private readonly Color C_Text = Color.FromArgb(50, 45, 40);
    private readonly Color C_Muted = Color.FromArgb(150, 145, 140);

    private ListBox _diaryList = null!;
    private TextBox _titleBox = null!;
    private TextBox _contentBox = null!;
    private DateTimePicker _datePicker = null!;
    private ComboBox _moodCombo = null!;
    private Label _statusLabel = null!;
    private TextBox _searchBox = null!;

    private List<DiaryEntry> _entries = new();
    private string _dataFile = "diaries.json";
    private bool _isEditing = false;

    public Form1()
    {
        InitializeComponent();
        this.Text = "DailyNote - 日记本";
        this.Size = new Size(1100, 680);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = C_Bg;
        this.Font = new Font("Microsoft YaHei UI", 10F);
        BuildUI();
        LoadData();
        RefreshList();
    }

    private void BuildUI()
    {
        // ===== 左侧面板 =====
        var sidebar = new Panel { Size = new Size(290, 680), Location = new Point(0, 0), BackColor = C_Sidebar };

        var logo = new Label { Text = "DailyNote", Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(22, 18), AutoSize = true };
        var sub = new Label { Text = "记录每一天", Font = new Font("Microsoft YaHei UI", 9F), ForeColor = Color.FromArgb(140, 145, 170), Location = new Point(22, 52), AutoSize = true };

        _searchBox = new TextBox { Size = new Size(246, 32), Location = new Point(22, 85), Font = new Font("Microsoft YaHei UI", 10F), BackColor = Color.FromArgb(65, 70, 92), ForeColor = Color.FromArgb(200, 200, 220), BorderStyle = BorderStyle.FixedSingle };
        var searchIcon = new Label { Text = "搜索日记...", Font = new Font("Microsoft YaHei UI", 9.5F), ForeColor = Color.FromArgb(130, 135, 160), Location = new Point(28, 90), AutoSize = true, Cursor = Cursors.IBeam };
        searchIcon.Click += (_, _) => _searchBox.Focus();
        _searchBox.Enter += (_, _) => searchIcon.Visible = false;
        _searchBox.Leave += (_, _) => { if (string.IsNullOrEmpty(_searchBox.Text)) searchIcon.Visible = true; };
        _searchBox.TextChanged += (_, _) => RefreshList();

        var newBtn = new Button { Text = "+ 写日记", Size = new Size(246, 42), Location = new Point(22, 125), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, BackColor = C_Accent, ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
        newBtn.Click += (_, _) => NewEntry();

        _diaryList = new ListBox { Size = new Size(246, 460), Location = new Point(22, 178), BackColor = Color.FromArgb(58, 63, 85), ForeColor = Color.FromArgb(220, 220, 240), BorderStyle = BorderStyle.None, Font = new Font("Microsoft YaHei UI", 10F), DrawMode = DrawMode.OwnerDrawVariable };
        _diaryList.DrawItem += (s, e) =>
        {
            if (e.Index < 0 || _diaryList.Items[e.Index] is not DiaryEntry item) return;
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = sel ? C_Accent : (e.Index % 2 == 0 ? Color.FromArgb(58, 63, 85) : Color.FromArgb(53, 58, 78));
            using var bb = new SolidBrush(bg);
            e.Graphics.FillRectangle(bb, e.Bounds);
            using var dF = new Font("Microsoft YaHei UI", 8F);
            using var dB = new SolidBrush(sel ? Color.White : Color.FromArgb(160, 165, 190));
            e.Graphics.DrawString(item.Date.ToString("MM/dd"), dF, dB, 10, e.Bounds.Y + 3);
            using var tF = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            using var tB = new SolidBrush(sel ? Color.White : Color.FromArgb(230, 230, 245));
            string title = item.Title.Length > 12 ? item.Title[..12] + "..." : item.Title;
            e.Graphics.DrawString(title, tF, tB, 55, e.Bounds.Y + 2);
            using var mF = new Font("Microsoft YaHei UI", 8F);
            e.Graphics.DrawString(item.Mood, mF, dB, 10, e.Bounds.Y + 24);
        };
        _diaryList.MeasureItem += (s, e) => e.ItemHeight = 44;
        _diaryList.SelectedIndexChanged += (_, _) => { if (_diaryList.SelectedItem is DiaryEntry en) LoadEntry(en); };

        sidebar.Controls.AddRange(new Control[] { logo, sub, _searchBox, searchIcon, newBtn, _diaryList });
        this.Controls.Add(sidebar);

        // ===== 右侧面板 =====
        var mainPanel = new Panel { Size = new Size(770, 640), Location = new Point(310, 20), BackColor = Color.FromArgb(255, 252, 248) };

        var editTitle = new Label { Text = "写日记", Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold), ForeColor = C_Text, Location = new Point(25, 15), AutoSize = true };

        _datePicker = new DateTimePicker { Size = new Size(200, 28), Location = new Point(25, 60), Font = new Font("Microsoft YaHei UI", 10F), Format = DateTimePickerFormat.Short };
        var dl = new Label { Text = "日期", Font = new Font("Microsoft YaHei UI", 9F), ForeColor = C_Muted, Location = new Point(25, 42), AutoSize = true };

        _moodCombo = new ComboBox { Size = new Size(200, 30), Location = new Point(470, 60), Font = new Font("Microsoft YaHei UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White };
        _moodCombo.Items.AddRange(new string[] { "😊 开心", "😐 一般", "😢 难过", "🔥 兴奋", "😴 疲惫" });
        _moodCombo.SelectedIndex = 0;
        var ml = new Label { Text = "心情", Font = new Font("Microsoft YaHei UI", 9F), ForeColor = C_Muted, Location = new Point(470, 42), AutoSize = true };

        _titleBox = new TextBox { Size = new Size(720, 34), Location = new Point(25, 110), Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle, ForeColor = C_Text };
        var tl = new Label { Text = "标题", Font = new Font("Microsoft YaHei UI", 9F), ForeColor = C_Muted, Location = new Point(25, 92), AutoSize = true };

        _contentBox = new TextBox { Size = new Size(720, 340), Location = new Point(25, 160), Font = new Font("Microsoft YaHei UI", 10.5F), Multiline = true, BorderStyle = BorderStyle.FixedSingle, ScrollBars = ScrollBars.Vertical, ForeColor = C_Text, BackColor = Color.FromArgb(254, 252, 249) };
        var cl = new Label { Text = "内容", Font = new Font("Microsoft YaHei UI", 9F), ForeColor = C_Muted, Location = new Point(25, 142), AutoSize = true };

        var sep = new Panel { Size = new Size(720, 1), Location = new Point(25, 515), BackColor = Color.FromArgb(230, 225, 218) };

        var saveBtn = new Button { Text = "💾 保存", Size = new Size(120, 40), Location = new Point(25, 530), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, BackColor = C_Accent, ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
        saveBtn.Click += (_, _) => SaveEntry();

        var deleteBtn = new Button { Text = "🗑 删除", Size = new Size(120, 40), Location = new Point(160, 530), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(200, 190, 180) }, BackColor = Color.White, ForeColor = Color.FromArgb(200, 80, 80), Font = new Font("Microsoft YaHei UI", 10F), Cursor = Cursors.Hand };
        deleteBtn.Click += (_, _) => DeleteEntry();

        var clearBtn = new Button { Text = "清空", Size = new Size(90, 40), Location = new Point(295, 530), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(210, 200, 190) }, BackColor = Color.White, ForeColor = C_Muted, Font = new Font("Microsoft YaHei UI", 9F), Cursor = Cursors.Hand };
        clearBtn.Click += (_, _) => ClearForm();

        _statusLabel = new Label { Text = "共 0 篇日记", Font = new Font("Microsoft YaHei UI", 9F), ForeColor = C_Muted, Location = new Point(25, 590), AutoSize = true };

        mainPanel.Controls.AddRange(new Control[] { editTitle, dl, _datePicker, ml, _moodCombo, tl, _titleBox, cl, _contentBox, sep, saveBtn, deleteBtn, clearBtn, _statusLabel });
        this.Controls.Add(mainPanel);
    }

    private void LoadData()
    {
        try { if (File.Exists(_dataFile)) _entries = JsonSerializer.Deserialize<List<DiaryEntry>>(File.ReadAllText(_dataFile)) ?? new(); }
        catch { _entries = new(); }
    }

    private void SaveData()
    {
        try { File.WriteAllText(_dataFile, JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true })); }
        catch (Exception ex) { MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void RefreshList()
    {
        string kw = _searchBox?.Text?.Trim().ToLower() ?? "";
        var q = _entries.AsEnumerable();
        if (!string.IsNullOrEmpty(kw))
            q = q.Where(e => e.Title.ToLower().Contains(kw) || e.Content.ToLower().Contains(kw));
        var sorted = q.OrderByDescending(e => e.Date).ThenByDescending(e => e.CreatedAt).ToList();
        _diaryList.Items.Clear();
        foreach (var e in sorted) _diaryList.Items.Add(e);
        _statusLabel.Text = "共 " + _entries.Count + " 篇日记";
    }

    private void NewEntry()
    {
        _isEditing = false;
        ClearForm();
        _datePicker.Value = DateTime.Today;
        _moodCombo.SelectedIndex = 0;
        _titleBox.Focus();
    }

    private void LoadEntry(DiaryEntry entry)
    {
        _isEditing = true;
        _titleBox.Text = entry.Title;
        _contentBox.Text = entry.Content;
        _datePicker.Value = entry.Date;
        for (int i = 0; i < _moodCombo.Items.Count; i++)
            if (_moodCombo.Items[i]!.ToString() == entry.Mood) { _moodCombo.SelectedIndex = i; break; }
    }

    private void SaveEntry()
    {
        string title = _titleBox.Text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            MessageBox.Show("请输入日记标题", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _titleBox.Focus(); return;
        }

        if (_isEditing && _diaryList.SelectedItem is DiaryEntry existing)
        {
            existing.Title = title;
            existing.Content = _contentBox.Text.Trim();
            existing.Date = _datePicker.Value.Date;
            existing.Mood = _moodCombo.SelectedItem?.ToString() ?? "开心";
        }
        else
        {
            _entries.Add(new DiaryEntry
            {
                Title = title,
                Content = _contentBox.Text.Trim(),
                Date = _datePicker.Value.Date,
                Mood = _moodCombo.SelectedItem?.ToString() ?? "开心"
            });
        }

        SaveData();
        RefreshList();
        _statusLabel.Text = "✅ 已保存  共 " + _entries.Count + " 篇日记";
    }

    private void DeleteEntry()
    {
        if (_diaryList.SelectedItem is DiaryEntry entry)
        {
            if (MessageBox.Show("确定删除「" + entry.Title + "」？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _entries.Remove(entry);
                SaveData();
                RefreshList();
                ClearForm();
                _statusLabel.Text = "🗑 已删除";
            }
        }
        else MessageBox.Show("请先选择一篇日记", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearForm()
    {
        _titleBox.Text = "";
        _contentBox.Text = "";
        _datePicker.Value = DateTime.Today;
        _moodCombo.SelectedIndex = 0;
    }
}
