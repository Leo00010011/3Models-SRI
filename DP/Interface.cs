namespace DP.Interface;
using System;
using System.Collections.Generic;


public interface IDocument : IEnumerable<char>
{
    string Id
    {
        get;
    }

    DateTime ModifiedDateTime
    {
        get;
    }
}

public interface IResult<TValue,KKey,MPiece> : IEnumerable<(KKey,MPiece)>
{

    int Length
    {
        get;
    }

    MPiece this[KKey index]
    {
        get;
    }

    TValue Result
    {
        get;
    }
}

