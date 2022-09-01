using System;
using System.Text;
using System.IO;

namespace VRCDynamicPoster;


public enum Category
{
    None, World, Avatar
}

/// <summary>
/// 画像ごとのエントリを示す
/// </summary>
public class Entry
{
    /// <summary>
    /// 種類
    /// </summary>
    public Category Category { get; set; } = Category.None;
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
    /// 画像ファイル名から抽出したId
    /// </summary>
    public string Id => Path.GetFileNameWithoutExtension(ImageSrcPath);
    /// <summary>
    /// ブラウザで開く用のURL
    /// </summary>
    public string Url => BaseUrl.Replace("{id}", Id);

    /// <summary>
    /// 画像とBlueprint IDの対応を示すEntry
    /// </summary>
    /// <param name="index">通し番号</param>
    /// <param name="srcImagePath">画像保存元</param>
    /// <param name="dstPath">Copy先</param>
    /// <param name="worldBaseUrl">VRCのWorld baseUrl。 "{id}" 部分が置換される</param>
    /// <param name="avatarBaseUrl">VRCのAvatar baseUrl。 "{id}" 部分が置換される</param>
    public Entry(int index, string srcImagePath, string dstDir, string worldBaseUrl, string avatarBaseUrl)
    {
        Index = index;
        ImageSrcPath = srcImagePath;
        ImageDstDir = dstDir;
        if (ImageSrcName.IndexOf("wrld_") != -1)
        {
            Category = Category.World;
            BaseUrl = worldBaseUrl;
        }
        else if (ImageSrcName.IndexOf("avtr_") != -1)
        {
            Category = Category.Avatar;
            BaseUrl = avatarBaseUrl;
        }
        else
        {
            throw new ArgumentException($"invalid filename. {nameof(srcImagePath)}={srcImagePath}");
        }
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
