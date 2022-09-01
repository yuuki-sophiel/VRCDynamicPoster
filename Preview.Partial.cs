using System;
using System.Collections;
using System.Collections.Generic;

namespace VRCDynamicPoster;
public partial class Preview
{
    public IEnumerable<Entry> Entries { get; internal set; }
    public Preview(IEnumerable<Entry> entries)
    {
        Entries = entries;
    }
}