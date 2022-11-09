namespace SRI;

using System.Collections;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class SRIModel<D, T> : ISRIModel<D, T>, ICollection<IDocument> where D: notnull where T: notnull
{
    protected Storage<IDocument, T, IWeight>? storage;

    public virtual int Count => storage!.Count;
    public bool IsReadOnly => true;

    public virtual SearchItem[] GetSearchItems(IDictionary<T, double> query, int snippetLen)
    {
        storage!.UpdateDocs(); /*analizar si es null*/ int count = 0; SearchItem[] result = new SearchItem[storage.Count];
        foreach (var item in storage)
            result[count++] = new SearchItem(item.Id, item.Name, item.GetSnippet(snippetLen), SimilarityRate(query, storage[item]));
        return result;
    }

    public virtual SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).ToArray();
    public abstract double SimilarityRate(IDictionary<T, double> doc1, (IDictionary<T, IWeight>, int) doc2);

    public virtual void Add(IDocument item) => storage!.Add(item);
    public virtual void Clear() => storage!.Clear();
    public virtual bool Contains(IDocument item) => storage!.Contains(item);
    public virtual void CopyTo(IDocument[] array, int arrayIndex) => storage!.CopyTo(array, arrayIndex);
    public virtual bool Remove(IDocument item) => storage!.Remove(item);

    public abstract IEnumerator<IDocument> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class VSM : SRIModel<string, string>, ISRIModel<string, string>, ICollection<IDocument>
{
    protected new VSMStorage storage;

    public VSM(IEnumerable<IDocument>? corpus = null) => storage = new VSMStorage(corpus);

    public override IEnumerator<IDocument> GetEnumerator() => storage.GetEnumerator();

    public override double SimilarityRate(IDictionary<string, double> doc1, (IDictionary<string, IWeight>, int) doc2)
    {
        if (storage.InvFrecTerms is null) return -1;
        double normaDoc1 = 0, normaDoc2 = 0, scalarMul = 0;

        foreach (var item in storage.InvFrecTerms)
        {
            try
            {
                normaDoc1 += Math.Pow(doc1[item.Key], 2);
            }
            finally { }
            try
            {
                if (!(doc2.Item1[item.Key] is Weight)) throw new ArgumentException("");
                Weight doc2Weight = (doc2.Item1[item.Key] as Weight)!;
                doc2Weight.Update(doc2.Item2, storage.Count, item.Value);
                double weight = doc2Weight.GetWeight();
                
                normaDoc2 += Math.Pow(weight, 2);
                scalarMul += doc1[item.Key] * weight;
            }
            finally { }
        }

        normaDoc1 = Math.Pow(normaDoc1, 0.5);
        normaDoc2 = Math.Pow(normaDoc2, 0.5);

        return scalarMul / (normaDoc1 * normaDoc2);
    }
}