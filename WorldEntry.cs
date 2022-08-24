using System;
using System.Text;

namespace VRCDynamicPoster;

/// <summary>
/// 画像ごとのエントリを示す
/// </summary>
public class WorldEntry
{
    /// <summary>
    /// 画像の保存先
    /// </summary>
    public string ImagePath { get; internal set; } = "";
    /// <summary>
    /// 画像のファイル名
    /// </summary>
    public string ImageFileName { get; internal set; } = "";
    /// <summary>
    /// 画像ファイル名から抽出したWorldId
    /// </summary>
    public string Id { get; internal set; } = "";
    /// <summary>
    /// ブラウザで開く用のURL
    /// </summary>
    public string Url { get; internal set; } = "";

    /// <summary>
    /// Worldの画像とBlueprint IDの対応を示すEntry
    /// </summary>
    /// <param name="imagePath">画像保存先</param>
    /// <param name="baseUrl">VRCのWorld baseUrl。 "{worldId}" 部分が置換される</param>
    public WorldEntry(string imagePath, string baseUrl)
    {
        ImagePath = imagePath;
        ImageFileName = System.IO.Path.GetFileName(imagePath);
        Id = System.IO.Path.GetFileNameWithoutExtension(imagePath);
        Url = baseUrl.Replace("{worldId}", Id);
    }

    public override string ToString() => Id;
}
