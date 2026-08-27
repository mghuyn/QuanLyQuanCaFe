using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using System.Windows.Media.Imaging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Drawing.Printing;

namespace QuanLyQuanCaFe.Services
{
    public class ReportExportService
    {
        private string GetReportRootFolder()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string GetBaoCaoFolder()
        {
            string folder = Path.Combine(GetReportRootFolder(), "BaoCao");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string GetHoaDonFolder()
        {
            string folder = Path.Combine(GetReportRootFolder(), "HoaDon");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string SafeXml(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&apos;");
        }

        private string SafeText(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value;
        }

        private string SafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "HD";
            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value.Trim();
        }

        private void OpenFile(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch
            {
                // File vẫn đã lưu thành công nếu máy không tự mở được.
            }
        }

        private string Money(decimal value)
        {
            return value.ToString("#,##0", new CultureInfo("vi-VN")) + " đ";
        }

        private void AppendExcelStyles(StringBuilder sb)
        {
            sb.AppendLine("<Styles>");
            sb.AppendLine("<Style ss:ID=\"Default\" ss:Name=\"Normal\"><Alignment ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\"/><Interior/><NumberFormat/><Protection/></Style>");
            sb.AppendLine("<Style ss:ID=\"Title\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"18\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#0F172A\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine("<Style ss:ID=\"SubTitle\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"11\" ss:Color=\"#475569\"/><Interior ss:Color=\"#E2E8F0\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine("<Style ss:ID=\"Section\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"12\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#2563EB\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine("<Style ss:ID=\"Header\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#1D4ED8\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#93C5FD\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#93C5FD\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#93C5FD\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#93C5FD\"/></Borders></Style>");
            sb.AppendLine("<Style ss:ID=\"Text\"><Alignment ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Color=\"#0F172A\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E2E8F0\"/></Borders></Style>");
            sb.AppendLine("<Style ss:ID=\"TextCenter\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Color=\"#0F172A\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E2E8F0\"/></Borders></Style>");
            sb.AppendLine("<Style ss:ID=\"Money\"><Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Color=\"#0F172A\"/><NumberFormat ss:Format=\"#,##0 &quot;đ&quot;\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E2E8F0\"/></Borders></Style>");
            sb.AppendLine("<Style ss:ID=\"Number\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Color=\"#0F172A\"/><NumberFormat ss:Format=\"#,##0\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E2E8F0\"/></Borders></Style>");
            sb.AppendLine("<Style ss:ID=\"Label\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#334155\"/><Interior ss:Color=\"#F1F5F9\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine("<Style ss:ID=\"Total\"><Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"12\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#16A34A\" ss:Pattern=\"Solid\"/><NumberFormat ss:Format=\"#,##0 &quot;đ&quot;\"/></Style>");
            sb.AppendLine("</Styles>");
        }

        private void StartWorksheet(StringBuilder sb, string name, params double[] columnWidths)
        {
            sb.AppendLine("<Worksheet ss:Name=\"" + SafeXml(name) + "\">");
            sb.AppendLine("<Table ss:DefaultRowHeight=\"22\">");
            foreach (double width in columnWidths)
                sb.AppendLine("<Column ss:Width=\"" + width.ToString(CultureInfo.InvariantCulture) + "\"/>");
        }

        private void EndWorksheet(StringBuilder sb)
        {
            sb.AppendLine("</Table>");
            sb.AppendLine("<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\"><FreezePanes/><FrozenNoSplit/><SplitHorizontal>1</SplitHorizontal><TopRowBottomPane>1</TopRowBottomPane><ProtectObjects>False</ProtectObjects><ProtectScenarios>False</ProtectScenarios></WorksheetOptions>");
            sb.AppendLine("</Worksheet>");
        }

        private void AppendCell(StringBuilder sb, string value, string style = "Text")
        {
            sb.Append("<Cell ss:StyleID=\"").Append(style).Append("\"><Data ss:Type=\"String\">");
            sb.Append(SafeXml(value));
            sb.AppendLine("</Data></Cell>");
        }

        private void AppendNumberCell(StringBuilder sb, decimal value, string style = "Money")
        {
            sb.Append("<Cell ss:StyleID=\"").Append(style).Append("\"><Data ss:Type=\"Number\">");
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("</Data></Cell>");
        }

        private void AppendIntCell(StringBuilder sb, int value, string style = "Number")
        {
            sb.Append("<Cell ss:StyleID=\"").Append(style).Append("\"><Data ss:Type=\"Number\">");
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("</Data></Cell>");
        }

        private void AppendRow(StringBuilder sb, string style, params string[] cells)
        {
            sb.AppendLine("<Row>");
            foreach (var cell in cells) AppendCell(sb, cell, style);
            sb.AppendLine("</Row>");
        }

        private void AppendMergedTitle(StringBuilder sb, string title, int mergeAcross)
        {
            sb.AppendLine("<Row ss:Height=\"34\">");
            sb.Append("<Cell ss:MergeAcross=\"").Append(mergeAcross).Append("\" ss:StyleID=\"Title\"><Data ss:Type=\"String\">");
            sb.Append(SafeXml(title));
            sb.AppendLine("</Data></Cell>");
            sb.AppendLine("</Row>");
        }

        private void AppendMergedSubTitle(StringBuilder sb, string title, int mergeAcross)
        {
            sb.AppendLine("<Row ss:Height=\"24\">");
            sb.Append("<Cell ss:MergeAcross=\"").Append(mergeAcross).Append("\" ss:StyleID=\"SubTitle\"><Data ss:Type=\"String\">");
            sb.Append(SafeXml(title));
            sb.AppendLine("</Data></Cell>");
            sb.AppendLine("</Row>");
        }

        public void XuatBaoCaoDoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime start = tuNgay.Date;
                DateTime end = denNgay.Date.AddDays(1);

                var hoaDons = db.HoaDonBans
                    .Where(x => x.NgayLapHoaDon >= start
                             && x.NgayLapHoaDon < end
                             && x.TrangThaiHoaDon != "CANCELLED")
                    .ToList();

                var chiTiets = db.ChiTietHoaDonBans
                    .Where(x => x.HoaDonBan.NgayLapHoaDon >= start
                             && x.HoaDonBan.NgayLapHoaDon < end
                             && x.HoaDonBan.TrangThaiHoaDon != "CANCELLED")
                    .ToList();

                var theoNgay = hoaDons
                    .GroupBy(x => x.NgayLapHoaDon.Date)
                    .Select(g => new
                    {
                        Ngay = g.Key,
                        SoHoaDon = g.Count(),
                        DoanhThu = g.Sum(x => x.TongTien)
                    })
                    .OrderBy(x => x.Ngay)
                    .ToList();

                var sanPhamBanChay = chiTiets
                    .GroupBy(x => x.BienTheSanPham != null && x.BienTheSanPham.SanPham != null ? x.BienTheSanPham.SanPham.TenSanPham : "Không xác định")
                    .Select(g => new
                    {
                        TenSanPham = g.Key,
                        SoLuong = g.Sum(x => x.SoLuong),
                        DoanhThu = g.Sum(x => x.ThanhTien ?? (x.SoLuong * x.DonGia - x.TienGiam))
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ToList();

                var theoDanhMuc = chiTiets
                    .GroupBy(x => x.BienTheSanPham != null && x.BienTheSanPham.SanPham != null && x.BienTheSanPham.SanPham.DanhMucSanPham != null
                        ? x.BienTheSanPham.SanPham.DanhMucSanPham.TenDanhMuc
                        : "Không xác định")
                    .Select(g => new
                    {
                        DanhMuc = g.Key,
                        SoLuong = g.Sum(x => x.SoLuong),
                        DoanhThu = g.Sum(x => x.ThanhTien ?? (x.SoLuong * x.DonGia - x.TienGiam))
                    })
                    .OrderByDescending(x => x.DoanhThu)
                    .ToList();

                decimal tongDoanhThu = hoaDons.Sum(x => x.TongTien);
                int tongHoaDon = hoaDons.Count;
                int tongSanPham = chiTiets.Sum(x => x.SoLuong);
                decimal trungBinhHoaDon = tongHoaDon > 0 ? tongDoanhThu / tongHoaDon : 0;

                var sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
                sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
                AppendExcelStyles(sb);

                StartWorksheet(sb, "TongQuan", 230, 200);
                AppendMergedTitle(sb, "BÁO CÁO DOANH THU", 1);
                AppendMergedSubTitle(sb, "Từ " + tuNgay.ToString("dd/MM/yyyy") + " đến " + denNgay.ToString("dd/MM/yyyy") + " | Xuất lúc " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), 1);
                sb.AppendLine("<Row/>");
                AppendRow(sb, "Header", "Chỉ tiêu", "Giá trị");
                sb.AppendLine("<Row>"); AppendCell(sb, "Tổng hóa đơn", "Label"); AppendIntCell(sb, tongHoaDon); sb.AppendLine("</Row>");
                sb.AppendLine("<Row>"); AppendCell(sb, "Tổng sản phẩm bán ra", "Label"); AppendIntCell(sb, tongSanPham); sb.AppendLine("</Row>");
                sb.AppendLine("<Row>"); AppendCell(sb, "Doanh thu trung bình / hóa đơn", "Label"); AppendNumberCell(sb, trungBinhHoaDon); sb.AppendLine("</Row>");
                sb.AppendLine("<Row>"); AppendCell(sb, "Tổng doanh thu", "Label"); AppendNumberCell(sb, tongDoanhThu, "Total"); sb.AppendLine("</Row>");
                EndWorksheet(sb);

                StartWorksheet(sb, "DoanhThuTheoNgay", 120, 110, 150);
                AppendMergedTitle(sb, "DOANH THU THEO NGÀY", 2);
                AppendRow(sb, "Header", "Ngày", "Số hóa đơn", "Doanh thu");
                foreach (var item in theoNgay)
                {
                    sb.AppendLine("<Row>");
                    AppendCell(sb, item.Ngay.ToString("dd/MM/yyyy"), "TextCenter");
                    AppendIntCell(sb, item.SoHoaDon);
                    AppendNumberCell(sb, item.DoanhThu);
                    sb.AppendLine("</Row>");
                }
                EndWorksheet(sb);

                StartWorksheet(sb, "SanPhamBanChay", 260, 110, 150);
                AppendMergedTitle(sb, "SẢN PHẨM BÁN CHẠY", 2);
                AppendRow(sb, "Header", "Sản phẩm", "Số lượng", "Doanh thu");
                foreach (var item in sanPhamBanChay)
                {
                    sb.AppendLine("<Row>");
                    AppendCell(sb, item.TenSanPham);
                    AppendIntCell(sb, item.SoLuong);
                    AppendNumberCell(sb, item.DoanhThu);
                    sb.AppendLine("</Row>");
                }
                EndWorksheet(sb);

                StartWorksheet(sb, "DoanhThuDanhMuc", 240, 110, 150);
                AppendMergedTitle(sb, "DOANH THU THEO DANH MỤC", 2);
                AppendRow(sb, "Header", "Danh mục", "Số lượng", "Doanh thu");
                foreach (var item in theoDanhMuc)
                {
                    sb.AppendLine("<Row>");
                    AppendCell(sb, item.DanhMuc);
                    AppendIntCell(sb, item.SoLuong);
                    AppendNumberCell(sb, item.DoanhThu);
                    sb.AppendLine("</Row>");
                }
                EndWorksheet(sb);

                StartWorksheet(sb, "HoaDon", 120, 140, 150, 160, 150, 120, 120, 140);
                AppendMergedTitle(sb, "DANH SÁCH HÓA ĐƠN", 7);
                AppendRow(sb, "Header", "Mã hóa đơn", "Ngày lập", "Bàn/Hình thức", "Khách hàng", "Thu ngân", "Trạng thái", "Thanh toán", "Tổng tiền");
                foreach (var hd in hoaDons.OrderBy(x => x.NgayLapHoaDon))
                {
                    sb.AppendLine("<Row>");
                    AppendCell(sb, hd.MaHoaDon, "TextCenter");
                    AppendCell(sb, hd.NgayLapHoaDon.ToString("dd/MM/yyyy HH:mm"), "TextCenter");
                    AppendCell(sb, hd.BanCafe != null ? hd.BanCafe.TenBan : hd.LoaiHoaDon);
                    AppendCell(sb, hd.KhachHang != null ? hd.KhachHang.HoTen : "Khách lẻ");
                    AppendCell(sb, hd.NhanVien != null ? hd.NhanVien.HoTen : "");
                    AppendCell(sb, hd.TrangThaiHoaDon, "TextCenter");
                    AppendCell(sb, hd.TrangThaiThanhToan, "TextCenter");
                    AppendNumberCell(sb, hd.TongTien);
                    sb.AppendLine("</Row>");
                }
                EndWorksheet(sb);

                sb.AppendLine("</Workbook>");

                string fileName = "BaoCaoDoanhThu_" + tuNgay.ToString("yyyyMMdd") + "_" + denNgay.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("HHmmss") + ".xls";
                string path = Path.Combine(GetBaoCaoFolder(), fileName);
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                OpenFile(path);
            }
        }

        public void XuatHoaDonBan(int maHoaDonBan)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var hd = db.HoaDonBans.FirstOrDefault(x => x.MaHoaDonBan == maHoaDonBan);
                if (hd == null) throw new Exception("Không tìm thấy hóa đơn cần in.");

                var items = db.ChiTietHoaDonBans
                    .Where(x => x.MaHoaDonBan == maHoaDonBan)
                    .ToList();

                SaveInvoiceText(hd, items);
                string pdfPath = SaveInvoicePdfAuto(hd, items);
                if (!string.IsNullOrWhiteSpace(pdfPath) && File.Exists(pdfPath))
                {
                    OpenFile(pdfPath);
                }
                else
                {
                    MessageBox.Show("Không tạo được file PDF hóa đơn. Ứng dụng đã lưu bản TXT trong thư mục Reports/HoaDon.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SaveInvoiceText(HoaDonBan hd, System.Collections.Generic.List<ChiTietHoaDonBan> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("HÓA ĐƠN BÁN HÀNG");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Mã hóa đơn: " + SafeText(hd.MaHoaDon));
            sb.AppendLine("Ngày lập: " + hd.NgayLapHoaDon.ToString("dd/MM/yyyy HH:mm"));
            sb.AppendLine("Bàn/Hình thức: " + SafeText(hd.BanCafe != null ? hd.BanCafe.TenBan : hd.LoaiHoaDon));
            sb.AppendLine("Khách hàng: " + SafeText(hd.KhachHang != null ? hd.KhachHang.HoTen : "Khách lẻ"));
            sb.AppendLine("Thu ngân: " + SafeText(hd.NhanVien != null ? hd.NhanVien.HoTen : ""));
            sb.AppendLine("Thanh toán: " + SafeText(hd.PhuongThucThanhToan));
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("MÓN | SL | ĐƠN GIÁ | THÀNH TIỀN");

            foreach (var item in items)
            {
                string ten = item.BienTheSanPham != null && item.BienTheSanPham.SanPham != null ? item.BienTheSanPham.SanPham.TenSanPham : "";
                decimal thanhTien = item.ThanhTien ?? (item.SoLuong * item.DonGia - item.TienGiam);
                sb.AppendLine(ten + " | " + item.SoLuong + " | " + item.DonGia.ToString("N0") + " | " + thanhTien.ToString("N0"));
                if (!string.IsNullOrWhiteSpace(item.YeuCauDacBiet)) sb.AppendLine("  Ghi chú: " + item.YeuCauDacBiet);
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Tiền hàng: " + Money(hd.TienHang));
            sb.AppendLine("Giảm giá: " + Money(hd.TienGiam));
            sb.AppendLine("Tổng tiền: " + Money(hd.TongTien));
            sb.AppendLine("Khách trả: " + Money(hd.TienKhachTra));
            sb.AppendLine("Tiền thừa: " + Money(hd.TienThua));
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Cảm ơn quý khách!");

            // Lưu tự động theo mã hóa đơn, không yêu cầu người dùng nhập tên file.
            string fileName = "HoaDon_" + SafeFileName(hd.MaHoaDon) + ".txt";
            string path = Path.Combine(GetHoaDonFolder(), fileName);
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private string SaveInvoicePdfAuto(HoaDonBan hd, System.Collections.Generic.List<ChiTietHoaDonBan> items)
        {
            string baseName = "HoaDon_" + SafeFileName(hd.MaHoaDon);
            string pdfPath = Path.Combine(GetHoaDonFolder(), baseName + ".pdf");

            // Nếu file cũ đang mở thì tạo file mới, tránh lỗi file đang bị khóa.
            if (File.Exists(pdfPath))
            {
                pdfPath = Path.Combine(
                    GetHoaDonFolder(),
                    baseName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf"
                );
            }

            // Không render WPF raw nữa vì một số PDF viewer hiển thị alpha thành đen.
            // Vẽ bill trực tiếp thành JPEG nền trắng rồi nhúng JPEG vào PDF.
            SaveInvoicePdfByGdi(hd, items, pdfPath);
            return pdfPath;
        }

        private void SaveInvoicePdfByGdi(HoaDonBan hd, System.Collections.Generic.List<ChiTietHoaDonBan> items, string pdfPath)
        {
            string tempJpeg = Path.Combine(GetHoaDonFolder(), "_tmp_" + SafeFileName(hd.MaHoaDon) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");

            using (var image = CreateInvoiceImage(hd, items))
            {
                var jpgEncoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                    .FirstOrDefault(x => x.MimeType == "image/jpeg");

                if (jpgEncoder != null)
                {
                    using (var encParams = new System.Drawing.Imaging.EncoderParameters(1))
                    {
                        encParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                        image.Save(tempJpeg, jpgEncoder, encParams);
                    }
                }
                else
                {
                    image.Save(tempJpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }

            byte[] jpgBytes = File.ReadAllBytes(tempJpeg);

            using (var tmp = System.Drawing.Image.FromFile(tempJpeg))
            {
                WriteJpegPdf(pdfPath, jpgBytes, tmp.Width, tmp.Height);
            }

            try { File.Delete(tempJpeg); } catch { }
        }

        private System.Drawing.Bitmap CreateInvoiceImage(HoaDonBan hd, System.Collections.Generic.List<ChiTietHoaDonBan> items)
        {
            int width = 840;
            int margin = 58;
            int y = 48;
            int estimatedHeight = 430 + Math.Max(1, items.Count) * 105;
            if (estimatedHeight < 980) estimatedHeight = 980;

            var bmp = new System.Drawing.Bitmap(width, estimatedHeight);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            using (var fontTitle = new System.Drawing.Font("Segoe UI", 26, System.Drawing.FontStyle.Bold))
            using (var fontNormal = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Regular))
            using (var fontBold = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold))
            using (var fontSmall = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Regular))
            using (var fontSmallItalic = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Italic))
            using (var fontTotal = new System.Drawing.Font("Segoe UI", 22, System.Drawing.FontStyle.Bold))
            using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            using (var linePen = new System.Drawing.Pen(System.Drawing.Color.Black, 1))
            {
                g.Clear(System.Drawing.Color.White);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                DrawCenteredText(g, "HÓA ĐƠN BÁN HÀNG", fontTitle, brush, width, y);
                y += 58;
                DrawDashedLine(g, linePen, margin, y, width - margin);
                y += 32;

                DrawText(g, "Mã hóa đơn: " + SafeText(hd.MaHoaDon), fontNormal, brush, margin, y); y += 38;
                DrawText(g, "Ngày lập: " + hd.NgayLapHoaDon.ToString("dd/MM/yyyy HH:mm"), fontNormal, brush, margin, y); y += 38;
                DrawText(g, "Bàn/Hình thức: " + SafeText(hd.BanCafe != null ? hd.BanCafe.TenBan : hd.LoaiHoaDon), fontNormal, brush, margin, y); y += 38;
                DrawText(g, "Khách hàng: " + SafeText(hd.KhachHang != null ? hd.KhachHang.HoTen : "Khách lẻ"), fontNormal, brush, margin, y); y += 38;
                DrawText(g, "Thu ngân: " + SafeText(hd.NhanVien != null ? hd.NhanVien.HoTen : ""), fontNormal, brush, margin, y); y += 38;

                DrawDashedLine(g, linePen, margin, y, width - margin);
                y += 34;

                int colMon = margin;
                int colSl = 420;
                int colGia = 520;
                int colTien = 670;
                int maxMonWidth = colSl - colMon - 16;

                DrawText(g, "Món", fontBold, brush, colMon, y);
                DrawText(g, "SL", fontBold, brush, colSl, y);
                DrawText(g, "Giá", fontBold, brush, colGia, y);
                DrawText(g, "Tiền", fontBold, brush, colTien, y);
                y += 34;
                DrawDashedLine(g, linePen, margin, y, width - margin);
                y += 20;

                foreach (var item in items)
                {
                    string ten = item.BienTheSanPham != null && item.BienTheSanPham.SanPham != null
                        ? item.BienTheSanPham.SanPham.TenSanPham
                        : "";

                    decimal thanhTien = item.ThanhTien ?? (item.SoLuong * item.DonGia - item.TienGiam);

                    int rowStartY = y;
                    int nameHeight = DrawWrappedText(g, ten, fontNormal, brush, colMon, y, maxMonWidth);
                    DrawText(g, item.SoLuong.ToString(), fontNormal, brush, colSl, y);
                    DrawRightText(g, item.DonGia.ToString("N0"), fontNormal, brush, colGia + 85, y);
                    DrawRightText(g, thanhTien.ToString("N0"), fontNormal, brush, colTien + 105, y);
                    y += Math.Max(34, nameHeight);

                    if (!string.IsNullOrWhiteSpace(item.YeuCauDacBiet))
                    {
                        y += 3;
                        int noteHeight = DrawWrappedText(g, "Ghi chú: " + item.YeuCauDacBiet, fontSmallItalic, brush, colMon, y, width - margin * 2);
                        y += Math.Max(28, noteHeight);
                    }

                    y += 10;
                }

                y += 8;
                DrawDashedLine(g, linePen, margin, y, width - margin);
                y += 42;

                DrawRightText(g, "Tổng tiền: " + Money(hd.TongTien), fontTotal, brush, width - margin, y); y += 58;
                DrawRightText(g, "Khách trả: " + Money(hd.TienKhachTra), fontSmall, brush, width - margin, y); y += 42;
                DrawRightText(g, "Tiền thừa: " + Money(hd.TienThua), fontSmall, brush, width - margin, y); y += 42;

                DrawDashedLine(g, linePen, margin, y, width - margin);
                y += 48;
                DrawCenteredText(g, "Cảm ơn quý khách!", fontBold, brush, width, y);
                y += 54;
            }

            int finalHeight = Math.Min(Math.Max(y + 30, 900), estimatedHeight);
            var cropped = new System.Drawing.Bitmap(width, finalHeight);
            using (var g2 = System.Drawing.Graphics.FromImage(cropped))
            {
                g2.Clear(System.Drawing.Color.White);
                g2.DrawImage(bmp, 0, 0, new System.Drawing.Rectangle(0, 0, width, finalHeight), System.Drawing.GraphicsUnit.Pixel);
            }
            bmp.Dispose();
            return cropped;
        }

        private void DrawText(System.Drawing.Graphics g, string text, System.Drawing.Font font, System.Drawing.Brush brush, int x, int y)
        {
            g.DrawString(text ?? "", font, brush, x, y);
        }

        private void DrawCenteredText(System.Drawing.Graphics g, string text, System.Drawing.Font font, System.Drawing.Brush brush, int width, int y)
        {
            var size = g.MeasureString(text ?? "", font);
            g.DrawString(text ?? "", font, brush, (width - size.Width) / 2, y);
        }

        private void DrawRightText(System.Drawing.Graphics g, string text, System.Drawing.Font font, System.Drawing.Brush brush, int rightX, int y)
        {
            var size = g.MeasureString(text ?? "", font);
            g.DrawString(text ?? "", font, brush, rightX - size.Width, y);
        }

        private int DrawWrappedText(System.Drawing.Graphics g, string text, System.Drawing.Font font, System.Drawing.Brush brush, int x, int y, int width)
        {
            var rect = new System.Drawing.RectangleF(x, y, width, 300);
            var format = new System.Drawing.StringFormat
            {
                Alignment = System.Drawing.StringAlignment.Near,
                LineAlignment = System.Drawing.StringAlignment.Near,
                Trimming = System.Drawing.StringTrimming.Word
            };
            g.DrawString(text ?? "", font, brush, rect, format);
            var size = g.MeasureString(text ?? "", font, width, format);
            return (int)Math.Ceiling(size.Height);
        }

        private void DrawDashedLine(System.Drawing.Graphics g, System.Drawing.Pen pen, int x1, int y, int x2)
        {
            var oldDash = pen.DashStyle;
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            g.DrawLine(pen, x1, y, x2, y);
            pen.DashStyle = oldDash;
        }

        private void WriteJpegPdf(string pdfPath, byte[] imageData, int imagePixelWidth, int imagePixelHeight)
        {
            var offsets = new System.Collections.Generic.List<long>();
            double pageWidth = 420;
            double pageHeight = pageWidth * imagePixelHeight / imagePixelWidth;

            using (var fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                WriteAscii(fs, "%PDF-1.4\n");
                WriteAscii(fs, "%\u00E2\u00E3\u00CF\u00D3\n");

                WriteObject(fs, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
                WriteObject(fs, offsets, 2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
                WriteObject(fs, offsets, 3,
                    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " +
                    pageWidth.ToString("0.##", CultureInfo.InvariantCulture) + " " +
                    pageHeight.ToString("0.##", CultureInfo.InvariantCulture) +
                    "] /Resources << /XObject << /Im1 4 0 R >> >> /Contents 5 0 R >>");

                offsets.Add(fs.Position);
                WriteAscii(fs, "4 0 obj\n");
                WriteAscii(fs, "<< /Type /XObject /Subtype /Image /Width " + imagePixelWidth +
                    " /Height " + imagePixelHeight +
                    " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " + imageData.Length + " >>\n");
                WriteAscii(fs, "stream\n");
                fs.Write(imageData, 0, imageData.Length);
                WriteAscii(fs, "\nendstream\nendobj\n");

                string content = "q\n" +
                    pageWidth.ToString("0.##", CultureInfo.InvariantCulture) + " 0 0 " +
                    pageHeight.ToString("0.##", CultureInfo.InvariantCulture) +
                    " 0 0 cm\n/Im1 Do\nQ\n";
                byte[] contentBytes = Encoding.ASCII.GetBytes(content);
                offsets.Add(fs.Position);
                WriteAscii(fs, "5 0 obj\n");
                WriteAscii(fs, "<< /Length " + contentBytes.Length + " >>\nstream\n");
                fs.Write(contentBytes, 0, contentBytes.Length);
                WriteAscii(fs, "endstream\nendobj\n");

                long xrefOffset = fs.Position;
                WriteAscii(fs, "xref\n");
                WriteAscii(fs, "0 6\n");
                WriteAscii(fs, "0000000000 65535 f \n");
                foreach (long offset in offsets)
                {
                    WriteAscii(fs, offset.ToString("D10") + " 00000 n \n");
                }

                WriteAscii(fs, "trailer\n");
                WriteAscii(fs, "<< /Size 6 /Root 1 0 R >>\n");
                WriteAscii(fs, "startxref\n");
                WriteAscii(fs, xrefOffset.ToString(CultureInfo.InvariantCulture) + "\n");
                WriteAscii(fs, "%%EOF");
            }
        }

        private FrameworkElement BuildInvoiceVisual(HoaDonBan hd, System.Collections.Generic.List<ChiTietHoaDonBan> items)
        {
            var root = new Border
            {
                // Kích thước bill nhỏ gọn giống mẫu in hóa đơn ban đầu.
                Width = 390,
                Background = Brushes.White,
                Padding = new Thickness(26, 28, 26, 28),
                BorderThickness = new Thickness(0)
            };

            var panel = new StackPanel();
            root.Child = panel;

            panel.Children.Add(new TextBlock
            {
                Text = "HÓA ĐƠN BÁN HÀNG",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = Brushes.Black
            });

            panel.Children.Add(MakeSeparator());
            panel.Children.Add(MakeInfoLine("Mã hóa đơn:", SafeText(hd.MaHoaDon)));
            panel.Children.Add(MakeInfoLine("Ngày lập:", hd.NgayLapHoaDon.ToString("dd/MM/yyyy HH:mm")));
            panel.Children.Add(MakeInfoLine("Bàn/Hình thức:", SafeText(hd.BanCafe != null ? hd.BanCafe.TenBan : hd.LoaiHoaDon)));
            panel.Children.Add(MakeInfoLine("Khách hàng:", SafeText(hd.KhachHang != null ? hd.KhachHang.HoTen : "Khách lẻ")));
            panel.Children.Add(MakeInfoLine("Thu ngân:", SafeText(hd.NhanVien != null ? hd.NhanVien.HoTen : "")));
            panel.Children.Add(MakeSeparator());

            var header = new Grid { Margin = new Thickness(0, 6, 0, 8) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(66) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(74) });
            header.Children.Add(MakeTableText("Món", true, TextAlignment.Left, 0));
            header.Children.Add(MakeTableText("SL", true, TextAlignment.Center, 1));
            header.Children.Add(MakeTableText("Giá", true, TextAlignment.Right, 2));
            header.Children.Add(MakeTableText("Tiền", true, TextAlignment.Right, 3));
            panel.Children.Add(header);
            panel.Children.Add(MakeThinSeparator());

            foreach (var item in items)
            {
                string ten = item.BienTheSanPham != null && item.BienTheSanPham.SanPham != null
                    ? item.BienTheSanPham.SanPham.TenSanPham
                    : "";

                decimal thanhTien = item.ThanhTien ?? (item.SoLuong * item.DonGia - item.TienGiam);

                var row = new Grid { Margin = new Thickness(0, 8, 0, 0) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(66) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(74) });

                row.Children.Add(MakeTableText(ten, false, TextAlignment.Left, 0));
                row.Children.Add(MakeTableText(item.SoLuong.ToString(), false, TextAlignment.Center, 1));
                row.Children.Add(MakeTableText(item.DonGia.ToString("N0"), false, TextAlignment.Right, 2));
                row.Children.Add(MakeTableText(thanhTien.ToString("N0"), false, TextAlignment.Right, 3));
                panel.Children.Add(row);

                if (!string.IsNullOrWhiteSpace(item.YeuCauDacBiet))
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = "Ghi chú: " + item.YeuCauDacBiet,
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 12,
                        FontStyle = FontStyles.Italic,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 2, 0, 0),
                        Foreground = Brushes.Black
                    });
                }
            }

