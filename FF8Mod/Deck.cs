﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FF8Mod
{
    public class Deck
    {
        public int DeckID { get; set; }
        public string FieldID { get; set; }
        public string FieldName { get; set; }
        public string EntityName { get; set; }
        public string EntityDescription { get; set; }
        public bool Enabled { get; set; }
    }
}