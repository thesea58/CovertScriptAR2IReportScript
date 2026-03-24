using ConvertScript.Models;
using ConvertScript.Parsing;
using Xunit;

namespace ConvertScript.Tests
{
    /// <summary>
    /// RpxParser クラスのユニットテスト。
    /// </summary>
    public class RpxParserTests
    {
        private readonly RpxParser _parser = new RpxParser();

        /// <summary>
        /// 基本的な .rpx XML を解析し、レポート名が正しく取得できることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_ReportName_IsExtractedCorrectly()
        {
            string xml = @"<ActiveReport Name=""TestReport"" Version=""2.0"">
                <Sections />
                <Script Language=""C#""><![CDATA[]]></Script>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Equal("TestReport", result.ReportName);
        }

        /// <summary>
        /// Script 要素の言語属性が正しく抽出されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_ScriptLanguage_IsExtractedCorrectly()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Script Language=""VB""><![CDATA[Dim x As Integer = 0]]></Script>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Equal("VB", result.ScriptLanguage);
        }

        /// <summary>
        /// Script 要素のコードが正しく抽出されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_ScriptCode_IsExtractedCorrectly()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Script Language=""C#""><![CDATA[private int x = 0;]]></Script>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Contains("private int x = 0;", result.ScriptCode);
        }

        /// <summary>
        /// Detail セクションが正しく解析されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_DetailSection_IsExtractedCorrectly()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Sections>
                    <Detail Name=""detail"" Height=""0.25in"">
                        <Controls />
                    </Detail>
                </Sections>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Single(result.Sections);
            Assert.Equal("detail", result.Sections[0].Name);
            Assert.Equal("Detail", result.Sections[0].SectionType);
        }

        /// <summary>
        /// セクションの OnFormat イベントが正しくイベントバインディングとして抽出されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_SectionOnFormatEvent_IsExtractedAsEventBinding()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Sections>
                    <Detail Name=""detail"" OnFormat=""detail_Format"">
                        <Controls />
                    </Detail>
                </Sections>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Single(result.EventBindings);
            EventBinding binding = result.EventBindings[0];
            Assert.Equal("detail", binding.ObjectName);
            Assert.Equal(EventObjectType.Section, binding.ObjectType);
            Assert.Equal("Format", binding.EventName);
            Assert.Equal("detail_Format", binding.HandlerName);
        }

        /// <summary>
        /// コントロールの OnBeforePrint イベントが正しく抽出されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_ControlOnBeforePrintEvent_IsExtractedAsEventBinding()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Sections>
                    <Detail Name=""detail"" Height=""0.25in"">
                        <Controls>
                            <TextBox Name=""txtAmount"" OnBeforePrint=""txtAmount_BeforePrint"" />
                        </Controls>
                    </Detail>
                </Sections>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            var controlBinding = result.EventBindings
                .FirstOrDefault(b => b.ObjectType == EventObjectType.Control);
            Assert.NotNull(controlBinding);
            Assert.Equal("txtAmount", controlBinding.ObjectName);
            Assert.Equal("BeforePrint", controlBinding.EventName);
            Assert.Equal("txtAmount_BeforePrint", controlBinding.HandlerName);
        }

        /// <summary>
        /// Events 要素からレポートレベルイベントが正しく抽出されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_ReportLevelEvents_AreExtractedFromEventsElement()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Sections />
                <Events>
                    <Event Name=""ReportStart"" Handler=""Report_ReportStart"" />
                    <Event Name=""ReportEnd"" Handler=""Report_ReportEnd"" />
                </Events>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            var reportEvents = result.EventBindings
                .Where(b => b.ObjectType == EventObjectType.Report)
                .ToList();

            Assert.Equal(2, reportEvents.Count);
            Assert.Contains(reportEvents, b => b.EventName == "ReportStart" && b.HandlerName == "Report_ReportStart");
            Assert.Contains(reportEvents, b => b.EventName == "ReportEnd" && b.HandlerName == "Report_ReportEnd");
        }

        /// <summary>
        /// セクション内のコントロールが正しく解析されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_Controls_AreExtractedWithCorrectSectionName()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Sections>
                    <Detail Name=""detail"" Height=""0.25in"">
                        <Controls>
                            <TextBox Name=""txtName"" />
                            <Label Name=""lblTitle"" />
                        </Controls>
                    </Detail>
                </Sections>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Single(result.Sections);
            var section = result.Sections[0];
            Assert.Equal(2, section.Controls.Count);
            Assert.All(section.Controls, c => Assert.Equal("detail", c.SectionName));
            Assert.Contains(section.Controls, c => c.Name == "txtName" && c.TypeName == "TextBox");
            Assert.Contains(section.Controls, c => c.Name == "lblTitle" && c.TypeName == "Label");
        }

        /// <summary>
        /// 複数のセクション種別が正しく解析されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_MultipleSectionTypes_AreAllExtracted()
        {
            string xml = @"<ActiveReport Name=""R"" Version=""2.0"">
                <Sections>
                    <PageHeader Name=""PageHeader"" Height=""0.5in"">
                        <Controls />
                    </PageHeader>
                    <Detail Name=""detail"" Height=""0.25in"">
                        <Controls />
                    </Detail>
                    <PageFooter Name=""PageFooter"" Height=""0.25in"">
                        <Controls />
                    </PageFooter>
                </Sections>
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Equal(3, result.Sections.Count);
            Assert.Contains(result.Sections, s => s.SectionType == "PageHeader");
            Assert.Contains(result.Sections, s => s.SectionType == "Detail");
            Assert.Contains(result.Sections, s => s.SectionType == "PageFooter");
        }

        /// <summary>
        /// ルート要素が存在しない場合に例外がスローされることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_NullRoot_ThrowsInvalidOperationException()
        {
            Assert.Throws<System.Xml.XmlException>(() => _parser.ParseXml(""));
        }

        /// <summary>
        /// Name 属性が存在しない場合にデフォルト名が使用されることを確認します。
        /// </summary>
        [Fact]
        public void ParseXml_MissingNameAttribute_UsesDefaultName()
        {
            string xml = @"<ActiveReport Version=""2.0"">
                <Sections />
            </ActiveReport>";

            RpxReport result = _parser.ParseXml(xml);

            Assert.Equal("UnknownReport", result.ReportName);
        }
    }
}
