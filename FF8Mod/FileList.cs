﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8Mod
{
    public class FileList
    {
        public List<string> Files;

        public FileList(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ReadStream(stream);
            }
        }

        public FileList(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                ReadStream(stream);
            }
        }

        private void ReadStream(Stream stream)
        {
            Files = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!String.IsNullOrWhiteSpace(line)) Files.Add(line);
                }
            }
        }

        public int GetIndex(string path)
        {
            var pathLower = path.ToLower();
            return Files.FindIndex(f => f.ToLower() == pathLower);
        }

        public List<string> GetDirectory(string path)
        {
            var result = Files.Where(f => f.StartsWith(path.ToLower()));
            if (result.Count() == 0) return null;
            return result.ToList();
        }
    }
}