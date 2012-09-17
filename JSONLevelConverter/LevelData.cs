using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using SonicRetro.SonLVL;

namespace JSONLevelConverter
{
    internal static class LevelData
    {
        internal static MultiFileIndexer<byte[]> Tiles; 
        internal static EngineVersion TileFmt;
        internal static Compression.CompressionType TileCmp;
        internal static MultiFileIndexer<Block> Blocks;
        internal static EngineVersion BlockFmt;
        internal static Compression.CompressionType BlockCmp;
        internal static MultiFileIndexer<Chunk> Chunks;
        internal static EngineVersion ChunkFmt;
        internal static Compression.CompressionType ChunkCmp;
        internal static byte[,] FGLayout;
        internal static bool[,] FGLoop;
        internal static ColorPalette BmpPal;
        internal static bool[,] BGLoop;
        internal static EngineVersion LayoutFmt;
        internal static Compression.CompressionType LayoutCmp;
        internal static ushort[,] Palette;
        internal static byte[,] BGLayout;
        internal static EngineVersion PaletteFmt;
        internal static List<ObjectEntry> Objects;
        internal static EngineVersion ObjectFmt;
        internal static List<RingEntry> Rings;
        internal static EngineVersion RingFmt;
        internal static List<CNZBumperEntry> Bumpers;
        internal static List<byte> ColInds1, ColInds2;
        internal static Compression.CompressionType ColIndCmp;
        internal static sbyte[][] ColArr1, ColArr2;
        internal static byte[] Angles;
        internal static EngineVersion EngineVersion;
        internal static int chunksz;
        internal static bool littleendian;
        public static List<StartPositionEntry> StartPositions;
        internal static Dictionary<string, byte[]> filecache;

        internal static byte[] ReadFile(string file, Compression.CompressionType cmp)
        {
            if (filecache.ContainsKey(file))
                return filecache[file];
            else
            {
                byte[] val = Compression.Decompress(file, cmp);
                filecache.Add(file, val);
                return val;
            }
        }

        internal static sbyte[][] GenerateRotatedCollision()
        {
            sbyte[][] result = new sbyte[256][];
            for (int i = 0; i < 256; i++)
            {
                result[i] = new sbyte[16];
                for (int y = 0; y < 16; y++)
                {
                    sbyte height = 0;
                    int misses = 0;
                    bool f = false;
                    int st = 16;
                    for (int x = 0; x < 16; x++)
                    {
                        switch (Math.Sign(ColArr1[i][15 - x]))
                        {
                            case 1:
                                if (ColArr1[i][15 - x] >= 16 - y)
                                {
                                    height = (sbyte)(x + 1);
                                    misses = 0;
                                    if (!f)
                                    {
                                        f = true;
                                        st = x;
                                    }
                                }
                                else
                                    misses++;
                                break;
                            case 0:
                                misses++;
                                break;
                            case -1:
                                if (ColArr1[i][15 - x] <= -y - 1)
                                {
                                    height = (sbyte)(x + 1);
                                    misses = 0;
                                    if (!f)
                                    {
                                        f = true;
                                        st = x;
                                    }
                                }
                                else
                                    misses++;
                                break;
                        }
                        if (x == 0 & misses == 1)
                            break;
                        if (misses == 3)
                            break;
                    }
                    sbyte negheight = 0;
                    misses = 0;
                    f = false;
                    int rst = 16;
                    for (int x = 0; x < 16; x++)
                    {
                        switch (Math.Sign(ColArr1[i][x]))
                        {
                            case 1:
                                if (ColArr1[i][x] >= 16 - y)
                                {
                                    negheight = (sbyte)(-x - 1);
                                    misses = 0;
                                    if (!f)
                                    {
                                        f = true;
                                        rst = x;
                                    }
                                }
                                else
                                    misses++;
                                break;
                            case 0:
                                misses++;
                                break;
                            case -1:
                                if (ColArr1[i][x] <= -y - 1)
                                {
                                    negheight = (sbyte)(-x - 1);
                                    misses = 0;
                                    if (!f)
                                    {
                                        f = true;
                                        rst = x;
                                    }
                                }
                                else
                                    misses++;
                                break;
                        }
                        if (x == 0 & misses == 1)
                            break;
                        if (misses == 3)
                            break;
                    }
                    result[i][y] = height;
                    if (Math.Abs(negheight) > height || st > rst)
                        result[i][y] = negheight;
                    if (rst > st)
                        result[i][y] = height;
                }
            }
            return result;
        }


