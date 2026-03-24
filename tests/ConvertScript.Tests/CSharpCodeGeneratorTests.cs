using ConvertScript.Generation;
using ConvertScript.Models;
using ConvertScript.Parsing;
using Xunit;

namespace ConvertScript.Tests
{
    /// <summary>
    /// CSharpCodeGenerator クラスのユニットテスト。
    /// </summary>
    public class CSharpCodeGeneratorTests
    {
        private readonly CSharpCodeGenerator _generator = new CSharpCodeGenerator();

        /// <summary>
        /// 生成コードに正しい名前空間が含まれることを確認します。
        /// </summary>
        [Fact]
        public void Generate_WithCustomNamespace_ContainsNamespace()
        {
            var report = CreateMinimalReport("MyReport");

            string code = _generator.Generate(report, "MyApp.Reports");

            Assert.Contains("namespace MyApp.Reports", code);
        }

        /// <summary>
        /// 生成コードにデフォルトの名前空間が使用されることを確認します。
        /// </summary>
        [Fact]
        public void Generate_WithDefaultNamespace_UsesDefaultNamespace()
        {
            var report = CreateMinimalReport("MyReport");

            string code = _generator.Generate(report);

            Assert.Contains("namespace KKReport.Scripts.Generated", code);
        }

        /// <summary>
        /// 生成コードにレポート名を含むクラス名が含まれることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ClassNameContainsReportName()
        {
            var report = CreateMinimalReport("SalesReport");

            string code = _generator.Generate(report);

            Assert.Contains("class SalesReportScript", code);
        }

        /// <summary>
        /// 生成コードが IReportScript を継承していることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ClassImplementsIReportScript()
        {
            var report = CreateMinimalReport("TestReport");

            string code = _generator.Generate(report);

            Assert.Contains(": KKReport.Scripts.IReportScript", code);
        }

        /// <summary>
        /// 生成コードに AttachEvents メソッドが含まれることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ContainsAttachEventsMethod()
        {
            var report = CreateMinimalReport("TestReport");

            string code = _generator.Generate(report);

            Assert.Contains("public void AttachEvents(GrapeCity.ActiveReports.SectionReport report)", code);
        }

        /// <summary>
        /// レポートレベルのイベントが AttachEvents に正しくアタッチされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ReportLevelEvent_IsAttachedInAttachEvents()
        {
            var report = CreateMinimalReport("TestReport");
            report.EventBindings.Add(new EventBinding
            {
                ObjectName = "report",
                ObjectType = EventObjectType.Report,
                EventName = "ReportStart",
                HandlerName = "Report_ReportStart",
                AccessPath = "report"
            });

            string code = _generator.Generate(report);

            Assert.Contains("report.ReportStart += Report_ReportStart;", code);
        }

        /// <summary>
        /// セクションレベルのイベントが AttachEvents に正しくアタッチされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_SectionLevelEvent_IsAttachedWithSectionField()
        {
            var report = CreateMinimalReport("TestReport");
            report.Sections.Add(new SectionInfo
            {
                Name = "detail",
                SectionType = "Detail",
                Controls = new List<ControlInfo>()
            });
            report.EventBindings.Add(new EventBinding
            {
                ObjectName = "detail",
                ObjectType = EventObjectType.Section,
                EventName = "Format",
                HandlerName = "detail_Format",
                AccessPath = "section_detail"
            });

            string code = _generator.Generate(report);

            Assert.Contains("detail.Format += detail_Format;", code);
            Assert.Contains("GrapeCity.ActiveReports.SectionReportModel.Detail detail = null!;", code);
        }

        /// <summary>
        /// コントロールレベルのイベントが AttachEvents に正しくアタッチされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ControlLevelEvent_IsAttachedWithControlField()
        {
            var report = CreateMinimalReport("TestReport");
            var section = new SectionInfo
            {
                Name = "detail",
                SectionType = "Detail",
                Controls = new List<ControlInfo>
                {
                    new ControlInfo { Name = "txtAmount", TypeName = "TextBox", SectionName = "detail" }
                }
            };
            report.Sections.Add(section);
            report.EventBindings.Add(new EventBinding
            {
                ObjectName = "txtAmount",
                ObjectType = EventObjectType.Control,
                EventName = "BeforePrint",
                HandlerName = "txtAmount_BeforePrint",
                AccessPath = "control_txtAmount"
            });

            string code = _generator.Generate(report);

            Assert.Contains("txtAmount.BeforePrint += txtAmount_BeforePrint;", code);
            Assert.Contains("GrapeCity.ActiveReports.SectionReportModel.TextBox txtAmount = null!;", code);
        }

        /// <summary>
        /// スクリプトコードのフィールドがクラスフィールドに変換されることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ScriptFields_AreConvertedToClassFields()
        {
            var report = CreateMinimalReport("TestReport");
            report.ScriptCode = "private int rowCount = 0;\nprivate string currentCategory = string.Empty;\n";

            string code = _generator.Generate(report);

            Assert.Contains("private int rowCount = 0;", code);
            Assert.Contains("private string currentCategory = string.Empty;", code);
        }

