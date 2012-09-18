using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSONLevelConverter.OtherJSON
{
   

    public class SLData
    {
        public static SLData Translate(JSONLevelData data)
        {
            var jm = toDoublAnotherArray(data.HeightMaps, a => (int)a);

            SLData d2=new SLData()
                          {
                              PaletteItems = toAnotherArray(data.PaletteItems,a=>a.ToArray()),
                              StartPositions = new List<SLDataStartPositionEntry>(data.StartPositions.Select(a => new SLDataStartPositionEntry(){Type=a.Type,X=a.X,Y=a.Y})),
                              AnimatedFiles = toTripleByteArray(data.AnimatedFiles),
                              Animations = new List<SLDataAnimation>(data.Animations.Select(a=>new SLDataAnimation(){AnimationFile=a.AnimationFile,AnimationTileIndex=a.AnimationTileIndex,
                                                                                                                     Frames = new List<SLDataAnimationFrame>(a.Frames.Select(b => new SLDataAnimationFrame() { StartingTileIndex = b.StartingTileIndex, Ticks = b.Ticks }))})),
                              Tiles = toDoubleByteArray(data.Tiles),
                              Blocks = toDoublAnotherArray(data.Blocks,a=>new SLDataPatternIndex(){Palette=a.Palette,Priority = a.Priority,Tile=a.Tile,XFlip=a.XFlip,YFlip = a.YFlip}),
                              Chunks = toDoublAnotherArray(data.Chunks, a => new SLDataChunkBlock(){Block = (short) a.Block,Solid1=(SLDataSolidity) a.Solid1,XFlip=a.XFlip,YFlip=a.YFlip}),
                              Foreground = toDoubleByteArray(data.Foreground),
                              ForegroundWidth = data.ForegroundWidth,
                              ForegroundHeight = data.ForegroundHeight,
                              Background = toDoubleByteArray(data.Background),
                              BackgroundWidth = data.BackgroundWidth,
                              BackgroundHeight = data.BackgroundHeight,
                              Palette = data.Palette,
                              Objects = toAnotherArray(data.Objects,a=>new SLDataObjectEntry(){ID=a.ID,SubType=a.SubType,X=a.X,Y=a.Y,XFlip=a.XFlip,YFlip=a.YFlip}),
                              ObjectFormat = data.ObjectFormat,
                              Rings = toAnotherArray(data.Rings,a=>new SLDataRingEntry(){X=a.X,Y=a.Y}),
                              RingFormat = data.RingFormat,
                              CNZBumpers = toAnotherArray(data.CNZBumpers,a=>new SLDataCNZBumperEntry(){ID=a.ID,X=a.X,Y=a.Y}),
                              CollisionIndexes1 = toByteArray(data.CollisionIndexes1),
                              CollisionIndexes2 = toByteArray(data.CollisionIndexes2),
                              HeightMaps = toDoublAnotherArray(data.HeightMaps,a=>(int)a),
                              RotatedHeightMaps = toDoublAnotherArray(data.RotatedHeightMaps, a => (int)a),
                              Angles = toByteArray(data.Angles),
                          };


            return d2;


        }


        private static T1[][][] toTripleAnotherArray<T1, T2>(T2[][][] angles, Func<T2, T1> map)
        {
            if (angles == null)
                return new T1[0][][];

            T1[][][] fm = new T1[angles.Length][][];
            for (int i = 0; i < angles.Length; i++)
            {
                fm[i] = toDoublAnotherArray(angles[i],map);
            }
            return fm;
        }
        private static T1[][] toDoublAnotherArray<T1, T2>(T2[][] angles, Func<T2, T1> map)
        {
            if (angles == null)
                return new T1[0][];

            T1[][] fm = new T1[angles.Length][];
            for (int i = 0; i < angles.Length; i++)
            {
                fm[i] = toAnotherArray(angles[i],map);
            }
            return fm;
        }

        private static T1[] toAnotherArray<T1, T2>(T2[] angles,Func<T2,T1> map)
        {

            if(angles==null)
                return new T1[0];


            T1[] fm = new T1[angles.Length];
            for (int i = 0; i < angles.Length; i++)
            {
                fm[i] = map(angles[i]);
            }
            return fm;
        }

        private static int[][][] toTripleByteArray(byte[][][] angles)
        {
            if (angles == null)
                return new int[0][][];

            int[][][] fm = new int[angles.Length][][];
            for (int i = 0; i < angles.Length; i++)
            {
                fm[i] = toDoubleByteArray(angles[i]);
            }
            return fm;
        }
        private static int[][] toDoubleByteArray(byte[][] angles)
        {
            if (angles == null)
                return new int[0][];

            int[][] fm = new int[angles.Length][];
            for (int i = 0; i < angles.Length; i++)
            {
                fm[i] = toByteArray(angles[i]);
            }
            return fm;
        }

        private static int[] toByteArray(byte[] angles)
        {
            if (angles == null)
                return new int[0];

            int[] fm = new int[angles.Length];
            for (int i = 0; i < angles.Length; i++)
            {
                fm[i] = angles[i];
            }
            return fm;
        }

        public AnimatedPaletteItem[][] PaletteItems { get; set; }
        public List<SLDataStartPositionEntry> StartPositions { get; set; }
        public int[][][] AnimatedFiles { get; set; }
        public List<SLDataAnimation> Animations { get; set; }
        public int[][] Tiles { get; set; }
        public SLDataPatternIndex[][] Blocks { get; set; }
        public SLDataChunkBlock[][] Chunks { get; set; }
        public int[][] Foreground { get; set; }
        public int ForegroundWidth { get; set; }
        public int ForegroundHeight { get; set; }
        public int[][] Background { get; set; }
        public int BackgroundWidth { get; set; }
        public int BackgroundHeight { get; set; }
        public string[][] Palette { get; set; }
        public SLDataObjectEntry[] Objects { get; set; }
        public string ObjectFormat { get; set; }
        public SLDataRingEntry[] Rings { get; set; }
        public string RingFormat { get; set; }
        public SLDataCNZBumperEntry[] CNZBumpers { get; set; }
        public int[] CollisionIndexes1 { get; set; }
        public int[] CollisionIndexes2 { get; set; }
        public int[][] HeightMaps { get; set; }
        public int[][] RotatedHeightMaps { get; set; }
        public int[] Angles { get; set; }
    }
    public class SLDataRingEntry
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    [Serializable()]
    public class SLDataCNZBumperEntry
    {
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public enum SLDataSolidity
    {
        NotSolid = 0,
        TopSolid = 1,
        LRBSolid = 2,
        AllSolid = 3
    }
    
    public class SLDataChunkBlock
    {
        public SLDataSolidity Solid1 { get; set; }
        public bool XFlip { get; set; }
        public bool YFlip { get; set; }
        public short Block { get; set; }
    }

    [Serializable()]
    public class SLDataObjectEntry
    {

        public int X { get; set; }
        public int Y { get; set; }
        public bool YFlip { get; set; }
        public bool XFlip { get; set; }
        public byte ID { get; set; }
        public byte SubType { get; set; }
    }

    public class SLDataStartPositionEntry
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; }
    }

    public class SLDataAnimation
    {
        public int AutomatedTiming { get; set; }
        public int AnimationFile { get; set; }
        public int AnimationTileIndex { get; set; }
        public int NumberOfTiles { get; set; }
        public List<SLDataAnimationFrame> Frames { get; set; }
    }
    public class SLDataAnimationFrame
    {
        public int StartingTileIndex { get; set; }
        public int Ticks { get; set; }
    }

    public class SLDataPatternIndex
    {
        public bool Priority { get; set; }
        public byte Palette { get; set; }
        public bool XFlip { get; set; }
        public bool YFlip { get; set; }
        public int Tile { get; set; }
    }
}
