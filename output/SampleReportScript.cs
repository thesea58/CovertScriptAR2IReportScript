using System;
using System.Drawing;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.SectionReportModel;

namespace KKReport.Scripts.Generated
{
    /// <summary>
    /// SampleReport のスクリプトを IReportScript として実装するクラス。
    /// </summary>
    public class SampleReportScript : KKReport.Scripts.IReportScript
    {

        #region セクション・コントロールフィールド

        private GrapeCity.ActiveReports.SectionReportModel.PageHeader PageHeader = null!;
        private GrapeCity.ActiveReports.SectionReportModel.Detail detail = null!;
        private GrapeCity.ActiveReports.SectionReportModel.GroupHeader GroupHeader1 = null!;
        private GrapeCity.ActiveReports.SectionReportModel.GroupFooter GroupFooter1 = null!;
        private GrapeCity.ActiveReports.SectionReportModel.PageFooter PageFooter = null!;
        private GrapeCity.ActiveReports.SectionReportModel.Label lblTitle = null!;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtTax = null!;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtPage = null!;
        private GrapeCity.ActiveReports.SectionReportModel.Label lblDate = null!;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtAmount = null!;
        private GrapeCity.ActiveReports.SectionReportModel.Label lblCategory = null!;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtCategory = null!;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtSubTotal = null!;
        private GrapeCity.ActiveReports.SectionReportModel.Label lblPage = null!;

        #endregion

        #region スクリプト変数フィールド

        private int rowCount = 0;
        private decimal totalAmount = 0m;
        private string currentCategory = string.Empty;

        #endregion

        /// <summary>
        /// 指定された SectionReport にすべてのイベントハンドラーをアタッチします。
        /// </summary>
        /// <param name="report">イベントをアタッチする対象の SectionReport インスタンス。</param>
        public void AttachEvents(GrapeCity.ActiveReports.SectionReport report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            // セクションフィールドの初期化
            PageHeader = (GrapeCity.ActiveReports.SectionReportModel.PageHeader)report.Sections["PageHeader"];
            detail = (GrapeCity.ActiveReports.SectionReportModel.Detail)report.Sections["detail"];
            GroupHeader1 = (GrapeCity.ActiveReports.SectionReportModel.GroupHeader)report.Sections["GroupHeader1"];
            GroupFooter1 = (GrapeCity.ActiveReports.SectionReportModel.GroupFooter)report.Sections["GroupFooter1"];
            PageFooter = (GrapeCity.ActiveReports.SectionReportModel.PageFooter)report.Sections["PageFooter"];

            // コントロールフィールドの初期化
            lblTitle = (GrapeCity.ActiveReports.SectionReportModel.Label)PageHeader.Controls["lblTitle"];
            txtTax = (GrapeCity.ActiveReports.SectionReportModel.TextBox)detail.Controls["txtTax"];
            txtPage = (GrapeCity.ActiveReports.SectionReportModel.TextBox)PageFooter.Controls["txtPage"];
            lblDate = (GrapeCity.ActiveReports.SectionReportModel.Label)PageHeader.Controls["lblDate"];
            txtAmount = (GrapeCity.ActiveReports.SectionReportModel.TextBox)detail.Controls["txtAmount"];
            lblCategory = (GrapeCity.ActiveReports.SectionReportModel.Label)GroupHeader1.Controls["lblCategory"];
            txtCategory = (GrapeCity.ActiveReports.SectionReportModel.TextBox)GroupHeader1.Controls["txtCategory"];
            txtSubTotal = (GrapeCity.ActiveReports.SectionReportModel.TextBox)GroupFooter1.Controls["txtSubTotal"];
            lblPage = (GrapeCity.ActiveReports.SectionReportModel.Label)PageFooter.Controls["lblPage"];

            // レポートレベルのイベントアタッチ
            report.ReportStart += Report_ReportStart;
            report.ReportEnd += Report_ReportEnd;
            report.FetchData += Report_FetchData;

            // セクションレベルのイベントアタッチ
            PageHeader.Format += PageHeader_Format;
            detail.Format += detail_Format;
            detail.BeforePrint += detail_BeforePrint;
            GroupHeader1.Format += GroupHeader1_Format;
            GroupFooter1.Format += GroupFooter1_Format;
            PageFooter.Format += PageFooter_Format;

            // コントロールレベルのイベントアタッチ
            lblTitle.BeforePrint += lblTitle_BeforePrint;
            txtTax.BeforePrint += txtTax_BeforePrint;
            txtPage.BeforePrint += txtPage_BeforePrint;

        }

        #region イベントハンドラー

