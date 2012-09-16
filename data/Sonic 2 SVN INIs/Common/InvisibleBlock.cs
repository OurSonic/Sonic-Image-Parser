﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using SonicRetro.SonLVL;

namespace S2ObjectDefinitions.Common
{
    class InvisibleBlock : ObjectDefinition
    {
        private Sprite img;

        public override void Init(Dictionary<string, string> data)
        {
            byte[] artfile = ObjectHelper.OpenArtFile("../art/nemesis/Monitor and contents.bin", Compression.CompressionType.Nemesis);
            byte[] mapfile = System.IO.File.ReadAllBytes("../mappings/sprite/obj74.bin");
            img = ObjectHelper.MapToBmp(artfile, mapfile, 0, 0);
        }

        public override ReadOnlyCollection<byte> Subtypes()
        {
            return new ReadOnlyCollection<byte>(new byte[] { 0 });
        }

        public override string Name()
        {
            return "Invisible solid block";
        }

        public override bool RememberState()
        {
            return true;
        }

        public override string SubtypeName(byte subtype)
        {
            return ((subtype >> 4) + 1) + "x" + ((subtype & 0xF) + 1) + " blocks";
        }

        public override BitmapBits Image()
        {
            return img.Image;
        }

        public override BitmapBits Image(byte subtype)
        {
            return img.Image;
        }

        public override Sprite GetSprite(ObjectEntry obj)
        {
            int w = ((obj.SubType >> 4) + 1) * 16;
            int h = ((obj.SubType & 0xF) + 1) * 16;
            BitmapBits bmp = new BitmapBits(w, h);
            bmp.DrawRectangle(0x1C, 0, 0, w - 1, h - 1);
            Sprite spr = new Sprite(new Sprite(bmp, new Point(-(w / 2), -(h / 2))), img);
            spr.Offset = new Point(spr.X + obj.X, spr.Y + obj.Y);
            return spr;
        }

        public override Rectangle Bounds(ObjectEntry obj, Point camera)
        {
            int w = ((obj.SubType >> 4) + 1) * 16;
            int h = ((obj.SubType & 0xF) + 1) * 16;
            return new Rectangle(obj.X - (w / 2) - camera.X, obj.Y - (h / 2) - camera.Y, w, h);
        }

        public override bool Debug { get { return true; } }
    }
}