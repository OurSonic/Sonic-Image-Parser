using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Newtonsoft.Json;

namespace JSONLevelConverter
{
    internal class JSONLevelData
    {
        public List<StartPositionEntry> StartPositions;
        public byte[][][] AnimatedFiles { get; set; }
        public List<Animation> Animations { get; set; }
        public byte[][] Tiles { get; set; }
        public PatternIndex[][] Blocks { get; set; }
        public ChunkBlock[][] Chunks { get; set; }
        public byte[] Foreground { get; set; }
        public int ForegroundWidth { get; set; }
        public int ForegroundHeight { get; set; }
        public byte[] Background { get; set; }
        public int BackgroundWidth { get; set; }
        public int BackgroundHeight { get; set; }
        public string[][] Palette { get; set; }
        public ObjectEntry[] Objects { get; set; }
        public string ObjectFormat { get; set; }
        public RingEntry[] Rings { get; set; }
        public string RingFormat { get; set; }
        public CNZBumperEntry[] CNZBumpers { get; set; }
        public byte[] CollisionIndexes1 { get; set; }
        public byte[] CollisionIndexes2 { get; set; }
        public sbyte[][] HeightMaps { get; set; }
        public sbyte[][] RotatedHeightMaps { get; set; }
        public byte[] Angles { get; set; }
    }
    internal class Animation
    {
        public int AnimationFile { get; set; }
        public int AnimationTileIndex { get; set; }
        public int NumberOfTiles { get; set; }
        public List<AnimationFrame> Frames { get; set; }
        public Animation(int animationFile, int animationTileIndex, int numberOfTiles)
        {
            AnimationFile = animationFile;
            AnimationTileIndex = animationTileIndex;
            NumberOfTiles = numberOfTiles;
            Frames=new List<AnimationFrame>();
        }
        public void AddFrame(AnimationFrame frame)
        {
            Frames.Add(frame);
        }
        public Animation()
        {
        }

    }
    internal class AnimationFrame
    {
        public int StartingTileIndex { get; set; }
        public int Ticks { get; set; }
        public AnimationFrame(int startingTileIndex, int ticks)
        {
            StartingTileIndex = startingTileIndex;
            Ticks = ticks;
        }
        public AnimationFrame()
        {
        }

    }

    internal class PatternIndex
    {
        public bool Priority { get; set; }
        private byte _pal;
        public byte Palette
        {
            get
            {
                return _pal;
            }
            set
            {
                _pal = (byte)(value & 0x3);
            }
        }
        public bool XFlip { get; set; }
        public bool YFlip { get; set; }
        private ushort _ind;
        public ushort Tile
        {
            get
            {
                return _ind;
            }
            set
            {
                _ind = (ushort)(value & 0x7FF);
            }
        }

        public static int Size { get { return 2; } }

        public PatternIndex() { }

        public PatternIndex(byte[] file, int address)
        {
            ushort val = ByteConverter.ToUInt16(file, address);
            Priority = (val & 0x8000) == 0x8000;
            Palette = (byte)((val >> 13) & 0x3);
            YFlip = (val & 0x1000) == 0x1000;
            XFlip = (val & 0x800) == 0x800;
            Tile = (ushort)(val & 0x7FF);
        }

        public byte[] GetBytes()
        {
            ushort val = Tile;
            if (XFlip) val |= 0x800;
            if (YFlip) val |= 0x1000;
            val |= (ushort)(Palette << 13);
            if (Priority) val |= 0x8000;
            return ByteConverter.GetBytes(val);
        }
    }

    internal class Block
    {
        [JsonIgnore]
        public PatternIndex[,] tiles { get; set; }

        public PatternIndex[] Tiles
        {
            get
            {
                List<PatternIndex> result = new List<PatternIndex>();
                for (int y = 0; y < 2; y++)
                    for (int x = 0; x < 2; x++)
                        result.Add(tiles[x, y]);
                return result.ToArray();
            }
            set { }
        }


        public static int Size { get { return PatternIndex.Size * 4; } }

        public Block()
        {
            tiles = new PatternIndex[2, 2];
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    tiles[x, y] = new PatternIndex();
        }

        public Block(byte[] file, int address)
        {
            tiles = new PatternIndex[2, 2];
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    tiles[x, y] = new PatternIndex(file, address + ((x + (y * 2)) * PatternIndex.Size));
        }