        internal static Color PaletteToColor(int line, int index, bool acceptTransparent)
        {
            if (acceptTransparent && index == 0)
                return Color.Transparent;
            return Color.FromArgb(
                (Palette[line, index] & 0xE) * 0x11,
                ((Palette[line, index] & 0xE0) >> 4) * 0x11,
                ((Palette[line, index] & 0xE00) >> 8) * 0x11
                );
        }


        internal static byte[] ASMToBin(string file)
        {
            return ASMToBin(file, 0);
        }

        internal static byte[] ASMToBin(string file, string label)
        {
            string[] fc = File.ReadAllLines(file);
            int sti = -1;
            for (int i = 0; i < fc.Length; i++)
            {
                if (fc[i].StartsWith(label + ":"))
                {
                    sti = i;
                    fc[i] = fc[i].Substring(label.Length + 1);
                }
            }
            if (sti == -1) return new byte[0];
            return ASMToBin(file, sti);
        }


        internal static byte[] ASMToBin(string file, int sti)
        {
            string[] fc = File.ReadAllLines(file);
            List<byte> result = new List<byte>();
            Dictionary<string, int> labels = new Dictionary<string, int>();
            int curaddr = 0;
            int st = 0;
            try
            {
                for (st = sti; st < fc.Length; st++)
                {
                    string[] ln = fc[st].Split(';')[0].Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (ln.Length == 0) continue;
                    if (!Char.IsWhiteSpace(fc[st], 0))
                    {
                        string[] l = ln[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        labels.Add(l[0], curaddr);
                        if (l.Length == 1)
                        {
                            string[] ln2 = new string[ln.Length - 1];
                            for (int i = 0; i < ln2.Length; i++)
                            {
                                ln2[i] = ln[i + 1];
                            }
                            ln = ln2;
                        }
                        else
                        {
                            ln[0] = l[1];
                        }
                        if (ln.Length == 0) continue;
                    }
                    if (!ln[0].StartsWith("dc.")) break;
                    string d = String.Empty;
                    for (int i = 1; i < ln.Length; i++)
                    {
                        d += ln[i];
                    }
                    string[] dats = d.Split(',');
                    switch (ln[0].Split('.')[1])
                    {
                        case "b":
                            curaddr += dats.Length;
                            break;
                        case "w":
                            curaddr += dats.Length * 2;
                            break;
                        case "l":
                            curaddr += dats.Length * 4;
                            break;
                    }
                }
                for (st = sti; st < fc.Length; st++)
                {
                    string[] ln = fc[st].Split(';')[0].Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (ln.Length == 0) continue;
                    if (!Char.IsWhiteSpace(fc[st], 0))
                    {
                        string[] l = ln[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (l.Length == 1)
                        {
                            string[] ln2 = new string[ln.Length - 1];
                            for (int i = 0; i < ln2.Length; i++)
                            {
                                ln2[i] = ln[i + 1];
                            }
                            ln = ln2;
                        }
                        else
                        {
                            ln[0] = l[1];
                        }
                        if (ln.Length == 0) continue;
                    }
                    if (!ln[0].StartsWith("dc.")) break;
                    string d = String.Empty;
                    for (int i = 1; i < ln.Length; i++)
                    {
                        d += ln[i];
                    }
                    string[] dats = d.Split(',');
                    switch (ln[0].Split('.')[1])
                    {
                        case "b":
                            foreach (string item in dats)
                                if (item.Contains("-"))
                                    if (item.StartsWith("-"))
                                        result.Add(ParseASMByte(item));
                                    else
                                        result.Add(unchecked((byte)ParseASMOffset(item, labels)));
                                else
                                    result.Add(ParseASMByte(item));
                            break;
                        case "w":
                            foreach (string item in dats)
                                if (item.Contains("-"))
                                    if (item.StartsWith("-"))
                                        result.AddRange(ByteConverter.GetBytes(ParseASMWord(item)));
                                    else
                                        result.AddRange(ByteConverter.GetBytes((short)ParseASMOffset(item, labels)));
                                else
                                    result.AddRange(ByteConverter.GetBytes(ParseASMWord(item)));
                            break;
                        case "l":
                            foreach (string item in dats)
                                if (item.Contains("-"))
                                    if (item.StartsWith("-"))
                                        result.AddRange(ByteConverter.GetBytes(ParseASMLong(item)));
                                    else
                                        result.AddRange(ByteConverter.GetBytes(ParseASMOffset(item, labels)));
                                else
                                    result.AddRange(ByteConverter.GetBytes(ParseASMLong(item)));
                            break;
                    }
                }
            }
            catch
            {
                throw new Exception("Error reading mappings file \"" + file + "\" at line " + st + ":");
            }
            return result.ToArray();
        }

        internal static byte ParseASMByte(string data)
        {
            if (data.StartsWith("-"))
            {
                if (data.StartsWith("-$"))
                    return unchecked((byte)-SByte.Parse(data.Substring(2), NumberStyles.HexNumber));
                else
                    return unchecked((byte)-SByte.Parse(data, NumberStyles.Integer, NumberFormatInfo.InvariantInfo));
            }
            else
            {
                if (data.StartsWith("$"))
                    return Byte.Parse(data.Substring(1), NumberStyles.HexNumber);
                else
                    return Byte.Parse(data, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
            }
        }

        internal static ushort ParseASMWord(string data)
        {
            if (data.StartsWith("-"))
            {
                if (data.StartsWith("-$"))
                    return unchecked((ushort)-Int16.Parse(data.Substring(2), NumberStyles.HexNumber));
                else
                    return unchecked((ushort)-Int16.Parse(data, NumberStyles.Integer, NumberFormatInfo.InvariantInfo));
            }
            else
            {
                if (data.StartsWith("$"))
                    return UInt16.Parse(data.Substring(1), NumberStyles.HexNumber);
                else
                    return UInt16.Parse(data, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
            }
        }

        internal static uint ParseASMLong(string data)
        {
            if (data.StartsWith("-"))
            {
                if (data.StartsWith("-$"))
                    return unchecked((uint)-Int32.Parse(data.Substring(2), NumberStyles.HexNumber));
                else
                    return unchecked((uint)-Int32.Parse(data, NumberStyles.Integer, NumberFormatInfo.InvariantInfo));
            }
            else
            {
                if (data.StartsWith("$"))
                    return UInt32.Parse(data.Substring(1), NumberStyles.HexNumber);
                else
                    return UInt32.Parse(data, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
            }
        }

        internal static int ParseASMOffset(string data, Dictionary<string, int> labels)
        {
            int label1 = 0;
            if (labels.ContainsKey(data.Split('-')[0]))
                label1 = labels[data.Split('-')[0]];
            int label2 = 0;
            if (labels.ContainsKey(data.Split('-')[1]))
                label2 = labels[data.Split('-')[1]];
            return label1 - label2;
        }

        internal static byte[] ProcessDPLC(byte[] artfile, DPLC dplc)
        {
            List<byte> result = new List<byte>();
            byte[] tmp;
            for (int i = 0; i < dplc.Count; i++)
            {
                tmp = new byte[dplc[i].TileCount * 0x20];
                Array.Copy(artfile, dplc[i].TileNum * 0x20, tmp, 0, tmp.Length);
                result.AddRange(tmp);
            }
            return result.ToArray();
        }

        internal static BitmapBits[] S2MapFrameDPLCToBmp(byte[] file, S2Mappings map, DPLC dplc, int startpal, out Point offset)
        {
            byte[] art = ProcessDPLC(file, dplc);
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            for (int i = 0; i < map.TileCount; i++)
            {
                left = Math.Min(map[i].X, left);
                right = Math.Max(map[i].X + (map[i].Width * 8), right);
                top = Math.Min(map[i].Y, top);
                bottom = Math.Max(map[i].Y + (map[i].Height * 8), bottom);
            }
            offset = new Point(left, top);
            BitmapBits[] bmp = new BitmapBits[] { new BitmapBits(right - left, bottom - top), new BitmapBits(right - left, bottom - top) };
            for (int i = map.TileCount - 1; i >= 0; i--)
            {
                BitmapBits pcbmp = new BitmapBits(map[i].Width * 8, map[i].Height * 8);
                int ti = 0;
                int pr = map[i].Tile.Priority ? 1 : 0;
                for (int x = 0; x < map[i].Width; x++)
                {
                    for (int y = 0; y < map[i].Height; y++)
                    {
                        pcbmp.DrawBitmapComposited(
                            ObjectHelper.TileToBmp8bpp(art, map[i].Tile.Tile + ti, (map[i].Tile.Palette + startpal) & 3),
                            new Point(x * 8, y * 8));
                        ti++;
                    }
                }
                pcbmp.Flip(map[i].Tile.XFlip, map[i].Tile.YFlip);
                bmp[pr].DrawBitmapComposited(pcbmp, new Point(map[i].X - left, map[i].Y - top));
            }
            return bmp;
        }

        internal static BitmapBits[] S2MapFrameToBmp(byte[] file, S2Mappings map, int startpal, out Point offset)
        {
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            for (int i = 0; i < map.TileCount; i++)
            {
                left = Math.Min(map[i].X, left);
                right = Math.Max(map[i].X + (map[i].Width * 8), right);
                top = Math.Min(map[i].Y, top);
                bottom = Math.Max(map[i].Y + (map[i].Height * 8), bottom);
            }
            offset = new Point(left, top);
            BitmapBits[] bmp = new BitmapBits[] { new BitmapBits(right - left, bottom - top), new BitmapBits(right - left, bottom - top) };
            for (int i = map.TileCount - 1; i >= 0; i--)
            {
                BitmapBits pcbmp = new BitmapBits(map[i].Width * 8, map[i].Height * 8);
                int ti = 0;
                int pr = map[i].Tile.Priority ? 1 : 0;
                for (int x = 0; x < map[i].Width; x++)
                {
                    for (int y = 0; y < map[i].Height; y++)
                    {
                        pcbmp.DrawBitmapComposited(
                            ObjectHelper.TileToBmp8bpp(file, map[i].Tile.Tile + ti, (map[i].Tile.Palette + startpal) & 3),
                            new Point(x * 8, y * 8));
                        ti++;
                    }
                }
                pcbmp.Flip(map[i].Tile.XFlip, map[i].Tile.YFlip);
                bmp[pr].DrawBitmapComposited(pcbmp, new Point(map[i].X - left, map[i].Y - top));
            }
            return bmp;
        }
        internal static BitmapBits TileToBmp8bpp(byte[] file, int index, int pal)
        {
            BitmapBits bmp = new BitmapBits(8, 8);
            if (file != null && index * 32 + 32 <= file.Length)
            {
                for (int i = 0; i < 32; i++)
                {
                    bmp.Bits[i * 2] = (byte)((file[i + (index * 32)] >> 4) + (pal * 16));
                    bmp.Bits[(i * 2) + 1] = (byte)((file[i + (index * 32)] & 0xF) + (pal * 16));
                    if (bmp.Bits[i * 2] % 16 == 0) bmp.Bits[i * 2] = 0;
                    if (bmp.Bits[(i * 2) + 1] % 16 == 0) bmp.Bits[(i * 2) + 1] = 0;
                }
            }
            return bmp;
        }


        internal static BitmapBits[] S1MapFrameToBmp(byte[] file, S1Mappings map, int startpal, out Point offset)
        {
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            for (int i = 0; i < map.TileCount; i++)
            {
                left = Math.Min(map[i].X, left);
                right = Math.Max(map[i].X + (map[i].Width * 8), right);
                top = Math.Min(map[i].Y, top);
                bottom = Math.Max(map[i].Y + (map[i].Height * 8), bottom);
            }
            offset = new Point(left, top);
            BitmapBits[] bmp = new BitmapBits[] { new BitmapBits(right - left, bottom - top), new BitmapBits(right - left, bottom - top) };
            for (int i = map.TileCount - 1; i >= 0; i--)
            {
                BitmapBits pcbmp = new BitmapBits(map[i].Width * 8, map[i].Height * 8);
                int ti = 0;
                int pr = map[i].Tile.Priority ? 1 : 0;
                for (int x = 0; x < map[i].Width; x++)
                {
                    for (int y = 0; y < map[i].Height; y++)
                    {
                        pcbmp.DrawBitmapComposited(
                            ObjectHelper.TileToBmp8bpp(file, map[i].Tile.Tile + ti, (map[i].Tile.Palette + startpal) & 3),
                            new Point(x * 8, y * 8));
                        ti++;
                    }
                }
                pcbmp.Flip(map[i].Tile.XFlip, map[i].Tile.YFlip);
                bmp[pr].DrawBitmapComposited(pcbmp, new Point(map[i].X - left, map[i].Y - top));
            }
            return bmp;
        }

        internal static BitmapBits[] S3KMapFrameToBmp(byte[] file, S3KMappings map, int startpal, out Point offset)
        {
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            for (int i = 0; i < map.TileCount; i++)
            {
                left = Math.Min(map[i].X, left);
                right = Math.Max(map[i].X + (map[i].Width * 8), right);
                top = Math.Min(map[i].Y, top);
                bottom = Math.Max(map[i].Y + (map[i].Height * 8), bottom);
            }
            offset = new Point(left, top);
            BitmapBits[] bmp = new BitmapBits[] { new BitmapBits(right - left, bottom - top), new BitmapBits(right - left, bottom - top) };
            for (int i = map.TileCount - 1; i >= 0; i--)
            {
                BitmapBits pcbmp = new BitmapBits(map[i].Width * 8, map[i].Height * 8);
                int ti = 0;
                int pr = map[i].Tile.Priority ? 1 : 0;
                for (int x = 0; x < map[i].Width; x++)
                {
                    for (int y = 0; y < map[i].Height; y++)
                    {
                        pcbmp.DrawBitmapComposited(
                            ObjectHelper.TileToBmp8bpp(file, map[i].Tile.Tile + ti, (map[i].Tile.Palette + startpal) & 3),
                            new Point(x * 8, y * 8));
                        ti++;
                    }
                }
                pcbmp.Flip(map[i].Tile.XFlip, map[i].Tile.YFlip);
                bmp[pr].DrawBitmapComposited(pcbmp, new Point(map[i].X - left, map[i].Y - top));
            }
            return bmp;
        }
        internal static BitmapBits[] S3KMapFrameDPLCToBmp(byte[] file, S3KMappings map, DPLC dplc, int startpal, out Point offset)
        {
            byte[] art = ProcessDPLC(file, dplc);
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            for (int i = 0; i < map.TileCount; i++)
            {
                left = Math.Min(map[i].X, left);
                right = Math.Max(map[i].X + (map[i].Width * 8), right);
                top = Math.Min(map[i].Y, top);
                bottom = Math.Max(map[i].Y + (map[i].Height * 8), bottom);
            }
            offset = new Point(left, top);
            BitmapBits[] bmp = new BitmapBits[] { new BitmapBits(right - left, bottom - top), new BitmapBits(right - left, bottom - top) };
            for (int i = map.TileCount - 1; i >= 0; i--)
            {
                BitmapBits pcbmp = new BitmapBits(map[i].Width * 8, map[i].Height * 8);
                int ti = 0;
                int pr = map[i].Tile.Priority ? 1 : 0;
                for (int x = 0; x < map[i].Width; x++)
                {
                    for (int y = 0; y < map[i].Height; y++)
                    {
                        pcbmp.DrawBitmapComposited(
                            TileToBmp8bpp(art, map[i].Tile.Tile + ti, (map[i].Tile.Palette + startpal) & 3),
                            new Point(x * 8, y * 8));
                        ti++;
                    }
                }
                pcbmp.Flip(map[i].Tile.XFlip, map[i].Tile.YFlip);
                bmp[pr].DrawBitmapComposited(pcbmp, new Point(map[i].X - left, map[i].Y - top));
            }
            return bmp;
        }

    }

    public enum EngineVersion
    {
        Invalid,
        S1,
        S2NA,
        S2,
        S3K,
        SCD,
        SCDPC,
        SKC
    }
}