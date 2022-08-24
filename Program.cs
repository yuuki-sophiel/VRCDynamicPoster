using System;
using System.Diagnostics;
using System.CodeDom;

namespace VRCDynamicPoster;
public class Program
{
    public static readonly string ASSET_WORLD_PATH = "asset/world";
    public static readonly string ASSET_SEARCH_PATTERN = "wrld_*";
    public static readonly string VRC_WORLD_BASE_URL = "https://vrchat.com/home/launch?worldId={worldId}";

    public static readonly string DST_DIR = "dst";
    public static readonly string PREVIEW_HTML_NAME = "index.html";
    public static readonly string PREVIEW_IMAGE_NAME = "index.bmp";
    private static void Main(string[] args)
    {
        // デフォルト or コマンドラインから設定
        var srcDir = args.Length > 2 ? args[1] : ASSET_WORLD_PATH;
        var pattern = args.Length > 3 ? args[2] : ASSET_SEARCH_PATTERN;
        var baseUrl = args.Length > 4 ? args[3] : VRC_WORLD_BASE_URL;
        // 現状帰る必要なさそう
        var dstDir = DST_DIR;
        var previewHtmlName = PREVIEW_HTML_NAME;
        var previewImageName = PREVIEW_IMAGE_NAME;

        Console.WriteLine("== VRCDynamicPoster ==");
        Console.WriteLine($"{nameof(srcDir)}={srcDir}");
        Console.WriteLine($"{nameof(pattern)}={pattern}");
        Console.WriteLine($"{nameof(baseUrl)}={baseUrl}");
        Console.WriteLine($"{nameof(dstDir)}={dstDir}");
        Console.WriteLine($"{nameof(previewHtmlName)}={previewHtmlName}");
        Console.WriteLine($"{nameof(previewImageName)}={previewImageName}");

        // 前回出力を破棄
        if (Directory.Exists(dstDir))
        {
            Directory.Delete(dstDir, true);
        }
        Directory.CreateDirectory(dstDir);

        // ファイル一覧を取得
        var entries =
            Directory.GetFiles(srcDir, pattern)
                     .Select(x => new WorldEntry(x, baseUrl))
                     .ToArray();
        Console.WriteLine("encode_files={");
        foreach (var e in entries)
        {
            Console.WriteLine($"\t{e},");
        }
        Console.WriteLine("}");

        // Preview生成
        var preview = new Preview(entries);
        var htmlText = preview.TransformText();
        File.WriteAllText($"{dstDir}/{previewHtmlName}", htmlText);
        Console.WriteLine($"preview page: {dstDir}/{previewHtmlName}");

        // asset/worldの画像コピー
        foreach (var e in entries)
        {
            File.Copy(e.ImagePath, $"{dstDir}/{e.ImageFileName}");
            Console.WriteLine($"copy: {e.ImagePath} => {dstDir}/{e.ImageFileName}");
        }

        // エンコードした画像を生成
        TextImageGenerator.Encode($"{dstDir}/{previewImageName}", entries);
        Console.WriteLine($"encode image: {dstDir}/{previewImageName}");

        // デコードテスト
        var decodeEntries = TextImageGenerator.Decode($"{dstDir}/{previewImageName}").ToArray();
        bool isVerifyPass = true;
        foreach (var (encode, decode) in entries.Zip(decodeEntries))
        {
            var isEquals = encode.Id.Equals(decode);
            if (!isEquals)
            {
                isVerifyPass = false;
                Console.WriteLine($"[FAIL] encode: {encode.Id}, decode: {decode}");
            }
        }
        Console.WriteLine($"decode test: {(isVerifyPass ? "PASS" : "FAIL")}");
        Console.WriteLine();

        Console.WriteLine("All processes are complete.");
    }
}