using System;
using System.Collections.Generic;

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
        internal static byte[,] BGLayout;
        internal static bool[,] BGLoop;
        internal static EngineVersion LayoutFmt;
        internal static Compression.CompressionType LayoutCmp;
        internal static ushort[,] Palette;
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