using System;
using System.Drawing;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.SectionReportModel;

namespace KKReport.Scripts.Generated
{
    /// <summary>
    /// UnknownReport のスクリプトを IReportScript として実装するクラス。
    /// </summary>
    public class UnknownReportScript : KKReport.Scripts.IReportScript
    {

        #region セクション・コントロールフィールド


        #endregion

        /// <summary>
        /// 指定された SectionReport にすべてのイベントハンドラーをアタッチします。
        /// </summary>
        /// <param name="report">イベントをアタッチする対象の SectionReport インスタンス。</param>
        public void AttachEvents(GrapeCity.ActiveReports.SectionReport report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

        }

        #region イベントハンドラー

        /// <summary>
        /// ActiveReport_ReportStart イベントハンドラー。
        /// </summary>
        /// <summary>
        /// レポートのデータソースを設定する処理
        /// "MAIN" テーブルが存在すれば、そのテーブルをデータソースとして設定する
        /// </summary>
        public void ActiveReport_ReportStart()
        {
        	// データソースを取得
        	System.Data.DataSet dsDataSource = rpt.DataSource as System.Data.DataSet;
        	// データソースを取得し、"MAIN" テーブルがあれば設定、それ以外は null に設定
        	rpt.DataSource = dsDataSource != null && dsDataSource.Tables.Contains("MAIN")
        		? dsDataSource.Tables["MAIN"]
        		: null;


        	System.Data.DataTable dt = rpt.DataSource as System.Data.DataTable;

        	if (dt == null) return;

        	// --- 処理A: 業者名称カラムの追加 (もし存在しなければ) ---
        	if (!dt.Columns.Contains("業者名称")) {
        		// Expression（計算式）を使用して列を作成
        		// 債権者コード と 債権者漢字名称 を結合
        		AddCol(dt, "業者名称", typeof(string));
        	}

        	for (int r = dt.Rows.Count - 1 ; r >= 0; r--)
        	{
        		string 債権者コード_temp = Convert.ToString(dt.Rows[r]["債権者コード"]);
        		string 債権者漢字名称_temp = Convert.ToString(dt.Rows[r]["債権者漢字名称"]);
        		// CR Formula: 業者名称 = {明細データ.債権者コード}+{明細データ.債権者漢字名称}
        		dt.Rows[r]["業者名称"] = 債権者コード_temp + 債権者漢字名称_temp;
        		//		if (filterValue != Convert.ToString(dt.Rows[r]["業者名称"]))
        		//			dt.Rows.RemoveAt(r);
        	}

        	// DataView を使用してフィルタとソートを適用
        	System.Data.DataView dv = new System.Data.DataView(dt);

        	// 業者名称で昇順ソート
        	dv.Sort = "業者名称 ASC";

        	// フィルタ後の結果を新しい DataTable として反映させる
        	rpt.DataSource = dv.ToTable();
        	rpt.DataMember = null;
        }

        /// <summary>
        /// ActiveReport_FetchData イベントハンドラー。
        /// </summary>
        public bool ActiveReport_FetchData(bool eof)
        {
        	// CR Formula: 業者名称 = {明細データ.債権者コード}+{明細データ.債権者漢字名称}
        	rpt.CalculatedFields["業者名称"].Value = Convert.ToString(rpt.Fields["債権者コード"].Value) + Convert.ToString(rpt.Fields["債権者漢字名称"].Value);
        //	this.Field16.HyperLink = "KP031110_業者名称:" + Convert.ToString(rpt.CalculatedFields["業者名称"].Value);
        	return eof;
        }

        /// <summary>
        /// AddCol イベントハンドラー。
        /// </summary>
        /// <summary>
        /// DataTable に指定した列が存在しない場合のみ、新しい列を追加する。
        /// 既存列がある場合は何もしない（重複追加による例外回避）。
        /// </summary>
        /// <param name="dt">対象の DataTable。</param>
        /// <param name="colName">追加する列名。</param>
        /// <param name="type">列のデータ型。</param>
        private void AddCol(System.Data.DataTable dt, string colName, System.Type type)
        {
        	if (!dt.Columns.Contains(colName))
        		dt.Columns.Add(new System.Data.DataColumn(colName, type));
        }

        /// <summary>
        /// Section2_Format イベントハンドラー。
        /// </summary>
        /// <summary>
        /// Section2の書式設定処理
        /// 各フィールドの配置（Alignment）を設定する
        /// </summary>
        public void Section2_Format()
        {
        	#region SET Alignment
        	// 各フィールドの表示位置を設定
        	Field15.Alignment = TextAlignment.Center;
        	Field1.Alignment = TextAlignment.Left;
        	Field2.Alignment = TextAlignment.Center;
        	Field13.Alignment = TextAlignment.Left;
        	Field3.Alignment = TextAlignment.Right;
        	Field8.Alignment = TextAlignment.Left;
        	Field9.Alignment = TextAlignment.Left;
        	Field12.Alignment = TextAlignment.Left;
        	Field6.Alignment = TextAlignment.Left;
        	Field7.Alignment = TextAlignment.Left;
        	Field14.Alignment = TextAlignment.Left;
        	Field5.Alignment = TextAlignment.Left;
        	Field10.Alignment = TextAlignment.Left;
        	#endregion
        }

