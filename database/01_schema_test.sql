-- =========================================================
-- 01_schema_test.sql  —  CHỈ DÙNG ĐỂ TEST RIÊNG TRÊN MÁY BẠN
-- Tạo CSDL test gồm đúng các bảng mà phần Hạnh kiểm cần dùng.
-- Định nghĩa các bảng GIỐNG HỆT CSDL chung của nhóm (QuanLyDiemTHCS)
-- nên khi ghép code không phải sửa gì ở phía C#.
-- Chạy:  mysql -u root -p < 01_schema_test.sql
-- =========================================================

DROP DATABASE IF EXISTS QuanLyDiemTHCS_Test;
CREATE DATABASE QuanLyDiemTHCS_Test
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE QuanLyDiemTHCS_Test;

CREATE TABLE tblNamHoc (
    MaNamHoc    INT AUTO_INCREMENT PRIMARY KEY,
    TenNamHoc   VARCHAR(50) NOT NULL,
    NamBatDau   INT NOT NULL,
    NamKetThuc  INT NOT NULL,
    DangSuDung  TINYINT(1) NOT NULL DEFAULT 1
);

CREATE TABLE tblHocKy (
    MaHocKy    INT AUTO_INCREMENT PRIMARY KEY,
    TenHocKy   VARCHAR(50) NOT NULL,
    HeSoHocKy  DECIMAL(4,2) NOT NULL DEFAULT 1
);

CREATE TABLE tblNhomMonHoc (
    MaNhom   INT AUTO_INCREMENT PRIMARY KEY,
    TenNhom  VARCHAR(100) NOT NULL,
    MoTa     VARCHAR(255)
);

CREATE TABLE tblGiaoVien (
    ID             INT AUTO_INCREMENT PRIMARY KEY,
    HoTen          VARCHAR(100) NOT NULL,
    NgayBDLamViec  DATE,
    ID_NhomMon     INT,
    CONSTRAINT FK_GiaoVien_NhomMonHoc
        FOREIGN KEY (ID_NhomMon) REFERENCES tblNhomMonHoc(MaNhom)
);

CREATE TABLE tblLopHoc (
    MaLop        INT AUTO_INCREMENT PRIMARY KEY,
    TenLop       VARCHAR(50) NOT NULL,
    Khoi         INT,
    NgayBatDau   DATE,
    NgayKetThuc  DATE,
    NhomLop      VARCHAR(50),
    MaNamHoc     INT NOT NULL,
    ID_GVCN      INT NULL,
    CONSTRAINT FK_LopHoc_NamHoc
        FOREIGN KEY (MaNamHoc) REFERENCES tblNamHoc(MaNamHoc),
    CONSTRAINT FK_LopHoc_GiaoVien
        FOREIGN KEY (ID_GVCN) REFERENCES tblGiaoVien(ID)
);

CREATE TABLE tblHocSinh (
    MaHS         INT AUTO_INCREMENT PRIMARY KEY,
    HoTen        VARCHAR(100) NOT NULL,
    NgaySinh     DATE,
    GioiTinh     VARCHAR(10),
    DiaChi       VARCHAR(200),
    SoDienThoai  VARCHAR(15),
    Email        VARCHAR(100),
    TrangThai    TINYINT(1) NOT NULL DEFAULT 1
);

CREATE TABLE tblPhanLop (
    MaPhanLop    INT AUTO_INCREMENT PRIMARY KEY,
    MaHS         INT NOT NULL,
    MaLop        INT NOT NULL,
    NgayPhanLop  DATE DEFAULT (CURRENT_DATE),
    CONSTRAINT FK_PhanLop_HocSinh
        FOREIGN KEY (MaHS) REFERENCES tblHocSinh(MaHS),
    CONSTRAINT FK_PhanLop_LopHoc
        FOREIGN KEY (MaLop) REFERENCES tblLopHoc(MaLop)
);

