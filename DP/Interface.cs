namespace DP.Interface;

using DP;
using System;
using System.Collections.Generic;

public interface IDocument : IEnumerable<char>
{
    string Id
    {
        get;
    }

    IEnumerable<char> Name
    {
        get;
    }

    stateDoc GetState();

    void UpdateDateTime();

    int ModalFrec { get; }

    IEnumerable<char> GetSnippet();
}

public interface IResult<TValue, KKey, MPiece> : IEnumerable<(KKey, MPiece)>
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