        public byte[] GetBytes()
        {
            List<byte> val = new List<byte>();
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    val.AddRange(tiles[x, y].GetBytes());
            return val.ToArray();
        }
    }

    internal enum Solidity : byte
    {
        NotSolid = 0,
        TopSolid = 1,
        LRBSolid = 2,
        AllSolid = 3
    }

    internal abstract class ChunkBlock
    {
        protected byte _so1;
        public Solidity Solid1
        {
            get
            {
                return (Solidity)_so1;
            }
            set
            {
                _so1 = (byte)(value & Solidity.AllSolid);
            }
        }

        public bool XFlip { get; set; }
        public bool YFlip { get; set; }
        private ushort _ind;
        public ushort Block
        {
            get
            {
                return _ind;
            }
            set
            {
                _ind = (ushort)(value & 0x3FF);
            }
        }

        public static int Size { get { return 2; } }

        public abstract byte[] GetBytes();
    }

    internal class S2ChunkBlock : ChunkBlock
    {
        private byte _so2;
        public Solidity Solid2
        {
            get
            {
                return (Solidity)_so2;
            }
            set
            {
                _so2 = (byte)(value & Solidity.AllSolid);
            }
        }

        public S2ChunkBlock() { }

        public S2ChunkBlock(byte[] file, int address)
        {
            ushort val = ByteConverter.ToUInt16(file, address);
            _so2 = (byte)((val >> 14) & 0x3);
            _so1 = (byte)((val >> 12) & 0x3);
            YFlip = (val & 0x800) == 0x800;
            XFlip = (val & 0x400) == 0x400;
            Block = (ushort)(val & 0x3FF);
        }

        public override byte[] GetBytes()
        {
            ushort val = Block;
            if (XFlip) val |= 0x400;
            if (YFlip) val |= 0x800;
            val |= (ushort)(_so1 << 12);
            val |= (ushort)(_so2 << 14);
            return ByteConverter.GetBytes(val);
        }
    }

    internal class S1ChunkBlock : ChunkBlock
    {
        public S1ChunkBlock() { }

        public S1ChunkBlock(byte[] file, int address)
        {
            ushort val = ByteConverter.ToUInt16(file, address);
            _so1 = (byte)((val >> 13) & 0x3);
            YFlip = (val & 0x1000) == 0x1000;
            XFlip = (val & 0x800) == 0x800;
            Block = (ushort)(val & 0x3FF);
        }

        public override byte[] GetBytes()
        {
            ushort val = Block;
            if (XFlip) val |= 0x800;
            if (YFlip) val |= 0x1000;
            val |= (ushort)(_so1 << 13);
            return ByteConverter.GetBytes(val);
        }
    }

    internal class Chunk
    {
        [JsonIgnore]
        public ChunkBlock[,] blocks { get; set; }

        public ChunkBlock[] Blocks
        {
            get
            {
                List<ChunkBlock> result = new List<ChunkBlock>();
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        result.Add(blocks[x, y]);
                return result.ToArray();
            }
            set { }
        }

        private int size;

        public static int Size { get { return ChunkBlock.Size * (int)Math.Pow(LevelData.chunksz / 16, 2); } }

        public Chunk()
        {
            size = LevelData.chunksz / 16;
            blocks = new ChunkBlock[size, size];
            switch (LevelData.ChunkFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.SCD:
                case EngineVersion.SCDPC:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S1ChunkBlock();
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S2ChunkBlock();
                    break;
            }
        }

        public Chunk(byte[] file, int address)
        {
            size = LevelData.chunksz / 16;
            blocks = new ChunkBlock[size, size];
            switch (LevelData.ChunkFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.SCD:
                case EngineVersion.SCDPC:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S1ChunkBlock(file, address + ((x + (y * size)) * ChunkBlock.Size));
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S2ChunkBlock(file, address + ((x + (y * size)) * ChunkBlock.Size));
                    break;
            }
        }

        public byte[] GetBytes()
        {
            List<byte> val = new List<byte>();
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    val.AddRange(blocks[x, y].GetBytes());
            return val.ToArray();
        }
    }

    [Serializable()]
    public abstract class Entry : IComparable<Entry>
    {
        [Browsable(false)]
        public ushort X { get; set; }
        [Browsable(false)]
        public ushort Y { get; set; }

        public abstract byte[] GetBytes();

        int IComparable<Entry>.CompareTo(Entry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }
    }

