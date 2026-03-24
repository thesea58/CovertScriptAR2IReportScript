namespace ConvertScript.Models
{
    /// <summary>
    /// イベントバインディングの種別を示す列挙型。
    /// </summary>
    public enum EventObjectType
    {
        /// <summary>レポートレベルのイベント。</summary>
        Report,
        /// <summary>セクションレベルのイベント。</summary>
        Section,
        /// <summary>コントロールレベルのイベント。</summary>
        Control
    }

    /// <summary>
    /// .rpx ファイル内のイベントバインディング情報を保持するクラス。
    /// </summary>
    public class EventBinding
    {
        /// <summary>イベントを発生させるオブジェクトの名前。</summary>
        public string ObjectName { get; set; } = string.Empty;

        /// <summary>オブジェクトの種別（Report / Section / Control）。</summary>
        public EventObjectType ObjectType { get; set; }

        /// <summary>イベント名（例: Format, BeforePrint, ReportStart）。</summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>イベントハンドラーのメソッド名。</summary>
        public string HandlerName { get; set; } = string.Empty;

        /// <summary>
        /// コード生成時のオブジェクトアクセスパス。
        /// </summary>
        public string AccessPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// .rpx ファイルのコントロール情報を保持するクラス。
    /// </summary>
    public class ControlInfo
    {
        /// <summary>コントロールの名前。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>コントロールの型名（TextBox, Label など）。</summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>コントロールが属するセクションの名前。</summary>
        public string SectionName { get; set; } = string.Empty;
    }

    /// <summary>
    /// .rpx ファイルのセクション情報を保持するクラス。
    /// </summary>
    public class SectionInfo
    {
        /// <summary>セクションの名前。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>セクションの種別（Detail, PageHeader, GroupHeader など）。</summary>
        public string SectionType { get; set; } = string.Empty;

        /// <summary>このセクションに属するコントロールのリスト。</summary>
        public List<ControlInfo> Controls { get; set; } = new List<ControlInfo>();
    }

    /// <summary>
    /// .rpx ファイルの解析結果を保持するクラス。
    /// </summary>
    public class RpxReport
    {
        /// <summary>レポート名。</summary>
        public string ReportName { get; set; } = string.Empty;

        /// <summary>スクリプト言語（C#, VB など）。</summary>
        public string ScriptLanguage { get; set; } = "C#";

        /// <summary>レポートに埋め込まれたスクリプトコード。</summary>
        public string ScriptCode { get; set; } = string.Empty;

        /// <summary>レポート内のセクションリスト。</summary>
        public List<SectionInfo> Sections { get; set; } = new List<SectionInfo>();

        /// <summary>レポート内のすべてのイベントバインディングリスト。</summary>
        public List<EventBinding> EventBindings { get; set; } = new List<EventBinding>();
    }
}
