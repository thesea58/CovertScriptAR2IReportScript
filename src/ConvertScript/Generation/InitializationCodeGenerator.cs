using System.Text;
using ConvertScript.Models;

namespace ConvertScript.Generation
{
    /// <summary>
    /// RpxReport オブジェクトから ActiveReports のセクションおよびコントロールを初期化する
    /// C# コードスニペットを生成するクラス。
    /// </summary>
    public class InitializationCodeGenerator
    {
        /// <summary>
        /// AR 型名から C# 型名へのマッピングテーブル。
        /// </summary>
        private static readonly Dictionary<string, string> ArTypeToCSharpType =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AR.Field",      "TextBox"    },
                { "AR.Label",      "Label"      },
                { "AR.Image",      "Picture"    },
                { "AR.Shape",      "Shape"      },
                { "AR.Line",       "Line"       },
                { "AR.CheckBox",   "CheckBox"   },
                { "AR.ReportInfo", "ReportInfo" },
                { "AR.SubReport",  "SubReport"  },
                { "AR.PageBreak",  "PageBreak"  },
                { "AR.Barcode",    "Barcode"    },
                { "AR.RichText",   "RichText"   },
            };

        /// <summary>
        /// RpxReport を解析し、セクションおよびコントロールの初期化コードスニペットを生成します。
        /// </summary>
        /// <param name="report">解析済みの RpxReport オブジェクト。</param>
        /// <param name="reportVarName">レポートオブジェクトの変数名。</param>
        /// <returns>生成された C# 初期化コード文字列。</returns>
        public string Generate(RpxReport report, string reportVarName = "_report")
        {
            var sb = new StringBuilder();

            foreach (SectionInfo section in report.Sections)
            {
                sb.AppendLine($"var {section.Name} = {reportVarName}.Sections[\"{section.Name}\"];");

                foreach (ControlInfo ctrl in section.Controls)
                {
                    string csharpType = GetCSharpType(ctrl.TypeName);
                    sb.AppendLine($"var {ctrl.Name} = {section.Name}.Controls[\"{ctrl.Name}\"] as {csharpType};");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// コントロールの型名から C# 型名を解決します。
        /// </summary>
        /// <param name="typeName">TypeName（例: "AR.Field", "TextBox" など）。</param>
        /// <returns>対応する C# 型名。不明な場合は "ARControl"。</returns>
        private static string GetCSharpType(string typeName)
        {
            if (ArTypeToCSharpType.TryGetValue(typeName, out string? mapped))
                return mapped ?? "ARControl";

            return "ARControl";
        }
    }
}