CREATE TABLE tblHanhKiem (
    MaHanhKiem  INT AUTO_INCREMENT PRIMARY KEY,
    MaHS        INT NOT NULL,
    MaLop       INT NOT NULL,
    MaHocKy     INT NOT NULL,
    XepLoai     VARCHAR(50),
    NhanXet     VARCHAR(255),
    CONSTRAINT FK_HanhKiem_HocSinh
        FOREIGN KEY (MaHS) REFERENCES tblHocSinh(MaHS),
    CONSTRAINT FK_HanhKiem_LopHoc
        FOREIGN KEY (MaLop) REFERENCES tblLopHoc(MaLop),
    CONSTRAINT FK_HanhKiem_HocKy
        FOREIGN KEY (MaHocKy) REFERENCES tblHocKy(MaHocKy)
);

-- ===================== DỮ LIỆU MẪU =====================
INSERT INTO tblNamHoc (TenNamHoc, NamBatDau, NamKetThuc) VALUES ('2025-2026', 2025, 2026);

INSERT INTO tblHocKy (TenHocKy, HeSoHocKy) VALUES ('HK1', 1), ('HK2', 2);

INSERT INTO tblNhomMonHoc (TenNhom, MoTa) VALUES ('Khoa học tự nhiên', 'Toán, Lý, Hóa, Sinh');

INSERT INTO tblGiaoVien (HoTen, NgayBDLamViec, ID_NhomMon) VALUES
('Nguyễn Thị Hoa', '2015-09-01', 1),
('Trần Văn Minh',  '2018-09-01', 1),
('Lê Thị Thu',     '2012-09-01', 1),
('Phạm Văn Long',  '2010-09-01', 1);

INSERT INTO tblLopHoc (TenLop, Khoi, NgayBatDau, NgayKetThuc, NhomLop, MaNamHoc, ID_GVCN) VALUES
('6A1', 6, '2025-09-05', '2026-05-31', NULL, 1, 1),
('6A2', 6, '2025-09-05', '2026-05-31', NULL, 1, 2),
('6A3', 6, '2025-09-05', '2026-05-31', NULL, 1, 3),
('7A1', 7, '2025-09-05', '2026-05-31', NULL, 1, 4);

INSERT INTO tblHocSinh (HoTen, NgaySinh, GioiTinh, DiaChi, SoDienThoai, Email) VALUES
('Nguyễn Văn An',   '2013-03-10', 'Nam', 'Hà Nội', '0900000001', NULL),
('Trần Thị Bích',   '2013-07-21', 'Nữ',  'Hà Nội', '0900000002', NULL),
('Lê Minh Cường',   '2013-01-15', 'Nam', 'Hà Nội', '0900000003', NULL),
('Phạm Thị Dung',   '2013-11-02', 'Nữ',  'Hà Nội', '0900000004', NULL),
('Hoàng Văn Em',    '2013-05-30', 'Nam', 'Hà Nội', '0900000005', NULL),
('Vũ Thị Hương',    '2013-09-18', 'Nữ',  'Hà Nội', '0900000006', NULL),
('Đặng Quốc Khánh', '2012-04-12', 'Nam', 'Hà Nội', '0900000007', NULL),
('Ngô Thị Lan',     '2012-08-25', 'Nữ',  'Hà Nội', '0900000008', NULL);

INSERT INTO tblPhanLop (MaHS, MaLop) VALUES
(1, 1), (2, 1), (3, 2), (4, 2), (5, 3), (6, 3), (7, 4), (8, 4);

INSERT INTO tblHanhKiem (MaHS, MaLop, MaHocKy, XepLoai, NhanXet) VALUES
(1, 1, 1, 'Tốt', 'Chăm ngoan, lễ phép, có ý thức học tập.'),
(2, 1, 1, 'Khá', 'Học tập tốt, tích cực tham gia hoạt động.'),
(3, 2, 1, 'Tốt', 'Ngoan ngoãn, đoàn kết với bạn bè.'),
(4, 2, 1, 'Khá', 'Cần cố gắng hơn trong việc giữ trật tự.'),
(5, 3, 1, 'Tốt', 'Tham gia tốt các hoạt động của lớp.'),
(6, 3, 1, 'Khá', 'Học tập ổn định, có tiến bộ.'),
(7, 4, 1, 'Tốt', 'Nghiêm túc, có trách nhiệm.'),
(8, 4, 1, 'Khá', 'Hòa đồng, lễ phép.');