            panel.Children.Add(MakeSeparator());

            panel.Children.Add(MakeMoneyLine("Tổng tiền:", Money(hd.TongTien), true));
            panel.Children.Add(MakeMoneyLine("Khách trả:", Money(hd.TienKhachTra), false));
            panel.Children.Add(MakeMoneyLine("Tiền thừa:", Money(hd.TienThua), false));

            panel.Children.Add(MakeSeparator());

            panel.Children.Add(new TextBlock
            {
                Text = "Cảm ơn quý khách!",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0),
                Foreground = Brushes.Black
            });

            return root;
        }

        private TextBlock MakeInfoLine(string label, string value)
        {
            return new TextBlock
            {
                Text = label + " " + value,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 7),
                Foreground = Brushes.Black
            };
        }

        private TextBlock MakeSeparator()
        {
            return new TextBlock
            {
                Text = "----------------------------------------",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Margin = new Thickness(0, 8, 0, 8),
                Foreground = Brushes.Black
            };
        }

        private TextBlock MakeThinSeparator()
        {
            return new TextBlock
            {
                Text = "----------------------------------------",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 2),
                Foreground = Brushes.Black
            };
        }

        private TextBlock MakeTableText(string text, bool isHeader, TextAlignment align, int column)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = isHeader ? 13 : 12.5,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = align,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Black
            };
            Grid.SetColumn(tb, column);
            return tb;
        }

        private Grid MakeMoneyLine(string label, string value, bool important)
        {
            var grid = new Grid { Margin = new Thickness(0, important ? 8 : 4, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var left = new TextBlock
            {
                Text = label,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = important ? 14 : 13,
                FontWeight = important ? FontWeights.Bold : FontWeights.Normal,
                Foreground = Brushes.Black
            };
            Grid.SetColumn(left, 0);

            var right = new TextBlock
            {
                Text = value,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = important ? 19 : 13,
                FontWeight = important ? FontWeights.Bold : FontWeights.Normal,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Right
            };
            Grid.SetColumn(right, 1);

            grid.Children.Add(left);
            grid.Children.Add(right);
            return grid;
        }

        private void SaveVisualAsPdf(FrameworkElement element, string pdfPath)
        {
            const double dpi = 192.0;
            const double scale = dpi / 96.0;

            /*
             * Fix lỗi PDF bị đen:
             * RenderTargetBitmap tạo ảnh có alpha trong suốt. Nếu đưa thẳng ảnh có alpha vào PDF,
             * một số PDF viewer sẽ hiển thị nền trong suốt thành màu đen.
             * Cách xử lý: bọc hóa đơn trong nền trắng, sau đó tự blend pixel BGRA lên nền trắng
             * và ghi ra DeviceRGB cho PDF.
             */

            element.Measure(new Size(element.Width, double.PositiveInfinity));
            double width = element.Width;
            double height = Math.Ceiling(element.DesiredSize.Height);

            var whiteRoot = new Border
            {
                Width = width,
                Height = height,
                Background = Brushes.White,
                Child = element
            };

            whiteRoot.Measure(new Size(width, height));
            whiteRoot.Arrange(new Rect(0, 0, width, height));
            whiteRoot.UpdateLayout();

            int pixelWidth = Math.Max(1, (int)Math.Ceiling(width * scale));
            int pixelHeight = Math.Max(1, (int)Math.Ceiling(height * scale));

            var rtb = new RenderTargetBitmap(
                pixelWidth,
                pixelHeight,
                dpi,
                dpi,
                PixelFormats.Pbgra32);

            rtb.Render(whiteRoot);

            int stride = pixelWidth * 4;
            byte[] bgra = new byte[stride * pixelHeight];
            rtb.CopyPixels(bgra, stride, 0);

            byte[] rgb = new byte[pixelWidth * pixelHeight * 3];

            for (int i = 0, j = 0; i < bgra.Length; i += 4, j += 3)
            {
                byte b = bgra[i];
                byte g = bgra[i + 1];
                byte r = bgra[i + 2];
                byte a = bgra[i + 3];

                // Pbgra32 là premultiplied alpha. Composite lên nền trắng:
                int white = 255 - a;

                int rr = r + white;
                int gg = g + white;
                int bb = b + white;

                if (rr > 255) rr = 255;
                if (gg > 255) gg = 255;
                if (bb > 255) bb = 255;

                rgb[j] = (byte)rr;
                rgb[j + 1] = (byte)gg;
                rgb[j + 2] = (byte)bb;
            }

            byte[] compressed;
            using (var ms = new MemoryStream())
            {
                using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, true))
                {
                    deflate.Write(rgb, 0, rgb.Length);
                }
                compressed = ms.ToArray();
            }

            WriteImagePdf(pdfPath, compressed, pixelWidth, pixelHeight, width, height);
        }

        private void WriteImagePdf(string pdfPath, byte[] imageData, int imagePixelWidth, int imagePixelHeight, double pageWidth, double pageHeight)
        {
            var offsets = new System.Collections.Generic.List<long>();

            using (var fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                WriteAscii(fs, "%PDF-1.4\n");
                WriteAscii(fs, "%\u00E2\u00E3\u00CF\u00D3\n");

                WriteObject(fs, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
                WriteObject(fs, offsets, 2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
                WriteObject(fs, offsets, 3,
                    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " +
                    pageWidth.ToString("0.##", CultureInfo.InvariantCulture) + " " +
                    pageHeight.ToString("0.##", CultureInfo.InvariantCulture) +
                    "] /Resources << /XObject << /Im1 4 0 R >> >> /Contents 5 0 R >>");

                offsets.Add(fs.Position);
                WriteAscii(fs, "4 0 obj\n");
                WriteAscii(fs, "<< /Type /XObject /Subtype /Image /Width " + imagePixelWidth + " /Height " + imagePixelHeight + " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /FlateDecode /Length " + imageData.Length + " >>\n");
                WriteAscii(fs, "stream\n");
                fs.Write(imageData, 0, imageData.Length);
                WriteAscii(fs, "\nendstream\nendobj\n");

                string content = "q\n" +
                    pageWidth.ToString("0.##", CultureInfo.InvariantCulture) + " 0 0 " +
                    pageHeight.ToString("0.##", CultureInfo.InvariantCulture) +
                    " 0 0 cm\n/Im1 Do\nQ\n";
                byte[] contentBytes = Encoding.ASCII.GetBytes(content);
                offsets.Add(fs.Position);
                WriteAscii(fs, "5 0 obj\n");
                WriteAscii(fs, "<< /Length " + contentBytes.Length + " >>\nstream\n");
                fs.Write(contentBytes, 0, contentBytes.Length);
                WriteAscii(fs, "endstream\nendobj\n");

                long xrefOffset = fs.Position;
                WriteAscii(fs, "xref\n");
                WriteAscii(fs, "0 6\n");
                WriteAscii(fs, "0000000000 65535 f \n");
                foreach (long offset in offsets)
                {
                    WriteAscii(fs, offset.ToString("D10") + " 00000 n \n");
                }

                WriteAscii(fs, "trailer\n");
                WriteAscii(fs, "<< /Size 6 /Root 1 0 R >>\n");
                WriteAscii(fs, "startxref\n");
                WriteAscii(fs, xrefOffset.ToString(CultureInfo.InvariantCulture) + "\n");
                WriteAscii(fs, "%%EOF");
            }
        }

        private void WriteObject(FileStream fs, System.Collections.Generic.List<long> offsets, int objectNumber, string body)
        {
            offsets.Add(fs.Position);
            WriteAscii(fs, objectNumber + " 0 obj\n");
            WriteAscii(fs, body + "\n");
            WriteAscii(fs, "endobj\n");
        }

        private void WriteAscii(FileStream fs, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            fs.Write(bytes, 0, bytes.Length);
        }

        private string CatChuoi(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Length <= maxLength) return value;
            return value.Substring(0, maxLength - 3) + "...";
        }

        private string RemoveVietnamese(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    char ch = c;
                    if (ch == 'đ') ch = 'd';
                    if (ch == 'Đ') ch = 'D';

                    // PDF đơn giản dùng font chuẩn WinAnsi, nên chỉ giữ ký tự ASCII để file mở ổn định.
                    if (ch <= 127) sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private string PdfEscape(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private void WriteSimplePdf(string path, System.Collections.Generic.List<string> lines)
        {
            var objects = new System.Collections.Generic.List<string>();

            // 1 Catalog
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");

            // 2 Pages
            objects.Add("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");

            // 3 Page
            objects.Add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 420 595] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>");

            // 4 Normal font
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

            // 5 Bold font
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

            // 6 Content stream
            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F2 16 Tf");
            content.AppendLine("50 555 Td");
            content.AppendLine("(" + PdfEscape(RemoveVietnamese(lines[0])) + ") Tj");
            content.AppendLine("0 -28 Td");
            content.AppendLine("/F1 10 Tf");

            for (int i = 1; i < lines.Count; i++)
            {
                string text = RemoveVietnamese(lines[i]);
                content.AppendLine("(" + PdfEscape(text) + ") Tj");
                content.AppendLine("0 -16 Td");
            }

            content.AppendLine("ET");

            string stream = content.ToString();
            objects.Add("<< /Length " + Encoding.ASCII.GetByteCount(stream) + " >>\nstream\n" + stream + "endstream");

            var pdf = new StringBuilder();
            var offsets = new System.Collections.Generic.List<int>();

            pdf.Append("%PDF-1.4\n");
            pdf.Append("%\u00E2\u00E3\u00CF\u00D3\n");

            for (int i = 0; i < objects.Count; i++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.Append((i + 1).ToString()).Append(" 0 obj\n");
                pdf.Append(objects[i]).Append("\n");
                pdf.Append("endobj\n");
            }

            int xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.Append("xref\n");
            pdf.Append("0 ").Append(objects.Count + 1).Append("\n");
            pdf.Append("0000000000 65535 f \n");

            foreach (int offset in offsets)
            {
                pdf.Append(offset.ToString("D10")).Append(" 00000 n \n");
            }

            pdf.Append("trailer\n");
            pdf.Append("<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\n");
            pdf.Append("startxref\n");
            pdf.Append(xrefOffset).Append("\n");
            pdf.Append("%%EOF");

            File.WriteAllText(path, pdf.ToString(), Encoding.ASCII);
        }

        private void PrintInvoice(HoaDonBan hd, System.Collections.Generic.List<ChiTietHoaDonBan> items)
        {
            var document = new FlowDocument();
            document.PagePadding = new Thickness(24);
            document.FontFamily = new FontFamily("Segoe UI");
            document.FontSize = 12;
            document.ColumnWidth = 320;
            document.PageWidth = 360;

            var title = new Paragraph(new Run("HÓA ĐƠN BÁN HÀNG"));
            title.FontSize = 18;
            title.FontWeight = FontWeights.Bold;
            title.TextAlignment = TextAlignment.Center;
            title.Margin = new Thickness(0, 0, 0, 12);
            document.Blocks.Add(title);

            document.Blocks.Add(new Paragraph(new Run("Mã hóa đơn: " + SafeText(hd.MaHoaDon))));
            document.Blocks.Add(new Paragraph(new Run("Ngày lập: " + hd.NgayLapHoaDon.ToString("dd/MM/yyyy HH:mm"))));
            document.Blocks.Add(new Paragraph(new Run("Bàn/Hình thức: " + SafeText(hd.BanCafe != null ? hd.BanCafe.TenBan : hd.LoaiHoaDon))));
            document.Blocks.Add(new Paragraph(new Run("Khách hàng: " + SafeText(hd.KhachHang != null ? hd.KhachHang.HoTen : "Khách lẻ"))));
            document.Blocks.Add(new Paragraph(new Run("Thu ngân: " + SafeText(hd.NhanVien != null ? hd.NhanVien.HoTen : ""))));
            document.Blocks.Add(new Paragraph(new Run("--------------------------------")));

            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn { Width = new GridLength(40) });
            table.Columns.Add(new TableColumn { Width = new GridLength(70) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);

            var header = new TableRow();
            header.FontWeight = FontWeights.Bold;
            header.Cells.Add(new TableCell(new Paragraph(new Run("Món"))));
            header.Cells.Add(new TableCell(new Paragraph(new Run("SL"))));
            header.Cells.Add(new TableCell(new Paragraph(new Run("Giá"))));
            header.Cells.Add(new TableCell(new Paragraph(new Run("Tiền"))));
            group.Rows.Add(header);

            foreach (var item in items)
            {
                string ten = item.BienTheSanPham != null && item.BienTheSanPham.SanPham != null ? item.BienTheSanPham.SanPham.TenSanPham : "";
                decimal thanhTien = item.ThanhTien ?? (item.SoLuong * item.DonGia - item.TienGiam);

                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(ten))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.SoLuong.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.DonGia.ToString("N0")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(thanhTien.ToString("N0")))));
                group.Rows.Add(row);

                if (!string.IsNullOrWhiteSpace(item.YeuCauDacBiet))
                {
                    var noteRow = new TableRow();
                    var noteCell = new TableCell(new Paragraph(new Run("Ghi chú: " + item.YeuCauDacBiet)));
                    noteCell.ColumnSpan = 4;
                    noteCell.FontStyle = FontStyles.Italic;
                    noteRow.Cells.Add(noteCell);
                    group.Rows.Add(noteRow);
                }
            }

            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run("--------------------------------")));

            var total = new Paragraph();
            total.TextAlignment = TextAlignment.Right;
            total.Inlines.Add(new Run("Tổng tiền: ") { FontWeight = FontWeights.Bold });
            total.Inlines.Add(new Run(Money(hd.TongTien)) { FontSize = 18, FontWeight = FontWeights.Bold });
            document.Blocks.Add(total);

            document.Blocks.Add(new Paragraph(new Run("Khách trả: " + Money(hd.TienKhachTra))) { TextAlignment = TextAlignment.Right });
            document.Blocks.Add(new Paragraph(new Run("Tiền thừa: " + Money(hd.TienThua))) { TextAlignment = TextAlignment.Right });

            var thanks = new Paragraph(new Run("Cảm ơn quý khách!"));
            thanks.TextAlignment = TextAlignment.Center;
            thanks.Margin = new Thickness(0, 16, 0, 0);
            thanks.FontWeight = FontWeights.Bold;
            document.Blocks.Add(thanks);

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                IDocumentPaginatorSource source = document;
                string jobName = "HoaDon_" + SafeFileName(hd.MaHoaDon);
                printDialog.PrintDocument(source.DocumentPaginator, jobName);
            }
        }
    }
}
