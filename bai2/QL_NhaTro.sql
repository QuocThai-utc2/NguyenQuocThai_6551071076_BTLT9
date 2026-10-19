IF DB_ID('QL_NhaTro') IS NULL CREATE DATABASE QL_NhaTro;
GO
USE QL_NhaTro;
GO
IF OBJECT_ID('dbo.KhachThue') IS NOT NULL DROP TABLE dbo.KhachThue;
IF OBJECT_ID('dbo.PhongTro') IS NOT NULL DROP TABLE dbo.PhongTro;
GO
CREATE TABLE dbo.PhongTro (
    MaPhong   INT IDENTITY(1,1) PRIMARY KEY,
    TenPhong  NVARCHAR(20)  NOT NULL,
    GiaThue   DECIMAL(18,0) NOT NULL,
    TinhTrang NVARCHAR(20)  NOT NULL DEFAULT N'Trống'   -- N'Trống' / N'Đang thuê'
);
CREATE TABLE dbo.KhachThue (
    MaKhach        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen          NVARCHAR(100) NOT NULL,
    GioiTinh       NVARCHAR(5)   NOT NULL,
    NgaySinh       DATE          NOT NULL,
    SoDienThoai    VARCHAR(15)   NULL,
    MaTinh         INT           NULL,
    TenTinh        NVARCHAR(100) NULL,
    MaPhuongXa     INT           NULL,
    TenPhuongXa    NVARCHAR(100) NULL,
    DaDangKyTamTru BIT           NOT NULL DEFAULT 0,
    MaPhong        INT           NOT NULL,
    CONSTRAINT FK_KhachThue_PhongTro FOREIGN KEY (MaPhong) REFERENCES dbo.PhongTro(MaPhong)
);
GO
INSERT INTO dbo.PhongTro (TenPhong, GiaThue, TinhTrang) VALUES
(N'P01', 1500000, N'Đang thuê'), (N'P02', 1600000, N'Đang thuê'), (N'P03', 1800000, N'Đang thuê'),
(N'P04', 2000000, N'Đang thuê'), (N'P05', 1700000, N'Đang thuê'), (N'P06', 1900000, N'Trống');
GO
INSERT INTO dbo.KhachThue (HoTen, GioiTinh, NgaySinh, SoDienThoai, MaTinh, TenTinh, MaPhuongXa, TenPhuongXa, DaDangKyTamTru, MaPhong) VALUES
(N'Nguyễn Văn An',   N'Nam', '20040512', '0912345678', 1,  N'Thành phố Hà Nội', NULL, N'Phường Ba Đình', 1, (SELECT MaPhong FROM dbo.PhongTro WHERE TenPhong=N'P03')),
(N'Nguyễn Văn Bảo',  N'Nam', '20030820', '0912345679', 1,  N'Thành phố Hà Nội', NULL, N'Phường Cửa Nam', 0, (SELECT MaPhong FROM dbo.PhongTro WHERE TenPhong=N'P03')),
(N'Trần Thị Bình',   N'Nữ',  '20010305', '0876543210', 37, N'Tỉnh Ninh Bình',   NULL, N'Phường Nam Định', 0, (SELECT MaPhong FROM dbo.PhongTro WHERE TenPhong=N'P01')),
(N'Lê Văn Cường',    N'Nam', '19981111', '0901234567', 38, N'Tỉnh Thanh Hóa',   NULL, N'Phường Hạc Thành', 0, (SELECT MaPhong FROM dbo.PhongTro WHERE TenPhong=N'P02')),
(N'Phạm Minh Đức',   N'Nam', '20000707', '0987654321', 38, N'Tỉnh Thanh Hóa',   NULL, N'Phường Hạc Thành', 1, (SELECT MaPhong FROM dbo.PhongTro WHERE TenPhong=N'P04')),
(N'Hoàng Thị Dung',  N'Nữ',  '20020214', '0765432109', 27, N'Tỉnh Bắc Ninh',    NULL, N'Phường Bắc Giang', 0, (SELECT MaPhong FROM dbo.PhongTro WHERE TenPhong=N'P05'));
GO
