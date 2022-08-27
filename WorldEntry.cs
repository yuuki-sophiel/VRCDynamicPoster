using System;
using System.Text;
using System.IO;

namespace VRCDynamicPoster;

/// <summary>
/// 画像ごとのエントリを示す
/// </summary>
public class WorldEntry
{
    /// <summary>
    /// 通し番号
    /// </summary>
    public int Index { get; set; }
    /// <summary>
    /// 元画像の保存先
    /// </summary>
    public string ImageSrcPath { get; set; } = "";
    /// <summary>
    /// 画像の保存先ディレクトリ
    /// </summary>
    public string ImageDstDir { get; set; } = "";
    /// <summary>
    /// 公開先URL
    /// </summary>
    public string BaseUrl { get; set; } = "";
    /// <summary>
    /// 画像の元ファイル名
    /// </summary>
    public string ImageSrcName => Path.GetFileName(ImageSrcPath);
    /// <summary>
    /// 画像のCopy先ファイル名
    /// </summary>
    public string ImageDstName => $"{Index}.png";
    /// <summary>
    /// 画像保存先
    /// </summary>
    public string ImageDstPath => Path.Combine(ImageDstDir, ImageDstName);
    /// <summary>
    /// 画像ファイル名から抽出したWorldId
    /// </summary>
    public string Id => Path.GetFileNameWithoutExtension(ImageSrcPath);
    /// <summary>
    /// ブラウザで開く用のURL
    /// </summary>
    public string Url => BaseUrl.Replace("{worldId}", Id);

    /// <summary>
    /// Worldの画像とBlueprint IDの対応を示すEntry
    /// </summary>
    /// <param name="index">通し番号</param>
    /// <param name="srcImagePath">画像保存元</param>
    /// <param name="dstPath">Copy先</param>
    /// <param name="baseUrl">VRCのWorld baseUrl。 "{worldId}" 部分が置換される</param>
    public WorldEntry(int index, string srcImagePath, string dstDir, string baseUrl)
    {
        Index = index;
        ImageSrcPath = srcImagePath;
        ImageDstDir = dstDir;
        BaseUrl = baseUrl;
    }

    /// <summary>
    /// DstDirにファイルをコピー
    /// </summary>
    public void Copy()
    {
        File.Copy(ImageSrcPath, ImageDstPath);
    }

    public override string ToString() => $"#{Index}:{Id}";
}
