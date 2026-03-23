namespace KKReport.Scripts
{
    /// <summary>
    /// ActiveReports Section Report のスクリプトをアタッチするインターフェース。
    /// </summary>
    public interface IReportScript
    {
        /// <summary>
        /// 指定された SectionReport インスタンスにすべてのイベントハンドラーをアタッチします。
        /// </summary>
        /// <param name="report">イベントをアタッチする対象の SectionReport インスタンス。</param>
        void AttachEvents(GrapeCity.ActiveReports.SectionReport report);
    }
}
