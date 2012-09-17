using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using JSONLevelConverter;

namespace SonicRetro.SonLVL
{
    public struct Sprite
    {
        public Point Offset;
        public BitmapBits Image;
        public int X { get { return Offset.X; } set { Offset.X = value; } }
        public int Y { get { return Offset.Y; } set { Offset.Y = value; } }
        public int Width { get { return Image.Width; } }
        public int Height { get { return Image.Height; } }
        public Size Size { get { return Image.Size; } }
        public int Left { get { return X; } }
        public int Top { get { return Y; } }
        public int Right { get { return X + Width; } }
        public int Bottom { get { return Y + Height; } }
        public Rectangle Bounds { get { return new Rectangle(Offset, Size); } }

        public Sprite(BitmapBits spr, Point off)
        {
            Image = spr;
            Offset = off;
        }

        public Sprite(params Sprite[] sprites)
        {
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                left = Math.Min(sprites[i].Left, left);
                right = Math.Max(sprites[i].Right, right);
                top = Math.Min(sprites[i].Top, top);
                bottom = Math.Max(sprites[i].Bottom, bottom);
            }
            Offset = new Point(left, top);
            Image = new BitmapBits(right - left, bottom - top);
            for (int i = 0; i < sprites.Length; i++)
                Image.DrawBitmapComposited(sprites[i].Image, new Point(sprites[i].X - left, sprites[i].Y - top));
        }
    }
    public class BitmapBits
    {
        public byte[] Bits { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Size Size
        {
            get
            {
                return new Size(Width, Height);
            }
        }

        public byte this[int x, int y]
        {
            get
            {
                return Bits[(y * Width) + x];
            }
            set
            {
                Bits[(y * Width) + x] = value;
            }
        }

        public BitmapBits(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new byte[width * height];
        }

        public BitmapBits(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new ArgumentException();
            BitmapData bmpd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            Width = bmpd.Width;
            Height = bmpd.Height;
            byte[] tmpbits = new byte[Math.Abs(bmpd.Stride) * bmpd.Height];
            Marshal.Copy(bmpd.Scan0, tmpbits, 0, tmpbits.Length);
            Bits = new byte[Width * Height];
            int j = 0;
            for (int i = 0; i < Bits.Length; i += Width)
            {
                Array.Copy(tmpbits, j, Bits, i, Width);
                j += Math.Abs(bmpd.Stride);
            }
            bmp.UnlockBits(bmpd);
        }

        public BitmapBits(BitmapBits source)
        {
            Width = source.Width;
            Height = source.Height;
            Bits = new byte[source.Bits.Length];
            Array.Copy(source.Bits, Bits, Bits.Length);
        }

        public Bitmap ToBitmap()
        {
            Bitmap newbmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            BitmapData newbmpd = newbmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] bmpbits = new byte[Math.Abs(newbmpd.Stride) * newbmpd.Height];
            for (int y = 0; y < Height; y++)
                Array.Copy(Bits, y * Width, bmpbits, y * Math.Abs(newbmpd.Stride), Width);
            Marshal.Copy(bmpbits, 0, newbmpd.Scan0, bmpbits.Length);
            newbmp.UnlockBits(newbmpd);
            return newbmp;
        }

        public Bitmap ToBitmap(ColorPalette palette)
        {
            Bitmap newbmp = ToBitmap();
            newbmp.Palette = palette;
            return newbmp;
        }

        public Bitmap ToBitmap(params Color[] palette)
        {
            Bitmap newbmp = ToBitmap();
            ColorPalette pal = newbmp.Palette;
            for (int i = 0; i < Math.Min(palette.Length, 256); i++)
                pal.Entries[i] = palette[i];
            newbmp.Palette = pal;
            return newbmp;
        }

        public void DrawBitmap(BitmapBits source, Point location)
        {
            int dstx = location.X; int dsty = location.Y;
            for (int i = 0; i < source.Height; i++)
            {
                int di = ((dsty + i) * Width) + dstx;
                int si = i * source.Width;
                Array.Copy(source.Bits, si, Bits, di, source.Width);
            }
        }

        public void DrawBitmapComposited(BitmapBits source, Point location)
        {
            int srcl = 0;
            if (location.X < 0)
                srcl = -location.X;
            int srct = 0;
            if (location.Y < 0)
                srct = -location.Y;
            int srcr = source.Width;
            if (srcr > Width - location.X)
                srcr = Width - location.X;
            int srcb = source.Height;
            if (srcb > Height - location.Y)
                srcb = Height - location.Y;
            for (int i = srct; i < srcb; i++)
                for (int x = srcl; x < srcr; x++)
                    if (source[x, i] != 0)
                        this[location.X + x, location.Y + i] = source[x, i];
        }

        public void Flip(bool XFlip, bool YFlip)
        {
            if (!XFlip & !YFlip)
                return;
            if (XFlip)
            {
                for (int y = 0; y < Height; y++)
                {
                    int addr = y * Width;
                    Array.Reverse(Bits, addr, Width);
                }
            }
            if (YFlip)
            {
                byte[] tmppix = new byte[Bits.Length];
                for (int y = 0; y < Height; y++)
                {
                    int srcaddr = y * Width;
                    int dstaddr = (Height - y - 1) * Width;
                    Array.Copy(Bits, srcaddr, tmppix, dstaddr, Width);
                }
                Bits = tmppix;
            }
        }

        public void Clear()
        {
            Array.Clear(Bits, 0, Bits.Length);
        }

        public static BitmapBits FromTile(byte[] art, int index)
        {
            BitmapBits bmp = new BitmapBits(8, 8);
            if (index * 32 + 32 <= art.Length)
            {
                for (int i = 0; i < 32; i++)
                {
                    bmp.Bits[i * 2] = (byte)(art[i + (index * 32)] >> 4);
                    bmp.Bits[(i * 2) + 1] = (byte)(art[i + (index * 32)] & 0xF);
                }
            }
            return bmp;
        }

        public byte[] ToTile()
        {
            List<byte> res = new List<byte>();
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x += 2)
                    res.Add((byte)(((this[x, y] & 0xF) << 4) | (this[x + 1, y] & 0xF)));
            return res.ToArray();
        }

        public void Rotate(int R)
        {
            byte[] tmppix = new byte[Bits.Length];
            switch (R)
            {
                case 1:
                    for (int y = 0; y < Height; y++)
                    {
                        int srcaddr = y * Width;
                        int dstaddr = (Width * (Width - 1)) + y;
                        for (int x = 0; x < Width; x++)
                        {
                            tmppix[dstaddr] = Bits[srcaddr + x];
                            dstaddr -= Width;
                        }
                    }
                    Bits = tmppix;
                    int h = Height;
                    Height = Width;
                    Width = h;
                    break;
                case 2:
                    Flip(true, true);
                    break;
                case 3:
                    for (int y = 0; y < Height; y++)
                    {
                        int srcaddr = y * Width;
                        int dstaddr = Height - 1 - y;
                        for (int x = 0; x < Width; x++)
                        {
                            tmppix[dstaddr] = Bits[srcaddr + x];
                            dstaddr += Width;
                        }
                    }
                    Bits = tmppix;
                    h = Height;
                    Height = Width;
                    Width = h;
                    break;
            }
        }

        public BitmapBits Scale(int factor)
        {
            BitmapBits res = new BitmapBits(Width * factor, Height * factor);
            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[x, y] = this[(x / factor), (y / factor)];
            return res;
        }

        public void DrawLine(byte index, int x1, int y1, int x2, int y2)
        {
            bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            if (steep)
            {
                int tmp = x1;
                x1 = y1;
                y1 = tmp;
                tmp = x2;
                x2 = y2;
                y2 = tmp;
            }
            if (x1 > x2)
            {
                int tmp = x1;
                x1 = x2;
                x2 = tmp;
                tmp = y1;
                y1 = y2;
                y2 = tmp;
            }
            int deltax = x2 - x1;
            int deltay = Math.Abs(y2 - y1);
            double error = 0;
            double deltaerr = (double)deltay / (double)deltax;
            int ystep;
            int y = y1;
            if (y1 < y2) ystep = 1; else ystep = -1;
            for (int x = x1; x <= x2; x++)
            {
                if (steep)
                {
                    if (x >= 0 & x < Height & y >= 0 & y < Width)
                        this[y, x] = index;
                }
                else
                {
                    if (y >= 0 & y < Height & x >= 0 & x < Width)
                        this[x, y] = index;
                }
                error = error + deltaerr;
                if (error >= 0.5)
                {
                    y = y + ystep;
                    error = error - 1.0;
                }
            }
        }

        public void DrawLine(byte index, Point p1, Point p2) { DrawLine(index, p1.X, p1.Y, p2.X, p2.Y); }

        public void DrawRectangle(byte index, int x, int y, int width, int height)
        {
            DrawLine(index, x, y, x + width, y);
            DrawLine(index, x, y, x, y + height);
            DrawLine(index, x + width, y, x + width, y + height);
            DrawLine(index, x, y + height, x + width, y + height);
        }

        public void DrawRectangle(byte index, Rectangle rect) { DrawRectangle(index, rect.X, rect.Y, rect.Width, rect.Height); }

        public void FillRectangle(byte index, int x, int y, int width, int height)
        {
            int srcl = 0;
            if (x < 0)
                srcl = -x;
            int srct = 0;
            if (y < 0)
                srct = -y;
            int srcr = width;
            if (srcr > Width - x)
                srcr = Width - x;
            int srcb = height;
            if (srcb > Height - y)
                srcb = Height - y;
            for (int cy = srct; cy < srcb; cy++)
                for (int cx = srcl; cx < srcr; cx++)
                    this[cx, cy] = index;
        }

        public void FillRectangle(byte index, Rectangle rect) { DrawRectangle(index, rect.X, rect.Y, rect.Width, rect.Height); }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) return true;
            BitmapBits other = obj as BitmapBits;
            if (other == null) return false;
            if (Width != other.Width | Height != other.Height) return false;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (this[x, y] != other[x, y])
                        return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    internal class S2MappingsTile
    {
        public sbyte Y { get; private set; }
        public byte Width { get; private set; }
        public byte Height { get; private set; }
        public PatternIndex Tile { get; private set; }
        public PatternIndex Tile2 { get; private set; }
        public short X { get; private set; }

        public static int Size { get { return 8; } }

        public S2MappingsTile(byte[] file, int address)
        {
            Y = unchecked((sbyte)file[address]);
            Width = (byte)(((file[address + 1] & 0xC) >> 2) + 1);
            Height = (byte)((file[address + 1] & 0x3) + 1);
            Tile = new PatternIndex(file, address + 2);
            Tile2 = new PatternIndex(file, address + 4);
            X = ByteConverter.ToInt16(file, address + 6);
        }
    }

    internal class S2Mappings
    {
        private S2MappingsTile[] Tiles { get; set; }
        public S2MappingsTile this[int index]
        {
            get { return Tiles[index]; }
        }

        public int TileCount { get { return Tiles.Length; } }

        public int Size { get { return (Tiles.Length * S2MappingsTile.Size) + 2; } }

        public S2Mappings(byte[] file, int address)
        {
            Tiles = new S2MappingsTile[ByteConverter.ToUInt16(file, address)];
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new S2MappingsTile(file, (i * S2MappingsTile.Size) + address + 2);
            }
        }
    }

    internal class S1MappingsTile
    {
        public sbyte Y { get; private set; }
        public byte Width { get; private set; }
        public byte Height { get; private set; }
        public PatternIndex Tile { get; private set; }
        public sbyte X { get; private set; }

        public static int Size { get { return 5; } }

        public S1MappingsTile(byte[] file, int address)
        {
            Y = unchecked((sbyte)file[address]);
            Width = (byte)(((file[address + 1] & 0xC) >> 2) + 1);
            Height = (byte)((file[address + 1] & 0x3) + 1);
            Tile = new PatternIndex(file, address + 2);
            X = unchecked((sbyte)file[address + 4]);
        }
    }

    internal class S1Mappings
    {
        private S1MappingsTile[] Tiles { get; set; }
        public S1MappingsTile this[int index]
        {
            get { return Tiles[index]; }
        }

        public int TileCount { get { return Tiles.Length; } }

        public int Size { get { return (Tiles.Length * S1MappingsTile.Size) + 1; } }

        public S1Mappings(byte[] file, int address)
        {
            Tiles = new S1MappingsTile[file[address]];
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new S1MappingsTile(file, (i * S1MappingsTile.Size) + address + 1);
            }
        }
    }

    internal class S3KMappingsTile
    {
        public sbyte Y { get; private set; }
        public byte Width { get; private set; }
        public byte Height { get; private set; }
        public PatternIndex Tile { get; private set; }
        public short X { get; private set; }

        public static int Size { get { return 6; } }

        public S3KMappingsTile(byte[] file, int address)
        {
            Y = unchecked((sbyte)file[address]);
            Width = (byte)(((file[address + 1] & 0xC) >> 2) + 1);
            Height = (byte)((file[address + 1] & 0x3) + 1);
            Tile = new PatternIndex(file, address + 2);
            X = ByteConverter.ToInt16(file, address + 4);
        }
    }

    internal class S3KMappings
    {
        private S3KMappingsTile[] Tiles { get; set; }
        public S3KMappingsTile this[int index]
        {
            get { return Tiles[index]; }
        }

        public int TileCount { get { return Tiles.Length; } }

        public int Size { get { return (Tiles.Length * S3KMappingsTile.Size) + 2; } }

        public S3KMappings(byte[] file, int address)
        {
            Tiles = new S3KMappingsTile[ByteConverter.ToUInt16(file, address)];
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new S3KMappingsTile(file, (i * S3KMappingsTile.Size) + address + 2);
            }
        }
    }


    internal class DPLCEntry
    {
        public byte TileCount { get; set; }
        public ushort TileNum { get; set; }

        public static int Size { get { return 2; } }

        public DPLCEntry(byte[] file, int address, EngineVersion version)
        {
            switch (version)
            {
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    TileNum = ByteConverter.ToUInt16(file, address);
                    TileCount = (byte)((TileNum >> 12) + 1);
                    TileNum &= 0xFFF;
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    TileNum = ByteConverter.ToUInt16(file, address);
                    TileCount = (byte)((TileNum & 0xF) + 1);
                    TileNum = (ushort)(TileNum >> 4);
                    break;
            }
        }
    }

    internal class DPLC
    {
        private DPLCEntry[] Tiles { get; set; }
        public DPLCEntry this[int index]
        {
            get { return Tiles[index]; }
        }

        public int Count { get { return Tiles.Length; } }

        public int Size { get { return (Tiles.Length * DPLCEntry.Size) + 2; } }

        public DPLC(byte[] file, int address, EngineVersion version)
        {
            Tiles = new DPLCEntry[ByteConverter.ToUInt16(file, address) + 1];
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new DPLCEntry(file, (i * DPLCEntry.Size) + address + 2, version);
            }
        }
    }
    public static class ObjectHelper
    {
        public static byte[] OpenArtFile(string file, Compression.CompressionType comp) { return LevelData.ReadFile(file, comp); }

        public static Sprite MapToBmp(byte[] artfile, byte[] mapfile, int frame, int startpal)
        {
            BitmapBits[] bmp = null;
            Point off = new Point();
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S1:
                    S1Mappings s1map = new S1Mappings(mapfile, ByteConverter.ToInt16(mapfile, frame * 2));
                    bmp = LevelData.S1MapFrameToBmp(artfile, s1map, startpal, out off);
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    S2Mappings s2map = new S2Mappings(mapfile, ByteConverter.ToInt16(mapfile, frame * 2));
                    bmp = LevelData.S2MapFrameToBmp(artfile, s2map, startpal, out off);
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    S3KMappings s3kmap = new S3KMappings(mapfile, ByteConverter.ToInt16(mapfile, frame * 2));
                    bmp = LevelData.S3KMapFrameToBmp(artfile, s3kmap, startpal, out off);
                    break;
            }
            BitmapBits Image = new BitmapBits(bmp[0].Width, bmp[0].Height);
            Image.DrawBitmapComposited(bmp[0], Point.Empty);
            Image.DrawBitmapComposited(bmp[1], Point.Empty);
            return new Sprite(Image, off);
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

        public static Sprite MapASMToBmp(byte[] artfile, string mapfileloc, int frame, int startpal)
        {
            return MapToBmp(artfile, LevelData.ASMToBin(mapfileloc), frame, startpal);
        }

        public static Sprite MapASMToBmp(byte[] artfile, string mapfileloc, string label, int startpal)
        {
            byte[] mapfile = LevelData.ASMToBin(mapfileloc, label);
            BitmapBits[] bmp = null;
            Point off = new Point();
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S1:
                    bmp = LevelData.S1MapFrameToBmp(artfile, new S1Mappings(mapfile, 0), startpal, out off);
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    bmp = LevelData.S2MapFrameToBmp(artfile, new S2Mappings(mapfile, 0), startpal, out off);
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    bmp = LevelData.S3KMapFrameToBmp(artfile, new S3KMappings(mapfile, 0), startpal, out off);
                    break;
            }
            BitmapBits Image = new BitmapBits(bmp[0].Width, bmp[0].Height);
            Image.DrawBitmapComposited(bmp[0], Point.Empty);
            Image.DrawBitmapComposited(bmp[1], Point.Empty);
            return new Sprite(Image, off);
        }

        public static Sprite MapDPLCToBmp(byte[] artfile, byte[] mapfile, byte[] dplc, int frame, int startpal)
        {
            BitmapBits[] bmp = null;
            Point off = new Point();
            DPLC dp = new DPLC(dplc, ByteConverter.ToInt16(dplc, frame * 2), LevelData.EngineVersion);
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    S2Mappings s2map = new S2Mappings(mapfile, ByteConverter.ToInt16(mapfile, frame * 2));
                    bmp = LevelData.S2MapFrameDPLCToBmp(artfile, s2map, dp, startpal, out off);
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    S3KMappings s3kmap = new S3KMappings(mapfile, ByteConverter.ToInt16(mapfile, frame * 2));
                    bmp = LevelData.S3KMapFrameDPLCToBmp(artfile, s3kmap, dp, startpal, out off);
                    break;
            }
            BitmapBits Image = new BitmapBits(bmp[0].Width, bmp[0].Height);
            Image.DrawBitmapComposited(bmp[0], Point.Empty);
            Image.DrawBitmapComposited(bmp[1], Point.Empty);
            return new Sprite(Image, off);
        }

        public static Sprite MapDPLCToBmp(byte[] artfile, byte[] mapfile, byte[] dplc, EngineVersion dplcversion, int frame, int startpal)
        {
            BitmapBits[] bmp = null;
            Point off = new Point();
            DPLC dp = new DPLC(dplc, ByteConverter.ToInt16(dplc, frame * 2), dplcversion);
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    S2Mappings s2map = new S2Mappings(mapfile, ByteConverter.ToInt16(mapfile, frame * 2));
                    bmp = LevelData.S2MapFrameDPLCToBmp(artfile, s2map, dp, startpal, out off);
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    S3KMappings s3kmap = new S3KMappings(mapfile, ByteConverter.ToInt16(mapfile, frame * 2));
                    bmp = LevelData.S3KMapFrameDPLCToBmp(artfile, s3kmap, dp, startpal, out off);
                    break;
            }
            BitmapBits Image = new BitmapBits(bmp[0].Width, bmp[0].Height);
            Image.DrawBitmapComposited(bmp[0], Point.Empty);
            Image.DrawBitmapComposited(bmp[1], Point.Empty);
            return new Sprite(Image, off);
        }

        public static Sprite MapASMDPLCToBmp(byte[] artfile, string mapfileloc, string label, string dplcloc, string dplclabel, int startpal)
        {
            byte[] mapfile = LevelData.ASMToBin(mapfileloc, label);
            byte[] dplcfile = LevelData.ASMToBin(dplcloc, dplclabel);
            BitmapBits[] bmp = null;
            Point off = new Point();
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    bmp = LevelData.S2MapFrameDPLCToBmp(artfile, new S2Mappings(mapfile, 0), new DPLC(dplcfile, 0, EngineVersion.S2), startpal, out off);
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    bmp = LevelData.S3KMapFrameDPLCToBmp(artfile, new S3KMappings(mapfile, 0), new DPLC(dplcfile, 0, EngineVersion.S3K), startpal, out off);
                    break;
            }
            BitmapBits Image = new BitmapBits(bmp[0].Width, bmp[0].Height);
            Image.DrawBitmapComposited(bmp[0], Point.Empty);
            Image.DrawBitmapComposited(bmp[1], Point.Empty);
            return new Sprite(Image, off);
        }

        public static Sprite MapASMDPLCToBmp(byte[] artfile, string mapfileloc, string label, string dplcloc, string dplclabel, EngineVersion dplcversion, int startpal)
        {
            byte[] mapfile = LevelData.ASMToBin(mapfileloc, label);
            byte[] dplcfile = LevelData.ASMToBin(dplcloc, dplclabel);
            BitmapBits[] bmp = null;
            Point off = new Point();
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    bmp = LevelData.S2MapFrameDPLCToBmp(artfile, new S2Mappings(mapfile, 0), new DPLC(dplcfile, 0, dplcversion), startpal, out off);
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    bmp = LevelData.S3KMapFrameDPLCToBmp(artfile, new S3KMappings(mapfile, 0), new DPLC(dplcfile, 0, dplcversion), startpal, out off);
                    break;
            }
            BitmapBits Image = new BitmapBits(bmp[0].Width, bmp[0].Height);
            Image.DrawBitmapComposited(bmp[0], Point.Empty);
            Image.DrawBitmapComposited(bmp[1], Point.Empty);
            return new Sprite(Image, off);
        }

        public static Sprite MapASMDPLCToBmp(byte[] artfile, string mapfileloc, string dplcloc, int frame, int startpal)
        {
            return MapDPLCToBmp(artfile, LevelData.ASMToBin(mapfileloc), LevelData.ASMToBin(dplcloc), frame, startpal);
        }

        public static Sprite MapASMDPLCToBmp(byte[] artfile, string mapfileloc, string dplcloc, EngineVersion dplcversion, int frame, int startpal)
        {
            return MapDPLCToBmp(artfile, LevelData.ASMToBin(mapfileloc), LevelData.ASMToBin(dplcloc), dplcversion, frame, startpal);
        }



        public static int ShiftLeft(int value, int num) { return value << num; }

        public static int ShiftRight(int value, int num) { return value >> num; }

        public static byte SetSubtypeMask(byte subtype, byte value, int mask) { return (byte)((subtype & ~mask) | (value & mask)); }
    }
}