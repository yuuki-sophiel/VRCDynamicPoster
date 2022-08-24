using System;
using System.Collections;
using System.Collections.Generic;

namespace VRCDynamicPoster;
public partial class Preview
{
    public IEnumerable<WorldEntry> Entries { get; internal set; }
    public Preview(IEnumerable<WorldEntry> entries)
    {
        Entries = entries;
    }
}