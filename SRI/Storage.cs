namespace SRI;

using System.Collections;
using System.Collections.Generic;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class Storage<T1, T2, V, D> : IStorage<T1, T2, V, D>, ICollection<D> where T1 : notnull where T2 : notnull
{
    protected abstract ISRIVector<T1, ISRIVector<T2, V>> MatrixStorage { get; set; }
    public virtual ISRIVector<T2, V> this[T1 index] => MatrixStorage[index];
    public abstract IEnumerable<D> corpus { get; }

    public abstract int Count { get; }
    public virtual bool IsReadOnly => MatrixStorage.IsReadOnly;

    public abstract ISRIVector<T2, V> GetKey2Vector(T1 doc);
    public abstract ISRIVector<T1, V> GetKey1Vector(T2 index);
    public abstract void UpdateDocs();

    public abstract void Add(D item);
    public abstract bool Remove(D item);
    public virtual void Clear() => MatrixStorage.Clear();
    public virtual bool Contains(D item) => ((IEnumerable<D>)this).Contains(item);
    public virtual void CopyTo(D[] array, int arrayIndex) => this.ToList().CopyTo(array, arrayIndex);

    public virtual IEnumerator<D> GetEnumerator() => corpus.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

public class VSMStorageDT : Storage<IDocument, string, IWeight, IDocument>, IStorage<IDocument, string, IWeight, IDocument>, ICollection<IDocument>
{
    private Dictionary<IDocument, int> DocsFrecModal;
    private SRIVectorDic<string, int> InvFrecTerms;
    private bool needUpdate;

    public VSMStorageDT(IEnumerable<IDocument>? corpus)
    {
        MatrixStorage = new SRIVectorDic<IDocument, ISRIVector<string, IWeight>>();
        DocsFrecModal = new Dictionary<IDocument, int>();
        InvFrecTerms = new SRIVectorDic<string, int>();
        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public override IEnumerable<IDocument> corpus => DocsFrecModal.Keys;

    public override int Count => DocsFrecModal.Count;

    protected override ISRIVector<IDocument, ISRIVector<string, IWeight>> MatrixStorage { get; set; }

    public override void Add(IDocument item)
    {
        needUpdate = true;

        int ModalFrec;
        var terms = GenWeightTerms(item, out ModalFrec);
        if (terms is null) return;

        MatrixStorage.Add(item, terms);
        DocsFrecModal.Add(item, ModalFrec);
    }

    public override ISRIVector<IDocument, IWeight> GetKey1Vector(string index)
    {
        SRIVectorDic<IDocument, IWeight> result = new SRIVectorDic<IDocument, IWeight>();
        foreach (var item in MatrixStorage)
        {
            try
            {
                result.Add(item.Item1, MatrixStorage[item.Item1][index]);
            }
            catch { }
        }

        return result;
    }

    public override ISRIVector<string, IWeight> GetKey2Vector(IDocument doc) => this[doc];

    public override bool Remove(IDocument item)
    {
        needUpdate = true;
        return (MatrixStorage as SRIVectorDic<IDocument, ISRIVector<string, IWeight>>)!.Remove(item);
    }

    public override void UpdateDocs()
    {
        foreach (var item in corpus)
        {
            switch (item.GetState())
            {
                case stateDoc.changed:
                    RemoveInvFrec(item);
                    int ModalFrec;
                    var terms = GenWeightTerms(item, out ModalFrec);
                    if (terms is null)
                        (MatrixStorage as SRIVectorDic<IDocument, ISRIVector<string, IWeight>>)!.Remove(item);
                    else
                    {
                        (MatrixStorage as SRIVectorDic<IDocument, ISRIVector<string, IWeight>>)![item] = terms;
                        DocsFrecModal[item] = ModalFrec;
                    }
                    break;
                case stateDoc.deleted:
                    RemoveInvFrec(item);
                    (MatrixStorage as SRIVectorDic<IDocument, ISRIVector<string, IWeight>>)!.Remove(item);
                    break;
                case stateDoc.notchanged:
                    break;
                default:
                    throw new NotImplementedException("Se añadió otra variante de stateDocs");
            }
        }

        UpdateAllWeight();
    }

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
        foreach (var item in MatrixStorage[doc])
            InvFrecTerms![item.Item1] -= 1;
    }

    private void UpdateAllWeight()
    {
        if (!needUpdate) return;
        foreach (var doc in MatrixStorage)
            foreach (var item in doc.Item2)
                (item.Item2 as VSMWeight)!.Update(DocsFrecModal[doc.Item1], Count, InvFrecTerms![item.Item1]);
        needUpdate = false;
    }
}

public class VSMStorageTD : Storage<string, IDocument, IWeight, IDocument>, IStorage<string, IDocument, IWeight, IDocument>, ICollection<IDocument>
{
    public Dictionary<IDocument, (int, double)> DocsFrecModal;
    protected bool needUpdate;

    public VSMStorageTD(IEnumerable<IDocument>? corpus)
    {
        MatrixStorage = new SRIVectorDic<string, ISRIVector<IDocument, IWeight>>();
        DocsFrecModal = new Dictionary<IDocument, (int, double)>();
        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    protected override ISRIVector<string, ISRIVector<IDocument, IWeight>> MatrixStorage { get; set; }
    public override IEnumerable<IDocument> corpus => DocsFrecModal.Select(x => x.Key);

    public override int Count => DocsFrecModal.Count;

    public override void Add(IDocument item)
    {
        needUpdate = true;

        int num = DocsFrecModal.Count;
        GenWeightTerms(item, in num);
    } 

    public override ISRIVector<string, IWeight> GetKey1Vector(IDocument index)
    {
        SRIVectorDic<string, IWeight> result = new SRIVectorDic<string, IWeight>();
        foreach (var item in MatrixStorage)
        {
            try
            {
                result.Add(item.Item1, item.Item2[index]);
            }
            catch { }
        }

        return result;
    }

    public override ISRIVector<IDocument, IWeight> GetKey2Vector(string doc) => MatrixStorage[doc];

    public override bool Remove(IDocument item) => throw new NotImplementedException();

    public override void UpdateDocs()
    {
        stateDoc state = stateDoc.notchanged;
        foreach (var item in corpus.Select((doc, i) => (doc, i)))
        {
            stateDoc stateDoc = item.doc.GetState();
            state = (stateDoc is stateDoc.changed || ((state is stateDoc.changed || state is stateDoc.deleted) &&
                   !(stateDoc is stateDoc.deleted))) ? stateDoc.changed : stateDoc;
            switch (state)
            {
                case stateDoc.changed:
                    IDocument doc = item.doc;
                    DocsFrecModal.Remove(item.doc);
                    GenWeightTerms(doc, in item.i);
                    break;
                case stateDoc.deleted:
                    Remove(item.doc);
                    break;
                case stateDoc.notchanged:
                    break;
                default:
                    throw new NotImplementedException("Se añadió otra variante de stateDocs");
            }
        }

        UpdateAllWeight();
    }

    protected virtual void GenWeightTerms(IDocument doc, in int numdoc)
    {
        int ModalFrec = 0;
        ProcesedDocument termsresult = new ProcesedDocument(doc);
        if (termsresult.Length == 0) return;

        foreach ((string, int) item in termsresult)
        {
            if (item.Item2 == 0) continue;
            ModalFrec = Math.Max(ModalFrec, item.Item2);
            if (!MatrixStorage.ContainsKey(item.Item1))
            {
                var docs = new SRIVectorDic<IDocument, IWeight>();
                docs.Add((doc, new VSMWeight(item.Item2)));
                MatrixStorage.Add(item.Item1, docs);
            }
            else
                MatrixStorage[item.Item1].Add((doc, new VSMWeight(item.Item2)));
        }

        DocsFrecModal.Add(doc, (numdoc, ModalFrec));
        doc.UpdateDateTime();
    }

    public virtual void UpdateAllWeight()
    {
        if (!needUpdate) return;
        double[] norma2 = new double[DocsFrecModal.Count];
        foreach (var doc in MatrixStorage)
        {
            foreach (var item in doc.Item2)
            {
                var frecModal = DocsFrecModal[item.Item1].Item2;
                (item.Item2 as VSMWeight)!.Update(frecModal, DocsFrecModal.Count, doc.Item2.Count);
                norma2[DocsFrecModal[item.Item1].Item1] += Math.Pow(item.Item2.Weight, 2);
            }
        }

        foreach (var item in corpus.Select((doc, i) => (doc, i)))
        {
            var value = DocsFrecModal[item.doc];
            DocsFrecModal[item.doc] = (value.Item1, Math.Sqrt(norma2[value.Item1]));
        }
        needUpdate = false;
    }

    internal IEnumerable<(IDocument, double)> GetAllDocs() => DocsFrecModal.Select(x => (x.Key, x.Value.Item2));
}

public class GVSMStorageTD : VSMStorageTD, IStorage<string, IDocument, IWeight, IDocument>, ICollection<IDocument>
{
    private SRIVectorDic<string, SRIVectorDic<int, double>> weightTerms;
    private SRIVectorDic<int, SRIVectorDic<int, double>> weightDocs;
    private SRIVectorDic<string, int> termsIndex;

    public GVSMStorageTD(IEnumerable<IDocument>? corpus) : base(null)
    {
        weightTerms = new SRIVectorDic<string, SRIVectorDic<int, double>>();
        weightDocs = new SRIVectorDic<int, SRIVectorDic<int, double>>();
        termsIndex = new SRIVectorDic<string, int>();

        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public override ISRIVector<string, IWeight> GetKey1Vector(IDocument index)
    {
        SRIVectorDic<string, IWeight> result = new SRIVectorDic<string, IWeight>();
        int count = 0;
        foreach (var item in MatrixStorage)
        {
            termsIndex.Add(item.Item1, count);
            try
            {
                result.Add(item.Item1, item.Item2[index]);
            }
            catch { }
            count ++;
        }

        return result;
    }

    public override void UpdateAllWeight()
    {
        base.UpdateAllWeight();
        
        SRIVectorLinked<int, int> docspattern = new SRIVectorLinked<int, int>();

        foreach (var item1 in DocsFrecModal)
        {
            int count = 0, index = 0, value = 0;
            foreach (var item2 in GetKey1Vector(item1.Key))
            {
                value = termsIndex[item2.Item1];
                index = (index << (value - count)) + 1;
                count = value;
            }

            docspattern.Add((DocsFrecModal[item1.Key].Item1, index));
        }
        
        SRIVectorDic<string/*term*/, SRIVectorDic<int/*doc*/, double>> resultTerms = new SRIVectorDic<string, SRIVectorDic<int, double>>();
        SRIVectorDic<int/*doc*/, SRIVectorDic<int/*term*/, double>> resultDocs = new SRIVectorDic<int, SRIVectorDic<int, double>>();

        foreach (var item1 in MatrixStorage)
        {
            var index = termsIndex[item1.Item1];
            var valueTerm = new SRIVectorDic<int, double>();

            foreach (var item2 in item1.Item2)
            {
                var docindex = DocsFrecModal[item2.Item1].Item1;
                var pattern = docspattern[docindex];

                if (!valueTerm.ContainsKey(pattern))
                {
                    valueTerm.Add((pattern, 1));
                }
                else
                {
                    valueTerm[pattern] += 1;
                }

                if (!resultDocs.ContainsKey(docindex))
                {
                    var docs = new SRIVectorDic<int, double>();
                    docs.Add((index, 1));
                    resultDocs.Add(docindex, docs);
                }
                else
                {
                    var docs = resultDocs[docindex];
                    if (docs!.ContainsKey(index))
                    {
                        docs.Add(index, 1);
                    }
                    else
                    {
                        docs[index] += 1;
                    }
                }
            }
            resultTerms.Add(item1.Item1, valueTerm);
        }

        weightTerms = resultTerms;
        weightDocs = resultDocs;
        needUpdate = false;
    }
}
