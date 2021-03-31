﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sleepey.FF8Mod.Archive
{
    public class FileIndex
    {
        public List<IndexEntry> Entries { get; set; } = new List<IndexEntry>();

        public FileIndex() { }

        public FileIndex(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ReadStream(stream);
            }
        }

        public FileIndex(IEnumerable<byte> data) : this()
        {
            using (var stream = new MemoryStream(data.ToArray()))
            {
                ReadStream(stream);
            }
        }

        private void ReadStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length - 11)
                {
                    Entries.Add(new IndexEntry(reader.ReadBytes(12)));
                }
            }
        }

        public IEnumerable<byte> Encode()
        {
            var length = Entries.Count * 12;
            var result = new byte[length];

            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    writer.Write(Entries[i].Length);
                    writer.Write(Entries[i].Location);
                    writer.Write(Entries[i].Compression);
                }
            }

            return result;
        }
    }
}
