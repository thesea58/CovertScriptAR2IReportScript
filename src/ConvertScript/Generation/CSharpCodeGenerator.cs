using System.Text;
using System.Text.RegularExpressions;
using ConvertScript.Models;

namespace ConvertScript.Generation
{
    /// <summary>
    /// RpxReport オブジェクトから IReportScript を実装する C# コードを生成するクラス。
    /// </summary>
    public class CSharpCodeGenerator
    {
        /// <summary>
        /// RpxReport を解析し、IReportScript を実装した C# クラスのソースコードを生成します。
        /// </summary>
        /// <param name="report">解析済みの RpxReport オブジェクト。</param>
        /// <param name="namespaceName">生成するクラスのネームスペース名。</param>
        /// <returns>生成された C# ソースコード文字列。</returns>
        public string Generate(RpxReport report, string namespaceName = "KKReport.Scripts.Generated")
        {
            var sb = new StringBuilder();

            // using ディレクティブ
            AppendUsingDirectives(sb);
            sb.AppendLine();

            // namespace 開始
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");

            string className = SanitizeIdentifier(report.ReportName) + "Script";

            // クラス XML ドキュメントとクラス宣言
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// {report.ReportName} のスクリプトを IReportScript として実装するクラス。");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public class {className} : KKReport.Scripts.IReportScript");
            sb.AppendLine("    {");

            // スクリプトレベル変数フィールド
            var scriptFields = ExtractScriptLevelFields(report.ScriptCode);

            // セクション・コントロールのフィールド宣言（ハンドラーから参照できるように）
            AppendSectionAndControlFields(sb, report);

            // スクリプトレベル変数フィールド
            AppendScriptFields(sb, scriptFields);

            // AttachEvents メソッド
            AppendAttachEventsMethod(sb, report);

            // イベントハンドラーメソッド
            AppendEventHandlers(sb, report);

            // クラス終了
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// using ディレクティブを出力します。
        /// </summary>
        private void AppendUsingDirectives(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Drawing;");
            sb.AppendLine("using GrapeCity.ActiveReports;");
            sb.AppendLine("using GrapeCity.ActiveReports.Document.Section;");
            sb.AppendLine("using GrapeCity.ActiveReports.SectionReportModel;");
        }

        /// <summary>
        /// セクションおよびコントロールへの参照をクラスフィールドとして出力します。
        /// ハンドラーメソッドから直接コントロール名でアクセスできるようにします。
        /// </summary>
        private void AppendSectionAndControlFields(StringBuilder sb, RpxReport report)
        {
            sb.AppendLine();
            sb.AppendLine("        #region セクション・コントロールフィールド");
            sb.AppendLine();

            // セクションフィールド（イベントバインディングに含まれるセクション）
            var usedSections = report.EventBindings
                .Where(b => b.ObjectType == EventObjectType.Section)
                .Select(b => b.ObjectName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // コントロールが属するセクションも追加
            var controlSections = report.EventBindings
                .Where(b => b.ObjectType == EventObjectType.Control)
                .Select(b => FindControl(b.ObjectName, report)?.SectionName)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (string sectionName in usedSections.Union(controlSections, StringComparer.OrdinalIgnoreCase))
            {
                string sectionType = GetSectionCSharpType(sectionName, report);
                sb.AppendLine($"        private {sectionType} {sectionName} = null!;");
            }

            // コントロールフィールド（イベントバインディングに含まれるコントロール）
            // さらに、スクリプト内で参照されているすべてのコントロールをフィールドとして宣言
            var usedControls = GetAllReferencedControls(report);

            foreach (var ctrl in usedControls)
            {
                string ctrlType = GetControlCSharpType(ctrl.TypeName);
                sb.AppendLine($"        private {ctrlType} {ctrl.Name} = null!;");
            }

            sb.AppendLine();
            sb.AppendLine("        #endregion");
        }

        /// <summary>
        /// スクリプトレベル変数フィールドを出力します。
        /// </summary>
        private void AppendScriptFields(StringBuilder sb, List<string> scriptFields)
        {
            if (scriptFields.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("        #region スクリプト変数フィールド");
            sb.AppendLine();
            foreach (string field in scriptFields)
            {
                sb.AppendLine($"        {field}");
            }
            sb.AppendLine();
            sb.AppendLine("        #endregion");
        }

        /// <summary>
        /// AttachEvents メソッドを出力します。
        /// セクション・コントロールフィールドの初期化とイベントのアタッチを行います。
        /// </summary>
        private void AppendAttachEventsMethod(StringBuilder sb, RpxReport report)
        {
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 指定された SectionReport にすべてのイベントハンドラーをアタッチします。");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"report\">イベントをアタッチする対象の SectionReport インスタンス。</param>");
            sb.AppendLine("        public void AttachEvents(GrapeCity.ActiveReports.SectionReport report)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (report == null)");
            sb.AppendLine("                throw new ArgumentNullException(nameof(report));");
            sb.AppendLine();

            // イベントバインディングのコレクション
            var sectionBindings = report.EventBindings
                .Where(b => b.ObjectType == EventObjectType.Section)
                .ToList();
            var controlBindings = report.EventBindings
                .Where(b => b.ObjectType == EventObjectType.Control)
                .ToList();
            var reportBindings = report.EventBindings
                .Where(b => b.ObjectType == EventObjectType.Report)
                .ToList();

            // セクションフィールドの初期化
            var usedSectionNames = sectionBindings
                .Select(b => b.ObjectName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // コントロールが属するセクションも追加（コントロールフィールド初期化に必要）
            var allControlsReferenced = GetAllReferencedControls(report);
            var controlSectionNames = allControlsReferenced
                .Select(c => c.SectionName)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allSectionNames = usedSectionNames
                .Union(controlSectionNames, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (allSectionNames.Count > 0)
            {
                sb.AppendLine("            // セクションフィールドの初期化");
                foreach (string sectionName in allSectionNames)
                {
                    string sectionType = GetSectionCSharpType(sectionName, report);
                    sb.AppendLine($"            {sectionName} = ({sectionType})report.Sections[\"{sectionName}\"];");
                }
                sb.AppendLine();
            }

            // コントロールフィールドの初期化
            if (allControlsReferenced.Count > 0)
            {
                sb.AppendLine("            // コントロールフィールドの初期化");
                foreach (var ctrl in allControlsReferenced)
                {
                    string ctrlType = GetControlCSharpType(ctrl.TypeName);
                    if (!string.IsNullOrEmpty(ctrl.SectionName))
                    {
                        sb.AppendLine($"            {ctrl.Name} = ({ctrlType}){ctrl.SectionName}.Controls[\"{ctrl.Name}\"];");
                    }
                    else
                    {
                        // セクション不明の場合はコメントを付与
                        sb.AppendLine($"            // TODO: セクションが特定できないため、手動で確認が必要です。");
                        sb.AppendLine($"            {ctrl.Name} = ({ctrlType})report.Sections[\"detail\"].Controls[\"{ctrl.Name}\"];");
                    }
                }
                sb.AppendLine();
            }

            // レポートレベルのイベントアタッチ
            if (reportBindings.Count > 0)
            {
                sb.AppendLine("            // レポートレベルのイベントアタッチ");
                foreach (var binding in reportBindings)
                {
                    sb.AppendLine($"            report.{binding.EventName} += {binding.HandlerName};");
                }
                sb.AppendLine();
            }

            // セクションレベルのイベントアタッチ
            if (sectionBindings.Count > 0)
            {
                sb.AppendLine("            // セクションレベルのイベントアタッチ");
                foreach (var binding in sectionBindings)
                {
                    sb.AppendLine($"            {binding.ObjectName}.{binding.EventName} += {binding.HandlerName};");
                }
                sb.AppendLine();
            }

            // コントロールレベルのイベントアタッチ
            if (controlBindings.Count > 0)
            {
                sb.AppendLine("            // コントロールレベルのイベントアタッチ");
                foreach (var binding in controlBindings)
                {
                    sb.AppendLine($"            {binding.ObjectName}.{binding.EventName} += {binding.HandlerName};");
                }
                sb.AppendLine();
            }

            sb.AppendLine("        }");
        }

        /// <summary>
        /// スクリプトから抽出されたイベントハンドラーメソッドを出力します。
        /// </summary>
        private void AppendEventHandlers(StringBuilder sb, RpxReport report)
        {
            if (string.IsNullOrWhiteSpace(report.ScriptCode))
                return;

            var methods = ExtractHandlerMethods(report.ScriptCode);
            if (methods.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("        #region イベントハンドラー");

            foreach (var method in methods)
            {
                sb.AppendLine();
                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// {method.Name} イベントハンドラー。");
                sb.AppendLine("        /// </summary>");

                // 先行コメントを出力
                foreach (string commentLine in method.PrecedingComments)
                {
                    string trimmedComment = commentLine.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedComment))
                        sb.AppendLine($"        {trimmedComment}");
                }

                // メソッド本体を適切なインデントで出力
                foreach (string bodyLine in method.BodyLines)
                {
                    if (string.IsNullOrWhiteSpace(bodyLine))
                        sb.AppendLine();
                    else
                        sb.AppendLine($"        {bodyLine}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("        #endregion");
        }

        /// <summary>
        /// スクリプトコードからフィールド宣言行（変数・定数）を抽出します。
        /// </summary>
        private List<string> ExtractScriptLevelFields(string scriptCode)
        {
            var fields = new List<string>();
            if (string.IsNullOrWhiteSpace(scriptCode))
                return fields;

            var lines = scriptCode.Split('\n');

            var fieldPattern = new Regex(
                @"^\s*(private|protected|public|internal|static|readonly|const)\s+\S+\s+\w+\s*(=.*)?;",
                RegexOptions.IgnoreCase);

            bool insideMethod = false;
            int braceDepth = 0;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (insideMethod)
                {
                    braceDepth += trimmed.Count(c => c == '{');
                    braceDepth -= trimmed.Count(c => c == '}');
                    if (braceDepth <= 0)
                    {
                        insideMethod = false;
                        braceDepth = 0;
                    }
                    continue;
                }

                // メソッド定義開始の検出
                bool isMethodStart = Regex.IsMatch(trimmed,
                    @"^\s*(private|protected|public|internal|static|override|virtual|async)\s+\S+\s+\w+\s*\(");

                if (isMethodStart)
                {
                    insideMethod = true;
                    braceDepth = trimmed.Count(c => c == '{') - trimmed.Count(c => c == '}');
                    if (braceDepth <= 0)
                    {
                        insideMethod = false;
                        braceDepth = 0;
                    }
                    continue;
                }

                if (fieldPattern.IsMatch(trimmed))
                {
                    fields.Add(trimmed);
                }
            }

            return fields;
        }

        /// <summary>
        /// スクリプトコードからメソッド定義を抽出し、MethodInfo リストとして返します。
        /// </summary>
        private List<MethodInfo> ExtractHandlerMethods(string scriptCode)
        {
            var methods = new List<MethodInfo>();
            if (string.IsNullOrWhiteSpace(scriptCode))
                return methods;

            var lines = scriptCode.Split('\n');

            // insideMethod: メソッドシグネチャを検出した後の状態
            // braceSeenInMethod: メソッド内で最初の { を見たかどうか
            bool insideMethod = false;
            bool braceSeenInMethod = false;
            int braceDepth = 0;
            var bodyBuffer = new List<string>();
            var commentBuffer = new List<string>();
            string currentMethodName = string.Empty;

            var fieldPattern = new Regex(
                @"^\s*(private|protected|public|internal|static|readonly|const)\s+\S+\s+\w+\s*(=.*)?;",
                RegexOptions.IgnoreCase);

            var methodStartPattern = new Regex(
                @"^\s*(private|protected|public|internal|static|override|virtual|async)\s+\S+\s+(\w+)\s*\(",
                RegexOptions.IgnoreCase);

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();
                string trimmed = line.Trim();

                if (!insideMethod)
                {
                    if (trimmed.StartsWith("//"))
                    {
                        commentBuffer.Add(trimmed);
                        continue;
                    }

                    if (fieldPattern.IsMatch(trimmed))
                    {
                        commentBuffer.Clear();
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        commentBuffer.Clear();
                        continue;
                    }

                    var match = methodStartPattern.Match(trimmed);
                    if (match.Success)
                    {
                        currentMethodName = match.Groups[2].Value;
                        insideMethod = true;
                        braceSeenInMethod = false;
                        braceDepth = 0;
                        bodyBuffer.Add(NormalizeMethodLine(line));

                        // メソッドシグネチャ行に { が含まれる場合
                        int openBraces = trimmed.Count(c => c == '{');
                        int closeBraces = trimmed.Count(c => c == '}');
                        if (openBraces > 0)
                        {
                            braceSeenInMethod = true;
                            braceDepth = openBraces - closeBraces;
                            if (braceDepth <= 0)
                            {
                                // シングルライン表現式メソッド（例: => 式）
                                insideMethod = false;
                                methods.Add(new MethodInfo(currentMethodName, commentBuffer.ToList(), bodyBuffer.ToList()));
                                bodyBuffer.Clear();
                                commentBuffer.Clear();
                            }
                        }
                    }
                }
                else
                {
                    int openBraces = trimmed.Count(c => c == '{');
                    int closeBraces = trimmed.Count(c => c == '}');

                    if (openBraces > 0)
                        braceSeenInMethod = true;

                    braceDepth += openBraces;
                    braceDepth -= closeBraces;
                    bodyBuffer.Add(NormalizeMethodLine(line));

                    if (braceSeenInMethod && braceDepth <= 0)
                    {
                        insideMethod = false;
                        braceSeenInMethod = false;
                        braceDepth = 0;
                        methods.Add(new MethodInfo(currentMethodName, commentBuffer.ToList(), bodyBuffer.ToList()));
                        bodyBuffer.Clear();
                        commentBuffer.Clear();
                    }
                }
            }

            return methods;
        }

        /// <summary>
        /// メソッド行の末尾の空白を除去します。
        /// </summary>
        private string NormalizeMethodLine(string line)
        {
            return line.TrimEnd();
        }

        /// <summary>
        /// レポート内で参照されているすべてのコントロールを返します。
        /// イベントバインディングに含まれるコントロールと、スクリプト内で参照されているコントロールを含みます。
        /// </summary>
        private List<ControlInfo> GetAllReferencedControls(RpxReport report)
        {
            // イベントバインディングに含まれるコントロール
            var eventControls = report.EventBindings
                .Where(b => b.ObjectType == EventObjectType.Control)
                .Select(b => FindControl(b.ObjectName, report))
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();

            // スクリプト内でアクセスされているコントロール（セクション内のコントロール名）
            var allControls = report.Sections.SelectMany(s => s.Controls).ToList();
            var referencedInScript = allControls
                .Where(ctrl => IsControlReferencedInScript(ctrl.Name, report.ScriptCode))
                .ToList();

            // 重複を除外して結合
            var result = eventControls.ToList();
            foreach (var ctrl in referencedInScript)
            {
                if (!result.Any(c => c.Name.Equals(ctrl.Name, StringComparison.OrdinalIgnoreCase)))
                    result.Add(ctrl);
            }

            return result;
        }

        /// <summary>
        /// スクリプトコード内でコントロール名が参照されているかどうかを確認します。
        /// </summary>
        private bool IsControlReferencedInScript(string controlName, string scriptCode)
        {
            if (string.IsNullOrWhiteSpace(scriptCode) || string.IsNullOrEmpty(controlName))
                return false;

            // コントロール名がプロパティアクセス・メソッド呼び出し・代入などで参照されているか確認
            return Regex.IsMatch(scriptCode, $@"\b{Regex.Escape(controlName)}\b");
        }

        /// <summary>
        /// セクション名からセクションの C# 型名を返します。
        /// </summary>
        private string GetSectionCSharpType(string sectionName, RpxReport report)
        {
            SectionInfo? section = report.Sections.FirstOrDefault(
                s => s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase));

            if (section == null)
                return "GrapeCity.ActiveReports.SectionReportModel.Section";

            return section.SectionType switch
            {
                "Detail"       => "GrapeCity.ActiveReports.SectionReportModel.Detail",
                "PageHeader"   => "GrapeCity.ActiveReports.SectionReportModel.PageHeader",
                "PageFooter"   => "GrapeCity.ActiveReports.SectionReportModel.PageFooter",
                "ReportHeader" => "GrapeCity.ActiveReports.SectionReportModel.ReportHeader",
                "ReportFooter" => "GrapeCity.ActiveReports.SectionReportModel.ReportFooter",
                "GroupHeader"  => "GrapeCity.ActiveReports.SectionReportModel.GroupHeader",
                "GroupFooter"  => "GrapeCity.ActiveReports.SectionReportModel.GroupFooter",
                _              => "GrapeCity.ActiveReports.SectionReportModel.Section"
            };
        }

        /// <summary>
        /// コントロール種別名から C# 型名を返します。
        /// </summary>
        private string GetControlCSharpType(string typeName)
        {
            return typeName switch
            {
                "TextBox"     => "GrapeCity.ActiveReports.SectionReportModel.TextBox",
                "Label"       => "GrapeCity.ActiveReports.SectionReportModel.Label",
                "CheckBox"    => "GrapeCity.ActiveReports.SectionReportModel.CheckBox",
                "Picture"     => "GrapeCity.ActiveReports.SectionReportModel.Picture",
                "Line"        => "GrapeCity.ActiveReports.SectionReportModel.Line",
                "Shape"       => "GrapeCity.ActiveReports.SectionReportModel.Shape",
                "SubReport"   => "GrapeCity.ActiveReports.SectionReportModel.SubReport",
                "Barcode"     => "GrapeCity.ActiveReports.SectionReportModel.Barcode",
                "Chart"       => "GrapeCity.ActiveReports.SectionReportModel.ARChart",
                "RichTextBox" => "GrapeCity.ActiveReports.SectionReportModel.RichTextBox",
                _             => "GrapeCity.ActiveReports.SectionReportModel.ARControl"
            };
        }

        /// <summary>
        /// コントロール名からレポート内の ControlInfo を検索します。
        /// </summary>
        private ControlInfo? FindControl(string controlName, RpxReport report)
        {
            foreach (var section in report.Sections)
            {
                var ctrl = section.Controls.FirstOrDefault(
                    c => c.Name.Equals(controlName, StringComparison.OrdinalIgnoreCase));
                if (ctrl != null)
                    return ctrl;
            }
            return null;
        }

        /// <summary>
        /// 文字列を C# 識別子として使用可能な形式にサニタイズします。
        /// </summary>
        private string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Report";

            string result = Regex.Replace(name, @"[^\w]", "_");
            if (char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }

        /// <summary>
        /// 抽出したメソッド情報を保持する内部クラス。
        /// </summary>
        private sealed class MethodInfo
        {
            /// <summary>メソッド名。</summary>
            public string Name { get; }

            /// <summary>メソッド直前のコメント行リスト。</summary>
            public List<string> PrecedingComments { get; }

            /// <summary>メソッド本体の行リスト（インデント正規化済み）。</summary>
            public List<string> BodyLines { get; }

            public MethodInfo(string name, List<string> precedingComments, List<string> bodyLines)
            {
                Name = name;
                PrecedingComments = precedingComments;
                BodyLines = bodyLines;
            }
        }
    }
}
