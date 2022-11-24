namespace SRI;

using System.Collections;
using System.Collections.Generic;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class Storage<D, T, V> : IStorage<D, T, V>, ICollection<D> where D : notnull where T : notnull
{
    public abstract ISRIVector<T, V> this[D index] { get; }

    public abstract int Count { get; }
    public abstract bool IsReadOnly { get; }

    public abstract ISRIVector<T, V> GetDocVector(D doc);
    public abstract ISRIVector<D, V> GetTermVector(T index);
    public abstract void UpdateDocs();

    public abstract void Add(D item);
    public abstract void Clear();
    public abstract bool Contains(D item);
    public abstract void CopyTo(D[] array, int arrayIndex);
    public abstract bool Remove(D item);

    public abstract IEnumerator<D> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
#region doc terms

public class VSMStorage_docterm : Storage<IDocument, string, IWeight>, IStorage<IDocument, string, IWeight>
{
    private SRIVectorDic<IDocument, (ISRIVector<string, IWeight>, int)> weightMatrix;

    public override ISRIVector<string, IWeight> this[IDocument index] => GetDocVector(index);

    public ISRIVector<string, int>? InvFrecTerms;

    private bool needUpdate;

    public VSMStorage_docterm(IEnumerable<IDocument>? corpus)
    {
        weightMatrix = new SRIVectorDic<IDocument, (ISRIVector<string, IWeight>, int)>();
        InvFrecTerms = new SRIVectorDic<string, int>();
        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public override int Count => weightMatrix.Count;

    public override bool IsReadOnly => true;

    public override void UpdateDocs()
    {
        foreach (var item in weightMatrix!)
        {
            switch (item.Item1.GetState())
            {
                case stateDoc.changed:
                    RemoveInvFrec(item.Item1);
                    int ModalFrec;
                    var terms = GenWeightTerms(item.Item1, out ModalFrec);
                    if (terms is null)
                        weightMatrix.Remove(item.Item1);
                    else
                        weightMatrix[item.Item1] = (terms, ModalFrec);
                    break;
                case stateDoc.deleted:
                    RemoveInvFrec(item.Item1);
                    weightMatrix.Remove(item.Item1);
                    break;
                case stateDoc.notchanged:
                    break;
                default:
                    throw new NotImplementedException("Se añadió otra variante de stateDocs");
            }
        }

        UpdateAllWeight();
    }

    public override ISRIVector<IDocument, IWeight> GetTermVector(string index)
    {
        if (weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        SRIVectorDic<IDocument, IWeight> result = new SRIVectorDic<IDocument, IWeight>();
        foreach (var item in weightMatrix)
        {
            try
            {
                result.Add(item.Item1, weightMatrix[item.Item1].Item1[index]);
            }
            catch { }
        }

        return result;
    }

    public override ISRIVector<string, IWeight> GetDocVector(IDocument doc) => GetInfoDoc(doc).Item1;

    public (ISRIVector<string, IWeight>, int) GetInfoDoc(IDocument doc) => weightMatrix[doc];

    private ISRIVector<string, IWeight>? GenWeightTerms(IDocument doc, out int ModalFrec)
    {
        ModalFrec = 0;
        ProcesedDocument termsresult = new ProcesedDocument(doc);
        if (termsresult.Length == 0) return null;

        SRIVectorDic<string, IWeight> result = new SRIVectorDic<string, IWeight>();
        foreach ((string, int) item in termsresult)
        {
            if (item.Item2 == 0) continue;
            if (!InvFrecTerms!.ContainsKey(item.Item1))
                InvFrecTerms!.Add(item.Item1, 1);
            else
                InvFrecTerms![item.Item1] += 1;
            ModalFrec = Math.Max(ModalFrec, item.Item2);
            result.Add(item.Item1, new VSMWeight(item.Item2));
        }

        doc.UpdateDateTime();
        return result;
    }

    private void RemoveInvFrec(IDocument doc)
    {
        foreach (var item in weightMatrix[doc].Item1)
            InvFrecTerms![item.Item1] -= 1;
    }

    private void UpdateAllWeight()
    {
        if (!needUpdate) return;
        foreach (var doc in weightMatrix)
            foreach (var item in doc.Item2.Item1)
                (item.Item2 as VSMWeight)!.Update(doc.Item2.Item2, Count, InvFrecTerms![item.Item1]);
        needUpdate = false;
    }

    public override void Add(IDocument item)
    {
        needUpdate = true;

        int ModalFrec;
        var terms = GenWeightTerms(item, out ModalFrec);
        if (terms is null) return;

        weightMatrix.Add(item, (terms, ModalFrec));
    }

    public override void Clear() => weightMatrix = new SRIVectorDic<IDocument, (ISRIVector<string, IWeight>, int)>();

    public override bool Contains(IDocument item) => weightMatrix.ContainsKey(item);

    public override void CopyTo(IDocument[] array, int arrayIndex)
    {
        foreach (var item in this.Take(array.Length - arrayIndex))
            array[arrayIndex++] = item;
    }

    public override bool Remove(IDocument item)
    {
        needUpdate = true;
        return weightMatrix.Remove(item);
    }

    public override IEnumerator<IDocument> GetEnumerator() => weightMatrix.Select(x => x.Item1).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => weightMatrix.Select(x => x.Item1).GetEnumerator();
}

#endregion

public class VSMStorage : Storage<string, int, IWeight>, IStorage<string, int, IWeight>
{
    private SRIVectorDic<string, SRIVectorDic<int, IWeight>> weightMatrix;
    private List<(int, IDocument, double)> FrecModal;
    private bool needUpdate;

    public override ISRIVector<int, IWeight> this[string index] => GetDocVector(index);

    public VSMStorage(IEnumerable<IDocument>? corpus)
    {
        weightMatrix = new SRIVectorDic<string, SRIVectorDic<int, IWeight>>();
        FrecModal = new List<(int, IDocument, double)>();
        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public override int Count => FrecModal.Count;

    public override bool IsReadOnly => true;

    public override void UpdateDocs()
    {
        stateDoc state = stateDoc.notchanged;
        for (int i = 0; i < FrecModal.Count; i++)
        {
            stateDoc stateDoc = FrecModal[i].Item2.GetState();
            state = (stateDoc is stateDoc.changed || ((state is stateDoc.changed || state is stateDoc.deleted) && 
                   !(stateDoc is stateDoc.deleted))) ? stateDoc.changed : stateDoc;
            switch (state)
            {
                case stateDoc.changed:
                    IDocument doc = FrecModal[i].Item2;
                    FrecModal.Remove(FrecModal[i]);
                    GenWeightTerms(doc, in i);
                    break;
                case stateDoc.deleted:
                    Remove(FrecModal[i].Item2);
                    i--;
                    break;
                case stateDoc.notchanged:
                    break;
                default:
                    throw new NotImplementedException("Se añadió otra variante de stateDocs");
            }
        }

        UpdateAllWeight();
    }

    public override ISRIVector<int, IWeight> GetDocVector(string index) => GetInfoDoc(index);

    public override ISRIVector<string, IWeight> GetTermVector(int doc)
    {
        if (weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        SRIVectorDic<string, IWeight> result = new SRIVectorDic<string, IWeight>();
        foreach (var item in weightMatrix)
        {
            try
            {
                result.Add(item.Item1, item.Item2[doc]);
            }
            catch { }
        }

        return result;
    }

    public ISRIVector<int, IWeight> GetInfoDoc(string index) => weightMatrix[index];

    private void GenWeightTerms(IDocument doc, in int numdoc)
    {
        int ModalFrec = 0;
        ProcesedDocument termsresult = new ProcesedDocument(doc);
        if (termsresult.Length == 0) return;

        foreach ((string, int) item in termsresult)
        {
            if (item.Item2 == 0) continue;
            ModalFrec = Math.Max(ModalFrec, item.Item2);
            if (!weightMatrix!.ContainsKey(item.Item1))
            {
                var docs = new SRIVectorDic<int, IWeight>();
                docs.Add((numdoc, new VSMWeight(item.Item2)));
                weightMatrix!.Add(item.Item1, docs);
            }
            else
                weightMatrix![item.Item1].Add((numdoc, new VSMWeight(item.Item2)));
        }

        FrecModal.Insert(numdoc, (ModalFrec, doc, 0));
        doc.UpdateDateTime();
    }

    private void UpdateAllWeight()
    {
        if (!needUpdate) return;
        double[] norma2 = new double[FrecModal.Count];
        foreach (var doc in weightMatrix)
        {
            foreach (var item in doc.Item2)
            {
                (item.Item2 as VSMWeight)!.Update(FrecModal[item.Item1].Item1, FrecModal.Count, doc.Item2.Count);
                norma2[item.Item1] += Math.Pow(item.Item2.Weight, 2);
            }
        }

        for (int i = 0; i < norma2.Length; i++)
            FrecModal[i] = (FrecModal[i].Item1, FrecModal[i].Item2, Math.Sqrt(norma2[i]));
        needUpdate = false;
    }

    public void Add(IDocument item)
    {
        needUpdate = true;

        int num = FrecModal.Count;
        GenWeightTerms(item, in num);
    }

    public override void Add(string item) => throw new NotImplementedException();

    public override void Clear() => weightMatrix = new SRIVectorDic<string, SRIVectorDic<int, IWeight>>();

    public override bool Contains(string item) => throw new NotImplementedException();

    public bool Contains(IDocument item) => throw new NotImplementedException();

    public override void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();

    public void CopyTo(IDocument[] array, int arrayIndex) => throw new NotImplementedException();

    public bool Remove(IDocument item) => throw new NotImplementedException();

    public override bool Remove(string item) => throw new NotImplementedException();

    public IEnumerable<(IDocument, double)> GetAllDocs() => FrecModal.Select(x => (x.Item2, x.Item3))!;

    public override IEnumerator<string> GetEnumerator() => weightMatrix!.Select(x => x.Item1).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => weightMatrix!.Select(x => x.Item1).GetEnumerator();
}