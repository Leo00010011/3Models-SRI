namespace DP.Interface;
using System;
using System.Collections.Generic;

public interface IDocument
{
    string Id
    {
        get;
    }

    DateTime ModifiedDateTime
    {
        get;
    }

    IEnumerable<char> CharDoc
    {
        get;
    }
}

public interface ICorpus
{
    IEnumerable<IDocument> documents
    {
        get;
    }
}

public interface IProcesedDocument
{
    int this[string index]
    {
        get;
    }

    IEnumerable<(string, int)> TermFreqInDoc
    {
        get;
    }
}

public interface IProcesedTerm
{
    int this[string index]
    {
        get;
    }

    IEnumerable<(string, int)> DocWithTermFreq
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