using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace VRCDynamicPoster;

public static class TextImageGenerator
{
    /// <summary>
    /// 1byteあたりのbit数。変更不可
    /// </summary>
    public const int BIT_PER_DATA = 8;
    /// <summary>
    /// 色の段階しきい値
    /// </summary>
    public const int COLOR_THRESHOLD = 128;
    /// <summary>
    /// データあたりの幅
    /// </summary>
    public static readonly int WIDTH_PER_BIT = 4;
    /// <summary>
    /// データあたりの高さ
    /// </summary>
    public static readonly int HEIGHT_PER_BIT = 4;
    /// <summary>
    /// 最低画像高さを横幅のどの程度にするか
    /// </summary>
    public static readonly float MIN_IMAGE_HEIGHT_RATIO = 0.1f;
    /// <summary>
    /// 背景色。値なし
    /// </summary>
    public static readonly Color NONE_COLOR = Color.Lime;
    /// <summary>
    /// 0
    /// </summary>
    public static readonly Color FALSE_COLOR = Color.Black;
    /// <summary>
    /// 1
    /// </summary>
    public static readonly Color TRUE_COLOR = Color.White;

    /// <summary>
    /// 文字列からbyte array作成
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    static byte[] EncodeASCII(string src)
        => Encoding.ASCII.GetBytes(src);

    /// <summary>
    /// byte arrayから文字列を作成
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    static string DecodeASCII(byte[] src)
        => string.Join("", Encoding.ASCII.GetChars(src));


    /// <summary>
    /// World Entryを画像にエンコード
    /// </summary>
    /// <param name="dstFilePath">保存先</param>
    /// <param name="entries">対象</param>
    public static void Encode(string dstFilePath, IEnumerable<WorldEntry> entries)
    {
        // 画像サイズ決定
        var dataEntries = entries.Select(x => EncodeASCII(x.Id)).ToArray();
        var dataWidth = dataEntries.Max(x => x.Length);
        var dataHeight = dataEntries.Length;
        if (dataWidth == 0 || dataHeight == 0)
            throw new ArgumentException($"エントリが空 {nameof(dataWidth)}={dataWidth}, {nameof(dataHeight)}={dataHeight}");

        // 画像生成
        var imageWidth = dataWidth * WIDTH_PER_BIT * BIT_PER_DATA;
        var imageHeight = Math.Max(dataHeight * HEIGHT_PER_BIT, (int)Math.Ceiling(imageWidth * MIN_IMAGE_HEIGHT_RATIO)); // 生成画像のアス比が極端にならないように
        using Image<Rgba32> image = new(imageWidth, imageHeight);
        image.Mutate(x =>
        {
            // 0を黒で塗りつぶし。1を白で塗りつぶし。背景色は緑
            x.Fill(NONE_COLOR);
            for (int entryIndex = 0; entryIndex < dataHeight; entryIndex++)
            {
                var targetEntry = dataEntries[entryIndex];
                var pixelY = entryIndex * HEIGHT_PER_BIT;
                for (int dataIndex = 0; dataIndex < targetEntry.Length; dataIndex++)
                {
                    var targetData = targetEntry[dataIndex];
                    for (int bitIndex = 0; bitIndex < BIT_PER_DATA; bitIndex++)
                    {
                        var targetBit = ((targetData >> bitIndex) & 0x1) != 0x0;
                        // 1なら白、0から黒。値がなければ透明
                        var color = (targetBit) ? TRUE_COLOR : FALSE_COLOR;
                        // 1byteごとに1byte*8bit/byte*4pixel、bitごとに4pixel
                        var pixelX =
                            (dataIndex * BIT_PER_DATA * WIDTH_PER_BIT)
                            + (bitIndex * WIDTH_PER_BIT);
                        var rect = new RectangleF(pixelX, pixelY, WIDTH_PER_BIT, HEIGHT_PER_BIT);
                        x.Fill(color, rect);
                    }
                }
            }
        });
        image.Save(dstFilePath);
    }

    /// <summary>
    /// TextImageGenerator.Encodeで生成した画像から文字列を復元
    /// </summary>
    /// <param name="imagePath"></param>
    /// <returns></returns>
    public static IEnumerable<string> Decode(string imagePath)
    {
        using var image = Image.Load<Rgba32>(imagePath);
        // 1byte=>8bit*4pixel/bit=>32pixel
        var dataCount = image.Width / BIT_PER_DATA / WIDTH_PER_BIT;
        // 1entry=>1entry*4pixel/entry=>4pixel
        var entryCount = image.Height / WIDTH_PER_BIT;

        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            // height/bitの中点を舐めていく
            var dstDatas = new List<int>(entryCount); // 文字列の元データ
            var pixelY = entryIndex * WIDTH_PER_BIT + WIDTH_PER_BIT / 2;
            for (int dataIndex = 0; dataIndex < dataCount; dataIndex++)
            {
                byte dstData = 0x0;
                for (int bitIndex = 0; bitIndex < BIT_PER_DATA; bitIndex++)
                {
                    // byte単位でシフト + bit単位でシフト + width/pixelの中点
                    var pixelX =
                        (dataIndex * BIT_PER_DATA * WIDTH_PER_BIT)
                        + (bitIndex * WIDTH_PER_BIT)
                        + (WIDTH_PER_BIT / 2);
                    var pixel = image[pixelX, pixelY];

                    // 劣化する可能性があるので、しきい値を設けて判定したほうが良い
                    if (pixel.R < COLOR_THRESHOLD && pixel.G > COLOR_THRESHOLD && pixel.B < COLOR_THRESHOLD)
                    {
                        // 緑。これ以上は空白なので処理不要
                        yield break;
                    }
                    else if (pixel.R < COLOR_THRESHOLD && pixel.G < COLOR_THRESHOLD && pixel.B < COLOR_THRESHOLD)
                    {
                        // 黒
                    }
                    else if (pixel.R > COLOR_THRESHOLD && pixel.G > COLOR_THRESHOLD && pixel.B > COLOR_THRESHOLD)
                    {
                        // 白
                        dstData |= (byte)(0x1 << bitIndex);
                    }
                    else
                    {
                        throw new InvalidDataException($"NONE, TRUE, FALSE以外のpixel値を検出. {nameof(pixelX)}={pixelX}, {nameof(pixelY)}={pixelY}, pixel={pixel}");
                    }
                }
                // 出来たデータをリトルエンディアンで組み立てる
                dstDatas.Add(dstData);
            }
            // 1行のデータがParseし終わったので文字列にする. 全部NONE_COLORの場合は無視
            if (dstDatas.Count > 0)
            {
                var dstStr = DecodeASCII(dstDatas.Select(x => (byte)x).ToArray());
                yield return dstStr;
            }
        }
    }
}