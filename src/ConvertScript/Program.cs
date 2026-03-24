using ConvertScript.Generation;
using ConvertScript.Models;
using ConvertScript.Parsing;

/// <summary>
/// ActiveReports .rpx ファイルを IReportScript 実装の C# コードに変換するコンソールアプリケーション。
/// </summary>
/// <remarks>
/// 使用方法:
///   ConvertScript &lt;input.rpx&gt; [output.cs] [--namespace &lt;名前空間&gt;]
/// </remarks>

if (args.Length == 0)
{
    Console.Error.WriteLine("使用方法: ConvertScript <input.rpx> [output.cs] [--namespace <名前空間>]");
    Console.Error.WriteLine();
    Console.Error.WriteLine("引数:");
    Console.Error.WriteLine("  input.rpx        変換対象の .rpx ファイルパス（必須）");
    Console.Error.WriteLine("  output.cs        出力先の .cs ファイルパス（省略時は標準出力）");
    Console.Error.WriteLine("  --namespace <名> 生成クラスのネームスペース（省略時: KKReport.Scripts.Generated）");
    return 1;
}

// 引数解析
string inputFile = args[0];
string? outputFile = null;
string namespaceName = "KKReport.Scripts.Generated";

for (int i = 1; i < args.Length; i++)
{
    if (args[i] == "--namespace" && i + 1 < args.Length)
    {
        namespaceName = args[++i];
    }
    else if (!args[i].StartsWith("--"))
    {
        outputFile = args[i];
    }
}

// 入力ファイルの存在確認
if (!File.Exists(inputFile))
{
    Console.Error.WriteLine($"エラー: ファイルが見つかりません: {inputFile}");
    return 2;
}

try
{
    // 1. .rpx ファイルの解析
    var parser = new RpxParser();
    RpxReport report = parser.Parse(inputFile);

    // 2. 分析結果の出力（標準エラー）
    PrintAnalysisResult(report);

    // 3. C# コードの生成
    var generator = new CSharpCodeGenerator();
    string generatedCode = generator.Generate(report, namespaceName);

    // 4. 出力
    if (outputFile != null)
    {
        File.WriteAllText(outputFile, generatedCode);
        Console.Error.WriteLine($"出力完了: {outputFile}");
    }
    else
    {
        Console.WriteLine(generatedCode);
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"変換エラー: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 3;
}

/// <summary>
/// 分析結果を標準エラーに出力します。
/// </summary>
static void PrintAnalysisResult(RpxReport report)
{
    Console.Error.WriteLine("=== A. 分析結果 ===");
    Console.Error.WriteLine($"レポート名: {report.ReportName}");
    Console.Error.WriteLine($"スクリプト言語: {report.ScriptLanguage}");
    Console.Error.WriteLine();

    Console.Error.WriteLine("--- セクション一覧 ---");
    foreach (var section in report.Sections)
    {
        Console.Error.WriteLine($"  [{section.SectionType}] {section.Name}");
        foreach (var ctrl in section.Controls)
        {
            Console.Error.WriteLine($"    - ({ctrl.TypeName}) {ctrl.Name}");
        }
    }
    Console.Error.WriteLine();

    // レポートイベント
    var reportEvents = report.EventBindings
        .Where(b => b.ObjectType == EventObjectType.Report)
        .ToList();
    if (reportEvents.Count > 0)
    {
        Console.Error.WriteLine("--- レポートレベルイベント ---");
        foreach (var ev in reportEvents)
        {
            Console.Error.WriteLine($"  report.{ev.EventName} -> {ev.HandlerName}");
        }
        Console.Error.WriteLine();
    }

    // セクションイベント
    var sectionEvents = report.EventBindings
        .Where(b => b.ObjectType == EventObjectType.Section)
        .ToList();
    if (sectionEvents.Count > 0)
    {
        Console.Error.WriteLine("--- セクションレベルイベント ---");
        foreach (var ev in sectionEvents)
        {
            Console.Error.WriteLine($"  {ev.ObjectName}.{ev.EventName} -> {ev.HandlerName}");
        }
        Console.Error.WriteLine();
    }

    // コントロールイベント
    var controlEvents = report.EventBindings
        .Where(b => b.ObjectType == EventObjectType.Control)
        .ToList();
    if (controlEvents.Count > 0)
    {
        Console.Error.WriteLine("--- コントロールレベルイベント ---");
        foreach (var ev in controlEvents)
        {
            Console.Error.WriteLine($"  {ev.ObjectName}.{ev.EventName} -> {ev.HandlerName}");
        }
        Console.Error.WriteLine();
    }

    Console.Error.WriteLine("=== B. 変換方針 ===");
    Console.Error.WriteLine("スクリプトレベル変数 → クラスのプライベートフィールドに変換");
    Console.Error.WriteLine("イベントハンドラーメソッド → クラスのプライベートメソッドに変換");
    Console.Error.WriteLine("AttachEvents メソッド内でセクション・コントロールを取得し、イベントをアタッチ");
    Console.Error.WriteLine();
}