        /// <summary>
        /// Section6_Format イベントハンドラー。
        /// </summary>
        /// <summary>
        /// Section6の書式設定処理
        /// 配置設定およびコントロールの背景透過・ハイパーリンク設定を行う
        /// </summary>
        public void Section6_Format()
        {
        	#region SET Alignment
        	// 各フィールドの表示位置を設定
        	Field22.Alignment = TextAlignment.Left;
        	Field17.Alignment = TextAlignment.Left;
        	Field21.Alignment = TextAlignment.Left;
        	Field23.Alignment = TextAlignment.Left;
        	Field16.Alignment = TextAlignment.Left;
        	Field18.Alignment = TextAlignment.Center;
        	Field20.Alignment = TextAlignment.Left;
        	Field19.Alignment = TextAlignment.Right;
        	Field11.Alignment = TextAlignment.Right;
        	Field4.Alignment = TextAlignment.Left;
        	帳票1.Alignment = TextAlignment.Right;
        	#endregion

        	#region set BackColor transparent
        	// セクション内の全コントロールに対して共通設定を適用
        	foreach (GrapeCity.ActiveReports.SectionReportModel.ARControl ctrl in this.Section6.Controls)
        	{
        		if (ctrl is TextBox)
        		{
        			TextBox tbx = (TextBox) ctrl;

        			// 背景透過
        			tbx.BackColor = System.Drawing.Color.Transparent;

        			// ハイパーリンクを設定
        			tbx.HyperLink = "KP031110_業者名称:" + this.txtGroupValueHeader.Text;

        			// 表示色を黒に設定
        			tbx.ForeColor = System.Drawing.Color.Black;

        			// 下線を解除
        			tbx.Font = new GrapeCity.ActiveReports.Document.Drawing.Font(tbx.Font, GrapeCity.ActiveReports.Document.Drawing.FontStyle.Regular);
        		}
        		if (ctrl is Label)
        		{
        			GrapeCity.ActiveReports.SectionReportModel.Label lbl = (Label) ctrl;

        			// 背景透過
        			lbl.BackColor = System.Drawing.Color.Transparent;

        			// ハイパーリンクを設定
        			lbl.HyperLink = "KP031110_業者名称:" + this.txtGroupValueHeader.Text;
        		}
        	}
        	#endregion
        }

        /// <summary>
        /// Section7_Format イベントハンドラー。
        /// </summary>
        /// <summary>
        /// Section7の書式設定処理
        /// グループ用ハイパーリンク設定および集計表示制御を行う
        /// </summary>
        public void Section7_Format()
        {
        	// グループ用ハイパーリンクを設定
        //	this.Field24.HyperLink = "KP031110_業者名称:" + this.txtGroupValueHeader.Text;

        	#region SET Alignment
        	// 各フィールドの表示位置を設定
        	Field24.Alignment = TextAlignment.Right;
        	Field25.Alignment = TextAlignment.Right;
        	Field28.Alignment = TextAlignment.Right;
        	Field29.Alignment = TextAlignment.Left;
        	Field30.Alignment = TextAlignment.Left;
        	Field31.Alignment = TextAlignment.Right;
        	#endregion

        	// 総件数行の場合のみ「総計」を表示
        	if (!this.Field28.Text.Equals(String.Empty) &&
        		Convert.ToInt32(this.Field28.Text) == (rpt.DataSource as System.Data.DataTable).Rows.Count)
        	{
        		this.Field28.Text = this.Field28.Text;
        		this.Field29.Text = "総計";
        		this.Field30.Text = "件";
        		this.Field31.Text = this.Field31.Text;
        	}
        	else
        	{
        		// 条件に合わない場合は非表示
        		this.Field28.Text = String.Empty;
        		this.Field29.Text = String.Empty;
        		this.Field30.Text = String.Empty;
        		this.Field31.Text = String.Empty;
        	}

        	// セクション内の全コントロールに対して共通設定を適用
        	foreach (GrapeCity.ActiveReports.SectionReportModel.ARControl ctrl in this.Section7.Controls)
        	{
        		if (ctrl is TextBox)
        		{
        			TextBox tbx = (TextBox) ctrl;

        			// 背景透過
        			tbx.BackColor = System.Drawing.Color.Transparent;

        			// ハイパーリンクを設定
        			tbx.HyperLink = "KP031110_業者名称:" + this.txtGroupValueHeader.Text;

        		}
        		if (ctrl is Label)
        		{
        			GrapeCity.ActiveReports.SectionReportModel.Label lbl = (Label) ctrl;

        			// 背景透過
        			lbl.BackColor = System.Drawing.Color.Transparent;

        			// ハイパーリンクを設定
        			lbl.HyperLink = "KP031110_業者名称:" + this.txtGroupValueHeader.Text;
        		}
        	}
        }

        /// <summary>
        /// Section3_Format イベントハンドラー。
        /// </summary>
        /// <summary>
        /// Section3の書式設定処理
        /// コントロールの背景色を透過に設定する
        /// </summary>
        public void Section3_Format()
        {
        	#region set BackColor transparent

        	// セクション内の全コントロールの背景を透過に設定
        	foreach (GrapeCity.ActiveReports.SectionReportModel.ARControl ctrl in this.Section3.Controls)
        	{
        		if (ctrl is TextBox)
        		{
        			TextBox tbx = (TextBox) ctrl;
        			tbx.BackColor = System.Drawing.Color.Transparent;
        		}
        		if (ctrl is Label)
        		{
        			GrapeCity.ActiveReports.SectionReportModel.Label lbl = (Label) ctrl;
        			lbl.BackColor = System.Drawing.Color.Transparent;
        		}
        	}
        	#endregion
        }public void Section6_BeforePrint()

        #endregion
    }
}
