using QuanLyQuanCaFe.Core;
using System;

namespace QuanLyQuanCaFe.Models
{
    public class PhieuKhoModel : BaseViewModel
    {
        public int MaPhieuKho { get; set; }
        public string MaCodePhieuKho { get; set; }
        public string LoaiPhieu { get; set; }
        public DateTime NgayLap { get; set; }
        public string GhiChu { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        public string LoaiPhieuText
        {
            get
            {
                return LoaiPhieu == "NHAP" ? "Phiếu nhập" : "Phiếu xuất";
            }
        }

        public string NgayLapText
        {
            get
            {
                return NgayLap.ToString("dd/MM/yyyy HH:mm");
            }
        }

        public string TongTienText
        {
            get
            {
                return TongTien.ToString("N0") + "đ";
            }
        }

        public string MauLoaiPhieu
        {
            get
            {
                return LoaiPhieu == "NHAP" ? "#0B63F6" : "#DC2626";
            }
        }

        public string NenLoaiPhieu
        {
            get
            {
                return LoaiPhieu == "NHAP" ? "#EFF6FF" : "#FEE2E2";
            }
        }
    }
}