using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;
using System;
using System.IO;
using System.Windows.Controls;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCReportCHoaDon : UserControl
    {
        private CrystalReportViewer report1;

        public UCReportCHoaDon()
        {
            InitializeComponent();

            report1 = new CrystalReportViewer();
            report1.Dock = System.Windows.Forms.DockStyle.Fill;
            report1.DisplayToolbar = true;
            report1.DisplayStatusBar = true;
            report1.ToolPanelView = ToolPanelViewType.None;

            ReportHost.Child = report1;
        }

        private void btnShowReport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ReportDocument rpt = new ReportDocument();

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "reportview",
                "reportHoaDon.rpt"
            );

            rpt.Load(path);

            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.ServerName = ".";
            connectionInfo.DatabaseName = "QuanLyQuanCaPheDb";
            connectionInfo.IntegratedSecurity = true;

            foreach (Table table in rpt.Database.Tables)
            {
                TableLogOnInfo logonInfo = table.LogOnInfo;
                logonInfo.ConnectionInfo = connectionInfo;
                table.ApplyLogOnInfo(logonInfo);
            }

            report1.ReportSource = rpt;
            report1.RefreshReport();
        }
    }
}