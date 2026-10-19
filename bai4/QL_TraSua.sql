IF DB_ID('QL_TraSua') IS NULL CREATE DATABASE QL_TraSua;
GO
USE QL_TraSua;
GO
IF OBJECT_ID('dbo.ChiTietHoaDon') IS NOT NULL DROP TABLE dbo.ChiTietHoaDon;
IF OBJECT_ID('dbo.HoaDon')        IS NOT NULL DROP TABLE dbo.HoaDon;
IF OBJECT_ID('dbo.MonUong')       IS NOT NULL DROP TABLE dbo.MonUong;
IF OBJECT_ID('dbo.DanhMucMon')    IS NOT NULL DROP TABLE dbo.DanhMucMon;
GO
CREATE TABLE dbo.DanhMucMon (
    MaDM  INT IDENTITY(1,1) PRIMARY KEY,
    TenDM NVARCHAR(50) NOT NULL
);
CREATE TABLE dbo.MonUong (
    MaMon   INT IDENTITY(1,1) PRIMARY KEY,
    TenMon  NVARCHAR(100) NOT NULL,
    GiaBan  DECIMAL(18,0) NOT NULL,
    DangBan BIT NOT NULL DEFAULT 1,
    MaDM    INT NOT NULL,
    CONSTRAINT FK_MonUong_DanhMuc FOREIGN KEY (MaDM) REFERENCES dbo.DanhMucMon(MaDM)
);
CREATE TABLE dbo.HoaDon (
    MaHD        INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap     DATETIME      NOT NULL,
    TongTien    DECIMAL(18,0) NOT NULL,
    HinhThucTT  NVARCHAR(20)  NOT NULL,   -- N'Tiền mặt' / N'Chuyển khoản'
    DaThanhToan BIT           NOT NULL
);
CREATE TABLE dbo.ChiTietHoaDon (
    MaHD   INT NOT NULL,
    MaMon  INT NOT NULL,
    SoLy   INT NOT NULL,
    DonGia DECIMAL(18,0) NOT NULL,
    CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHD, MaMon),
    CONSTRAINT FK_CT_HoaDon FOREIGN KEY (MaHD)  REFERENCES dbo.HoaDon(MaHD),
    CONSTRAINT FK_CT_Mon    FOREIGN KEY (MaMon) REFERENCES dbo.MonUong(MaMon)
);
GO
INSERT INTO dbo.DanhMucMon (TenDM) VALUES (N'Trà sữa'), (N'Trà trái cây'), (N'Topping'), (N'Đồ ăn vặt');
GO
INSERT INTO dbo.MonUong (TenMon, GiaBan, DangBan, MaDM)
SELECT v.TenMon, v.GiaBan, v.DangBan, d.MaDM
FROM (VALUES
 (N'Trà sữa trân châu', 35000, 1, N'Trà sữa'),
 (N'Trà sữa matcha',    40000, 1, N'Trà sữa'),
 (N'Hồng trà sữa',      32000, 1, N'Trà sữa'),
 (N'Trà sữa khoai môn', 38000, 1, N'Trà sữa'),
 (N'Trà sữa socola',    38000, 0, N'Trà sữa'),
 (N'Trà đào cam sả',    42000, 1, N'Trà trái cây'),
 (N'Trà vải',           40000, 1, N'Trà trái cây'),
 (N'Trà dâu tây',       45000, 1, N'Trà trái cây'),
 (N'Trân châu trắng',    5000, 1, N'Topping'),
 (N'Thạch dừa',          5000, 1, N'Topping'),
 (N'Pudding trứng',      8000, 1, N'Topping'),
 (N'Khoai tây chiên',   30000, 1, N'Đồ ăn vặt'),
 (N'Gà viên chiên',     35000, 1, N'Đồ ăn vặt'),
 (N'Bánh tráng trộn',   25000, 1, N'Đồ ăn vặt')
) AS v(TenMon, GiaBan, DangBan, TenDM)
JOIN dbo.DanhMucMon d ON d.TenDM = v.TenDM;
GO
-- 2 hóa đơn mẫu
INSERT INTO dbo.HoaDon (NgayLap, TongTien, HinhThucTT, DaThanhToan) VALUES (GETDATE(), 100000, N'Tiền mặt', 1);
INSERT INTO dbo.ChiTietHoaDon (MaHD, MaMon, SoLy, DonGia)
SELECT SCOPE_IDENTITY(), MaMon, 2, 35000 FROM dbo.MonUong WHERE TenMon = N'Trà sữa trân châu';
INSERT INTO dbo.ChiTietHoaDon (MaHD, MaMon, SoLy, DonGia)
SELECT (SELECT MAX(MaHD) FROM dbo.HoaDon), MaMon, 1, 30000 FROM dbo.MonUong WHERE TenMon = N'Khoai tây chiên';

INSERT INTO dbo.HoaDon (NgayLap, TongTien, HinhThucTT, DaThanhToan) VALUES (GETDATE(), 124000, N'Chuyển khoản', 1);
INSERT INTO dbo.ChiTietHoaDon (MaHD, MaMon, SoLy, DonGia)
SELECT (SELECT MAX(MaHD) FROM dbo.HoaDon), MaMon, 1, 40000 FROM dbo.MonUong WHERE TenMon = N'Trà sữa matcha';
INSERT INTO dbo.ChiTietHoaDon (MaHD, MaMon, SoLy, DonGia)
SELECT (SELECT MAX(MaHD) FROM dbo.HoaDon), MaMon, 2, 42000 FROM dbo.MonUong WHERE TenMon = N'Trà đào cam sả';
GO
