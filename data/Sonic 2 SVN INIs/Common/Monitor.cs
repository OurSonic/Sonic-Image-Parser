using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using SonicRetro.SonLVL;

namespace S2ObjectDefinitions.Common
{
    class Monitor : ObjectDefinition
    {
        private Point offset;
        private BitmapBits img;
        private List<Point> offsets = new List<Point>();
        private List<BitmapBits> imgs = new List<BitmapBits>();

        public override void Init(Dictionary<string, string> data)
        {
            List<byte> tmpartfile = new List<byte>();
            tmpartfile.AddRange(ObjectHelper.OpenArtFile("../art/nemesis/Monitor and contents.bin", Compression.CompressionType.Nemesis));
            tmpartfile.AddRange(new byte[0x2A80 - tmpartfile.Count]);
            tmpartfile.AddRange(ObjectHelper.OpenArtFile("../art/nemesis/Sonic lives counter.bin", Compression.CompressionType.Nemesis));
            byte[] artfile = tmpartfile.ToArray();
            byte[] mapfile = System.IO.File.ReadAllBytes("../mappings/sprite/obj26.bin");
            img = ObjectHelper.MapToBmp(artfile, mapfile, 1, 0, out offset);
            Point off;
            BitmapBits im;
            for (int i = 0; i < 11; i++)
            {
                im = ObjectHelper.MapToBmp(artfile, mapfile, i + 1, 0, out off);
                imgs.Add(im);
                offsets.Add(off);
            }
        }

        public override ReadOnlyCollection<byte> Subtypes()
        {
            return new ReadOnlyCollection<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        public override string Name()
        {
            return "Monitor";
        }

        public override bool RememberState()
        {
            return true;
        }

        public override string SubtypeName(byte subtype)
        {
            switch (subtype)
            {
                case 0:
                    return "Static";
                case 1:
                    return "Sonic";
                case 2:
                    return "Tails";
                case 3:
                    return "Eggman";
                case 4:
                    return "Rings";
                case 5:
                    return "Shoes";
                case 6:
                    return "Shield";
                case 7:
                    return "Invincibility";
                case 8:
                    return "Teleport";
                case 9:
                    return "Random";
                case 10:
                    return "Broken";
                default:
                    return "Invalid";
            }
        }

        public override BitmapBits Image()
        {
            return img;
        }

        public override BitmapBits Image(byte subtype)
        {
            if (subtype <= 10)
                return imgs[subtype];
            else
                return img;
        }

        public override Rectangle Bounds(ObjectEntry obj, Point camera)
        {
            if (obj.SubType <= 10)
                return new Rectangle(obj.X + offsets[obj.SubType].X - camera.X, obj.Y + offsets[obj.SubType].Y - camera.Y, imgs[obj.SubType].Width, imgs[obj.SubType].Height);
            else
                return new Rectangle(obj.X + offset.X - camera.X, obj.Y + offset.Y - camera.Y, img.Width, img.Height);
        }

        public override void Draw(BitmapBits bmp, Point camera, ObjectEntry obj, bool includeDebug)
        {
            byte subtype = obj.SubType;
            if (subtype > 10) subtype = 0;
            BitmapBits bits = new BitmapBits(imgs[subtype]);
            bits.Flip(obj.XFlip, obj.YFlip);
            bmp.DrawBitmapComposited(bits, new Point(obj.X + offsets[subtype].X - camera.X, obj.Y + offsets[subtype].Y - camera.Y));
        }

        public override Type ObjectType { get { return typeof(MonitorS2ObjectEntry); } }
    }

    public class MonitorS2ObjectEntry : S2ObjectEntry
    {
        public MonitorS2ObjectEntry() : base() { }
        public MonitorS2ObjectEntry(byte[] file, int address) : base(file, address) { }

        public MonitorType Contents
        {
            get
            {
                if (SubType > 10) return MonitorType.Invalid;
                return (MonitorType)SubType;
            }
            set
            {
                SubType = (byte)value;
            }
        }
    }

    public enum MonitorType
    {
        Static,
        Sonic,
        Tails,
        Eggman,
        Rings,
        Shoes,
        Shield,
        Invincibility,
        Teleport,
        Random,
        Broken,
        Invalid
    }
}