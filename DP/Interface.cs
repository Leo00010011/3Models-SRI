namespace DP.Interface;
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

    DateTime ModifiedDateTime
    {
        get;
    }
}

public interface IResult<TValue,KKey,MPiece> : IEnumerable<(string,int)>
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


public interface IProcesedCorpus
{
    int CantidadDeDocumentos
    {
        get;
    }

    int CantidadDeTerminos
    {
        get;
    }

    IResult<IDocument,string,int> GetProcesedDocument(string id);

    IResult<string,string,int> GetProcesedTerm(string term);

    int Frequency(string DocId, string term);

    /// <summary>
    /// Frecuencia invertida de los documentos
    /// </summary>
    /// <param name="term"></param>
    /// <returns></returns>
    int InvertedFrequency(string term);

    bool UpdateCorpus(IEnumerable<IDocument> corpus);

}