        /// <summary>
        /// Report_ReportStart イベントハンドラー。
        /// </summary>
        // レポート開始イベントハンドラ
        private void Report_ReportStart(object sender, EventArgs e)
        {
            rowCount = 0;
            totalAmount = 0m;
            currentCategory = string.Empty;
        }

        /// <summary>
        /// Report_ReportEnd イベントハンドラー。
        /// </summary>
        // レポート終了イベントハンドラ
        private void Report_ReportEnd(object sender, EventArgs e)
        {
            // 合計行数を出力
            System.Diagnostics.Debug.WriteLine("合計行数: " + rowCount);
            System.Diagnostics.Debug.WriteLine("合計金額: " + totalAmount.ToString("¥#,##0"));
        }

        /// <summary>
        /// Report_FetchData イベントハンドラー。
        /// </summary>
        // データ取得イベントハンドラ
        private void Report_FetchData(object sender, GrapeCity.ActiveReports.SectionReportEventArgs eArgs)
        {
            if (eArgs.EOF)
                return;
            totalAmount += Convert.ToDecimal(eArgs.GetValue("Amount"));
        }

        /// <summary>
        /// PageHeader_Format イベントハンドラー。
        /// </summary>
        // ページヘッダーフォーマットイベントハンドラ
        private void PageHeader_Format(object sender, EventArgs e)
        {
            lblDate.Text = "日付: " + DateTime.Today.ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// lblTitle_BeforePrint イベントハンドラー。
        /// </summary>
        // タイトルラベル印刷前イベントハンドラ
        private void lblTitle_BeforePrint(object sender, EventArgs e)
        {
            if (rowCount == 0)
            {
                lblTitle.ForeColor = System.Drawing.Color.Navy;
            }
        }

        /// <summary>
        /// detail_BeforePrint イベントハンドラー。
        /// </summary>
        // 詳細セクション印刷前イベントハンドラ
        private void detail_BeforePrint(object sender, EventArgs e)
        {
            rowCount++;
            if (rowCount % 2 == 0)
            {
                detail.BackColor = System.Drawing.Color.LightGray;
            }
            else
            {
                detail.BackColor = System.Drawing.Color.White;
            }
        }

        /// <summary>
        /// detail_Format イベントハンドラー。
        /// </summary>
        // 詳細セクションフォーマットイベントハンドラ
        private void detail_Format(object sender, EventArgs e)
        {
            decimal amount = Convert.ToDecimal(txtAmount.Text.Replace("¥", "").Replace(",", ""));
            decimal taxAmount = Math.Round(amount * 0.1m, 0);
            txtTax.Text = "¥" + taxAmount.ToString("#,##0");
        }

        /// <summary>
        /// txtTax_BeforePrint イベントハンドラー。
        /// </summary>
        // 税額テキストボックス印刷前イベントハンドラ
        private void txtTax_BeforePrint(object sender, EventArgs e)
        {
            decimal amount = Convert.ToDecimal(txtAmount.Text.Replace("¥", "").Replace(",", ""));
            if (amount >= 100000)
            {
                txtTax.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                txtTax.ForeColor = System.Drawing.Color.Black;
            }
        }

        /// <summary>
        /// GroupHeader1_Format イベントハンドラー。
        /// </summary>
        // グループヘッダーフォーマットイベントハンドラ
        private void GroupHeader1_Format(object sender, EventArgs e)
        {
            currentCategory = txtCategory.Text;
            lblCategory.Text = "カテゴリ: " + currentCategory;
        }

        /// <summary>
        /// GroupFooter1_Format イベントハンドラー。
        /// </summary>
        // グループフッターフォーマットイベントハンドラ
        private void GroupFooter1_Format(object sender, EventArgs e)
        {
            decimal subTotal = Convert.ToDecimal(txtSubTotal.Text.Replace("¥", "").Replace(",", ""));
            if (subTotal > 500000)
            {
                txtSubTotal.ForeColor = System.Drawing.Color.DarkGreen;
                txtSubTotal.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            }
        }

        /// <summary>
        /// PageFooter_Format イベントハンドラー。
        /// </summary>
        // ページフッターフォーマットイベントハンドラ
        private void PageFooter_Format(object sender, EventArgs e)
        {
            lblPage.Text = "ページ: " + this.CurrentPage.ToString() + " / " + this.PageCount.ToString();
        }

        /// <summary>
        /// txtPage_BeforePrint イベントハンドラー。
        /// </summary>
        // ページ番号テキストボックス印刷前イベントハンドラ
        private void txtPage_BeforePrint(object sender, EventArgs e)
        {
            txtPage.Text = this.CurrentPage.ToString();
        }

        #endregion
    }
}