    [Serializable()]
    public abstract class ObjectEntry : Entry, IComparable<ObjectEntry>
    {
        [DefaultValue(false)]
        [DisplayName("Y Flip")]
        public virtual bool YFlip { get; set; }
        [DefaultValue(false)]
        [DisplayName("X Flip")]
        public virtual bool XFlip { get; set; }
        private byte id;
        [Browsable(false)]
        public virtual byte ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        [Browsable(false)]
        public virtual byte SubType { get; set; }

        protected bool isLoaded = false;

        int IComparable<ObjectEntry>.CompareTo(ObjectEntry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }
    }

    [DefaultProperty("ID")]
    [Serializable()]
    public class S2ObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [DisplayName("Remember State")]
        public virtual bool RememberState { get; set; }

        [DefaultValue(false)]
        [DisplayName("Long Distance")]
        public bool LongDistance { get; set; }

        public static int Size { get { return 6; } }

        public S2ObjectEntry() { isLoaded = true; }

        public S2ObjectEntry(byte[] file, int address)
        {
            X = ByteConverter.ToUInt16(file, address);
            ushort val = ByteConverter.ToUInt16(file, address + 2);
            RememberState = (val & 0x8000) == 0x8000;
            YFlip = (val & 0x4000) == 0x4000;
            XFlip = (val & 0x2000) == 0x2000;
            LongDistance = (val & 0x1000) == 0x1000;
            Y = (ushort)(val & 0xFFF);
            ID = file[address + 4];
            SubType = file[address + 5];
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (LongDistance) val |= 0x1000;
            if (XFlip) val |= 0x2000;
            if (YFlip) val |= 0x4000;
            if (RememberState) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add(ID);
            ret.Add(SubType);
            return ret.ToArray();
        }
    }

    [DefaultProperty("ID")]
    [Serializable()]
    public class S1ObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [DisplayName("Remember State")]
        public virtual bool RememberState { get; set; }

        public override byte ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = (byte)(value & 0x7F);
            }
        }

        public static int Size { get { return 6; } }

        public S1ObjectEntry() { isLoaded = true; }

        public S1ObjectEntry(byte[] file, int address)
        {
            X = ByteConverter.ToUInt16(file, address);
            ushort val = ByteConverter.ToUInt16(file, address + 2);
            YFlip = (val & 0x8000) == 0x8000;
            XFlip = (val & 0x4000) == 0x4000;
            Y = (ushort)(val & 0xFFF);
            ID = file[address + 4];
            RememberState = (file[address + 4] & 0x80) == 0x80;
            ID = (byte)(ID & 0x7F);
            SubType = file[address + 5];
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (XFlip) val |= 0x4000;
            if (YFlip) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add((byte)(ID | (RememberState ? 0x80 : 0)));
            ret.Add(SubType);
            return ret.ToArray();
        }
    }

    [DefaultProperty("ID")]
    [Serializable()]
    public class S3KObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [DisplayName("Make object manager ignore Y position")]
        public virtual bool SomeFlag { get; set; }

        public static int Size { get { return 6; } }

        public S3KObjectEntry() { isLoaded = true; }

        public S3KObjectEntry(byte[] file, int address)
        {
            X = ByteConverter.ToUInt16(file, address);
            ushort val = ByteConverter.ToUInt16(file, address + 2);
            SomeFlag = (val & 0x8000) == 0x8000;
            YFlip = (val & 0x4000) == 0x4000;
            XFlip = (val & 0x2000) == 0x2000;
            Y = (ushort)(val & 0xFFF);
            ID = file[address + 4];
            SubType = file[address + 5];
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (XFlip) val |= 0x2000;
            if (YFlip) val |= 0x4000;
            if (SomeFlag) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add(ID);
            ret.Add(SubType);
            return ret.ToArray();
        }
    }

    [DefaultProperty("ID")]
    [Serializable()]
    public class SCDObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [DisplayName("Remember State")]
        public virtual bool RememberState { get; set; }

        [DisplayName("Show in Present")]
        public virtual bool ShowPresent { get; set; }
        [DisplayName("Show in Past")]
        public virtual bool ShowPast { get; set; }
        [DisplayName("Show in Future")]
        public virtual bool ShowFuture { get; set; }

        public override byte ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = (byte)(value & 0x7F);
            }
        }

        public static int Size { get { return 8; } }

        public SCDObjectEntry() { isLoaded = true; }

        public SCDObjectEntry(byte[] file, int address)
        {
            X = ByteConverter.ToUInt16(file, address);
            ushort val = ByteConverter.ToUInt16(file, address + 2);
            YFlip = (val & 0x8000) == 0x8000;
            XFlip = (val & 0x4000) == 0x4000;
            Y = (ushort)(val & 0xFFF);
            ID = file[address + 4];
            RememberState = (file[address + 4] & 0x80) == 0x80;
            ID = (byte)(ID & 0x7F);
            SubType = file[address + 5];
            ShowPresent = (file[address + 6] & 0x40) == 0x40;
            ShowPast = (file[address + 6] & 0x80) == 0x80;
            ShowFuture = (file[address + 6] & 0x20) == 0x20;
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (XFlip) val |= 0x4000;
            if (YFlip) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add((byte)(ID | (RememberState ? 0x80 : 0)));
            ret.Add(SubType);
            byte b = 0;
            if (ShowPresent) b |= 0x40;
            if (ShowPast) b |= 0x80;
            if (ShowFuture) b |= 0x20;
            ret.Add(b);
            ret.Add(0);
            return ret.ToArray();
        }
    }

    [Serializable()]
    internal abstract class RingEntry : Entry, IComparable<RingEntry>
    {
        int IComparable<RingEntry>.CompareTo(RingEntry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }
    }

    [DefaultProperty("Count")]
    [Serializable()]
    internal class S2RingEntry : RingEntry
    {
        [DefaultValue(Direction.Horizontal)]
        public Direction Direction { get; set; }
        private byte _count;
        [DefaultValue(1)]
        public byte Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = Math.Max(Math.Min(value, (byte)8), (byte)1);
            }
        }

        public static int Size { get { return 4; } }

        public S2RingEntry() { Count = 1; }

        public S2RingEntry(byte[] file, int address)
        {
            X = ByteConverter.ToUInt16(file, address);
            ushort val = ByteConverter.ToUInt16(file, address + 2);
            Direction = (val & 0x8000) == 0x8000 ? Direction.Vertical : Direction.Horizontal;
            Count = (byte)(((val & 0x7000) >> 12) + 1);
            Y = (ushort)(val & 0xFFF);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            val |= (ushort)((Count - 1) << 12);
            if (Direction == Direction.Vertical) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            return ret.ToArray();
        }
    }

    [Serializable()]
    internal class S3KRingEntry : RingEntry
    {
        public static int Size { get { return 4; } }

        public S3KRingEntry() { }

        public S3KRingEntry(byte[] file, int address)
        {
            X = ByteConverter.ToUInt16(file, address);
            Y = ByteConverter.ToUInt16(file, address + 2);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ret.AddRange(ByteConverter.GetBytes(Y));
            return ret.ToArray();
        }
    }

    public enum Direction
    {
        Horizontal,
        Vertical
    }

    [DefaultProperty("ID")]
    [Serializable()]
    internal class CNZBumperEntry : Entry, IComparable<CNZBumperEntry>
    {
        [Browsable(false)]
        public ushort ID { get; set; }

        [DefaultValue("0000")]
        [DisplayName("ID")]
        public string _ID
        {
            get
            {
                return ID.ToString("X4");
            }
            set
            {
                ID = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public static int Size { get { return 6; } }

        public CNZBumperEntry() { }

        public CNZBumperEntry(byte[] file, int address)
        {
            ID = ByteConverter.ToUInt16(file, address);
            X = ByteConverter.ToUInt16(file, address + 2);
            Y = ByteConverter.ToUInt16(file, address + 4);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(ID));
            ret.AddRange(ByteConverter.GetBytes(X));
            ret.AddRange(ByteConverter.GetBytes(Y));
            return ret.ToArray();
        }

        int IComparable<CNZBumperEntry>.CompareTo(CNZBumperEntry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }
    }

    internal class StartPositionEntry : Entry
    {
        public static int Size { get { return 4; } }

        public string Type { get; set; }
        public StartPositionEntry() { }

        public StartPositionEntry(byte[] file, int address)
        {
            X = ByteConverter.ToUInt16(file, address);
            Y = ByteConverter.ToUInt16(file, address + 2);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ret.AddRange(ByteConverter.GetBytes(Y));
            return ret.ToArray();
        }
    }
}