using ConvertScript.Generation;
using ConvertScript.Models;
using Xunit;

namespace ConvertScript.Tests
{
    /// <summary>
    /// InitializationCodeGenerator クラスのユニットテスト。
    /// </summary>
    public class InitializationCodeGeneratorTests
    {
        private readonly InitializationCodeGenerator _generator = new InitializationCodeGenerator();

        /// <summary>
        /// セクションのない空のレポートでは空文字列が返ることを確認します。
        /// </summary>
        [Fact]
        public void Generate_EmptyReport_ReturnsEmpty()
        {
            var report = new RpxReport();

            string code = _generator.Generate(report);

            Assert.Equal(string.Empty, code.Trim());
        }

        /// <summary>
        /// セクションのみ（コントロールなし）の場合、セクション行のみが生成されることを確認します。
        /// </summary>
        [Fact]
        public void Generate_SectionWithNoControls_GeneratesSectionLine()
        {
            var report = new RpxReport();
            report.Sections.Add(new SectionInfo { Name = "Section7", SectionType = "GroupFooter" });

            string code = _generator.Generate(report);

            Assert.Contains("var Section7 = _report.Sections[\"Section7\"];", code);
        }

        /// <summary>
        /// デフォルト以外の reportVarName が使用されることを確認します。
        /// </summary>
        [Fact]
        public void Generate_CustomReportVarName_UsesCustomVarName()
        {
            var report = new RpxReport();
            report.Sections.Add(new SectionInfo { Name = "detail", SectionType = "Detail" });

            string code = _generator.Generate(report, "myReport");

            Assert.Contains("var detail = myReport.Sections[\"detail\"];", code);
        }

        /// <summary>
        /// AR.Field 型が TextBox にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArField_MapsToTextBox()
        {
            var report = CreateReportWithControl("Section1", "Field28", "AR.Field");

            string code = _generator.Generate(report);

            Assert.Contains("var Field28 = Section1.Controls[\"Field28\"] as TextBox;", code);
        }

        /// <summary>
        /// AR.Label 型が Label にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArLabel_MapsToLabel()
        {
            var report = CreateReportWithControl("Section1", "Text1", "AR.Label");

            string code = _generator.Generate(report);

            Assert.Contains("var Text1 = Section1.Controls[\"Text1\"] as Label;", code);
        }

        /// <summary>
        /// AR.Image 型が Picture にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArImage_MapsToPicture()
        {
            var report = CreateReportWithControl("Section1", "Image1", "AR.Image");

            string code = _generator.Generate(report);

            Assert.Contains("var Image1 = Section1.Controls[\"Image1\"] as Picture;", code);
        }

        /// <summary>
        /// AR.Shape 型が Shape にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArShape_MapsToShape()
        {
            var report = CreateReportWithControl("Section1", "Shape1", "AR.Shape");

            string code = _generator.Generate(report);

            Assert.Contains("var Shape1 = Section1.Controls[\"Shape1\"] as Shape;", code);
        }

        /// <summary>
        /// AR.Line 型が Line にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArLine_MapsToLine()
        {
            var report = CreateReportWithControl("Section1", "Line1", "AR.Line");

            string code = _generator.Generate(report);

            Assert.Contains("var Line1 = Section1.Controls[\"Line1\"] as Line;", code);
        }

        /// <summary>
        /// AR.CheckBox 型が CheckBox にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArCheckBox_MapsToCheckBox()
        {
            var report = CreateReportWithControl("Section1", "Check1", "AR.CheckBox");

            string code = _generator.Generate(report);

            Assert.Contains("var Check1 = Section1.Controls[\"Check1\"] as CheckBox;", code);
        }

        /// <summary>
        /// AR.ReportInfo 型が ReportInfo にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArReportInfo_MapsToReportInfo()
        {
            var report = CreateReportWithControl("Section1", "ReportInfo1", "AR.ReportInfo");

            string code = _generator.Generate(report);

            Assert.Contains("var ReportInfo1 = Section1.Controls[\"ReportInfo1\"] as ReportInfo;", code);
        }

        /// <summary>
        /// AR.SubReport 型が SubReport にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArSubReport_MapsToSubReport()
        {
            var report = CreateReportWithControl("Section1", "Sub1", "AR.SubReport");

            string code = _generator.Generate(report);

            Assert.Contains("var Sub1 = Section1.Controls[\"Sub1\"] as SubReport;", code);
        }

        /// <summary>
        /// AR.PageBreak 型が PageBreak にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArPageBreak_MapsToPageBreak()
        {
            var report = CreateReportWithControl("Section1", "PB1", "AR.PageBreak");

            string code = _generator.Generate(report);

            Assert.Contains("var PB1 = Section1.Controls[\"PB1\"] as PageBreak;", code);
        }

        /// <summary>
        /// AR.Barcode 型が Barcode にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArBarcode_MapsToBarcode()
        {
            var report = CreateReportWithControl("Section1", "BC1", "AR.Barcode");

            string code = _generator.Generate(report);

            Assert.Contains("var BC1 = Section1.Controls[\"BC1\"] as Barcode;", code);
        }

        /// <summary>
        /// AR.RichText 型が RichText にマッピングされることを確認します。
        /// </summary>
        [Fact]
        public void Generate_ArRichText_MapsToRichText()
        {
            var report = CreateReportWithControl("Section1", "RT1", "AR.RichText");

            string code = _generator.Generate(report);

            Assert.Contains("var RT1 = Section1.Controls[\"RT1\"] as RichText;", code);
        }

        /// <summary>
        /// 未知の型名の場合 ARControl にフォールバックすることを確認します。
        /// </summary>
        [Fact]
        public void Generate_UnknownType_FallsBackToARControl()
        {
            var report = CreateReportWithControl("Section1", "Ctrl1", "AR.Unknown");

            string code = _generator.Generate(report);

            Assert.Contains("var Ctrl1 = Section1.Controls[\"Ctrl1\"] as ARControl;", code);
        }

        /// <summary>
        /// 複数のセクションとコントロールが正しい順序で生成されることを確認します。
        /// </summary>
        [Fact]
        public void Generate_MultipleSectionsAndControls_ProducesCorrectOutput()
        {
            var report = new RpxReport();

            var section7 = new SectionInfo { Name = "Section7", SectionType = "GroupFooter" };
            section7.Controls.Add(new ControlInfo { Name = "Field28", TypeName = "AR.Field", SectionName = "Section7" });
            section7.Controls.Add(new ControlInfo { Name = "Field29", TypeName = "AR.Field", SectionName = "Section7" });
            report.Sections.Add(section7);

            string code = _generator.Generate(report);

            var lines = code.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal("var Section7 = _report.Sections[\"Section7\"];", lines[0].Trim());
            Assert.Equal("var Field28 = Section7.Controls[\"Field28\"] as TextBox;", lines[1].Trim());
            Assert.Equal("var Field29 = Section7.Controls[\"Field29\"] as TextBox;", lines[2].Trim());
        }

        /// <summary>
        /// 1 つのコントロールを持つ RpxReport を生成するヘルパーメソッド。
        /// </summary>
        private static RpxReport CreateReportWithControl(string sectionName, string controlName, string typeName)
        {
            var report = new RpxReport();
            var section = new SectionInfo { Name = sectionName, SectionType = "Detail" };
            section.Controls.Add(new ControlInfo { Name = controlName, TypeName = typeName, SectionName = sectionName });
            report.Sections.Add(section);
            return report;
        }
    }
}