        /// <summary>
        /// スクリプトのメソッドがイベントハンドラーとして出力されることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ScriptMethods_AreOutputAsEventHandlers()
        {
            var report = CreateMinimalReport("TestReport");
            report.ScriptCode = @"private void MyHandler(object sender, EventArgs e)
{
    int x = 1;
}";

            string code = _generator.Generate(report);

            Assert.Contains("private void MyHandler(object sender, EventArgs e)", code);
            Assert.Contains("int x = 1;", code);
        }

        /// <summary>
        /// 生成コードに null チェックが含まれることを確認します。
        /// </summary>
        [Fact]
        public void Generate_AttachEvents_ContainsNullCheck()
        {
            var report = CreateMinimalReport("TestReport");

            string code = _generator.Generate(report);

            Assert.Contains("throw new ArgumentNullException(nameof(report));", code);
        }

        /// <summary>
        /// セクションのキャストコードが AttachEvents に含まれることを確認します。
        /// </summary>
        [Fact]
        public void Generate_SectionInitialization_ContainsCastExpression()
        {
            var report = CreateMinimalReport("TestReport");
            report.Sections.Add(new SectionInfo
            {
                Name = "detail",
                SectionType = "Detail",
                Controls = new List<ControlInfo>()
            });
            report.EventBindings.Add(new EventBinding
            {
                ObjectName = "detail",
                ObjectType = EventObjectType.Section,
                EventName = "Format",
                HandlerName = "detail_Format",
                AccessPath = "section_detail"
            });

            string code = _generator.Generate(report);

            Assert.Contains(
                "detail = (GrapeCity.ActiveReports.SectionReportModel.Detail)report.Sections[\"detail\"];",
                code);
        }

        /// <summary>
        /// 完全な .rpx XML を解析し、正しい C# コードが生成されることを確認する統合テスト。
        /// </summary>
        [Fact]
        public void Generate_FullRpxReport_ProducesValidStructure()
        {
            string xml = @"<ActiveReport Name=""InvoiceReport"" Version=""2.0"">
                <Sections>
                    <PageHeader Name=""PageHeader"" Height=""0.5in"" OnFormat=""PageHeader_Format"">
                        <Controls>
                            <Label Name=""lblTitle"" Text=""請求書"" />
                        </Controls>
                    </PageHeader>
                    <Detail Name=""detail"" Height=""0.25in"" OnFormat=""detail_Format"">
                        <Controls>
                            <TextBox Name=""txtAmount"" DataField=""Amount"" OnBeforePrint=""txtAmount_BeforePrint"" />
                        </Controls>
                    </Detail>
                </Sections>
                <Script Language=""C#""><![CDATA[
private int lineNo = 0;

private void PageHeader_Format(object sender, EventArgs e)
{
    lblTitle.Text = ""請求書"";
}

private void detail_Format(object sender, EventArgs e)
{
    lineNo++;
}

private void txtAmount_BeforePrint(object sender, EventArgs e)
{
    if (lineNo % 2 == 0)
        txtAmount.BackColor = System.Drawing.Color.LightBlue;
}
                ]]></Script>
                <Events>
                    <Event Name=""ReportStart"" Handler=""Report_ReportStart"" />
                </Events>
            </ActiveReport>";

            var parser = new RpxParser();
            RpxReport report = parser.ParseXml(xml);
            string code = _generator.Generate(report, "TestNamespace");

            // 基本構造の確認
            Assert.Contains("namespace TestNamespace", code);
            Assert.Contains("class InvoiceReportScript : KKReport.Scripts.IReportScript", code);
            Assert.Contains("public void AttachEvents(GrapeCity.ActiveReports.SectionReport report)", code);

            // フィールドの確認
            Assert.Contains("private int lineNo = 0;", code);

            // イベントアタッチの確認
            Assert.Contains("PageHeader.Format += PageHeader_Format;", code);
            Assert.Contains("detail.Format += detail_Format;", code);
            Assert.Contains("txtAmount.BeforePrint += txtAmount_BeforePrint;", code);

            // ハンドラーの確認
            Assert.Contains("private void PageHeader_Format(object sender, EventArgs e)", code);
            Assert.Contains("private void detail_Format(object sender, EventArgs e)", code);
            Assert.Contains("private void txtAmount_BeforePrint(object sender, EventArgs e)", code);
        }

        /// <summary>
        /// 最小限の RpxReport オブジェクトを作成するヘルパーメソッド。
        /// </summary>
        private static RpxReport CreateMinimalReport(string reportName)
        {
            return new RpxReport
            {
                ReportName = reportName,
                ScriptLanguage = "C#",
                ScriptCode = string.Empty,
                Sections = new List<SectionInfo>(),
                EventBindings = new List<EventBinding>()
            };
        }
    }
}
