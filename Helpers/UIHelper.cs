using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManagementSystem.Helpers
{
    public static class UIHelper
    {
        // ─── COLOR PALETTE ───────────────────────────────────────────────────────
        public static Color PrimaryColor    = Color.FromArgb(30,  136, 229);   // Blue
        public static Color AccentColor     = Color.FromArgb(0,   200, 150);   // Teal
        public static Color DangerColor     = Color.FromArgb(229, 57,  53);    // Red
        public static Color WarningColor    = Color.FromArgb(255, 167, 38);    // Orange
        public static Color SuccessColor    = Color.FromArgb(67,  160, 71);    // Green
        public static Color BgColor         = Color.FromArgb(245, 247, 250);
        public static Color CardColor       = Color.White;
        public static Color SidebarColor    = Color.FromArgb(25,  35,  55);
        public static Color TextPrimary     = Color.FromArgb(30,  30,  50);
        public static Color TextSecondary   = Color.FromArgb(120, 130, 150);
        public static Color BorderColor     = Color.FromArgb(220, 225, 235);

        // ─── FONTS ───────────────────────────────────────────────────────────────
        public static Font FontTitle   = new("Segoe UI", 20, FontStyle.Bold);
        public static Font FontSubtitle = new("Segoe UI", 13, FontStyle.Bold);
        public static Font FontBody    = new("Segoe UI", 10);
        public static Font FontSmall   = new("Segoe UI", 9);
        public static Font FontBold    = new("Segoe UI", 10, FontStyle.Bold);

        // ─── STYLED BUTTON ───────────────────────────────────────────────────────
        public static Button CreateButton(string text, Color backColor, Color foreColor = default)
        {
            if (foreColor == default) foreColor = Color.White;
            var btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = FontBold,
                Height = 38,
                Cursor = Cursors.Hand,
                Padding = new Padding(0),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(backColor, 0.1f);
            return btn;
        }

        public static Button PrimaryBtn(string text) => CreateButton(text, PrimaryColor);
        public static Button SuccessBtn(string text) => CreateButton(text, SuccessColor);
        public static Button DangerBtn(string text)  => CreateButton(text, DangerColor);
        public static Button NeutralBtn(string text) => CreateButton(text, Color.FromArgb(200,205,215), TextPrimary);

        // ─── STYLED TEXTBOX ──────────────────────────────────────────────────────
        public static TextBox CreateTextBox(string placeholder = "")
        {
            var tb = new TextBox
            {
                Font = FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 32,
                BackColor = Color.White,
                ForeColor = TextPrimary,
            };
            return tb;
        }

        // ─── STYLED LABEL ────────────────────────────────────────────────────────
        public static Label CreateLabel(string text, Font? font = null, Color? color = null)
        {
            return new Label
            {
                Text = text,
                Font = font ?? FontBody,
                ForeColor = color ?? TextPrimary,
                AutoSize = true,
            };
        }

        // ─── STYLED DATAGRIDVIEW ─────────────────────────────────────────────────
        public static DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = CardColor,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                GridColor = BorderColor,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = FontBody,
                Dock = DockStyle.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = CardColor,
                    ForeColor = TextPrimary,
                    SelectionBackColor = Color.FromArgb(230, 242, 255),
                    SelectionForeColor = TextPrimary,
                    Padding = new Padding(4),
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = SidebarColor,
                    ForeColor = Color.White,
                    Font = FontBold,
                    SelectionBackColor = SidebarColor,
                    SelectionForeColor = Color.White,
                    Padding = new Padding(6, 4, 6, 4),
                },
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 36 },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 253),
                }
            };
            return dgv;
        }

        // ─── CARD PANEL ──────────────────────────────────────────────────────────
        public static Panel CreateCard(int padding = 16)
        {
            return new Panel
            {
                BackColor = CardColor,
                Padding = new Padding(padding),
                Margin = new Padding(8),
            };
        }

        // ─── STAT CARD ───────────────────────────────────────────────────────────
        public static Panel CreateStatCard(string title, string value, Color accent)
        {
            var card = new Panel
            {
                Width = 200,
                Height = 100,
                BackColor = CardColor,
                Margin = new Padding(8),
                Padding = new Padding(16),
            };

            // Left accent bar
            var bar = new Panel
            {
                Width = 6,
                Dock = DockStyle.Left,
                BackColor = accent,
            };

            var inner = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor,
                Padding = new Padding(12, 8, 8, 8),
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = FontSmall,
                ForeColor = TextSecondary,
                AutoSize = false,
                Width = 160,
                Height = 22,
                Dock = DockStyle.Top,
            };
            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = false,
                Width = 160,
                Height = 42,
                Dock = DockStyle.Fill,
            };

            inner.Controls.Add(lblValue);
            inner.Controls.Add(lblTitle);
            card.Controls.Add(inner);
            card.Controls.Add(bar);
            return card;
        }

        // ─── SEARCH BOX ──────────────────────────────────────────────────────────
        public static Panel CreateSearchBar(out TextBox searchBox)
        {
            var panel = new Panel { Height = 46, Dock = DockStyle.Top, BackColor = BgColor, Padding = new Padding(0, 6, 0, 6) };
            searchBox = new TextBox
            {
                PlaceholderText = "🔍  Search...",
                Font = FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = TextPrimary,
            };
            panel.Controls.Add(searchBox);
            return panel;
        }
    }
}
