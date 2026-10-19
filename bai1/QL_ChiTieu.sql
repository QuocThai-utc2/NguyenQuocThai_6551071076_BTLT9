IF DB_ID('QL_ChiTieu') IS NULL CREATE DATABASE QL_ChiTieu;
GO
USE QL_ChiTieu;
GO
IF OBJECT_ID('dbo.ChiTieu') IS NOT NULL DROP TABLE dbo.ChiTieu;
GO
CREATE TABLE dbo.ChiTieu (
    MaCT     INT IDENTITY(1,1) PRIMARY KEY,
    NgayChi  DATE          NOT NULL,
    NoiDung  NVARCHAR(100) NOT NULL,
    LoaiChi  NVARCHAR(30)  NOT NULL,
    SoTien   DECIMAL(18,0) NOT NULL,
    GhiChu   NVARCHAR(200) NULL
);
GO
-- Dữ liệu mẫu: lấy ngày theo tháng hiện tại để luôn nằm trong bộ lọc mặc định
DECLARE @d DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
INSERT INTO dbo.ChiTieu (NgayChi, NoiDung, LoaiChi, SoTien, GhiChu) VALUES
(@d,                 N'Cơm trưa',           N'Ăn uống', 35000,  NULL),
(DATEADD(DAY,1,@d),  N'Xăng xe',            N'Đi lại',  60000,  NULL),
(DATEADD(DAY,2,@d),  N'Photo giáo trình',   N'Học tập', 25000,  NULL),
(DATEADD(DAY,3,@d),  N'Xem phim',           N'Giải trí',90000,  N'Đi với nhóm bạn'),
(DATEADD(DAY,4,@d),  N'Trà sữa',            N'Ăn uống', 40000,  NULL),
(DATEADD(DAY,5,@d),  N'Mua đồ dùng',        N'Khác',    150000, NULL),
(DATEADD(DAY,6,@d),  N'Tiền điện',          N'Khác',    500000, N'Hóa đơn tháng trước'),
(DATEADD(DAY,7,@d),  N'Ăn sáng',            N'Ăn uống', 25000,  NULL),
(DATEADD(DAY,8,@d),  N'Gửi xe tháng',       N'Đi lại',  50000,  NULL),
(DATEADD(DAY,9,@d),  N'Mua sách tiếng Anh', N'Học tập', 180000, NULL),
(DATEADD(DAY,10,@d), N'Cà phê với bạn',     N'Ăn uống', 45000,  NULL),
(DATEADD(DAY,11,@d), N'Vé xe buýt',         N'Đi lại',  30000,  NULL);
GO
