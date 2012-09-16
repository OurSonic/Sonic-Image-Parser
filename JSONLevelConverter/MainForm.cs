using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace JSONLevelConverter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.fileSelector1.FileName = pre+@"Sonic 3K SVN INIs\S3KLVL.ini";
            fileSelector1_FileNameChanged(this.fileSelector1, null);
            button1.Enabled = true;
            button2.Enabled = true;
            getFiles();
        }

        private Dictionary<string, Tuple<List<string>, List<Animation>>> animations;
        private Dictionary<string, Dictionary<string, string>> ini;
        string Dir = Environment.CurrentDirectory;
        private Dictionary<string, byte[][]> AnimationFiles = new Dictionary<string, byte[][]>();
        private static string pre = Directory.GetCurrentDirectory() + "/../../../Data/";
        private static string presk { get { return pre + "Sonic 3K SVN INIs/Build Scipts/"; } }

        public void getFiles()
        {
            var lines = File.ReadAllLines(pre + "AnimationFiles.txt");
            int lineIndex = 0;

            foreach (var line in lines)
            {
                if (lineIndex++ % 2 == 0)
                {
                    //ArtUnc_AniAIZ1_0:		binclude "Levels/AIZ/Animated Tiles/Act1 0.bin"
                    var f = line.Replace("\t", "").Replace("binclude ", "").Replace("\"", "").Split(':');

                    if (File.Exists(pre + f[1]))
                    {
                        var tmp = Compression.Decompress(pre+ f[1], Compression.CompressionType.Uncompressed);
                        List<byte[]> tiles = new List<byte[]>(tmp.Length / 32);

                        var m = new byte[tmp.Length / 32][];
                        for (int i = 0; i < tmp.Length; i += 32)
                        {
                            byte[] tile = new byte[32];
                            Array.Copy(tmp, i, tile, 0, 32);
                            tiles.Add(tile);
                        }
                        AnimationFiles.Add(f[0], tiles.ToArray());
                    }
                }
            }




            var vlines = File.ReadAllLines(pre + "AnimationScripts.txt");

             animations=new Dictionary<string, Tuple<List<string>,List<Animation>>>();

            var len = "dc.w ".Length;
            Animation curAni = null;
            int index = 0;
            int lastDone = 0;
            List<Animation> anim = null; List<string> animFiles = null; 
            foreach (var curc in vlines)
            {
                if(!curc.StartsWith("\t\t"))
                {
                    
                    animations.Add(curc.Split(':')[0], new Tuple<List<string>, List<Animation>>(animFiles=new List<string>(), anim = new List<Animation>()));
                    continue;
                }
                var cur = curc.Replace("\t\t",""); 
                  cur = cur.Replace(";", "");
                if (cur.StartsWith("dc.l") && index != 1)
                {
                    lastDone = index; 
                }

                switch (index++ - lastDone)
                { 
                    case 0:
                        curAni = new Animation();
                        var mm = cur.Replace("dc.l ", "");
                        var f = (mm.Contains('+') ? mm.Split('+') : mm.Split('-'))[0];
                        if(!AnimationFiles.ContainsKey(f))
                            throw new Exception();
                        var d = AnimationFiles[f];
                        var count = 0;
                        if (!animFiles.Contains(f))
                        {
                            count = animFiles.Count;
                            animFiles.Add(f);
                        }
                        else
                        {
                            count = animFiles.IndexOf(f);
                        } 
                        curAni.AnimationFile = count; 
                        anim.Add(curAni);
                        break;
                    case 1:
                        curAni.AnimationTileIndex = int.Parse(cur.Substring(len, cur.Length - len).Replace("$", ""), NumberStyles.HexNumber) / 32;
                        break;
                    case 2:
                        curAni.Frames = new List<AnimationFrame>(int.Parse(cur.Substring(len, cur.Length - len).Replace("$", ""), NumberStyles.HexNumber));
                        break;
                    case 3:
                        curAni.NumberOfTiles = int.Parse(cur.Substring(len, cur.Length - len).Replace("$", "").Split(',')[0], NumberStyles.HexNumber);
                        break;
                    default:
                        {
                            var m = cur.Replace("dc.b", "").Replace("$", "").Replace(" ", "").Split(',');

                            curAni.Frames.Add(new AnimationFrame(int.Parse(m[0], NumberStyles.HexNumber),
                               m.Length == 1 ? 0 :
                                int.Parse(m[1], NumberStyles.HexNumber)));

                        }
                        break;
                }
            }


          //  var dc=animations.Select(a => a.Key).Aggregate("", (a, b) => a + b + "\r\n");
          //  Console.Write(dc);
        }


        private void fileSelector1_FileNameChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fileSelector1.FileName)) return;
            if (!File.Exists(fileSelector1.FileName)) return;

            try
            {
                ini = IniFile.Load(fileSelector1.FileName);
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (NotSupportedException)
            {
                return;
            }
            catch (System.Security.SecurityException)
            {
                return;
            }

            Environment.CurrentDirectory = Path.GetDirectoryName(fileSelector1.FileName);
            try
            {
                LevelData.EngineVersion = (EngineVersion)Enum.Parse(typeof(EngineVersion), ini[string.Empty].GetValueOrDefault("version", "S2"));
            }
            catch
            {
                LevelData.EngineVersion = EngineVersion.Invalid;
            }
            LevelData.littleendian = false;
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S1:
                    LevelData.chunksz = 256;
                    break;
                case EngineVersion.SCDPC:
                    LevelData.chunksz = 256;
                    LevelData.littleendian = true;
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                    LevelData.chunksz = 128;
                    break;
                case EngineVersion.SKC:
                    LevelData.chunksz = 128;
                    LevelData.littleendian = true;
                    break;
                default:
                    throw new NotImplementedException("Game type " + LevelData.EngineVersion.ToString() + " is not supported!");
            }
            comboBox1.Items.Clear();
            foreach (KeyValuePair<string, Dictionary<string, string>> item in ini)
            {
                if (!string.IsNullOrEmpty(item.Key))
                    comboBox1.Items.Add(item.Key);
            }
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                button1.Enabled = false;
            else
                button1.Enabled = true;
            button2.Enabled = File.Exists(fileSelector1.FileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConvertLevel();
        }

        private void ConvertLevel()
        {
            
            Dictionary<string, string> egr = ini[string.Empty];
            try
            {
                LevelData.EngineVersion = (EngineVersion)Enum.Parse(typeof(EngineVersion), egr.GetValueOrDefault("version", "S2"));
            }
            catch
            {
                LevelData.EngineVersion = EngineVersion.Invalid;
            }
            LevelData.littleendian = false;
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S1:
                    LevelData.chunksz = 256;
                    break;
                case EngineVersion.SCDPC:
                    LevelData.chunksz = 256;
                    LevelData.littleendian = true;
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                    LevelData.chunksz = 128;
                    break;
                case EngineVersion.SKC:
                    LevelData.chunksz = 128;
                    LevelData.littleendian = true;
                    break;
                default:
                    throw new NotImplementedException("Game type " + LevelData.EngineVersion.ToString() + " is not supported!");
            }
            string level = (string)comboBox1.SelectedItem;
            Dictionary<string, string> gr = ini[level];

    

            LevelData.TileFmt = gr.ContainsKey("tile8fmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), gr["tile8fmt"]) : egr.ContainsKey("tile8fmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), egr["tile8fmt"]) : LevelData.EngineVersion;
            LevelData.BlockFmt = gr.ContainsKey("block16fmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), gr["block16fmt"]) : egr.ContainsKey("block16fmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), egr["block16fmt"]) : LevelData.EngineVersion;
            LevelData.ChunkFmt = gr.ContainsKey("chunkfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), gr["chunkfmt"]) : egr.ContainsKey("chunkfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), egr["chunkfmt"]) : LevelData.EngineVersion;
            LevelData.LayoutFmt = gr.ContainsKey("layoutfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), gr["layoutfmt"]) : egr.ContainsKey("layoutfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), egr["layoutfmt"]) : LevelData.EngineVersion;
            LevelData.PaletteFmt = gr.ContainsKey("palettefmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), gr["palettefmt"]) : egr.ContainsKey("palettefmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), egr["palettefmt"]) : LevelData.EngineVersion;
            LevelData.ObjectFmt = gr.ContainsKey("objectsfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), gr["objectsfmt"]) : egr.ContainsKey("objectsfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), egr["objectsfmt"]) : LevelData.EngineVersion;
            LevelData.RingFmt = gr.ContainsKey("ringsfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), gr["ringsfmt"]) : egr.ContainsKey("ringsfmt") ? (EngineVersion)Enum.Parse(typeof(EngineVersion), egr["ringsfmt"]) : LevelData.EngineVersion;
            switch (LevelData.ChunkFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.SCDPC:
                    LevelData.chunksz = 256;
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    LevelData.chunksz = 128;
                    break;
            }
            string defcmp;
            string[] tilelist = gr["tile8"].Split('|');
            byte[] tmp = null;
            List<byte> data = new List<byte>();
            LevelData.Tiles = new MultiFileIndexer<byte[]>();



            if (LevelData.TileFmt != EngineVersion.SCDPC)
            {
                switch (LevelData.TileFmt)
                {
                    case EngineVersion.S1:
                    case EngineVersion.S2NA:
                        defcmp = "Nemesis";
                        break;
                    case EngineVersion.S2:
                        defcmp = "Kosinski";
                        break;
                    case EngineVersion.S3K:
                    case EngineVersion.SKC:
                        defcmp = "KosinskiM";
                        break;
                    default:
                        defcmp = string.Empty;
                        break;
                }
                LevelData.TileCmp = (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("tile8cmp", egr.GetValueOrDefault("tile8cmp", defcmp)));
                foreach (string tileent in tilelist)
                {
                    tmp = null;
                    string[] tileentsp = tileent.Split(':');
                    int off = -1;
                    if (tileentsp.Length > 1)
                    {
                        string offstr = tileentsp[1];
                        if (offstr.StartsWith("0x"))
                            off = int.Parse(offstr.Substring(2), System.Globalization.NumberStyles.HexNumber);
                        else
                            off = int.Parse(offstr, System.Globalization.NumberStyles.Integer);
                    }
                    if (File.Exists(presk+tileentsp[0]))
                    {
                        tmp = Compression.Decompress(presk + tileentsp[0], LevelData.TileCmp);
                        List<byte[]> tiles = new List<byte[]>();
                        for (int i = 0; i < tmp.Length; i += 32)
                        {
                            byte[] tile = new byte[32];
                            Array.Copy(tmp, i, tile, 0, 32);
                            tiles.Add(tile);
                        }
                        LevelData.Tiles.AddFile(tiles, off == -1 ? off : off / 32);
                    }
                    else
                        LevelData.Tiles.AddFile(new List<byte[]>() { new byte[32] }, off == -1 ? off : off / 32);
                }
            }
            else
            {
                LevelData.TileCmp = Compression.CompressionType.SZDD;
                if (File.Exists(presk+gr["tile8"]))
                {
                    tmp = Compression.Decompress(presk+gr["tile8"], Compression.CompressionType.SZDD);
                    int sta = ByteConverter.ToInt32(tmp, 0xC);
                    int numt = ByteConverter.ToInt32(tmp, 8);
                    List<byte[]> tiles = new List<byte[]>();
                    for (int i = 0; i < numt; i++)
                    {
                        byte[] tile = new byte[32];
                        Array.Copy(tmp, sta, tile, 0, 32);
                        tiles.Add(tile);
                        sta += 32;
                    }
                    LevelData.Tiles.AddFile(tiles, -1);
                }
                else
                    LevelData.Tiles.AddFile(new List<byte[]>() { new byte[32] }, -1);
            }
            LevelData.Blocks = new MultiFileIndexer<Block>();
            switch (LevelData.BlockFmt)
            {
                case EngineVersion.S1:
                    defcmp = "Enigma";
                    break;
                case EngineVersion.S2:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    defcmp = "Kosinski";
                    break;
                case EngineVersion.SCDPC:
                case EngineVersion.S2NA:
                    defcmp = "Uncompressed";
                    break;
                default:
                    defcmp = string.Empty;
                    break;
            }
            LevelData.BlockCmp = (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("block16cmp", egr.GetValueOrDefault("block16cmp", defcmp)));
            tilelist = gr["block16"].Split('|');
            foreach (string tileent in tilelist)
            {
                string[] tileentsp = tileent.Split(':');
                int off = -1;
                if (tileentsp.Length > 1)
                {
                    string offstr = tileentsp[1];
                    if (offstr.StartsWith("0x"))
                        off = int.Parse(offstr.Substring(2), System.Globalization.NumberStyles.HexNumber);
                    else
                        off = int.Parse(offstr, System.Globalization.NumberStyles.Integer);
                }
                if (File.Exists(presk + tileentsp[0]))
                {
                    tmp = Compression.Decompress(presk+tileentsp[0], LevelData.BlockCmp);
                    List<Block> tmpblk = new List<Block>();
                    if (LevelData.EngineVersion == EngineVersion.SKC)
                        LevelData.littleendian = false;
                    for (int ba = 0; ba < tmp.Length; ba += Block.Size)
                        tmpblk.Add(new Block(tmp, ba));
                    if (LevelData.EngineVersion == EngineVersion.SKC)
                        LevelData.littleendian = true;
                    LevelData.Blocks.AddFile(tmpblk, off == -1 ? off : off / Block.Size);
                }
                else
                    LevelData.Blocks.AddFile(new List<Block>() { new Block() }, off == -1 ? off : off / Block.Size);
            }
            if (LevelData.Blocks.Count == 0)
                LevelData.Blocks.AddFile(new List<Block>() { new Block() }, -1);
            LevelData.Chunks = new MultiFileIndexer<Chunk>();
            switch (LevelData.ChunkFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.S2:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    defcmp = "Kosinski";
                    break;
                case EngineVersion.SCDPC:
                case EngineVersion.S2NA:
                    defcmp = "Uncompressed";
                    break;
                default:
                    defcmp = string.Empty;
                    break;
            }
            LevelData.ChunkCmp = (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("chunk" + LevelData.chunksz + "cmp", egr.GetValueOrDefault("chunk" + LevelData.chunksz + "cmp", defcmp)));
            tilelist = gr["chunk" + LevelData.chunksz].Split('|');
            data = new List<byte>();
            int fileind = 0;
            List<Chunk> tmpchnk = null;
            foreach (string tileent in tilelist)
            {
                string[] tileentsp = tileent.Split(':');
                int off = -1;
                if (tileentsp.Length > 1)
                {
                    string offstr = tileentsp[1];
                    if (offstr.StartsWith("0x"))
                        off = int.Parse(offstr.Substring(2), System.Globalization.NumberStyles.HexNumber);
                    else
                        off = int.Parse(offstr, System.Globalization.NumberStyles.Integer);
                }
                if (File.Exists(presk + tileentsp[0]))
                {
                    tmp = Compression.Decompress(presk+tileentsp[0], LevelData.ChunkCmp);
                    tmpchnk = new List<Chunk>();
                    if (fileind == 0)
                    {
                        switch (LevelData.ChunkFmt)
                        {
                            case EngineVersion.S1:
                            case EngineVersion.SCD:
                            case EngineVersion.SCDPC:
                                tmpchnk.Add(new Chunk());
                                break;
                        }
                    }
                    if (LevelData.EngineVersion == EngineVersion.SKC)
                        LevelData.littleendian = false;
                    for (int ba = 0; ba < tmp.Length; ba += Chunk.Size)
                        tmpchnk.Add(new Chunk(tmp, ba));
                    if (LevelData.EngineVersion == EngineVersion.SKC)
                        LevelData.littleendian = true;
                    LevelData.Chunks.AddFile(tmpchnk, off == -1 ? off : off / Chunk.Size);
                    fileind++;
                }
                else
                    LevelData.Chunks.AddFile(new List<Chunk>() { new Chunk() }, off == -1 ? off : off / Chunk.Size);
            }
            if (LevelData.Chunks.Count == 0)
                LevelData.Chunks.AddFile(new List<Chunk>() { new Chunk() }, -1);
            ushort fgw, fgh, bgw, bgh;
            LevelData.FGLoop = null;
            LevelData.BGLoop = null;
            switch (LevelData.LayoutFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                case EngineVersion.SCDPC:
                    defcmp = "Uncompressed";
                    break;
                case EngineVersion.S2:
                    defcmp = "Kosinski";
                    break;
                default:
                    defcmp = "Uncompressed";
                    break;
            }
            LevelData.LayoutCmp = (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("layoutcmp", egr.GetValueOrDefault("layoutcmp", defcmp)));
            switch (LevelData.LayoutFmt)
            {
                case EngineVersion.S1:
                    int s1xmax = int.Parse(ini[string.Empty]["levelwidthmax"], System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
                    int s1ymax = int.Parse(ini[string.Empty]["levelheightmax"], System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (File.Exists(presk + gr["fglayout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["fglayout"], LevelData.LayoutCmp);
                        fgw = (ushort)(tmp[0] + 1);
                        fgh = (ushort)(tmp[1] + 1);
                        LevelData.FGLayout = new byte[fgw, fgh];
                        LevelData.FGLoop = new bool[fgw, fgh];
                        for (int lr = 0; lr < fgh; lr++)
                            for (int lc = 0; lc < fgw; lc++)
                            {
                                if ((lr * fgw) + lc + 2 >= tmp.Length) break;
                                LevelData.FGLayout[lc, lr] = (byte)(tmp[(lr * fgw) + lc + 2] & 0x7F);
                                LevelData.FGLoop[lc, lr] = (tmp[(lr * fgw) + lc + 2] & 0x80) == 0x80;
                            }
                    }
                    else
                    {
                        LevelData.FGLayout = new byte[s1xmax, s1ymax];
                        LevelData.FGLoop = new bool[s1xmax, s1ymax];
                    }
                    if (File.Exists(presk + gr["bglayout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["bglayout"], LevelData.LayoutCmp);
                        bgw = (ushort)(tmp[0] + 1);
                        bgh = (ushort)(tmp[1] + 1);
                        LevelData.BGLayout = new byte[bgw, bgh];
                        LevelData.BGLoop = new bool[bgw, bgh];
                        for (int lr = 0; lr < bgh; lr++)
                            for (int lc = 0; lc < bgw; lc++)
                            {
                                LevelData.BGLayout[lc, lr] = (byte)(tmp[(lr * bgw) + lc + 2] & 0x7F);
                                LevelData.BGLoop[lc, lr] = (tmp[(lr * bgw) + lc + 2] & 0x80) == 0x80;
                            }
                    }
                    else
                    {
                        LevelData.BGLayout = new byte[s1xmax, s1ymax];
                        LevelData.BGLoop = new bool[s1xmax, s1ymax];
                    }
                    break;
                case EngineVersion.S2NA:
                    s1xmax = int.Parse(ini[string.Empty]["levelwidthmax"], System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
                    s1ymax = int.Parse(ini[string.Empty]["levelheightmax"], System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (File.Exists(presk + gr["fglayout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["fglayout"], LevelData.LayoutCmp);
                        fgw = (ushort)(tmp[0] + 1);
                        fgh = (ushort)(tmp[1] + 1);
                        LevelData.FGLayout = new byte[fgw, fgh];
                        for (int lr = 0; lr < fgh; lr++)
                            for (int lc = 0; lc < fgw; lc++)
                            {
                                if ((lr * fgw) + lc + 2 >= tmp.Length) break;
                                LevelData.FGLayout[lc, lr] = tmp[(lr * fgw) + lc + 2];
                            }
                    }
                    else
                        LevelData.FGLayout = new byte[s1xmax, s1ymax];
                    if (File.Exists(presk + gr["bglayout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["bglayout"], LevelData.LayoutCmp);
                        bgw = (ushort)(tmp[0] + 1);
                        bgh = (ushort)(tmp[1] + 1);
                        LevelData.BGLayout = new byte[bgw, bgh];
                        for (int lr = 0; lr < bgh; lr++)
                            for (int lc = 0; lc < bgw; lc++)
                            {
                                LevelData.BGLayout[lc, lr] = tmp[(lr * bgw) + lc + 2];
                            }
                    }
                    else
                        LevelData.BGLayout = new byte[s1xmax, s1ymax];
                    break;
                case EngineVersion.S2:
                    LevelData.FGLayout = new byte[128, 16];
                    LevelData.BGLayout = new byte[128, 16];
                    if (File.Exists(presk + gr["layout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["layout"], LevelData.LayoutCmp);
                        for (int la = 0; la < tmp.Length; la += 256)
                        {
                            for (int laf = 0; laf < 128; laf++)
                                LevelData.FGLayout[laf, la / 256] = tmp[la + laf];
                            for (int lab = 0; lab < 128; lab++)
                                LevelData.BGLayout[lab, la / 256] = tmp[la + lab + 128];
                        }
                    }
                    break;
                case EngineVersion.S3K:
                    if (File.Exists(presk + gr["layout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["layout"], LevelData.LayoutCmp);
                        fgw = ByteConverter.ToUInt16(tmp, 0);
                        bgw = ByteConverter.ToUInt16(tmp, 2);
                        fgh = ByteConverter.ToUInt16(tmp, 4);
                        bgh = ByteConverter.ToUInt16(tmp, 6);
                        LevelData.FGLayout = new byte[fgw, fgh];
                        LevelData.BGLayout = new byte[bgw, bgh];
                        for (int la = 0; la < Math.Max(fgh, bgh) * 4; la += 4)
                        {
                            ushort lfp = ByteConverter.ToUInt16(tmp, 8 + la);
                            if (lfp != 0)
                                for (int laf = 0; laf < fgw; laf++)
                                    LevelData.FGLayout[laf, la / 4] = tmp[lfp - 0x8000 + laf];
                            ushort lbp = ByteConverter.ToUInt16(tmp, 8 + la + 2);
                            if (lbp != 0)
                                for (int lab = 0; lab < bgw; lab++)
                                    LevelData.BGLayout[lab, la / 4] = tmp[lbp - 0x8000 + lab];
                        }
                    }
                    else
                    {
                        LevelData.FGLayout = new byte[128, 16];
                        LevelData.BGLayout = new byte[128, 16];
                    }
                    break;
                case EngineVersion.SKC:
                    if (File.Exists(presk + gr["layout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["layout"], LevelData.LayoutCmp);
                        fgw = ByteConverter.ToUInt16(tmp, 0);
                        bgw = ByteConverter.ToUInt16(tmp, 2);
                        fgh = ByteConverter.ToUInt16(tmp, 4);
                        bgh = ByteConverter.ToUInt16(tmp, 6);
                        LevelData.FGLayout = new byte[fgw, fgh];
                        LevelData.BGLayout = new byte[bgw, bgh];
                        for (int la = 0; la < Math.Max(fgh, bgh) * 4; la += 4)
                        {
                            ushort lfp = ByteConverter.ToUInt16(tmp, 8 + la);
                            if (lfp != 0)
                                for (int laf = 0; laf < fgw; laf++)
                                    LevelData.FGLayout[laf, la / 4] = tmp[(lfp - 0x8000 + laf) ^ 1];
                            ushort lbp = ByteConverter.ToUInt16(tmp, 8 + la + 2);
                            if (lbp != 0)
                                for (int lab = 0; lab < bgw; lab++)
                                    LevelData.BGLayout[lab, la / 4] = tmp[(lbp - 0x8000 + lab) ^ 1];
                        }
                    }
                    else
                    {
                        LevelData.FGLayout = new byte[128, 16];
                        LevelData.BGLayout = new byte[128, 16];
                    }
                    break;
                case EngineVersion.SCDPC:
                    LevelData.FGLayout = new byte[64, 8];
                    if (File.Exists(presk + gr["fglayout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["fglayout"], LevelData.LayoutCmp);
                        for (int lr = 0; lr < 8; lr++)
                            for (int lc = 0; lc < 64; lc++)
                            {
                                if ((lr * 64) + lc >= tmp.Length) break;
                                LevelData.FGLayout[lc, lr] = tmp[(lr * 64) + lc];
                            }
                    }
                    LevelData.BGLayout = new byte[64, 8];
                    if (File.Exists(presk + gr["bglayout"]))
                    {
                        tmp = Compression.Decompress(presk+gr["bglayout"], LevelData.LayoutCmp);
                        for (int lr = 0; lr < 8; lr++)
                            for (int lc = 0; lc < 64; lc++)
                                LevelData.BGLayout[lc, lr] = tmp[(lr * 64) + lc];
                    }
                    break;
            }
            LevelData.Palette = new ushort[4, 16];
            string[] palentstr;
            if (gr.ContainsKey("palette"))
            {
                palentstr = gr["palette"].Split('|');
                for (byte pn = 0; pn < palentstr.Length; pn++)
                {
                    string[] palent = palentstr[pn].Split(':');
                    tmp = File.ReadAllBytes(presk + palent[0]);
                    ushort[] palfile;
                    if (LevelData.PaletteFmt != EngineVersion.SCDPC)
                    {
                        palfile = new ushort[tmp.Length / 2];
                        for (int pi = 0; pi < tmp.Length; pi += 2)
                            palfile[pi / 2] = ByteConverter.ToUInt16(tmp, pi);
                    }
                    else
                    {
                        palfile = new ushort[tmp.Length / 4];
                        for (int pi = 0; pi < tmp.Length; pi += 4)
                            palfile[pi / 4] = (ushort)((tmp[pi] >> 4) | (tmp[pi + 1] & 0xF0) | ((tmp[pi + 2] >> 4) << 8));
                    }
                    int src = int.Parse(palent[1], System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
                    int dest = int.Parse(palent[2], System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
                    for (int pa = 0; pa < int.Parse(palent[3], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture); pa++)
                        LevelData.Palette[(pa + dest) / 16, (pa + dest) % 16] = palfile[pa + src];
                }
            }
            LevelData.Objects = new List<ObjectEntry>();
            if (gr.ContainsKey("objects"))
            {
                if (File.Exists(presk + gr["objects"]))
                {
                    tmp = Compression.Decompress(presk+gr["objects"], (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("objectscmp", "Uncompressed")));
                    switch (LevelData.ObjectFmt)
                    {
                        case EngineVersion.S1:
                            for (int oa = 0; oa < tmp.Length; oa += S1ObjectEntry.Size)
                            {
                                if (ByteConverter.ToUInt16(tmp, oa) == 0xFFFF) break;
                                ObjectEntry ent = new S1ObjectEntry(tmp, oa);
                                LevelData.Objects.Add(ent);
                            }
                            break;
                        case EngineVersion.S2:
                        case EngineVersion.S2NA:
                            for (int oa = 0; oa < tmp.Length; oa += S2ObjectEntry.Size)
                            {
                                if (ByteConverter.ToUInt16(tmp, oa) == 0xFFFF) break;
                                ObjectEntry ent = new S2ObjectEntry(tmp, oa);
                                LevelData.Objects.Add(ent);
                            }
                            break;
                        case EngineVersion.S3K:
                        case EngineVersion.SKC:
                            for (int oa = 0; oa < tmp.Length; oa += S3KObjectEntry.Size)
                            {
                                if (ByteConverter.ToUInt16(tmp, oa) == 0xFFFF) break;
                                ObjectEntry ent = new S3KObjectEntry(tmp, oa);
                                LevelData.Objects.Add(ent);
                            }
                            break;
                        case EngineVersion.SCDPC:
                            for (int oa = 0; oa < tmp.Length; oa += SCDObjectEntry.Size)
                            {
                                if (ByteConverter.ToUInt64(tmp, oa) == 0xFFFFFFFFFFFFFFFF) break;
                                ObjectEntry ent = new SCDObjectEntry(tmp, oa);
                                LevelData.Objects.Add(ent);
                            }
                            break;
                    }
                }
            }
            LevelData.Rings = new List<RingEntry>();
            if (gr.ContainsKey("rings"))
            {
                switch (LevelData.RingFmt)
                {
                    case EngineVersion.S2:
                    case EngineVersion.S2NA:
                        if (File.Exists(presk + gr["rings"]))
                        {
                            tmp = Compression.Decompress(presk+gr["rings"], (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("ringscmp", "Uncompressed")));
                            for (int oa = 0; oa < tmp.Length; oa += S2RingEntry.Size)
                            {
                                if (ByteConverter.ToUInt16(tmp, oa) == 0xFFFF) break;
                                LevelData.Rings.Add(new S2RingEntry(tmp, oa));
                            }
                        }
                        break;
                    case EngineVersion.S3K:
                    case EngineVersion.SKC:
                        if (File.Exists(presk + gr["rings"]))
                        {
                            tmp = Compression.Decompress(presk + gr["rings"], (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("ringscmp", "Uncompressed")));
                            for (int oa = 4; oa < tmp.Length; oa += S3KRingEntry.Size)
                            {
                                if (ByteConverter.ToUInt16(tmp, oa) == 0xFFFF) break;
                                LevelData.Rings.Add(new S3KRingEntry(tmp, oa));
                            }
                        }
                        break;
                }
            }
            if (gr.ContainsKey("bumpers"))
            {
                LevelData.Bumpers = new List<CNZBumperEntry>();
                if (File.Exists(presk + gr["bumpers"]))
                {
                    tmp = Compression.Decompress(presk + gr["bumpers"], (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("bumperscmp", "Uncompressed")));
                    for (int i = 0; i < tmp.Length; i += CNZBumperEntry.Size)
                    {
                        if (ByteConverter.ToUInt16(tmp, i + 2) == 0xFFFF) break;
                        CNZBumperEntry ent = new CNZBumperEntry(tmp, i);
                        LevelData.Bumpers.Add(ent);
                    }
                }
            }
            else
                LevelData.Bumpers = null;

            LevelData.StartPositions = new List<StartPositionEntry>();
            if (gr.ContainsKey("startpos"))
            {
                string[] stposs = gr["startpos"].Split('|');
                foreach (string item in stposs)
                {
                    string[] stpos = item.Split(':');
                    if (File.Exists(presk + stpos[0]))
                    {
                        StartPositionEntry ent = new StartPositionEntry(System.IO.File.ReadAllBytes(presk + stpos[0]), 0);
                        LevelData.StartPositions.Add(ent);
                    }
                    else
                    {
                        StartPositionEntry ent = new StartPositionEntry();
                        LevelData.StartPositions.Add(ent);
                    }
                }
            }

            LevelData.ColInds1 = new List<byte>();
            LevelData.ColInds2 = new List<byte>();
            switch (LevelData.EngineVersion)
            {
                case EngineVersion.S1:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                case EngineVersion.SCDPC:
                    defcmp = "Uncompressed";
                    break;
                case EngineVersion.S2:
                    defcmp = "Kosinski";
                    break;
                default:
                    defcmp = "Uncompressed";
                    break;
            }
            LevelData.ColIndCmp = (Compression.CompressionType)Enum.Parse(typeof(Compression.CompressionType), gr.GetValueOrDefault("colindcmp", egr.GetValueOrDefault("colindcmp", defcmp)));
            switch (LevelData.ChunkFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.SCD:
                case EngineVersion.SCDPC:
                    if (gr.ContainsKey("colind") && File.Exists(presk + gr["colind"]))
                        LevelData.ColInds1.AddRange(Compression.Decompress(presk + gr["colind"], LevelData.ColIndCmp));
                    LevelData.ColInds2 = LevelData.ColInds1;
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                    if (gr.ContainsKey("colind1") && File.Exists(presk + gr["colind1"]))
                        LevelData.ColInds1.AddRange(Compression.Decompress(presk + gr["colind1"], LevelData.ColIndCmp));
                    if (gr.ContainsKey("colind2"))
                    {
                        if (File.Exists(presk + gr["colind2"]))
                            LevelData.ColInds2.AddRange(Compression.Decompress(presk + gr["colind2"], LevelData.ColIndCmp));
                    }
                    else
                        LevelData.ColInds2 = LevelData.ColInds1;
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    if (gr.ContainsKey("colind"))
                    {
                        if (File.Exists(presk + gr["colind"]))
                        {
                            tmp = Compression.Decompress(presk + gr["colind"], LevelData.ColIndCmp);
                            int colindt = int.Parse(gr.GetValueOrDefault("colindsz", "1"));
                            switch (colindt)
                            {
                                case 1:
                                    for (int i = 0; i < 0x600; i += 2)
                                    {
                                        LevelData.ColInds1.Add(tmp[i]);
                                        LevelData.ColInds2.Add(tmp[i + 1]);
                                    }
                                    break;
                                case 2:
                                    for (int i = 0; i < 0x600; i += 2)
                                        LevelData.ColInds1.Add((byte)ByteConverter.ToUInt16(tmp, i));
                                    for (int i = 0x600; i < 0xC00; i += 2)
                                        LevelData.ColInds2.Add((byte)ByteConverter.ToUInt16(tmp, i));
                                    break;
                            }
                        }
                        else
                        {
                            LevelData.ColInds1.AddRange(new byte[0x300]);
                            LevelData.ColInds2.AddRange(new byte[0x300]);
                        }
                    }
                    break;
            }
            if (LevelData.EngineVersion != EngineVersion.S3K && LevelData.EngineVersion != EngineVersion.SKC)
            {
                if (LevelData.ColInds1.Count < LevelData.Blocks.Count)
                    LevelData.ColInds1.AddRange(new byte[LevelData.Blocks.Count - LevelData.ColInds1.Count]);
                if (LevelData.ColInds2.Count < LevelData.Blocks.Count)
                    LevelData.ColInds2.AddRange(new byte[LevelData.Blocks.Count - LevelData.ColInds2.Count]);
            }
            LevelData.ColArr1 = new sbyte[256][];
            LevelData.ColArr2 = new sbyte[256][];
            if (File.Exists(presk + gr.GetValueOrDefault("colarr1", ini[string.Empty].GetValueOrDefault("colarr1", string.Empty))))
                tmp = Compression.Decompress(presk + gr.GetValueOrDefault("colarr1", ini[string.Empty].GetValueOrDefault("colarr1", null)), Compression.CompressionType.Uncompressed);
            else
                tmp = new byte[256 * 16];
            for (int i = 0; i < 256; i++)
            {
                LevelData.ColArr1[i] = new sbyte[16];
                for (int j = 0; j < 16; j++)
                    LevelData.ColArr1[i][j] = unchecked((sbyte)tmp[(i * 16) + j]);
            }
            if (File.Exists(presk + gr.GetValueOrDefault("colarr2", ini[string.Empty].GetValueOrDefault("colarr2", string.Empty))))
                tmp = Compression.Decompress(presk + gr.GetValueOrDefault("colarr2", ini[string.Empty].GetValueOrDefault("colarr2", null)), Compression.CompressionType.Uncompressed);
            else
                tmp = new byte[256 * 16];
            for (int i = 0; i < 256; i++)
            {
                LevelData.ColArr2[i] = new sbyte[16];
                for (int j = 0; j < 16; j++)
                    LevelData.ColArr2[i][j] = unchecked((sbyte)tmp[(i * 16) + j]);
            }
            if (File.Exists(presk + gr.GetValueOrDefault("angles", ini[string.Empty].GetValueOrDefault("angles", string.Empty))))
                LevelData.Angles = Compression.Decompress(presk + gr.GetValueOrDefault("angles", ini[string.Empty].GetValueOrDefault("angles", string.Empty)), Compression.CompressionType.Uncompressed);
            else
                LevelData.Angles = new byte[256];
            bool LE = LevelData.littleendian;
            LevelData.littleendian = false;
            JSONLevelData outdata = new JSONLevelData();
            outdata.Tiles = LevelData.Tiles.ToArray();

            outdata.Animations = new List<Animation>();
            var cd = gr.GetValueOrDefault("animations", string.Empty);
            if (cd != string.Empty)
            {
                Tuple<List<string>, List<Animation>> df = animations["AniPLC_" + cd];
                outdata.AnimatedFiles = AnimationFiles.Where(a => df.Value1.Contains(a.Key)).Select(a => a.Value).ToArray();
                outdata.Animations = df.Value2;
            }
            else
            {
                Console.Write("");
            }
            List<PatternIndex[]> tmpblockdata = new List<PatternIndex[]>();
            foreach (Block item in LevelData.Blocks)
                tmpblockdata.Add(item.Tiles);
            outdata.Blocks = tmpblockdata.ToArray();
            outdata.StartPositions = LevelData.StartPositions;
            switch (LevelData.ChunkFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.SCDPC:
                    tmpchnk = new List<Chunk>() { new Chunk() };
                    LevelData.Chunks.RemoveAt(0);
                    foreach (Chunk item in LevelData.Chunks)
                    {
                        Chunk[] newchnk = new Chunk[4];
                        for (int i = 0; i < 4; i++)
                            newchnk[i] = new Chunk();
                        for (int y = 0; y < 8; y++)
                        {
                            for (int x = 0; x < 8; x++)
                            {
                                S2ChunkBlock blk = (S2ChunkBlock)newchnk[0].blocks[x, y];
                                blk.Block = item.blocks[x, y].Block;
                                blk.Solid1 = item.blocks[x, y].Solid1;
                                blk.Solid2 = blk.Solid1;
                                blk.XFlip = item.blocks[x, y].XFlip;
                                blk.YFlip = item.blocks[x, y].YFlip;
                                blk = (S2ChunkBlock)newchnk[1].blocks[x, y];
                                blk.Block = item.blocks[x + 8, y].Block;
                                blk.Solid1 = item.blocks[x + 8, y].Solid1;
                                blk.Solid2 = blk.Solid1;
                                blk.XFlip = item.blocks[x + 8, y].XFlip;
                                blk.YFlip = item.blocks[x + 8, y].YFlip;
                                blk = (S2ChunkBlock)newchnk[2].blocks[x, y];
                                blk.Block = item.blocks[x, y + 8].Block;
                                blk.Solid1 = item.blocks[x, y + 8].Solid1;
                                blk.Solid2 = blk.Solid1;
                                blk.XFlip = item.blocks[x, y + 8].XFlip;
                                blk.YFlip = item.blocks[x, y + 8].YFlip;
                                blk = (S2ChunkBlock)newchnk[3].blocks[x, y];
                                blk.Block = item.blocks[x + 8, y + 8].Block;
                                blk.Solid1 = item.blocks[x + 8, y + 8].Solid1;
                                blk.Solid2 = blk.Solid1;
                                blk.XFlip = item.blocks[x + 8, y + 8].XFlip;
                                blk.YFlip = item.blocks[x + 8, y + 8].YFlip;
                            }
                        }
                        tmpchnk.AddRange(newchnk);
                    }
                    byte[,] newFG = new byte[LevelData.FGLayout.GetLength(0) * 2, LevelData.FGLayout.GetLength(1) * 2];
                    for (int y = 0; y < LevelData.FGLayout.GetLength(1); y++)
                    {
                        for (int x = 0; x < LevelData.FGLayout.GetLength(0); x++)
                        {
                            if (LevelData.FGLayout[x, y] != 0)
                            {
                                newFG[x * 2, y * 2] = (byte)((LevelData.FGLayout[x, y] * 4) - 3);
                                newFG[(x * 2) + 1, y * 2] = (byte)((LevelData.FGLayout[x, y] * 4) - 2);
                                newFG[x * 2, (y * 2) + 1] = (byte)((LevelData.FGLayout[x, y] * 4) - 1);
                                newFG[(x * 2) + 1, (y * 2) + 1] = (byte)((LevelData.FGLayout[x, y] * 4));
                            }
                        }
                    }
                    LevelData.FGLayout = newFG;
                    byte[,] newBG = new byte[LevelData.BGLayout.GetLength(0) * 2, LevelData.BGLayout.GetLength(1) * 2];
                    for (int y = 0; y < LevelData.BGLayout.GetLength(1); y++)
                    {
                        for (int x = 0; x < LevelData.BGLayout.GetLength(0); x++)
                        {
                            if (LevelData.BGLayout[x, y] != 0)
                            {
                                newBG[x * 2, y * 2] = (byte)((LevelData.BGLayout[x, y] * 4) - 3);
                                newBG[(x * 2) + 1, y * 2] = (byte)((LevelData.BGLayout[x, y] * 4) - 2);
                                newBG[x * 2, (y * 2) + 1] = (byte)((LevelData.BGLayout[x, y] * 4) - 1);
                                newBG[(x * 2) + 1, (y * 2) + 1] = (byte)((LevelData.BGLayout[x, y] * 4));
                            }
                        }
                    }
                    LevelData.BGLayout = newBG;
                    break;
                default:
                    tmpchnk = LevelData.Chunks.ToList();
                    break;
            }
            List<ChunkBlock[]> tmpchnkdata = new List<ChunkBlock[]>();
            foreach (Chunk chunk in tmpchnk)
                tmpchnkdata.Add(chunk.Blocks);
            outdata.Chunks = tmpchnkdata.ToArray();


            string[][] m = new string[LevelData.Palette.GetLength(0)][];

            for (int i = 0; i < LevelData.Palette.GetLength(0); i++)
            {
                m[i] = new string[LevelData.Palette.GetLength(1)];
                for (int j = 0; j < LevelData.Palette.GetLength(1); j++)
                {
                    m[i][j] = LevelData.Palette[i, j].ToString("X3");

                    m[i][j] = (int.Parse(m[i][j].Substring(2, 1), NumberStyles.HexNumber) * 0x11).ToString("X2")
                        + (int.Parse(m[i][j].Substring(1, 1), NumberStyles.HexNumber) * 0x11).ToString("X2")
                        + (int.Parse(m[i][j].Substring(0, 1), NumberStyles.HexNumber) * 0x11).ToString("X2");
                    //                    m[i][j] = m[i][j].Substring(2, 2) + m[i][j].Substring(0, 2) + m[i][j].Substring(4, 2);

                }
            }




            outdata.Palette = m;
            outdata.ForegroundWidth = LevelData.FGLayout.GetLength(0);
            outdata.BackgroundWidth = LevelData.BGLayout.GetLength(0);
            outdata.ForegroundHeight = LevelData.FGLayout.GetLength(1);
            outdata.BackgroundHeight = LevelData.BGLayout.GetLength(1);
            List<byte> tmplayout = new List<byte>();
            for (int y = 0; y < outdata.ForegroundHeight; y++)
                for (int x = 0; x < outdata.ForegroundWidth; x++)
                    tmplayout.Add(LevelData.FGLayout[x, y]);
            outdata.Foreground = tmplayout.ToArray();
            tmplayout = new List<byte>();
            for (int y = 0; y < outdata.BackgroundHeight; y++)
                for (int x = 0; x < outdata.BackgroundWidth; x++)
                    tmplayout.Add(LevelData.BGLayout[x, y]);
            outdata.Background = tmplayout.ToArray();
            outdata.Objects = LevelData.Objects.ToArray();
            outdata.ObjectFormat = LevelData.ObjectFmt.ToString();
            outdata.Rings = LevelData.Rings.ToArray();
            outdata.RingFormat = LevelData.RingFmt.ToString();
            outdata.CNZBumpers = LevelData.Bumpers == null ? null : LevelData.Bumpers.ToArray();
            outdata.CollisionIndexes1 = LevelData.ColInds1.ToArray();
            switch (LevelData.ChunkFmt)
            {
                case EngineVersion.S1:
                case EngineVersion.SCD:
                case EngineVersion.SCDPC:
                    outdata.CollisionIndexes2 = outdata.CollisionIndexes1;
                    break;
                default:
                    outdata.CollisionIndexes2 = LevelData.ColInds2.ToArray();
                    break;
            }
            outdata.HeightMaps = LevelData.ColArr1;
            outdata.RotatedHeightMaps = LevelData.ColArr2;
            outdata.Angles = LevelData.Angles;
            StreamWriter sw = new StreamWriter(level + ".js");
            JsonSerializer js = new JsonSerializer();
            JsonTextWriter jw = new JsonTextWriter(sw);
#if DEBUG
            jw.Formatting = Formatting.Indented;
            jw.Indentation = 1;
            jw.IndentChar = '\t';
#endif
            js.Serialize(jw, outdata);
            jw.Close();

            File.WriteAllText("../LevelOutput/" + level + ".js", File.ReadAllText(level + ".js"));
            File.WriteAllText("../LevelOutput/" + level + ".min.js", new Jsonner(File.ReadAllText(level + ".js")).get());
            LevelData.littleendian = LE;
        }


  /*      private string[] addAnimations(JSONLevelData outdata)
        {
             
            //line 56291
            string data =
                @"		dc.l ArtUnc_AniCNZ__0+$3000000
		dc.w $5640
		dc.b $10
		dc.b 9
		dc.b	0, $12
		dc.b  $24, $36
		dc.b  $48, $5A
		dc.b  $6C, $7E
		dc.b	9, $1B
		dc.b  $2D, $3F
		dc.b  $51, $63
		dc.b  $75, $87
		dc.l ArtUnc_AniCNZ__0+$3000000
		dc.w $5760
		dc.b $10
		dc.b 9
		dc.b	9, $1B
		dc.b  $2D, $3F
		dc.b  $51, $63
		dc.b  $75, $87
		dc.b	0, $12
		dc.b  $24, $36
		dc.b  $48, $5A
		dc.b  $6C, $7E
		dc.l ArtUnc_AniCNZ__1+$3000000
		dc.w $5880
		dc.b $10
		dc.b $10
		dc.b	0, $10
		dc.b  $20, $30
		dc.b  $40, $50
		dc.b  $60, $70
		dc.b  $80, $90
		dc.b  $A0, $B0
		dc.b  $C0, $D0
		dc.b  $E0, $F0
		dc.l ArtUnc_AniCNZ__2+$3000000
		dc.w $5A80
		dc.b 8
		dc.b $20
		dc.b	0, $20
		dc.b  $40, $60
		dc.b  $80, $A0
		dc.b  $C0, $E0
		dc.l ArtUnc_AniCNZ__3+$3000000
		dc.w $5E80
		dc.b 8
		dc.b $10
		dc.b	0, $10
		dc.b  $20, $30
		dc.b  $40, $50
		dc.b  $60, $70
		dc.l ArtUnc_AniCNZ__4+$3000000
		dc.w $6080
		dc.b 6
		dc.b 4
		dc.b	0,   4
		dc.b	8,   0
		dc.b	4,   8
		dc.l ArtUnc_AniCNZ__5+$1000000
		dc.w $6500
		dc.b 4
		dc.b $14
		dc.b	0, $14
		dc.b  $28, $3C";
            var len = "dc.w ".Length;
            Animation curAni = null;
            int index = 0;
            int lastDone = 0;
            foreach (var curc in data.Replace("\t", "").Split('\n'))
            {
                var cur = curc.Replace(";", "");
                if (cur.StartsWith("dc.l") && index != 1)
                {
                    lastDone = index;
                }

                switch (index++ - lastDone)
                {
                    case 0:
                        curAni = new Animation();
                        curAni.AnimationFile = outdata.Animations.Count;//fix

                        outdata.Animations.Add(curAni);
                        break;
                    case 1:
                        curAni.AnimationTileIndex = int.Parse(cur.Substring(len, cur.Length - len).Replace("$", ""), NumberStyles.HexNumber) / 32;
                        break;
                    case 2:
                        curAni.Frames = new List<AnimationFrame>(int.Parse(cur.Substring(len, cur.Length - len).Replace("$", ""), NumberStyles.HexNumber));
                        break;
                    case 3:
                        curAni.NumberOfTiles = int.Parse(cur.Substring(len, cur.Length - len).Replace("$", "").Split(',')[0], NumberStyles.HexNumber);
                        break;
                    default:
                        {
                            var m = cur.Replace("dc.b", "").Replace("$", "").Replace(" ", "").Split(',');

                            curAni.Frames.Add(new AnimationFrame(int.Parse(m[0], NumberStyles.HexNumber),
                               m.Length == 1 ? 0 :
                                int.Parse(m[1], NumberStyles.HexNumber)));

                        }
                        break;
                }
            }
            return;






            var animation = new Animation(0, 0x5cc0 / 0x20, 0xc);
            animation.AddFrame(new AnimationFrame(0x3c, 0x4f));
            animation.AddFrame(new AnimationFrame(0x30, 5));
            animation.AddFrame(new AnimationFrame(0x18, 5));
            animation.AddFrame(new AnimationFrame(0xC, 5));
            animation.AddFrame(new AnimationFrame(0x0, 0x4f));
            animation.AddFrame(new AnimationFrame(0xC, 3));
            animation.AddFrame(new AnimationFrame(0x18, 3));
            animation.AddFrame(new AnimationFrame(0x24, 1));
            animation.AddFrame(new AnimationFrame(0x30, 1));
            outdata.Animations.Add(animation);


            animation = new Animation(0, 0x5e40 / 0x20, 0xc);
            animation.AddFrame(new AnimationFrame(0x18, 5));
            animation.AddFrame(new AnimationFrame(0x24, 5));
            animation.AddFrame(new AnimationFrame(0x30, 5));
            animation.AddFrame(new AnimationFrame(0x3C, 0x27));
            animation.AddFrame(new AnimationFrame(0x0, 5));
            animation.AddFrame(new AnimationFrame(0xC, 5));
            animation.AddFrame(new AnimationFrame(0x18, 5));
            animation.AddFrame(new AnimationFrame(0x24, 5));

            outdata.Animations.Add(animation);

            animation = new Animation(1, 0x5FC0 / 0x20, 6);
            animation.AddFrame(new AnimationFrame(0, 7));
            animation.AddFrame(new AnimationFrame(6, 3));
            animation.AddFrame(new AnimationFrame(0xc, 3));
            animation.AddFrame(new AnimationFrame(0x12, 3));
            animation.AddFrame(new AnimationFrame(0x18, 7));
            animation.AddFrame(new AnimationFrame(0x12, 3));
            animation.AddFrame(new AnimationFrame(0xc, 3));
            animation.AddFrame(new AnimationFrame(6, 3));

            outdata.Animations.Add(animation);
        }
*/
        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                comboBox1.SelectedIndex = i;
                ConvertLevel();
                Application.DoEvents();
            }
            System.Diagnostics.Process.Start(Dir);
        }
    }

    public class Tuple<T, T1>
    {
        public T Value1;
        public T1 Value2;
        public Tuple(T t,T1 t1)
        {
            Value1 = t;
            Value2 = t1;
        } 
    }
}