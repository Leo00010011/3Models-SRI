namespace SRI;

using System.Collections;
using System.Collections.Generic;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class Storage<D, T, V> : IStorage<D, T, V>, ICollection<IDocument> where D : notnull where T : notnull
{
    public abstract (IDictionary<T, V>, int) this[D index] { get; }

    public abstract int Count { get; }
    public abstract bool IsReadOnly { get; }

    public abstract void Add(IDocument item);
    public abstract void Clear();
    public abstract bool Contains(IDocument item);
    public abstract void CopyTo(IDocument[] array, int arrayIndex);
    public abstract IDictionary<T, V>? GenWeightTerms(D doc, out int ModalFrec);
    public abstract IDictionary<D, IWeight> GetTermVector(T index);
    public abstract bool Remove(IDocument item);
    public abstract void UpdateDocs();

    public abstract IEnumerator<IDocument> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

public class VSMStorage : IStorage<IDocument, string, IWeight>
{
    private Dictionary<IDocument, (IDictionary<string, IWeight>, int)> weightMatrix;

    public (IDictionary<string, IWeight>, int) this[IDocument index] => weightMatrix[index];

    public IDictionary<string, int>? InvFrecTerms;

    public VSMStorage(IEnumerable<IDocument>? corpus)
    {
        weightMatrix = new Dictionary<IDocument, (IDictionary<string, IWeight>, int)>();
        InvFrecTerms = new Dictionary<string, int>();
        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
    }

    public int Count => weightMatrix.Count;

    public bool IsReadOnly => true;

    public IDictionary<string, IWeight>? GenWeightTerms(IDocument doc, out int ModalFrec)
    {
        ModalFrec = 0;
        ProcesedDocument termsresult = new ProcesedDocument(doc);
        if (termsresult.Length == 0) return null;

        Dictionary<string, IWeight> result = new Dictionary<string, IWeight>();
        foreach ((string, int) item in termsresult)
        {
            if (item.Item2 == 0) continue;
            if (!InvFrecTerms!.ContainsKey(item.Item1))
                InvFrecTerms!.Add(item.Item1, 1);
            else
                InvFrecTerms![item.Item1] += 1;
            ModalFrec = Math.Max(ModalFrec, item.Item2);
            result.Add(item.Item1, new Weight(item.Item2));
        }

        return result;
    }

    public IDictionary<IDocument, IWeight> GetTermVector(string index)
    {
        if (weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        LinkedList<KeyValuePair<IDocument, IWeight>> values = new LinkedList<KeyValuePair<IDocument, IWeight>>();
        foreach (var item in weightMatrix)
        {
            try
            {
                values.AddLast(new KeyValuePair<IDocument, IWeight>(item.Key, weightMatrix[item.Key].Item1[index]));
            }
            catch { }
        }

        return new Dictionary<IDocument, IWeight>(values);
    }

    public void UpdateDocs()
    {
        foreach (var item in weightMatrix!.Keys)
        {
            switch (item.GetState())
            {
                case stateDoc.changed:
                    RemoveInvFrec(item);
                    int ModalFrec;
                    var terms = GenWeightTerms(item, out ModalFrec);
                    if (terms is null)
                        weightMatrix.Remove(item);
                    else
                        weightMatrix[item] = (new Dictionary<string, IWeight>(terms), ModalFrec);
                    break;
                case stateDoc.deleted:
                    RemoveInvFrec(item);
                    weightMatrix.Remove(item);
                    break;
                case stateDoc.notchanged:
                    break;
                default:
                    throw new NotImplementedException("Se añadió otra variante de stateDocs");
            }
        }
    }

    private void RemoveInvFrec(IDocument doc)
    {
        foreach (var item in weightMatrix[doc].Item1.Keys)
            InvFrecTerms![item] -= 1;
    }

    public void Add(IDocument item)
    {
        ProcesedDocument termsresult = new ProcesedDocument(item);
        if (termsresult.Length == 0) return;

        int ModalFrec;
        var terms = GenWeightTerms(item, out ModalFrec);
        if (terms is null) return;

        weightMatrix.Add(item, (terms, ModalFrec));
    }

    public void Clear() => weightMatrix = new Dictionary<IDocument, (IDictionary<string, IWeight>, int)>();

    public bool Contains(IDocument item) => weightMatrix.ContainsKey(item);

    public void CopyTo(IDocument[] array, int arrayIndex)
    {
        foreach (var item in this.Take(array.Length - arrayIndex))
            array[arrayIndex++] = item;
    }

    public bool Remove(IDocument item) => weightMatrix.Remove(item);

    public IEnumerator<IDocument> GetEnumerator() => weightMatrix.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => weightMatrix.Keys.GetEnumerator();
}