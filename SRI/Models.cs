namespace SRI;

using System.Collections;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class SRIModel<D, T, V> : ISRIModel<D, T, V>, ICollection<D> where D : notnull where T : notnull
{
    public bool IsReadOnly => true;

    public abstract int Count { get; }

    public abstract SearchItem[] GetSearchItems(ISRIVector<T, V> query, int snippetLen);

    public virtual SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();
    public abstract double SimilarityRate(ISRIVector<T, V> doc1, ISRIVector<T, V> doc2);

    public abstract void Add(D item);
    public abstract void Clear();
    public abstract bool Contains(D item);
    public abstract void CopyTo(D[] array, int arrayIndex);
    public abstract bool Remove(D item);

    public abstract IEnumerator<D> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class VSM_docterm : SRIModel<IDocument, string, IWeight>, ISRIModel<IDocument, string, IWeight>, ICollection<IDocument>
{
    protected Storage<IDocument, string, IWeight>? storage;

    public override int Count => storage!.Count;

    public VSM_docterm(IEnumerable<IDocument>? corpus = null) => storage = new VSMStorage_docterm(corpus);

    public override IEnumerator<IDocument> GetEnumerator() => storage!.GetEnumerator();

    public override SearchItem[] GetSearchItems(ISRIVector<string, IWeight> query, int snippetLen)
    {
        storage!.UpdateDocs(); /*analizar si es null*/ int count = 0; SearchItem[] result = new SearchItem[storage.Count];
        foreach (var item in storage)
            result[count++] = new SearchItem(item.Id, item.Name, item.GetSnippet(snippetLen), SimilarityRate(query, storage[item]));
        return result;
    }

    public override double SimilarityRate(ISRIVector<string, IWeight> doc1, ISRIVector<string, IWeight> doc2)
    {
        if ((storage as VSMStorage_docterm)!.InvFrecTerms is null) return -1;

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

    public ISRIVector<string, IWeight> CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);

        SRIVectorLinked<string, IWeight> query = new SRIVectorLinked<string, IWeight>();
        foreach ((string, int) item in results)
        {
            
        }

        return query;
    }

    public override void Add(IDocument item) => throw new NotImplementedException();

    public override void Clear() => throw new NotImplementedException();

    public override bool Contains(IDocument item) => throw new NotImplementedException();

    public override void CopyTo(IDocument[] array, int arrayIndex) => throw new NotImplementedException();

    public override bool Remove(IDocument item) => throw new NotImplementedException();
}

public class VSM : ICollection<IDocument>
{
    protected VSMStorage? storage;

    public int Count => storage!.Count;

    public bool IsReadOnly => true;

    public VSM(IEnumerable<IDocument>? corpus = null) => storage = new VSMStorage(corpus);

    public SearchItem[] GetSearchItems(ISRIVector<string, IWeight> query, int snippetLen)
    {
        storage!.UpdateDocs(); /*analizar si es null*/ int count = 0; SearchItem[] result = new SearchItem[storage.Count]; double[] score = new double[storage.Count];

        double queryscore = 0;
        foreach (var item in query)
        {
            queryscore += Math.Pow(item.Item2.Weight, 2);
            foreach (var item1 in storage[item.Item1])
            {
                score[item1.Item2.Item2] += item.Item2.Weight * item1.Item2.Item1.Weight;
            }
        }
        queryscore = Math.Sqrt(queryscore);

        foreach (var item in this)
        {
            result[count] = new SearchItem(item.Id, item.Name, item.GetSnippet(snippetLen), score[count++] / queryscore);
        }
        return result;
    }

    public double SimilarityRate(ISRIVector<string, IWeight> doc1, ISRIVector<string, IWeight> doc2) => throw new NotImplementedException();

    public SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();

    public void Add(IDocument item) => storage!.Add(item);
    public void Clear() => storage!.Clear();
    public bool Contains(IDocument item) => storage!.Contains(item);
    public void CopyTo(IDocument[] array, int arrayIndex) => storage!.CopyTo(array, arrayIndex);
    public bool Remove(IDocument item) => storage!.Remove(item);

    public IEnumerator<IDocument> GetEnumerator() => storage!.GetAllDocs().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
