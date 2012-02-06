using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SonicImageParser
{
    public class ChunkConsumer
    {
        string dir2 = @"B:\segastuff\Sonic3\Project\";
        public static int Grey = Color.FromArgb(32, 32, 32).ToArgb();
        public static int Yellow = Color.FromArgb(224, 224, 0).ToArgb();
        private string gmc;

        public ChunkConsumer()
        {
            Info n;
            n = fullLoad();
            // n=open();
             
            /*
            Bitmap jc = new Bitmap(n.LevelWidth * 128, n.LevelHeight * 128);

            for (int j = 0; j < n.LevelHeight; j++)
            {
                for (int i = 0; i < n.LevelWidth; i++)
                {
                    /*
                                        var lc = n.lowPlane[i + j * n.LevelWidth];
                                        if (lc != -1)
                                            n.chunks[lc].drawBitmap(jc, i, j);
                                        * /
                    var lc = n.highPlane[i + j * n.LevelWidth];
                    if (lc > 0)
                        n.chunks[lc].drawBitmap(jc, i, j);

                }
            }   jc.Save("b:\\highlow.bmp");
             */


        }

        public string getString()
        {
            return gmc;
        }









        private Info open()
        {

            XmlSerializer sl = new XmlSerializer(typeof(Info));
            var n = (Info)sl.Deserialize(File.OpenRead("B:\\chunks.xml"));
            /*  foreach (var chunk in n.chunks)
              {
                  chunk.setParent(n);
              }*/
            return n;
        }

        private void solidLoad(Info n)
        {


            n.heightMap1 = getHightMap(n, new Bitmap(dir2 + "plane solid n 1.png"));
            n.heightMap2 = getHightMap(n, new Bitmap(dir2 + "plane solid n 2.png"));

        }

        private static int[] getHightMap(Info n, Bitmap b)
        {
            int[] df = new int[n.LevelWidth*8*n.LevelHeight*8];
            Dictionary<Point, int> pts = new Dictionary<Point, int>();

            for (int a = 0; a < b.Height; a += 16)
            {
                for (int bc = 0; bc < b.Width; bc += 16)
                {
                    byte[] dff = new byte[16];

                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            var m = b.GetPixel((bc) + x, (a) + y);
                            var dd = m.ToArgb();
                            if ((dd == Yellow))
                            {
                                dff[x] = 16;
                                break;
                            }
                            if ((dd == Grey))
                            {
                                dff[x] = (byte) (16 - y);
                                break;
                            }
                        }
                    }
                    var im = BitConverter.ToInt32(dff, 0);

                    if (im != 0)
                        pts.Add(new Point(bc/16, a/16), im);
                    df[bc/16 + a/16*n.LevelWidth*8] = im;
                }
            }
            return df;
        }

        private int colorsEqual(int[] colors, int[] gm)
        {
            var bad = false;
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] != gm[i])
                {
                    bad = true;
                    break;
                }
            }
            if (!bad) return 0;
            bad = false;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (colors[(x) + (y) * 8] != gm[(7 - x) + (y) * 8])
                    {
                        bad = true;
                        break;
                    }
                }
            }
            if (!bad) return 1;
            bad = false;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (colors[(x) + (y) * 8] != gm[(x) + (7 - y) * 8])
                    {
                        bad = true;
                        break;
                    }
                }
            }
            if (!bad) return 2;
            bad = false;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (colors[(x) + (y) * 8] != gm[(7 - x) + (7 - y) * 8])
                    {
                        bad = true;
                        break;
                    }
                }
            }
            if (!bad) return 3;


            return -1;
        }

        private Info fullLoad()
        {


            Info n = new Info();
            n.tileData = new List<TileData>();
            var bg = new Bitmap(dir2 + "plane.png");
            n.LevelWidth = bg.Width / 128;
            n.LevelHeight = bg.Height / 128;

            int last;
            for (int y = 0; y < bg.Height; y += 8)
            {
                for (int x = 0; x < bg.Width; x += 8)
                {
                    int[] gm = new int[8 * 8];
                    for (int _y = 0; _y < 8; _y++)
                    {
                        for (int _x = 0; _x < 8; _x++)
                        {
                            gm[_x + _y * 8] = bg.GetPixel(x + _x, y + _y).ToArgb();
                        }
                    }


                    if (!n.tileData.Any(a => (last = colorsEqual(a.Colors, gm)) != -1))
                    {
                        TileData dm;
                        n.tileData.Add(dm = new TileData() { Colors = gm });
                        dm.setInfo(n);
                    }
                }
            }




            for (int y = 0; y < bg.Height; y += 16)
            {
                for (int x = 0; x < bg.Width; x += 16)
                {
                    Tile[] gm = new Tile[4];
                    for (int _y = 0; _y < 2; _y++)
                    {
                        for (int _x = 0; _x < 2; _x++)
                        {
                            int[] cm = new int[8 * 8];
                            for (int __y = 0; __y < 8; __y++)
                            {
                                for (int __x = 0; __x < 8; __x++)
                                {
                                    var d = bg.GetPixel(x + _x * 8 + __x, y + _y * 8 + __y).ToArgb();
                                    cm[__x + __y * 8] = d;
                                }
                            }
                            last = -1;

                            TileData mj;
                            if ((mj = n.tileData.First(a => (last = colorsEqual(a.Colors, cm)) != -1)) != null)
                            {
                                Tile md;
                                gm[_x + _y * 2] = (md = new Tile(n.tileData.IndexOf(mj), last));
                                md.setInfo(n);
                            }
                        }
                    }
                    if (!n.tilePieces.Any(a => a.Equals(gm)))
                    {
                        n.tilePieces.Add(new TilePiece(gm));
                    }
                }
            }

            bg = null;

            var b = new Bitmap(dir2 + "ChunkList.png");
            int inj = 0;
            List<int> colors = new List<int>();
            List<string> jsons = new List<string>();
            var inc = 0;
            for (int offy = 0; offy < b.Height; offy += 128)
            {
                for (int offx = 0; offx < b.Width; offx += 128)
                {

                    int[] j = new int[8 * 8];

                    for (int d = 0; d < 8; d++)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            int[] mc = new int[16 * 16];
                            for (int _x = 0; _x < 16; _x++)
                            {
                                for (int _y = 0; _y < 16; _y++)
                                {
                                    var m = b.GetPixel(k * 16 + offx + _x, d * 16 + offy + _y);
                                    var dl = m.ToArgb();
                                    mc[_x + _y * 16] = dl;
                                }
                            }
                            var ddf = n.tilePieces.FirstOrDefault(a => a.Compare(mc));
                            if (ddf!=null && n.tilePieces.IndexOf(ddf) > -1)
                            {
                                j[k + d * 8] = n.tilePieces.IndexOf(ddf);
                            }
                            else
                            {
                                Console.WriteLine(inj++);
                            }

                        }
                    }

                    TileChunk fd;
                    n.chunks.Add(fd = new TileChunk(j));
                    fd.setInfo(n);

                }
            } 
            n.pallet = colors.ToArray();
            inj = 0;
            var highs = getColors(n, dir2 + "plane.png");
            foreach (var high in highs)
            {
               
                    var mcd = n.chunks.FirstOrDefault(a => a.Compare(high));
                    if (mcd != null)
                    {
                        n.chunkLocations.Add(n.chunks.IndexOf(mcd));
                    }
                    else
                    {
                        Console.WriteLine(inj++);
                        
                    }
            }

            solidLoad(n);
            save(n);
            Application.Exit();
            /*         n.highPlane = highs.Select(a =>
            {
                var firstOrDefault = n.chunks.FirstOrDefault(bc => compareBitmap(bc, a));
                if (firstOrDefault != null)
                    return firstOrDefault.Index;
                else
                    return 0;
            }).ToArray();
               var lows = getColors(n, dir2 + "plane low.png");

               n.lowPlane = new List<int>(lows.Select(a =>
               {
                       var firstOrDefault = n.getChunk().FirstOrDefault(bc => compareBitmap(bc, a));
                if (firstOrDefault != null)
                    return firstOrDefault.Index;
                else
                    return (byte) 0;
               }).AsParallel());*/





            save(n);
            return n;
        }


        private void save(Info n)
        {

            XmlSerializer sl = new XmlSerializer(typeof(Info));

            FileStream cd;
            sl.Serialize(cd = File.OpenWrite("b:\\chunks4.js"), n);
            cd.Close();


            System.Web.Script.Serialization.JavaScriptSerializer oSerializer =
                new System.Web.Script.Serialization.JavaScriptSerializer();
            oSerializer.MaxJsonLength *= 10;
            string sJSON = oSerializer.Serialize(n);
            File.WriteAllText("b:\\chunks43.js", sJSON);

            cd.Close();


            gmc = sJSON;








        }
        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
        public static byte[] compressFile(string inFile)
        {
            string _data = inFile;

            byte[] byteArray = Encoding.ASCII.GetBytes(_data);

            MemoryStream _st = new MemoryStream(byteArray);

            var d = new SharpLZW.LZWEncoder();
            return d.EncodeToByteList(inFile);


        }








        private int[][] getColors(Info n, string s)
        {
            var b = new Bitmap(s);
            //var mj= b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);


            var md = Enumerable.Range(0, b.Height / 128).SelectMany(a => Enumerable.Range(0, b.Width / 128).Select(bc =>
                                                                                     {

                                                                                         int[] j = new int[128 * 128];

                                                                                         for (int k = 0; k < 128; k++)
                                                                                         {
                                                                                             for (int d = 0; d < 128; d++)
                                                                                             {
                                                                                                 var m = b.GetPixel(k + bc * 128, d + a * 128);
                                                                                                 j[k + d * 128] = m.ToArgb();
                                                                                             }
                                                                                         }
                                                                                         return j;
                                                                                     })).ToArray();

            return md;
        }
        /*
        private bool compareBitmap(Chunk c, byte[] bitmap1)
        {
            byte[] bitmap = c.getBitmap();
            for (int k = 0; k < 128; k++)
            {
                for (int d = 0; d < 128; d++)
                {
                    var m = bitmap[k + d * 128];
                    var j = bitmap1[k + d * 128];
                    if (!m.Equals(j))
                        return false;
                }

            }
            return true;
        }*/
    }
    public class Info
    {
        public List<int> chunkLocations = new List<int>();
        public List<TileChunk> chunks = new List<TileChunk>();
        public List<TilePiece> tilePieces = new List<TilePiece>();
        public List<TileData> tileData = new List<TileData>();


        public int[] highPlane;
        public int[] lowPlane;
        public int[] pallet;

        public int[] heightMap1;
        public int[] heightMap2;
        public int LevelWidth;
        public int LevelHeight; 
    }
    public class TileChunk
    {
        public int[] TPIndexes { get; set; }
        private Info inf;
        public void setInfo(Info info)
        {
            inf = info;
        }
        public TileChunk(int[] gm)
        {
            TPIndexes = gm;
        }
        public TileChunk()
        {
        }

        public bool Compare(int[] high)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var tp = inf.tilePieces[TPIndexes[x + y * 8]];
                    for (int _y = 0; _y < 2; _y++)
                    {
                        for (int _x = 0; _x < 2; _x++)
                        {
                            var t = tp.Tiles[_x + _y*2]; 

                            for (int __y = 0; __y < 8; __y++)
                            {
                                for (int __x = 0; __x < 8; __x++)
                                {
                                    if(t.Get(__x,__y)!=high[(x*16+_x*8+__x)+(y*16+_y*8+__y)*128])
                                    {
                                        return false;
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return true;

        }
    }

    public class TilePiece
    {
        public TilePiece(Tile[] gm)
        {
            Tiles = gm;
        }
        public TilePiece()
        {
        }

        public Tile[] Tiles { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is TilePiece)
            {
                return ((TilePiece)obj).Tiles.Equals(this.Tiles);
            }
            var ts = (Tile[])obj;
            for (int i = 0; i < Tiles.Length; i++)
            {
                if (!ts[i].Equals(Tiles[i]))
                    return false;
            }
            return true;

        }

        public bool Equals(TilePiece other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Tiles, Tiles);
        }

        public override int GetHashCode()
        {
            return (Tiles != null ? Tiles.GetHashCode() : 0);
        }

        public bool Compare(int[] mc)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int _x = (x / 8);
                    int _y = (y / 8);
                    if (mc[x + y * 16] != Tiles[_x + _y * 2].Get(x - _x * 8, y - _y * 8))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    public class TileData
    {
        public int[] Colors { get; set; }

        private Info inf;
        public void setInfo(Info info)
        {
            inf = info;
        }
    }
    public class Tile
    {
        public Tile(int indexOf, int last)
        {
            TileIndex = indexOf;
            State = last;
        }
        public Tile()
        {
        }

        public int State { get; set; }
        public int TileIndex { get; set; }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Tile)) return false;
            return Equals((Tile)obj);
        }

        public bool Equals(Tile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.State == State && other.TileIndex == TileIndex;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (State * 397) ^ TileIndex;
            }
        }

        public int Get(int x, int y)
        {
            var dm = inf.tileData[TileIndex];
            switch (State)
            {
                case 0:
                    return dm.Colors[(x) + (y) * 8];
                case 1:
                    return dm.Colors[(7 - x) + (y) * 8];
                case 2:
                    return dm.Colors[(x) + (7 - y) * 8];
                case 3:
                    return dm.Colors[(7 - x) + (7 - y) * 8];
            }
            return 0;
        }

        private Info inf;
        public void setInfo(Info info)
        {
            inf = info;
        }
    }/*
    public class Chunk
    {
        private Info parent;
        private byte[] bmap;
        public byte[] getBitmap()
        {
            return bmap;
        }
        public string BitmapString
        {
            get { return Convert.ToBase64String(bmap); }
            set
            {

                bmap = Convert.FromBase64String(value);
                //            string d= System.Text.Encoding.UTF8.GetString(bmap);

            }
        }
        public int Index { get; set; }

        public Chunk(byte[] bitmap, int index, Info parent)
        {
            this.parent = parent;
            bmap = bitmap;
            Index = index;
        }
        public Chunk()
        {
        }

        public void drawBitmap(Bitmap jc, int i, int j)
        {
            for (int k = 0; k < 128; k++)
            {
                for (int l = 0; l < 128; l++)
                {
                    /*if (parent.heightMap[(i * 128 + k) + (j * 128 + l) * 128 * parent.LevelWidth])
                    {
                        jc.SetPixel(i * 128 + k, j * 128 + l, Color.FromArgb(ChunkConsumer.Grey));

                    }
                    else* /
                    jc.SetPixel(i * 128 + k, j * 128 + l, Color.FromArgb(parent.pallet[bmap[k + l * 128]]));

                }
            }

        }

        public void setParent(Info info)
        {

            parent = info;
        }
    }*/

}


