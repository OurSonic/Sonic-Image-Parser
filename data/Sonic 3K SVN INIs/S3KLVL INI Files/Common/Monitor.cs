﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using SonicRetro.SonLVL;

namespace S3KObjectDefinitions.Common
{
    class Monitor : ObjectDefinition
    {
        private Point offset;
        private BitmapBits img;
        private int imgw, imgh;
        private List<Point> offsets = new List<Point>();
        private List<BitmapBits> imgs = new List<BitmapBits>();
        private List<int> imgws = new List<int>();
        private List<int> imghs = new List<int>();

        public override void Init(Dictionary<string, string> data)
        {
            List<byte> tmpartfile = new List<byte>();
            tmpartfile.AddRange(ObjectHelper.OpenArtFile("../General/Sprites/Monitors/Monitors.bin", Compression.CompressionType.Nemesis));
            tmpartfile.AddRange(new byte[0x6200 - tmpartfile.Count]);
            tmpartfile.AddRange(ObjectHelper.OpenArtFile("../General/Sprites/HUD Icon/Sonic life icon.bin", Compression.CompressionType.Nemesis));
            byte[] artfile = tmpartfile.ToArray();
            img = ObjectHelper.MapASMToBmp(artfile, "../General/Sprites/Monitors/Map - Monitor.asm", 0, 0, out offset);
            imgw = img.Width;
            imgh = img.Height;
            Point off;
            BitmapBits im;
            for (int i = 0; i < 11; i++)
            {
                im = ObjectHelper.MapASMToBmp(artfile, "../General/Sprites/Monitors/Map - Monitor.asm", i + 1, 0, out off);
                imgs.Add(im);
                offsets.Add(off);
                imgws.Add(im.Width);
                imghs.Add(im.Height);
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
                    return "1-up";
                case 2:
                    return "Eggman";
                case 3:
                    return "Rings";
                case 4:
                    return "Shoes";
                case 5:
                    return "Fire Shield";
                case 6:
                    return "Lightning Shield";
                case 7:
                    return "Water Shield";
                case 8:
                    return "Invincibility";
                case 9:
                    return "S";
                case 10:
                    return "Broken";
                default:
                    return "Invalid";
            }
        }

        public override string FullName(byte subtype)
        {
            return SubtypeName(subtype) + " " + Name();
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

        public override Rectangle Bounds(Point loc, byte subtype)
        {
            if (subtype <= 10)
                return new Rectangle(loc.X + offsets[subtype].X, loc.Y + offsets[subtype].Y, imgws[subtype], imghs[subtype]);
            else
                return new Rectangle(loc.X + offset.X, loc.Y + offset.Y, imgw, imgh);
        }

        public override void Draw(BitmapBits bmp, Point loc, byte subtype, bool XFlip, bool YFlip, bool includeDebug)
        {
            if (subtype > 10) subtype = 0;
            BitmapBits bits = new BitmapBits(imgs[subtype]);
            bits.Flip(XFlip, YFlip);
            bmp.DrawBitmapComposited(bits, new Point(loc.X + offsets[subtype].X, loc.Y + offsets[subtype].Y));
        }

        public override Type ObjectType { get { return typeof(MonitorS3KObjectEntry); } }
    }

    public class MonitorS3KObjectEntry : S3KObjectEntry
    {
        public MonitorS3KObjectEntry() : base() { }
        public MonitorS3KObjectEntry(byte[] file, int address) : base(file, address) { }

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
        OneUp,
        Eggman,
        Rings,
        Shoes,
        FireShield,
        LightningShield,
        WaterShield,
        Invincibility,
        S,
        Broken,
        Invalid
    }
}