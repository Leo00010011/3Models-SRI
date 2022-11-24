namespace SRI;

using System.Collections;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class SRIModel<T1, T2, V, Q, D> : ISRIModel<T1, T2, V, Q, D>, ICollection<D> where T1 : notnull where T2 : notnull where Q : notnull
{
    protected virtual ICollection<D>? Storage { get; set; }

    public bool IsReadOnly => true;

    public int Count => Storage!.Count;

    public abstract SearchItem[] GetSearchItems(ISRIVector<Q, V> query, int snippetLen);

    public virtual SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();
    public abstract double SimilarityRate(ISRIVector<T2, V> doc1, ISRIVector<T2, V> doc2);

    public virtual void Add(D item) => Storage!.Add(item);
    public virtual void Clear() => Storage!.Clear();
    public virtual bool Contains(D item) => Storage!.Contains(item);
    public virtual void CopyTo(D[] array, int arrayIndex) => Storage!.CopyTo(array, arrayIndex);
    public virtual bool Remove(D item) => Storage!.Remove(item);

    public virtual IEnumerator<D> GetEnumerator() => Storage!.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public abstract class WModel<T1, T2, D> : SRIModel<T1, T2, IWeight, string, D>, ISRIModel<T1, T2, IWeight, string, D>, ICollection<D> where T1 : notnull where T2 : notnull
{
    public override double SimilarityRate(ISRIVector<T2, IWeight> doc1, ISRIVector<T2, IWeight> doc2)
    {
        if ((Storage as VSMStorageDocTerm)!.InvFrecTerms is null) return -1;

        double normaDoc1 = 0, scalarMul = 0;
        foreach (var item1 in doc1)
        {
            var value1 = item1.Item2.Weight;
            normaDoc1 += Math.Pow(value1, 2);

            try
            {
                var item2 = doc2[item1.Item1];
                var value2 = item2.Weight;
                scalarMul += value1 * value2;
            }
            catch { }
        }
        normaDoc1 = Math.Sqrt(normaDoc1);

        double normaDoc2 = 0;
        foreach (var item in doc2)
        {
            var value = item.Item2.Weight;
            normaDoc2 += Math.Pow(value, 2);
        }
        normaDoc2 = Math.Sqrt(normaDoc2);

        return scalarMul / (normaDoc1 * normaDoc2);
    }

    public override SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();
}

public abstract class WMDocTerm : WModel<IDocument, string, IDocument>, ISRIModel<IDocument, string, IWeight, string, IDocument>, ICollection<IDocument>
{
    public WMDocTerm(IEnumerable<IDocument>? corpus = null) => Storage = new VSMStorageDocTerm(corpus);

    public override SearchItem[] GetSearchItems(ISRIVector<string, IWeight> query, int snippetLen)
    {
        ((VSMStorageDocTerm)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0; SearchItem[] result = new SearchItem[Storage.Count];
        foreach (var item in Storage)
            result[count++] = new SearchItem(item.Id, item.Name, item.GetSnippet(snippetLen), SimilarityRate(query, ((VSMStorageDocTerm)Storage)[item]));
        return result;
    }
}

public class VSMDocTerm : WMDocTerm, ISRIModel<IDocument, string, IWeight, string, IDocument>, ICollection<IDocument>
{
    public VSMDocTerm(IEnumerable<IDocument>? corpus = null) : base(corpus) { }

    public static ISRIVector<string, IWeight> CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);

        SRIVectorLinked<string, IWeight> query = new SRIVectorLinked<string, IWeight>();
        int modalFrec = results.MaxBy(x => x.Item2).Item2;

        foreach ((string, int) item in results)
            query.Add(item.Item1, new QueryVSMWeight(item.Item2, modalFrec));
        return query;
    }

}

public abstract class WMTermDoc : WModel<string, int, IDocument>, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public WMTermDoc(IEnumerable<IDocument>? corpus = null) => Storage = new VSMStorageTermDoc(corpus);

    public override SearchItem[] GetSearchItems(ISRIVector<string, IWeight> query, int snippetLen)
    {
        ((VSMStorageTermDoc)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0;
        SearchItem[] result = new SearchItem[Storage.Count];
        double[] score = new double[Storage.Count];

        double queryscore = 0;
        foreach (var item in query)
        {
            queryscore += Math.Pow(item.Item2.Weight, 2);
            foreach (var item1 in ((VSMStorageTermDoc)Storage!)[item.Item1])
            {
                score[item1.Item1] += item.Item2.Weight * item1.Item2.Weight;
            }
        }
        queryscore = Math.Sqrt(queryscore);

        foreach (var item in ((VSMStorageTermDoc)Storage).GetAllDocs())
        {
            result[count] = new SearchItem(item.Item1.Id, item.Item1.Name, item.Item1.GetSnippet(snippetLen), score[count++] / (queryscore * item.Item2));
        }
        return result;
    }
}

public class VSMTermDoc : WMTermDoc, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public VSMTermDoc(IEnumerable<IDocument>? corpus = null) : base(corpus) { }

    public static ISRIVector<string, IWeight> CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);

        SRIVectorLinked<string, IWeight> query = new SRIVectorLinked<string, IWeight>();
        int modalFrec = results.MaxBy(x => x.Item2).Item2;

        foreach ((string, int) item in results)
            query.Add(item.Item1, new QueryVSMWeight(item.Item2, modalFrec));
        return query;
    }

}
