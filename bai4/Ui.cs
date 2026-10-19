namespace BanHangTraSua;

public record Item(int Id, string Text)
{
    public override string ToString() => Text;
}

internal static class Ui
{
    public static Label Lbl(Control p, string text, int x, int y)
    {
        var c = new Label { Text = text, AutoSize = true, Location = new Point(x, y + 4) };
        p.Controls.Add(c); return c;
    }
    public static TextBox Txt(Control p, int x, int y, int w)
    {
        var c = new TextBox { Location = new Point(x, y), Width = w };
        p.Controls.Add(c); return c;
    }
    public static ComboBox Cbo(Control p, int x, int y, int w)
    {
        var c = new ComboBox { Location = new Point(x, y), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
        p.Controls.Add(c); return c;
    }
    public static DateTimePicker Dtp(Control p, int x, int y, int w)
    {
        var c = new DateTimePicker { Location = new Point(x, y), Width = w, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        p.Controls.Add(c); return c;
    }
    public static NumericUpDown Nud(Control p, int x, int y, int w, decimal max, decimal inc = 1, decimal min = 0)
    {
        var c = new NumericUpDown
        {
            Location = new Point(x, y), Width = w, Minimum = min, Maximum = max, Increment = inc,
            ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right
        };
        p.Controls.Add(c); return c;
    }
    public static Button Btn(Control p, string text, int x, int y, int w, EventHandler onClick)
    {
        var c = new Button { Text = text, Location = new Point(x, y), Size = new Size(w, 30), UseVisualStyleBackColor = true };
        c.Click += onClick;
        p.Controls.Add(c); return c;
    }
    public static GroupBox Grp(Control p, string text, int x, int y, int w, int h)
    {
        var c = new GroupBox { Text = text, Location = new Point(x, y), Size = new Size(w, h) };
        p.Controls.Add(c); return c;
    }
    public static DataGridView Grid(Control p, int x, int y, int w, int h)
    {
        var c = new DataGridView
        {
            Location = new Point(x, y), Size = new Size(w, h),
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
            AutoGenerateColumns = false, BackgroundColor = SystemColors.Window,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        p.Controls.Add(c); return c;
    }
    public static DataGridViewTextBoxColumn Col(DataGridView g, string prop, string header, int w,
        string? fmt = null, bool right = false, bool fill = false)
    {
        var col = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, Width = w };
        if (fmt != null) col.DefaultCellStyle.Format = fmt;
        if (right) col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if (fill) col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        g.Columns.Add(col); return col;
    }
    public static void Info(string m) => MessageBox.Show(m, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static void Warn(string m) => MessageBox.Show(m, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Error(string m) => MessageBox.Show(m, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
