using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;
using QuanLyQuanCaFe.reports;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace QuanLyQuanCaFe.Services
{
    public class CrystalHoaDonService
    {
        public void XemHoaDon(int maHoaDonBan)
        {
            var rpt = new reportHoaDon();

            try
            {
                ApplyConnection(rpt);

                rpt.RecordSelectionFormula = "{vwRpt_HoaDonBan_DonGian.MaHoaDonBan} = " + maHoaDonBan;

                var frm = new System.Windows.Forms.Form();
                frm.Text = "In hóa đơn Crystal Report";
                frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                frm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

                var viewer = new CrystalReportViewer();
                viewer.Dock = System.Windows.Forms.DockStyle.Fill;
                viewer.DisplayToolbar = true;
                viewer.DisplayStatusBar = true;
                viewer.ToolPanelView = ToolPanelViewType.None;
                viewer.ReportSource = rpt;

                frm.Controls.Add(viewer);

                frm.FormClosed += (s, e) =>
                {
                    try
                    {
                        rpt.Close();
                        rpt.Dispose();
                    }
                    catch { }
                };

                viewer.RefreshReport();
                frm.ShowDialog();
            }
            catch
            {
                try
                {
                    rpt.Close();
                    rpt.Dispose();
                }
                catch { }

                throw;
            }
        }

        private void ApplyConnection(ReportDocument report)
        {
            var connectionInfo = BuildConnectionInfoFromAppConfig();

            foreach (Table table in report.Database.Tables)
            {
                TableLogOnInfo logonInfo = table.LogOnInfo;
                logonInfo.ConnectionInfo = connectionInfo;
                table.ApplyLogOnInfo(logonInfo);
            }

            foreach (ReportDocument subReport in report.Subreports)
            {
                foreach (Table table in subReport.Database.Tables)
                {
                    TableLogOnInfo logonInfo = table.LogOnInfo;
                    logonInfo.ConnectionInfo = connectionInfo;
                    table.ApplyLogOnInfo(logonInfo);
                }
            }
        }

        private ConnectionInfo BuildConnectionInfoFromAppConfig()
        {
            string efConn = "";

            if (ConfigurationManager.ConnectionStrings["QuanLyQuanCaPheDbEntities1"] != null)
                efConn = ConfigurationManager.ConnectionStrings["QuanLyQuanCaPheDbEntities1"].ConnectionString;

            string providerConn = ExtractProviderConnectionString(efConn);

            var sqlBuilder = new SqlConnectionStringBuilder(providerConn);

            var info = new ConnectionInfo();
            info.ServerName = sqlBuilder.DataSource;
            info.DatabaseName = sqlBuilder.InitialCatalog;

            if (sqlBuilder.IntegratedSecurity)
            {
                info.IntegratedSecurity = true;
            }
            else
            {
                info.UserID = sqlBuilder.UserID;
                info.Password = sqlBuilder.Password;
                info.IntegratedSecurity = false;
            }

            return info;
        }

        private string ExtractProviderConnectionString(string efConn)
        {
            if (string.IsNullOrWhiteSpace(efConn))
                throw new Exception("Không tìm thấy connection string QuanLyQuanCaPheDbEntities1 trong App.config.");

            string key1 = "provider connection string=\"";
            int start = efConn.IndexOf(key1, StringComparison.OrdinalIgnoreCase);

            if (start >= 0)
            {
                start += key1.Length;
                int end = efConn.IndexOf("\"", start, StringComparison.OrdinalIgnoreCase);

                if (end > start)
                    return efConn.Substring(start, end - start);
            }

            string key2 = "provider connection string=&quot;";
            start = efConn.IndexOf(key2, StringComparison.OrdinalIgnoreCase);

            if (start >= 0)
            {
                start += key2.Length;
                int end = efConn.IndexOf("&quot;", start, StringComparison.OrdinalIgnoreCase);

                if (end > start)
                    return efConn.Substring(start, end - start);
            }

            return efConn;
        }
    }
}