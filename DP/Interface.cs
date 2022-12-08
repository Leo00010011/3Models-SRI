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

    IEnumerable<char> GetSnippet(int snippetLen);
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

public interface ILazyMatcher
{

    bool AtFinalState
    {
        get;
    }
    
    bool MatchStep(char step);

    bool PeekStep(char step);

    bool Match(IEnumerable<char> text);

    bool Match(string text);

    void ResetState();

    ILazyMatcher CloneParsing();
}
