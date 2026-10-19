IF DB_ID('QL_DaNgoai') IS NULL CREATE DATABASE QL_DaNgoai;
GO
USE QL_DaNgoai;
GO
IF OBJECT_ID('dbo.ChuyenDi') IS NOT NULL DROP TABLE dbo.ChuyenDi;
GO
CREATE TABLE dbo.ChuyenDi (
    MaChuyen    INT IDENTITY(1,1) PRIMARY KEY,
    TenChuyen   NVARCHAR(100) NOT NULL,
    DiaDiem     NVARCHAR(100) NOT NULL,
    NgayDi      DATE          NOT NULL,
    SoNguoi     INT           NOT NULL,
    NganSach    DECIMAL(18,0) NOT NULL,
    PhuongTien  NVARCHAR(30)  NOT NULL,
    DaHoanThanh BIT           NOT NULL DEFAULT 0,
    GhiChu      NVARCHAR(200) NULL
);
GO
-- Ngày đi tính từ hôm nay để luôn nằm trong vùng dự báo 16 ngày (trừ chuyến cuối)
DECLARE @t DATE = CAST(GETDATE() AS DATE);
INSERT INTO dbo.ChuyenDi (TenChuyen, DiaDiem, NgayDi, SoNguoi, NganSach, PhuongTien, DaHoanThanh, GhiChu) VALUES
(N'Cắm trại đồi chè',     N'Đà Lạt',   DATEADD(DAY,3,@t),  6, 3500000, N'Ô tô',     0, NULL),
(N'Săn mây Sa Pa',        N'Sa Pa',    DATEADD(DAY,5,@t),  4, 5000000, N'Xe khách', 0, N'Mang áo ấm'),
(N'Tắm biển cuối tuần',   N'Vũng Tàu', DATEADD(DAY,7,@t),  8, 4000000, N'Ô tô',     0, NULL),
(N'Phố cổ Hội An',        N'Hội An',   DATEADD(DAY,9,@t),  3, 6000000, N'Tàu hỏa',  0, NULL),
(N'Lặn ngắm san hô',      N'Nha Trang',DATEADD(DAY,12,@t), 5, 7000000, N'Tàu hỏa',  0, NULL),
(N'Du lịch Phú Quốc',     N'Phú Quốc', DATEADD(DAY,14,@t), 4, 9000000, N'Ô tô',     0, NULL),
(N'Thăm Cố đô',           N'Huế',      DATEADD(DAY,18,@t), 2, 3000000, N'Xe máy',   0, N'Ngoài vùng dự báo 16 ngày'),
(N'Chuyến Hà Giang cũ',   N'Hà Giang', DATEADD(DAY,-20,@t),5, 6500000, N'Xe máy',   1, N'Đã hoàn thành');
GO
