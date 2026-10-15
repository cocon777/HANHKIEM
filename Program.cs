using System;
using System.Windows.Forms;
using HanhKiemApp.Forms;

namespace HanhKiemApp
{
    internal static class Program
    {
        // Điểm bắt đầu (entry point) của chương trình test riêng phần Hạnh kiểm
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Kiểm tra kết nối CSDL trước, lỗi thì báo ngay cho dễ sửa
            try
            {
                if (!DatabaseHelper.KiemTraKetNoi(out string loi))
                {
                    MessageBox.Show(
                        "Không kết nối được CSDL MySQL.\n\n" +
                        "Hãy kiểm tra file appsettings.json (Server, Uid, Pwd, DatabaseName).\n\n" +
                        "Chi tiết lỗi: " + loi,
                        "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                // VD: chưa có file appsettings.json
                MessageBox.Show(ex.Message, "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.Run(new frmHanhKiem());
        }
    }
}
