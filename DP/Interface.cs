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

public interface ICorpus : IEnumerable<IDocument>
{

}


public interface IProcesedDocument : IEnumerable<(string,int)>
{
    int this[string index]
    {
        get;
    }

    IDocument RawDoc
    {
        get;
    }
}

public interface IProcesedTerm : IEnumerable<(string, int)>
{
    int this[string index]
    {
        get;
    }

    string Term 
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

    IProcesedDocument GetProcesedDocument(string id);

    IProcesedTerm GetProcesedTerm(string term);

    int Frequency(string DocId, string term);

    /// <summary>
    /// Frecuencia invertida de los documentos
    /// </summary>
    /// <param name="term"></param>
    /// <returns></returns>
    int InvertedFrequency(string term);

    bool UpdateCorpus(ICorpus corpus);

}