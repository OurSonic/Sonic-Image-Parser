using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace SonicImageParser
{
    class Program
    {
        static void Main(string[] args)
        {

            StringBuilder sb=new StringBuilder();
            foreach (var fileInfo in new DirectoryInfo(@"B:\code\oursonic\javascript\").GetFiles("*.js"))
            {
                if (fileInfo.Name.Equals("curCompress.js")) continue;
                sb.AppendLine(File.ReadAllText(fileInfo.FullName));
            }
            ////Yahoo.Yui.Compressor.JavaScriptCompressor j = new Yahoo.Yui.Compressor.JavaScriptCompressor(sb.ToString());
            ////File.WriteAllText(@"B:\code\oursonic\javascript\curCompress.js",j.Compress());
            

//            try
//            {

         // new ChunkConsumer(@"B:\segastuff\Sonic3\Project\levels\mushroom1\", "mushroom1");
       //     new ChunkConsumer(@"B:\segastuff\Sonic3\Project\levels\mushroom2\", "mushroom2");
         //   new ChunkConsumer(@"B:\segastuff\Sonic3\Project\levels\casino1\", "casino1");
          //  new ChunkConsumer(@"B:\segastuff\Sonic3\Project\levels\casino2\", "casino2");
           // new ChunkConsumer(@"B:\segastuff\Sonic2\project\levels\emerald1\", "s2emerald1");
                



//            }
//
//            catch(Exception ex )
//            {
//                Console.Write(ex.ToString());
//                Console.ReadLine();
//            }



            var img = @"B:\code\oursonic\assets\Sprites\untitled.png";

          /*var    b = new Bitmap(img);
            int c = 0;
            var get = getIndexes(img);
            foreach (var tuple in get)
            {
                var w = tuple.Item2 - tuple.Item1;
                Bitmap j = new Bitmap(w, b.Height);
                for (int k = 0; k < w; k++)
                {
                    for (int d = 0; d < b.Height; d++)
                    {
                        var m = b.GetPixel(tuple.Item1 + k, d);
                        j.SetPixel(k, d, m);
                    }

                }
                j.Save(@"B:\code\oursonic\assets\Sprites\spindash" + c + ".png");
                c++;
            }*/
        }

        static List<Tuple<int, int>> getIndexes(string image)
        {
            var b = new Bitmap(image);
            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();
            Tuple<int, int> cur = null;
            for (int i = 0; i < b.Width; i++)
            {
                bool blankline = true;
                for (int d = 0; d < b.Height; d++)
                {
                    var m = b.GetPixel(i, d);
                    if ((m.A!=0))
                    {
                        blankline = false;
                        break;
                    }

                }

                if (blankline)
                {
                    if (cur != null)
                    {
                        cur = new Tuple<int, int>(cur.Item1, i);
                        indexes.Add(cur);
                        cur = null;
                    }
                }
                else
                {
                    if (cur == null) { cur = new Tuple<int, int>(i, 0); }
                }
                //       if(blankline)

            }
            if (cur != null)
            {
                cur = new Tuple<int, int>(cur.Item1, b.Width);
                indexes.Add(cur);
            }
            
            return indexes;
        }

    }
}


public class Tuple<T,T2>
{
    public T Item1 { get; set; }
    public T2 Item2 { get; set; }
    public Tuple(T v, T2 v2)
    {
        Item1 = v;
        Item2 = v2;
} 

}