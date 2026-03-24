using System.Xml.Linq;
using ConvertScript.Models;

namespace ConvertScript.Parsing
{
    /// <summary>
    /// ActiveReports の .rpx ファイルを解析するパーサークラス。
    /// </summary>
    public class RpxParser
    {
        /// <summary>
        /// ActiveReports のセクション種別名のセット。
        /// </summary>
        private static readonly HashSet<string> SectionTypeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PageHeader", "PageFooter", "ReportHeader", "ReportFooter",
            "Detail", "GroupHeader", "GroupFooter"
        };

        /// <summary>
        /// レポートレベルのイベント名のセット。
        /// </summary>
        private static readonly HashSet<string> ReportEventNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ReportStart", "ReportEnd", "NoData", "FetchData", "DataInitialize"
        };

        /// <summary>
        /// セクションレベルのイベント名のセット。
        /// </summary>
        private static readonly HashSet<string> SectionEventNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Format", "BeforePrint", "AfterPrint"
        };

        /// <summary>
        /// ActiveReports の On プレフィックス付きイベント属性名からイベント名への変換マップ。
        /// </summary>
        private static readonly Dictionary<string, string> OnPrefixEventMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "OnFormat",      "Format"      },
            { "OnBeforePrint", "BeforePrint" },
            { "OnAfterPrint",  "AfterPrint"  }
        };

        /// <summary>
        /// 指定されたファイルパスの .rpx ファイルを解析し、RpxReport オブジェクトを返します。
        /// </summary>
        /// <param name="filePath">.rpx ファイルのパス。</param>
        /// <returns>解析結果の RpxReport オブジェクト。</returns>
        public RpxReport Parse(string filePath)
        {
            string xmlContent = File.ReadAllText(filePath);
            return ParseXml(xmlContent);
        }

        /// <summary>
        /// XML 文字列を解析し、RpxReport オブジェクトを返します。
        /// </summary>
        /// <param name="xmlContent">.rpx ファイルの XML 文字列。</param>
        /// <returns>解析結果の RpxReport オブジェクト。</returns>
        public RpxReport ParseXml(string xmlContent)
        {
            XDocument doc = XDocument.Parse(xmlContent);
            XElement? root = doc.Root;

            if (root == null)
                throw new InvalidOperationException(".rpx ファイルのルート要素が見つかりません。");

            var report = new RpxReport
            {
                ReportName = root.Attribute("Name")?.Value ?? "UnknownReport"
            };

            // スクリプト要素の解析
            ParseScript(root, report);

            // セクション要素の解析
            ParseSections(root, report);

            // レポートレベルのイベントを Events 要素から取得
            ParseReportLevelEvents(root, report);

            return report;
        }

        /// <summary>
        /// Script 要素からスクリプトコードと言語を解析します。
        /// </summary>
        private void ParseScript(XElement root, RpxReport report)
        {
            XElement? scriptElement = root.Descendants("Script").FirstOrDefault();
            if (scriptElement == null)
                return;

            string lang = scriptElement.Attribute("Language")?.Value ?? "C#";
            report.ScriptLanguage = lang;
            report.ScriptCode = scriptElement.Value.Trim();
        }

        /// <summary>
        /// Sections 要素配下のセクションとコントロール、イベントバインディングを解析します。
        /// </summary>
        private void ParseSections(XElement root, RpxReport report)
        {
            // Sections コンテナ要素または直下の Section 要素を検索
            IEnumerable<XElement> sectionElements = root
                .Descendants()
                .Where(e => SectionTypeNames.Contains(e.Name.LocalName));

            foreach (XElement sectionEl in sectionElements)
            {
                string sectionName = sectionEl.Attribute("Name")?.Value ?? sectionEl.Name.LocalName;
                string sectionType = sectionEl.Name.LocalName;

                var sectionInfo = new SectionInfo
                {
                    Name = sectionName,
                    SectionType = sectionType
                };

                // セクションの On* イベント属性を解析
                foreach (var kvp in OnPrefixEventMap)
                {
                    string? handlerName = sectionEl.Attribute(kvp.Key)?.Value;
                    if (!string.IsNullOrEmpty(handlerName))
                    {
                        report.EventBindings.Add(new EventBinding
                        {
                            ObjectName = sectionName,
                            ObjectType = EventObjectType.Section,
                            EventName = kvp.Value,
                            HandlerName = handlerName,
                            AccessPath = $"section_{sectionName}"
                        });
                    }
                }

                // コントロールの解析
                ParseControls(sectionEl, sectionInfo, report);

                report.Sections.Add(sectionInfo);
            }
        }

        /// <summary>
        /// セクション内の Controls 要素配下のコントロールを解析します。
        /// </summary>
        private void ParseControls(XElement sectionEl, SectionInfo sectionInfo, RpxReport report)
        {
            XElement? controlsEl = sectionEl.Element("Controls");
            if (controlsEl == null)
                return;

            foreach (XElement controlEl in controlsEl.Elements())
            {
                string controlName = controlEl.Attribute("Name")?.Value ?? string.Empty;
                if (string.IsNullOrEmpty(controlName))
                    continue;

                string typeName = controlEl.Name.LocalName;

                var controlInfo = new ControlInfo
                {
                    Name = controlName,
                    TypeName = typeName,
                    SectionName = sectionInfo.Name
                };

                sectionInfo.Controls.Add(controlInfo);

                // コントロールの On* イベント属性を解析
                foreach (var kvp in OnPrefixEventMap)
                {
                    string? handlerName = controlEl.Attribute(kvp.Key)?.Value;
                    if (!string.IsNullOrEmpty(handlerName))
                    {
                        report.EventBindings.Add(new EventBinding
                        {
                            ObjectName = controlName,
                            ObjectType = EventObjectType.Control,
                            EventName = kvp.Value,
                            HandlerName = handlerName,
                            AccessPath = $"control_{controlName}"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Events 要素からレポートレベルのイベントバインディングを解析します。
        /// </summary>
        private void ParseReportLevelEvents(XElement root, RpxReport report)
        {
            XElement? eventsEl = root.Element("Events");
            if (eventsEl == null)
            {
                // Events 要素がない場合は、ルート属性からレポートイベントを検索
                foreach (string eventName in ReportEventNames)
                {
                    string? handlerName = root.Attribute("On" + eventName)?.Value;
                    if (!string.IsNullOrEmpty(handlerName))
                    {
                        AddReportEventIfNotDuplicate(report, eventName, handlerName);
                    }
                }
                return;
            }

            foreach (XElement eventEl in eventsEl.Elements("Event"))
            {
                string? eventName = eventEl.Attribute("Name")?.Value;
                string? handlerName = eventEl.Attribute("Handler")?.Value;

                if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(handlerName))
                    continue;

                AddReportEventIfNotDuplicate(report, eventName, handlerName);
            }
        }

        /// <summary>
        /// 重複を避けながらレポートレベルのイベントバインディングを追加します。
        /// </summary>
        private static void AddReportEventIfNotDuplicate(RpxReport report, string eventName, string handlerName)
        {
            bool exists = report.EventBindings.Any(b =>
                b.ObjectType == EventObjectType.Report &&
                b.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                report.EventBindings.Add(new EventBinding
                {
                    ObjectName = "report",
                    ObjectType = EventObjectType.Report,
                    EventName = eventName,
                    HandlerName = handlerName,
                    AccessPath = "report"
                });
            }
        }
    }
}
