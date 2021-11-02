// Copyright (c) 2021, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.

namespace ZlibSharp;

public enum ZlibResult
{
    VersionError = -6,
    BufError,
    MemError,
    DataError,
    StreamError,
    ErrNo,
    Ok,
    StreamEnd,
    NeedDict,
}
