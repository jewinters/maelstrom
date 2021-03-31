﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Sleepey.FF8Mod.Exe
{
    public class BinaryPatch
    {
        public uint Offset { get; set; } = 0;
        public IEnumerable<byte> OriginalData { get; set; } = new List<byte>();
        public IEnumerable<byte> NewData { get; set; } = new List<byte>();

        public BinaryPatch() { }

        public BinaryPatch(uint offset, IEnumerable<byte> origData, IEnumerable<byte> newData)
        {
            Offset = offset;
            OriginalData = origData;
            NewData = newData;
        }

        public void Apply(string targetFile)
        {
            Patch(targetFile, false);
        }

        public void Remove(string targetFile)
        {
            Patch(targetFile, true);
        }

        private void Patch(string targetFile, bool remove)
        {
            using (var stream = new FileStream(targetFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            using (var reader = new BinaryReader(stream))
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(Offset, SeekOrigin.Begin);

                if (remove)
                {
                    // remove patch
                    writer.Write(OriginalData);
                }
                else
                {
                    // apply patch
                    writer.Write(NewData);
                }
            }
        }
    }
}
