using System;
using System.Data;
using System.IO;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace HanhKiemApp
{
    /// <summary>
    /// Lớp tiện ích kết nối MySQL và gọi stored procedure.
    /// GIỐNG HỆT bản dùng trong project thi bằng lái: Uid/Pwd đọc từ appsettings.json
    /// (file này nằm trong .gitignore, chỉ commit appsettings.example.json).
    /// Khi ghép vào project nhóm: nếu nhóm đã có DatabaseHelper thì dùng của nhóm,
    /// bỏ file này đi (các hàm GoiProcedureTraVeBang / GoiProcedureKhongTraVe dùng như nhau).
    /// </summary>
    public static class DatabaseHelper
    {
        private static string? _connectionString;

        private static string ConnectionString
        {
            get
            {
                _connectionString ??= TaoChuoiKetNoiTuFile();
                return _connectionString;
            }
        }

        private static string TaoChuoiKetNoiTuFile()
        {
            string duongDan = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(duongDan))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file appsettings.json.\n" +
                    "Hãy copy file 'appsettings.example.json' thành 'appsettings.json' " +
                    "rồi điền đúng Uid/Pwd MySQL của bạn vào đó.",
                    duongDan);
            }

            string noiDung = File.ReadAllText(duongDan);
            using JsonDocument doc = JsonDocument.Parse(noiDung);
            JsonElement db = doc.RootElement.GetProperty("Database");

            string server = db.GetProperty("Server").GetString() ?? "localhost";
            int port = db.GetProperty("Port").GetInt32();
            string tenDb = db.GetProperty("DatabaseName").GetString() ?? "QuanLyDiemTHCS_Test";
            string uid = db.GetProperty("Uid").GetString() ?? "root";
            string pwd = db.GetProperty("Pwd").GetString() ?? "";

            return $"Server={server};Port={port};Database={tenDb};Uid={uid};Pwd={pwd};";
        }

        private static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        /// <summary>Gọi stored procedure và trả về DataTable (dùng cho thủ tục "Xem").</summary>
        public static DataTable GoiProcedureTraVeBang(string tenProcedure, params MySqlParameter[] thamSo)
        {
            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = new MySqlCommand(tenProcedure, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (thamSo != null) cmd.Parameters.AddRange(thamSo);

            DataTable table = new DataTable();
            conn.Open();
            using MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(table);
            return table;
        }

        /// <summary>Gọi stored procedure không cần dữ liệu trả về (Thêm/Sửa/Xoá).</summary>
        public static void GoiProcedureKhongTraVe(string tenProcedure, params MySqlParameter[] thamSo)
        {
            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = new MySqlCommand(tenProcedure, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (thamSo != null) cmd.Parameters.AddRange(thamSo);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Chạy 1 câu SELECT (có tham số @TenThamSo) và trả về DataTable.
        /// Dùng khi KHÔNG muốn dùng stored procedure.
        /// </summary>
        public static DataTable LayBang(string sql, params MySqlParameter[] thamSo)
        {
            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            if (thamSo != null) cmd.Parameters.AddRange(thamSo);

            DataTable table = new DataTable();
            conn.Open();
            using MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(table);
            return table;
        }

        /// <summary>
        /// Chạy 1 câu INSERT / UPDATE / DELETE (có tham số @TenThamSo).
        /// </summary>
        public static void ThucThi(string sql, params MySqlParameter[] thamSo)
        {
            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            if (thamSo != null) cmd.Parameters.AddRange(thamSo);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        /// <summary>Kiểm tra kết nối CSDL, dùng khi mở chương trình.</summary>
        public static bool KiemTraKetNoi(out string loi)
        {
            try
            {
                using MySqlConnection conn = GetConnection();
                conn.Open();
                loi = string.Empty;
                return true;
            }
            catch (Exception ex) when (ex is not FileNotFoundException)
            {
                loi = ex.Message;
                return false;
            }
        }
    }
}
