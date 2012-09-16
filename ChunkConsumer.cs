//#define tga
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
        static string dir2;
        static string scriptName;
        public static int Grey = Color.FromArgb(32, 32, 32).ToArgb();
        public static int Whitish = Color.FromArgb(32, 32, 32).ToArgb();
        public static int Yellow = Color.FromArgb(231, 231, 231).ToArgb();
        private string gmc;
        private SpecialBitmap[] letters;

        public ChunkConsumer(string dir, string scr)
        {
            GC.Collect();


            dir2 = dir;
            scriptName = scr;
#if tga

            foreach (var file in new DirectoryInfo(dir2).GetFiles())
            {
                var df = FreeImageAPI.FreeImage.Load(FreeImageAPI.FREE_IMAGE_FORMAT.FIF_TARGA, file.FullName, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                FreeImageAPI.FreeImage.Save(FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG, df, file.FullName.Replace("tga", "png"), FreeImageAPI.FREE_IMAGE_SAVE_FLAGS.DEFAULT);
                df.SetNull();
                Console.WriteLine(file);
            }

            GC.Collect();

#endif

            letters = new SpecialBitmap[16];
            for (int i = 0; i < 16; i++)
            {
                var f = @"B:\segastuff\Sonic3\Project\letters\";
                letters[i] = new SpecialBitmap(f + (i.ToString("X")) + ".png");
            }

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
                                            n.Chunks[lc].drawBitmap(jc, i, j);
                                        * /
                    var lc = n.highPlane[i + j * n.LevelWidth];
                    if (lc > 0)
                        n.Chunks[lc].drawBitmap(jc, i, j);

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
            var n = (Info)sl.Deserialize(File.OpenRead("B:\\Chunks.xml"));
            /*  foreach (var chunk in n.Chunks)
              {
                  chunk.setParent(n);
              }*/
            return n;
        }

        private void solidLoad(Info n)
        {


            Dictionary<string, int> keycache = new Dictionary<string, int>();
            SpecialBitmap jz;
            getAngleMap(n, jz = new SpecialBitmap(dir2 + "plane solid ang 1.png"), keycache, 1);
            getAngleMap(n, jz = new SpecialBitmap(dir2 + "plane solid ang 2.png"), keycache, 2);

            getHightMap(n, jz = new SpecialBitmap(dir2 + "plane solid n 1.png"), keycache, 1);
            getHightMap(n, jz = new SpecialBitmap(dir2 + "plane solid n 2.png"), keycache, 2);



            n.HeightMaps = keycache.OrderBy(a => a.Value).Select(a => a.Key).ToArray();

        }





        unsafe public class SpecialBitmap
        {
            private struct PixelData
            {
                public byte blue;
                public byte green;
                public byte red;
                public byte alpha;

                public override string ToString()
                {
                    return "(" + alpha.ToString() + ", " + red.ToString() + ", " + green.ToString() + ", " + blue.ToString() + ")";
                }
            }

            public Bitmap workingBitmap = null;
            private int width = 0;
            private BitmapData bitmapData = null;
            private Byte* pBase = null;

            public SpecialBitmap(Bitmap inputBitmap)
            {
                workingBitmap = inputBitmap;
                Width = workingBitmap.Width;
                Height = workingBitmap.Height;
                LockImage();
            }
            public SpecialBitmap(string name)
            {
                workingBitmap = new Bitmap(name);
                Width = workingBitmap.Width;
                Height = workingBitmap.Height;
                LockImage();
            }

            public void LockImage()
            {
                Rectangle bounds = new Rectangle(Point.Empty, workingBitmap.Size);

                width = (int)(bounds.Width * sizeof(PixelData));
                if (width % 4 != 0) width = 4 * (width / 4 + 1);

                //Lock Image
                bitmapData = workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                pBase = (Byte*)bitmapData.Scan0.ToPointer();
            }

            private PixelData* pixelData = null;
            public int Width;
            public int Height;

            public int GetPixel(int x, int y)
            {
                pixelData = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                return (int)MakeArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }
            public Color GetPixelColor(int x, int y)
            {
                pixelData = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                return Color.FromArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            public int GetPixelNext()
            {
                pixelData++;
                return (int)MakeArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            private const int ARGBAlphaShift = 24;
            private const int ARGBRedShift = 16;
            private const int ARGBGreenShift = 8;
            private const int ARGBBlueShift = 0;
            private static long MakeArgb(byte alpha, byte red, byte green, byte blue)
            {
                return (long)((uint)(red << ARGBRedShift |
                             green << ARGBGreenShift |
                             blue << ARGBBlueShift |
                             alpha << ARGBAlphaShift)) & 0xffffffff;
            }
            public void SetPixel(int x, int y, Color color)
            {
                PixelData* data = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                data->alpha = color.A;
                data->red = color.R;
                data->green = color.G;
                data->blue = color.B;
            }

            public void UnlockImage(string saveto)
            {
                workingBitmap.UnlockBits(bitmapData);
                if (saveto != null)
                    workingBitmap.Save(saveto);

                workingBitmap.Dispose();
                workingBitmap = null;
                bitmapData = null;
                pBase = null;
            }

            public void close()
            {
                UnlockImage(null);

            }
        }


        private void getAngleMap(Info n, SpecialBitmap b, Dictionary<string, int> keycache, int map)
        {


            Console.WriteLine("agaga ss");
            TileChunk curChunk = n.Chunks[n.ChunkMap[0][0]];
            for (int a = 0; a < b.Height; a += 16)
            {
                for (int bc = 0; bc < b.Width; bc += 16)
                {
                    int chunky = a / 128;
                    int chunkx = bc / 128;

                    curChunk = n.Chunks[n.ChunkMap[chunkx][chunky]];
                    byte[] dff = new byte[17];
                    int piecex = (bc - chunkx * 128) / 16;
                    int piecey = (a - chunky * 128) / 16;
                    if (chunkx == 4 && chunky == 15)
                    {
                        if (piecex == 6 && piecey == 2)
                        {
                            Console.WriteLine((chunkx * 128));
                        }
                    }


                    var lt = new int[2] { -1, -1 };
                    for (int i = 0; i < 2; i++)
                    {
                        foreach (var letter in letters)
                        {
                            bool notletter = false;
                            for (int x = 0; x < 8; x++)
                            {
                                for (int y = 0; y < 16; y++)
                                {
                                    var m = b.GetPixel((bc) + x + i * 8, (a) + y);
                                    var fc = letter.GetPixelColor(x, y);
                                    if (fc.A != 0)
                                    {
                                        if (m != fc.ToArgb())
                                        {
                                            notletter = true;
                                            break;
                                        }
                                    }
                                }
                                if (notletter) break;
                            }
                            if (!notletter)
                            {
                                lt[i] = Array.IndexOf(letters, letter);
                            }
                        }
                    }
                    if (lt[0] == -1 || lt[1] == -1)
                    {
                        continue;
                    }

                    if (map == 1)
                        curChunk.angleMap1[piecex][piecey] = lt[0].ToString("X") + lt[1].ToString("X");
                    else
                        curChunk.angleMap2[piecex][piecey] = lt[0].ToString("X") + lt[1].ToString("X");
                }
            }
            b.close();
        }


        private void getHightMap(Info n, SpecialBitmap b, Dictionary<string, int> keycache, int map)
        {

            for (int x = 0; x < plane.Width; x++)
            {
                Color black = Color.Black;
                for (int y = 0; y < plane.Height; y++)
                {
                    var j = b.GetPixel(x, y);
                    if (plane.GetPixel(x, y) == j)
                    {
                        b.SetPixel(x, y, black);
                    }

                }
            }

            Console.WriteLine("solids ss");
            TileChunk curChunk = n.Chunks[n.ChunkMap[0][0]];
            for (int a = 0; a < b.Height; a += 16)
            {
                for (int bc = 0; bc < b.Width; bc += 16)
                {
                    int chunky = a / 128;
                    int chunkx = bc / 128;

                    curChunk = n.Chunks[n.ChunkMap[chunkx][chunky]];
                    byte[] dff = new byte[17];
                    int piecex = (bc - chunkx * 128) / 16;
                    int piecey = (a - chunky * 128) / 16;

                    for (int x = 0; x < 16; x++)
                    {
                        bool lineStarted = false;
                        for (int y = 0; y < 16; y++)
                        {
                            var m = b.GetPixel((bc) + x, (a) + y);

                            if ((m == Yellow) || (m == Grey) || (m == Whitish))
                            {
                                if (!lineStarted)
                                {
                                    lineStarted = true;
                                    dff[x + 1] = (byte)(16 - y);
                                    if (dff[0] != 2)
                                        dff[0] = 0;

                                }
                            }
                            else if (lineStarted)
                            {
                                dff[x + 1] = (byte)(y);

                                dff[0] = 2;
                                break;
                            }
                        }
                    }

                    string mf = "";
                    for (int i = 0; i < dff.Length; i++)
                    {
                        mf += toBase17(dff[i]);
                    }
                    int f;
                    if (!keycache.TryGetValue(mf, out f))
                    {
                        keycache.Add(mf, f = keycache.Count);
                    }
                    if (map == 1)
                        curChunk.heightMap1[piecex][piecey] = f;
                    else
                        curChunk.heightMap2[piecex][piecey] = f;


                }
            }
            b.close();
        }

        private static char toBase17(byte b)
        {
            switch (b)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return b.ToString()[0];
                case 10:
                    return 'a';
                case 11:
                    return 'b';
                case 12:
                    return 'c';
                case 13:
                    return 'd';
                case 14:
                    return 'e';
                case 15:
                    return 'f';
                case 16:
                    return 'g';

            }
            throw new Exception("h " + b);

        }

        private int colorsEqual(int[][] colors, int[][] gm)
        {
            var bad = false;
            for (int i = 0; i < colors.Length; i++)
            {
                for (int j = 0; j < colors[i].Length; j++)
                {
                    if (colors[i][j] != gm[i][j])
                    {
                        bad = true;
                        break;
                    }
                }
                if (bad) break;
            }
            if (!bad) return 0;
            bad = false;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (colors[(x)][(y)] != gm[(7 - x) ][(y) ])
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
                    if (colors[(x) ][ (y) ] != gm[(x) ][ (7 - y) ])
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
                    if (colors[(x) ][ (y) ] != gm[(7 - x) ][ (7 - y) ])
                    {
                        bad = true;
                        break;
                    }
                }
            }
            if (!bad) return 3;


            return -1;
        }

        private static SpecialBitmap plane;
        private Info fullLoad()
        {
            plane = new SpecialBitmap(dir2 + "plane.png");
            Info n = new Info(plane.Width / 128, plane.Height / 128);

            n.Tiles = new List<Tiles>();
            List<int> pallet = new List<int>();

            int last;
            for (int y = 0; y < plane.Height; y += 8)
            {
                for (int x = 0; x < plane.Width; x += 8)
                {
                    int[][] gm = Help.init2D<int>(8, 8);
                    for (int _y = 0; _y < 8; _y++)
                    {
                        for (int _x = 0; _x < 8; _x++)
                        {
                            var pind = 0;
                            var m = plane.GetPixel(x + _x, y + _y);
                            if ((pind = pallet.IndexOf(m)) == -1)
                            {
                                pallet.Add(m);
                                pind = pallet.Count;
                            }
                            gm[_x ][_y ] = pind;
                        }
                    }

                    if (!n.Tiles.Any(a => (last = colorsEqual(a.colors, gm)) != -1))
                    {
                        Console.WriteLine("Tiles: " + n.Tiles.Count);

                        Tiles dm;
                        n.Tiles.Add(dm = new Tiles() { colors = gm });
                        dm.setInfo(n);
                    }
                }
            }



            Console.WriteLine("tp");


            for (int y = 0; y < plane.Height; y += 16)
            {
                for (int x = 0; x < plane.Width; x += 16)
                {
                    Tile[] gm = new Tile[4];
                    for (int _y = 0; _y < 2; _y++)
                    {
                        for (int _x = 0; _x < 2; _x++)
                        {
                            int[][] cm = Help.init2D<int>(8, 8);
                            for (int __y = 0; __y < 8; __y++)
                            {
                                for (int __x = 0; __x < 8; __x++)
                                {
                                    var d = plane.GetPixel(x + _x * 8 + __x, y + _y * 8 + __y);
                                    cm[__x ][ __y ] = pallet.IndexOf(d);
                                }
                            }
                            last = -1;

                            Tiles mj;
                            if ((mj = n.Tiles.FirstOrDefault(a => (last = colorsEqual(a.colors, cm)) != -1)) != null)
                            {
                                Tile md;
                                gm[_x + _y * 2] = (md = new Tile(n.Tiles.IndexOf(mj), last));
                                md.setInfo(n);
                            }
                            else
                            {
                                Tile md;
                                gm[_x + _y * 2] = (md = new Tile(0, 0));
                                md.setInfo(n);
                            }
                        }
                    }
                    if (!n.Blocks.Any(a => a.Equals(gm)))
                    {
                        Console.WriteLine("peices: " + n.Blocks.Count);
                        n.Blocks.Add(new TilePiece(gm));
                    }
                }
            }

            Console.WriteLine("chunks ");

            var b = new SpecialBitmap(dir2 + "ChunkList.png");
            int inj = 0;
            var inc = 0;
            for (int offy = 0; offy < b.Height; offy += 128)
            {
                for (int offx = 0; offx < b.Width; offx += 128)
                {

                    int[][] j = Help.init2D<int>(8, 8);

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
                                    var dl = m;
                                    mc[_x + _y * 16] = pallet.IndexOf(dl);
                                }
                            }
                            var ddf = n.Blocks.FirstOrDefault(a => a.Compare(mc));
                            if (ddf != null && n.Blocks.IndexOf(ddf) > -1)
                            {
                                j[k][d] = n.Blocks.IndexOf(ddf);
                                Console.WriteLine("chunk: " + n.Blocks.IndexOf(ddf));

                            }
                            else
                            {
                                Console.WriteLine(inj++);
                            }

                        }
                    }

                    TileChunk fd;
                    n.Chunks.Add(fd = new TileChunk(j));
                    fd.hLayer = Help.init2D<int>(8,8);
                    fd.setInfo(n);

                }
            }
            b.close();
            inj = 0;
            var highs = getColors(n, plane, pallet);
            int ind = 0;
            foreach (var high in highs)
            {
                
                var x = ind % n.LevelWidth;
                var y = ind / n.LevelWidth;

                ind++;

                var mcd = n.Chunks.FirstOrDefault(a => a.Compare(high));
                if (mcd != null)
                {
                    n.ChunkMap[x][y] = n.Chunks.IndexOf(mcd);
                }
                else
                {
                    Console.WriteLine(inj++);
                }
            }
            highs = getColors(n, dir2 + "plane highs.png", pallet);
            for (int index = 0; index < highs.Length; index++)
            {

                var x = index % n.LevelWidth;
                var y = index / n.LevelWidth;



                int[] high = highs[index];
                TileChunk ch = n.Chunks[n.ChunkMap[x][y]];
                ch.hLayer = Help.init2D<int>(8, 8);

                for (int i = 0; i < ch.tilePieces.Length; i++)
                {
                    int _x = i % 8;
                    int _y = i / 8;

                    TilePiece tp = n.Blocks[ch.tilePieces[_x][_y]];
                    bool good = true;
                    bool allbalck = true;
                    for (int j = 0; j < 16; j++)
                    {
                        for (int k = 0; k < 16; k++)
                        {
                            int p = tp.getPixel(j, k);
                            if (p != 0) allbalck = false;
                            if (high[(_x * 16 + j) + (_y * 16 + k) * 128] != p)
                            {
                                good = false;
                                break;
                            }
                        }
                    }
                    if (allbalck) continue;
                    ch.hLayer[_x][_y] = good ? 1 : 0;
                }

            }

            Console.WriteLine("solid ");

            solidLoad(n);
            n.Palette = pallet.Select(a =>
            {
                var c = Color.FromArgb(a);

                return
                    ((c.R.ToString("X").Length == 1 ? "0" : "") + c.R.ToString("X")) +
                    ((c.G.ToString("X").Length == 1 ? "0" : "") + c.G.ToString("X")) +
                    ((c.B.ToString("X").Length == 1 ? "0" : "") + c.B.ToString("X"));

            }).ToArray();

            save(n);
            Application.Exit();
            /*         n.highPlane = highs.Select(a =>
            {
                var firstOrDefault = n.Chunks.FirstOrDefault(bc => compareBitmap(bc, a));
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

 
        private static void CompareBitmapImages(Bitmap b1, Bitmap b2, Bitmap drawTo)
        {
            if (b1.Width != b2.Width || b1.Height != b2.Height)
            {
                return;
            }

            if (b1.PixelFormat != b2.PixelFormat)
            {
                return;
            }

            int bytes = 0;
            if (b1.PixelFormat == PixelFormat.Format32bppArgb)
            {
                bytes = b1.Width * b1.Height * 4;
            }
            else
            {
                return;
            }

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];
            byte[] b3bytes = new byte[bytes];

            BitmapData bmd1 = b1.LockBits(new Rectangle(0, 0, b1.Width - 1, b1.Height - 1), ImageLockMode.ReadOnly, b1.PixelFormat);
            BitmapData bmd2 = b2.LockBits(new Rectangle(0, 0, b2.Width - 1, b2.Height - 1), ImageLockMode.ReadOnly, b2.PixelFormat);
            BitmapData drawT3 = drawTo.LockBits(new Rectangle(0, 0, drawTo.Width - 1, drawTo.Height - 1), ImageLockMode.WriteOnly, drawTo.PixelFormat);

            System.Runtime.InteropServices.Marshal.Copy(bmd1.Scan0, b1bytes, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(bmd2.Scan0, b2bytes, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(drawT3.Scan0, b3bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    b3bytes[n] = b1bytes[n];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(b3bytes, 0, drawT3.Scan0, bytes);

            b1.UnlockBits(bmd1);
            b2.UnlockBits(bmd2);
            drawTo.UnlockBits(drawT3);
        }

        private void save(Info n)
        {

            XmlSerializer sl = new XmlSerializer(typeof(Info));

            FileStream cd;
            sl.Serialize(cd = File.OpenWrite(@"B:\code\oursonic\" + scriptName + ".xml"), n);
            cd.Close();


            System.Web.Script.Serialization.JavaScriptSerializer oSerializer =
                new System.Web.Script.Serialization.JavaScriptSerializer();
            oSerializer.MaxJsonLength *= 10;
            string sJSON = oSerializer.Serialize(n);
            File.WriteAllText(@"B:\code\oursonic\" + scriptName + ".js", sJSON);

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
       







        private int[][] getColors(Info n, string s, List<int> pallet)
        {
            var b = new SpecialBitmap(s);

            var g = getColors(n, b, pallet);


            b.close();

            return g;
        }

        private int[][] getColors(Info n, SpecialBitmap b, List<int> pallet)
        {


            var md = Enumerable.Range(0, b.Height / 128).SelectMany(a =>
                                                                        {
                                                                            Console.Write("COlor color: " + a);
                                                                            return Enumerable.Range(0, b.Width / 128).Select(bc =>
                                                                                                                               {

                                                                                                                                   int[] j = new int[128 * 128];

                                                                                                                                   for (int k = 0; k < 128; k++)
                                                                                                                                   {
                                                                                                                                       for (int d = 0; d < 128; d++)
                                                                                                                                       {
                                                                                                                                           var m = b.GetPixel(k + bc * 128, d + a * 128);
                                                                                                                                           j[k + d * 128] = pallet.IndexOf(m);
                                                                                                                                       }
                                                                                                                                   }
                                                                                                                                   return j;
                                                                                                                               });
                                                                        }).ToArray();
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
        public int[][] ChunkMap;
        public List<TileChunk> Chunks=new List<TileChunk>();
        public List<TilePiece> Blocks=new List<TilePiece>();
        public List<Tiles> Tiles=new List<Tiles>();
        public string[] Palette;
        public string[] HeightMaps;
        public Info(int w, int h)
        {
            LevelWidth = w;
            LevelHeight = h;
            ChunkMap = new int[LevelWidth][];
            for (int x = 0; x < LevelWidth; x++)
            {
                ChunkMap[x] = new int[LevelHeight];
                for (int y = 0; y < LevelHeight; y++)
                {
                    ChunkMap[x][y] = 0;
                }
            }
        }
        public Info( )
        {
         }
        public int LevelWidth;
        public int LevelHeight;
    }
    public class TileChunk
    {
        public int[][] tilePieces { get; set; }
        public int[][] hLayer { get; set; }

        public int[][] heightMap1;
        public int[][] heightMap2;
        public string[][] angleMap1;
        public string[][] angleMap2;

        private Info inf;
        public void setInfo(Info info)
        {
            inf = info;
        }
        public TileChunk(int[][] gm)
            : this()
        {
            tilePieces = gm;
            
        }
        public TileChunk()
        {
            heightMap1 = new int[8][];
            heightMap2 = new int[8][];
            angleMap1 = new string[8][];
            angleMap2 = new string[8][];
            hLayer=new int[8][];
            for (int i = 0; i < 8; i++)
            {
                hLayer[i] = new int[8];
                heightMap1[i] = new int[8];
                heightMap2[i] = new int[8];
                angleMap1[i] = new string[8];
                angleMap2[i] = new string[8];

            }
        }

        public bool Compare(int[] high)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var tp = inf.Blocks[tilePieces[x][y]];
                    for (int _y = 0; _y < 2; _y++)
                    {
                        for (int _x = 0; _x < 2; _x++)
                        {
                            var t = tp.tiles[_x + _y * 2];

                            for (int __y = 0; __y < 8; __y++)
                            {
                                for (int __x = 0; __x < 8; __x++)
                                {
                                    if (t.Get(__x, __y) != high[(x * 16 + _x * 8 + __x) + (y * 16 + _y * 8 + __y) * 128])
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
            tiles = gm;
        }
        public TilePiece()
        {
        }

        public Tile[] tiles { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is TilePiece)
            {
                return ((TilePiece)obj).tiles.Equals(this.tiles);
            }
            var ts = (Tile[])obj;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (!ts[i].Equals(tiles[i]))
                    return false;
            }
            return true;

        }

        public bool Equals(TilePiece other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.tiles, tiles);
        }

        public override int GetHashCode()
        {
            return (tiles != null ? tiles.GetHashCode() : 0);
        }

        public bool Compare(int[] mc)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int _x = (x / 8);
                    int _y = (y / 8);
                    if (mc[x + y * 16] != tiles[_x + _y * 2].Get(x - _x * 8, y - _y * 8))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public int getPixel(int x, int y)
        {
            int _x = (x / 8);
            int _y = (y / 8);

            return tiles[_x + _y * 2].Get(x - _x * 8, y - _y * 8);
        }
    }
    public class Tiles
    {
        public int[][] colors { get; set; }

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
            var dm = inf.Tiles[TileIndex];
            switch (State)
            {
                case 0:
                    return dm.colors[(x)][ (y) ];
                case 1:
                    return dm.colors[(7 - x) ][ (y) ];
                case 2:
                    return dm.colors[(x) ][ (7 - y) ];
                case 3:
                    return dm.colors[(7 - x) ][ (7 - y) ];
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
                    jc.SetPixel(i * 128 + k, j * 128 + l, Color.FromArgb(parent.Palette[bmap[k + l * 128]]));

                }
            }

        }

        public void setParent(Info info)
        {

            parent = info;
        }
    }*/

}


