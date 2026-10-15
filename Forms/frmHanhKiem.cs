using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace HanhKiemApp.Forms
{
    /// <summary>
    /// Màn hình: Quản lý hạnh kiểm học sinh.
    /// Tìm kiếm/lọc + Danh sách + Thêm / Sửa / Xoá (dùng câu SQL có tham số, KHÔNG dùng stored procedure).
    /// Giao diện tạo bằng code (không cần file .Designer.cs) giống các form của project thi bằng lái.
    /// </summary>
    public class frmHanhKiem : Form
    {
        private static readonly string[] DanhSachXepLoai = { "Tốt", "Khá", "Trung bình", "Yếu" };
        private static readonly Color MauChinh = Color.FromArgb(31, 90, 150);

        // ----- Các câu SQL dùng trong form (không dùng stored procedure) -----
        private const string SqlDsHocSinh =
            @"SELECT hs.MaHS, hs.HoTen, pl.MaLop, l.TenLop
              FROM tblHocSinh hs
              LEFT JOIN tblPhanLop pl
                     ON pl.MaPhanLop = (SELECT MAX(p2.MaPhanLop) FROM tblPhanLop p2 WHERE p2.MaHS = hs.MaHS)
              LEFT JOIN tblLopHoc l ON l.MaLop = pl.MaLop
              WHERE hs.TrangThai = 1
              ORDER BY hs.HoTen";
        private const string SqlDsLop = "SELECT MaLop, TenLop FROM tblLopHoc ORDER BY TenLop";
        private const string SqlDsHocKy = "SELECT MaHocKy, TenHocKy FROM tblHocKy ORDER BY MaHocKy";
        private const string SqlTimKiem =
            @"SELECT hk.MaHanhKiem, hk.MaHS, hs.HoTen, hk.MaLop, l.TenLop,
                     hk.MaHocKy, k.TenHocKy, hk.XepLoai, hk.NhanXet
              FROM tblHanhKiem hk
              JOIN tblHocSinh hs ON hs.MaHS = hk.MaHS
              JOIN tblLopHoc  l  ON l.MaLop = hk.MaLop
              JOIN tblHocKy   k  ON k.MaHocKy = hk.MaHocKy";
        private const string ThongBaoTrung = "Học sinh này đã có hạnh kiểm trong lớp và học kỳ này.";

        // ----- Bộ lọc bên trái -----
        private ComboBox cboLocHocSinh = null!, cboLocLop = null!, cboLocHocKy = null!, cboLocXepLoai = null!;
        private TextBox txtLocNhanXet = null!;

        // ----- Lưới danh sách -----
        private DataGridView grid = null!;

        // ----- Khung nhập liệu -----
        private TextBox txtMaHK = null!, txtNhanXet = null!;
        private ComboBox cboHocSinh = null!, cboLop = null!, cboHocKy = null!, cboXepLoai = null!;

        // true khi đang nạp dữ liệu bằng code -> bỏ qua các sự kiện SelectedIndexChanged / SelectionChanged
        private bool _dangNap;

        public frmHanhKiem()
        {
            Text = "Quản lý hạnh kiểm học sinh";
            Width = 1150;
            Height = 780;
            MinimumSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            BuildUI();
            NapDanhMuc();
            TaiDanhSach();
        }

        // ================================================================
        //  DỰNG GIAO DIỆN
        // ================================================================
        private void BuildUI()
        {
            // ----- Thanh tiêu đề màu xanh -----
            Panel header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = MauChinh };
            header.Controls.Add(new Label
            {
                Text = "Quản lý hạnh kiểm học sinh",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            });

            Panel body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 244, 249)
            };

            // Thứ tự Add + BringToFront giống các form bằng lái: control Fill phải nằm trên cùng
            Controls.Add(body);
            Controls.Add(header);
            body.BringToFront();

            Panel phai = BuildKhuVucPhai();
            Panel trai = BuildKhuVucTrai();
            body.Controls.Add(phai);
            body.Controls.Add(trai);
            phai.BringToFront();
        }

        // ----- Khu vực bên trái: bộ lọc + menu thao tác -----
        private Panel BuildKhuVucTrai()
        {
            Panel p = new Panel { Dock = DockStyle.Left, Width = 280 };

            GroupBox grpLoc = new GroupBox
            {
                Text = "Tìm kiếm / Lọc",
                Dock = DockStyle.Top,
                Height = 410,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            TableLayoutPanel t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            cboLocHocSinh = TaoCombo();
            cboLocLop = TaoCombo();
            cboLocHocKy = TaoCombo();
            cboLocXepLoai = TaoCombo();
            cboLocXepLoai.Items.Add("-- Tất cả --");
            cboLocXepLoai.Items.AddRange(DanhSachXepLoai);
            cboLocXepLoai.SelectedIndex = 0;

            txtLocNhanXet = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            txtLocNhanXet.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TaiDanhSach(); }
            };

            Button btnTim = TaoNut("Tìm kiếm", MauChinh, Color.White, (s, e) => TaiDanhSach());
            btnTim.Dock = DockStyle.Fill;
            btnTim.Width = 100;
            btnTim.Margin = new Padding(0, 4, 0, 0);

            ThemDong(t, TaoNhan("Học sinh:")); ThemDong(t, cboLocHocSinh);
            ThemDong(t, TaoNhan("Lớp:")); ThemDong(t, cboLocLop);
            ThemDong(t, TaoNhan("Học kỳ:")); ThemDong(t, cboLocHocKy);
            ThemDong(t, TaoNhan("Xếp loại:")); ThemDong(t, cboLocXepLoai);
            ThemDong(t, TaoNhan("Nhận xét (tìm theo từ khóa):")); ThemDong(t, txtLocNhanXet);
            ThemDong(t, btnTim);
            grpLoc.Controls.Add(t);

            // ----- Menu thao tác bên dưới bộ lọc -----
            FlowLayoutPanel nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 12, 0, 0)
            };
            nav.Controls.Add(TaoNutMenu("Danh sách hạnh kiểm", (s, e) => TaiDanhSach()));
            nav.Controls.Add(TaoNutMenu("Thêm mới", (s, e) => ThemMoi()));
            nav.Controls.Add(TaoNutMenu("Sửa", (s, e) => CapNhat()));
            nav.Controls.Add(TaoNutMenu("Xóa", (s, e) => Xoa()));
            nav.Controls.Add(TaoNutMenu("Làm mới", (s, e) => LamMoi()));

            p.Controls.Add(nav);
            p.Controls.Add(grpLoc);
            nav.BringToFront();
            return p;
        }

        // ----- Khu vực bên phải: lưới + khung nhập + nút -----
        private Panel BuildKhuVucPhai()
        {
            Panel p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 0, 0) };

            grid = TaoLuoi();
            p.Controls.Add(grid);
            p.Controls.Add(BuildKhuVucNhap());
            p.Controls.Add(new Label
            {
                Text = "Danh sách hạnh kiểm",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            });
            grid.BringToFront();
            return p;
        }

        private Panel BuildKhuVucNhap()
        {
            Panel p = new Panel { Dock = DockStyle.Bottom, Height = 310 };

            GroupBox grp = new GroupBox
            {
                Text = "Thông tin hạnh kiểm",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.White
            };

            txtMaHK = new TextBox { ReadOnly = true, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboHocSinh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboLop = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboHocKy = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboXepLoai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboXepLoai.Items.AddRange(DanhSachXepLoai);
            cboXepLoai.SelectedIndex = 0;
            txtNhanXet = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                MaxLength = 255,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 6, 3, 6)
            };

            // Chọn học sinh -> tự chọn lớp hiện tại của học sinh đó (lấy từ tblPhanLop)
            cboHocSinh.SelectedIndexChanged += (s, e) =>
            {
                if (_dangNap) return;
                if (cboHocSinh.SelectedItem is DataRowView drv && drv["MaLop"] != DBNull.Value)
                    cboLop.SelectedValue = Convert.ToInt32(drv["MaLop"]);
            };

            TableLayoutPanel t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 4; i++) t.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            t.Controls.Add(TaoNhan("Mã hạnh kiểm:"), 0, 0); t.Controls.Add(txtMaHK, 1, 0);
            t.Controls.Add(TaoNhan("Xếp loại:"), 2, 0);     t.Controls.Add(cboXepLoai, 3, 0);
            t.Controls.Add(TaoNhan("Học sinh:"), 0, 1);     t.Controls.Add(cboHocSinh, 1, 1);
            t.Controls.Add(TaoNhan("Nhận xét:"), 2, 1);     t.Controls.Add(txtNhanXet, 3, 1);
            t.SetRowSpan(txtNhanXet, 3);
            t.Controls.Add(TaoNhan("Lớp:"), 0, 2);          t.Controls.Add(cboLop, 1, 2);
            t.Controls.Add(TaoNhan("Học kỳ:"), 0, 3);       t.Controls.Add(cboHocKy, 1, 3);
            grp.Controls.Add(t);

            // ----- Hàng nút: Thêm mới | Sửa | Xóa | Làm mới (RightToLeft nên add ngược) -----
            FlowLayoutPanel nut = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 54,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            nut.Controls.Add(TaoNut("Làm mới", Color.FromArgb(220, 224, 230), Color.Black, (s, e) => LamMoi()));
            nut.Controls.Add(TaoNut("Xóa", Color.FromArgb(231, 76, 60), Color.White, (s, e) => Xoa()));
            nut.Controls.Add(TaoNut("Sửa", Color.FromArgb(36, 99, 189), Color.White, (s, e) => CapNhat()));
            nut.Controls.Add(TaoNut("Thêm mới", Color.White, MauChinh, (s, e) => ThemMoi()));

            p.Controls.Add(grp);
            p.Controls.Add(nut);
            grp.BringToFront();
            return p;
        }

        private DataGridView TaoLuoi()
        {
            DataGridView g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };

            g.Columns.Add(Cot("MaHanhKiem", "Mã HK", 8));
            g.Columns.Add(Cot("HoTen", "Họ tên học sinh", 24));
            g.Columns.Add(Cot("TenLop", "Lớp", 8));
            g.Columns.Add(Cot("TenHocKy", "Học kỳ", 9));
            g.Columns.Add(Cot("XepLoai", "Xếp loại", 12));
            g.Columns.Add(Cot("NhanXet", "Nhận xét", 39));
            // Các cột ẩn, dùng để đổ lại lên ComboBox khi chọn dòng
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaHS", DataPropertyName = "MaHS", Visible = false });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaLop", DataPropertyName = "MaLop", Visible = false });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaHocKy", DataPropertyName = "MaHocKy", Visible = false });

            g.SelectionChanged += (s, e) => { if (!_dangNap) HienDongHienTai(); };
            return g;
        }

        private static DataGridViewTextBoxColumn Cot(string ten, string tieuDe, float tyLe)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = ten,
                DataPropertyName = ten,
                HeaderText = tieuDe,
                FillWeight = tyLe
            };
        }

        // ---------- Các hàm tạo control nhỏ ----------
        private static Label TaoNhan(string text) =>
            new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 0, 3, 3) };

        private static ComboBox TaoCombo() =>
            new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 8) };

        private static void ThemDong(TableLayoutPanel t, Control c)
        {
            int dong = t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(c, 0, dong);
        }

        private static Button TaoNut(string text, Color nen, Color chu, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 120,
                Height = 36,
                Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = nen,
                ForeColor = chu
            };
            btn.FlatAppearance.BorderColor = MauChinh;
            btn.Click += onClick;
            return btn;
        }

        private static Button TaoNutMenu(string text, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 255,
                Height = 38,
                Margin = new Padding(0, 0, 0, 2),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.Transparent
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        // ================================================================
        //  NẠP DỮ LIỆU CHO CÁC COMBOBOX
        // ================================================================
        private void NapDanhMuc()
        {
            try
            {
                _dangNap = true;

                DataTable hs = DatabaseHelper.LayBang(SqlDsHocSinh);
                hs.Columns.Add("HienThi", typeof(string));
                foreach (DataRow r in hs.Rows)
                    r["HienThi"] = r["TenLop"] == DBNull.Value ? r["HoTen"].ToString() : $"{r["HoTen"]} ({r["TenLop"]})";

                DataTable lop = DatabaseHelper.LayBang(SqlDsLop);
                DataTable hk = DatabaseHelper.LayBang(SqlDsHocKy);

                GanCombo(cboLocHocSinh, hs, "MaHS", "HienThi", true);
                GanCombo(cboLocLop, lop, "MaLop", "TenLop", true);
                GanCombo(cboLocHocKy, hk, "MaHocKy", "TenHocKy", true);

                GanCombo(cboHocSinh, hs, "MaHS", "HienThi", false);
                GanCombo(cboLop, lop, "MaLop", "TenLop", false);
                GanCombo(cboHocKy, hk, "MaHocKy", "TenHocKy", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh mục: " + ex.Message);
            }
            finally
            {
                _dangNap = false;
            }
        }

        // Mỗi ComboBox dùng 1 bản sao riêng của DataTable (nếu dùng chung, các ComboBox sẽ chọn theo nhau)
        private static void GanCombo(ComboBox cbo, DataTable nguon, string cotMa, string cotHienThi, bool coTatCa)
        {
            DataTable bang = nguon.Copy();
            if (coTatCa)
            {
                DataRow dong = bang.NewRow();
                dong[cotMa] = 0;                       // 0 = "Tất cả" (sẽ đổi thành NULL khi gọi thủ tục)
                dong[cotHienThi] = "-- Tất cả --";
                bang.Rows.InsertAt(dong, 0);
            }
            cbo.DisplayMember = cotHienThi;
            cbo.ValueMember = cotMa;
            cbo.DataSource = bang;
            if (bang.Rows.Count > 0) cbo.SelectedIndex = 0;
        }

        // ================================================================
        //  TẢI DANH SÁCH (SELECT có điều kiện lọc)
        // ================================================================
        // Trả về mã đang chọn, hoặc null nếu đang chọn "-- Tất cả --"
        private static int? LayId(ComboBox c) =>
            c.SelectedValue is int v && v != 0 ? v : null;

        private void TaiDanhSach()
        {
            try
            {
                // Chỉ ghép thêm điều kiện cho những ô lọc mà người dùng có chọn / nhập
                List<string> dieuKien = new List<string>();
                List<MySqlParameter> thamSo = new List<MySqlParameter>();

                if (LayId(cboLocHocSinh) is int maHS)
                {
                    dieuKien.Add("hk.MaHS = @MaHS");
                    thamSo.Add(new MySqlParameter("@MaHS", maHS));
                }
                if (LayId(cboLocLop) is int maLop)
                {
                    dieuKien.Add("hk.MaLop = @MaLop");
                    thamSo.Add(new MySqlParameter("@MaLop", maLop));
                }
                if (LayId(cboLocHocKy) is int maHocKy)
                {
                    dieuKien.Add("hk.MaHocKy = @MaHocKy");
                    thamSo.Add(new MySqlParameter("@MaHocKy", maHocKy));
                }
                if (cboLocXepLoai.SelectedIndex > 0)
                {
                    dieuKien.Add("hk.XepLoai = @XepLoai");
                    thamSo.Add(new MySqlParameter("@XepLoai", cboLocXepLoai.Text));
                }
                string tuKhoa = txtLocNhanXet.Text.Trim();
                if (tuKhoa.Length > 0)
                {
                    dieuKien.Add("hk.NhanXet LIKE @TuKhoa");
                    thamSo.Add(new MySqlParameter("@TuKhoa", "%" + tuKhoa + "%"));
                }

                string sql = SqlTimKiem
                           + (dieuKien.Count > 0 ? " WHERE " + string.Join(" AND ", dieuKien) : "")
                           + " ORDER BY hk.MaHanhKiem";

                DataTable bang = DatabaseHelper.LayBang(sql, thamSo.ToArray());

                grid.DataSource = bang;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách hạnh kiểm: " + ex.Message);
                return;
            }

            if (grid.Rows.Count == 0) LamMoiForm();
            else HienDongHienTai();
        }

        // Đổ dòng đang chọn trên lưới xuống khung nhập
        private void HienDongHienTai()
        {
            if (grid.CurrentRow == null) return;
            DataGridViewRow r = grid.CurrentRow;

            bool cu = _dangNap;
            _dangNap = true;
            try
            {
                txtMaHK.Text = r.Cells["MaHanhKiem"].Value?.ToString();
                cboHocSinh.SelectedValue = Convert.ToInt32(r.Cells["MaHS"].Value);
                cboLop.SelectedValue = Convert.ToInt32(r.Cells["MaLop"].Value);
                cboHocKy.SelectedValue = Convert.ToInt32(r.Cells["MaHocKy"].Value);
                cboXepLoai.SelectedItem = r.Cells["XepLoai"].Value?.ToString();
                txtNhanXet.Text = r.Cells["NhanXet"].Value?.ToString();
            }
            finally
            {
                _dangNap = cu;
            }
        }

        // ================================================================
        //  THÊM / SỬA / XOÁ
        // ================================================================
        private bool LayDuLieuForm(out int maHS, out int maLop, out int maHocKy, out string xepLoai, out string nhanXet)
        {
            maHS = maLop = maHocKy = 0;
            xepLoai = nhanXet = string.Empty;

            if (cboHocSinh.SelectedValue is not int hs ||
                cboLop.SelectedValue is not int lop ||
                cboHocKy.SelectedValue is not int hk)
            {
                MessageBox.Show("Vui lòng chọn Học sinh, Lớp và Học kỳ.");
                return false;
            }
            if (cboXepLoai.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Xếp loại.");
                return false;
            }

            maHS = hs; maLop = lop; maHocKy = hk;
            xepLoai = cboXepLoai.SelectedItem.ToString()!;
            nhanXet = txtNhanXet.Text.Trim();
            return true;
        }

        private void ThemMoi()
        {
            if (!LayDuLieuForm(out int maHS, out int maLop, out int maHocKy, out string xepLoai, out string nhanXet)) return;
            try
            {
                if (DaCoHanhKiem(maHS, maLop, maHocKy, null))
                {
                    MessageBox.Show(ThongBaoTrung);
                    return;
                }

                DatabaseHelper.ThucThi(
                    @"INSERT INTO tblHanhKiem (MaHS, MaLop, MaHocKy, XepLoai, NhanXet)
                      VALUES (@MaHS, @MaLop, @MaHocKy, @XepLoai, @NhanXet)",
                    new MySqlParameter("@MaHS", maHS),
                    new MySqlParameter("@MaLop", maLop),
                    new MySqlParameter("@MaHocKy", maHocKy),
                    new MySqlParameter("@XepLoai", xepLoai),
                    new MySqlParameter("@NhanXet", nhanXet));

                MessageBox.Show("Thêm hạnh kiểm thành công!");
                LamMoiForm();
                TaiDanhSach();
            }
            catch (Exception ex)
            {
                // Có cả lỗi SIGNAL từ thủ tục (VD: "Học sinh này đã có hạnh kiểm...")
                MessageBox.Show("Lỗi khi thêm: " + ex.Message);
            }
        }

        private void CapNhat()
        {
            if (!int.TryParse(txtMaHK.Text, out int maHanhKiem))
            {
                MessageBox.Show("Hãy chọn một dòng trong danh sách để sửa.");
                return;
            }
            if (!LayDuLieuForm(out int maHS, out int maLop, out int maHocKy, out string xepLoai, out string nhanXet)) return;

            try
            {
                if (DaCoHanhKiem(maHS, maLop, maHocKy, maHanhKiem))
                {
                    MessageBox.Show(ThongBaoTrung);
                    return;
                }

                DatabaseHelper.ThucThi(
                    @"UPDATE tblHanhKiem
                      SET MaHS = @MaHS, MaLop = @MaLop, MaHocKy = @MaHocKy,
                          XepLoai = @XepLoai, NhanXet = @NhanXet
                      WHERE MaHanhKiem = @MaHanhKiem",
                    new MySqlParameter("@MaHanhKiem", maHanhKiem),
                    new MySqlParameter("@MaHS", maHS),
                    new MySqlParameter("@MaLop", maLop),
                    new MySqlParameter("@MaHocKy", maHocKy),
                    new MySqlParameter("@XepLoai", xepLoai),
                    new MySqlParameter("@NhanXet", nhanXet));

                MessageBox.Show("Cập nhật thành công!");
                TaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật: " + ex.Message);
            }
        }

        private void Xoa()
        {
            if (!int.TryParse(txtMaHK.Text, out int maHanhKiem))
            {
                MessageBox.Show("Hãy chọn một dòng trong danh sách để xóa.");
                return;
            }

            DialogResult xacNhan = MessageBox.Show("Bạn có chắc muốn xóa hạnh kiểm này?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (xacNhan != DialogResult.Yes) return;

            try
            {
                DatabaseHelper.ThucThi(
                    "DELETE FROM tblHanhKiem WHERE MaHanhKiem = @MaHanhKiem",
                    new MySqlParameter("@MaHanhKiem", maHanhKiem));

                MessageBox.Show("Xóa thành công!");
                TaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message);
            }
        }

        // Kiểm tra trùng: 1 học sinh - 1 lớp - 1 học kỳ chỉ được có 1 dòng hạnh kiểm.
        // boQuaMa: khi sửa thì bỏ qua chính dòng đang sửa.
        private static bool DaCoHanhKiem(int maHS, int maLop, int maHocKy, int? boQuaMa)
        {
            string sql = @"SELECT COUNT(*) FROM tblHanhKiem
                           WHERE MaHS = @MaHS AND MaLop = @MaLop AND MaHocKy = @MaHocKy";
            List<MySqlParameter> thamSo = new List<MySqlParameter>
            {
                new MySqlParameter("@MaHS", maHS),
                new MySqlParameter("@MaLop", maLop),
                new MySqlParameter("@MaHocKy", maHocKy)
            };
            if (boQuaMa is int ma)
            {
                sql += " AND MaHanhKiem <> @BoQua";
                thamSo.Add(new MySqlParameter("@BoQua", ma));
            }

            DataTable bang = DatabaseHelper.LayBang(sql, thamSo.ToArray());
            return Convert.ToInt32(bang.Rows[0][0]) > 0;
        }

        // Xóa trắng khung nhập để nhập bản ghi mới
        private void LamMoiForm()
        {
            _dangNap = true;
            try
            {
                txtMaHK.Clear();
                txtNhanXet.Clear();
                if (cboHocSinh.Items.Count > 0) cboHocSinh.SelectedIndex = 0;
                if (cboLop.Items.Count > 0) cboLop.SelectedIndex = 0;
                if (cboHocKy.Items.Count > 0) cboHocKy.SelectedIndex = 0;
                cboXepLoai.SelectedIndex = 0;
            }
            finally
            {
                _dangNap = false;
            }
        }

        // Nút "Làm mới": bỏ hết bộ lọc, xóa khung nhập và tải lại danh sách
        private void LamMoi()
        {
            cboLocHocSinh.SelectedIndex = 0;
            cboLocLop.SelectedIndex = 0;
            cboLocHocKy.SelectedIndex = 0;
            cboLocXepLoai.SelectedIndex = 0;
            txtLocNhanXet.Clear();
            LamMoiForm();
            TaiDanhSach();
        }
    }
}
