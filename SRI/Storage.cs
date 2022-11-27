namespace SRI;

using System.Collections;
using System.Collections.Generic;
using DP;
using DP.Interface;
using SRI.Interface;
using Utils;

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
    public Dictionary<IDocument, int> DocsFrecModal;
    protected SRIVectorDic<string, (int, int)> InvFrecTerms;
    protected bool needUpdate;

    public VSMStorageDT(IEnumerable<IDocument>? corpus)
    {
        MatrixStorage = new SRIVectorDic<IDocument, ISRIVector<string, IWeight>>();
        DocsFrecModal = new Dictionary<IDocument, int>();
        InvFrecTerms = new SRIVectorDic<string, (int, int)>();
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

    protected virtual ISRIVector<string, IWeight>? GenWeightTerms(IDocument doc, out int ModalFrec)
    {
        ModalFrec = 0;
        ProcesedDocument termsresult = new ProcesedDocument(doc);
        if (termsresult.Length == 0) return null;

        SRIVectorDic<string, IWeight> result = new SRIVectorDic<string, IWeight>();
        foreach ((string, int) item in termsresult)
        {
            if (item.Item2 == 0) continue;
            if (!InvFrecTerms!.ContainsKey(item.Item1))
            {
                InvFrecTerms!.Add(item.Item1, (InvFrecTerms!.Count, 1));
            }
            else
            {
                var value = InvFrecTerms![item.Item1];
                InvFrecTerms![item.Item1] = (value.Item1, value.Item2 + 1);
            }
            ModalFrec = Math.Max(ModalFrec, item.Item2);
            result.Add(item.Item1, new VSMWeight(item.Item2));
        }

        doc.UpdateDateTime();
        return result;
    }

    protected virtual void RemoveInvFrec(IDocument doc)
    {
        foreach (var item in MatrixStorage[doc])
        {
            var value = InvFrecTerms![item.Item1];
            InvFrecTerms![item.Item1] = (value.Item1, value.Item2 - 1);
        }
    }

    public virtual void UpdateAllWeight()
    {
        if (!needUpdate) return;
        foreach (var doc in MatrixStorage)
            foreach (var item in doc.Item2)
                (item.Item2 as VSMWeight)!.Update(DocsFrecModal[doc.Item1], Count, InvFrecTerms![item.Item1].Item2);
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
            if (item.Item2.ContainsKey(index))
                result.Add(item.Item1, item.Item2[index]);
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
    private SRIVectorDic<string, ISRIVector<string, int>> weightTerms;
    private SRIVectorDic<int, ISRIVector<int, int>> weightDocs;

    public GVSMStorageTD(IEnumerable<IDocument>? corpus) : base(null)
    {
        weightTerms = new SRIVectorDic<string, ISRIVector<string, int>>();
        weightDocs = new SRIVectorDic<int, ISRIVector<int, int>>();

        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public new ISRIVector<int, int> GetKey1Vector(IDocument index) => weightDocs[DocsFrecModal[index].Item1];

    public new ISRIVector<string, int> GetKey2Vector(string doc) => weightTerms[doc];

    private IEnumerable<(string, IWeight)> GenDocVector(IDocument index) => MatrixStorage.Where(x => x.Item2.ContainsKey(index)).Select(x => (x.Item1, x.Item2[index]));

    private SRIVectorDic<string, int> GenTermsIndex()
    {
        SRIVectorDic<string, int> result = new SRIVectorDic<string, int>();
        int count = 0;
        for (var item = MatrixStorage.GetEnumerator(); item.MoveNext(); count++)
        {
            result.Add(item.Current.Item1, count);
            count++;
        }

        return result;
    }

    public override void UpdateAllWeight()
    {
        base.UpdateAllWeight();
        needUpdate = true;

        SRIVectorLinked<int, string> docspattern = new SRIVectorLinked<int, string>();
        SRIVectorDic<string, int> termsIndex = GenTermsIndex();

        foreach (var item1 in DocsFrecModal)
        {
            int count = 0; string index = "";
            foreach (var item2 in GenDocVector(item1.Key))
            {
                int value = termsIndex[item2.Item1];
                index += '0'.RepeatChar(value - count) + "1";
                count = value;
            }

            docspattern.Add((item1.Value.Item1, index));
        }

        SRIVectorDic<string/*term*/, ISRIVector<string/*doc*/, int>> resultTerms = new SRIVectorDic<string, ISRIVector<string, int>>();
        SRIVectorDic<int/*doc*/, ISRIVector<int/*term*/, int>> resultDocs = new SRIVectorDic<int, ISRIVector<int, int>>();

        foreach (var item1 in MatrixStorage)
        {
            var index = termsIndex[item1.Item1];
            var valueTerm = new SRIVectorDic<string, int>();

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
                    var docs = new SRIVectorDic<int, int>();
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

public class GVSMStorageDT : VSMStorageDT, IStorage<IDocument, string, IWeight, IDocument>, ICollection<IDocument>
{
    private SRIVectorDic<string, ISRIVector<string, int>> weightTerms;
    private SRIVectorDic<int, ISRIVector<int, int>> weightDocs;

    public GVSMStorageDT(IEnumerable<IDocument>? corpus) : base(null)
    {
        weightTerms = new SRIVectorDic<string, ISRIVector<string, int>>();
        weightDocs = new SRIVectorDic<int, ISRIVector<int, int>>();

        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public new ISRIVector<string, int> GetKey1Vector(string index) => weightTerms[index];

    public new ISRIVector<int, int> GetKey2Vector(IDocument doc) => weightDocs[DocsFrecModal[doc]];

    public override void UpdateAllWeight()
    {
        base.UpdateAllWeight();
        needUpdate = true;

        SRIVectorLinked<int, string> docspattern = new SRIVectorLinked<int, string>();

        foreach (var item1 in MatrixStorage)
        {
            int count = 0; string index = "";
            foreach (var item2 in item1.Item2)
            {
                int value = InvFrecTerms[item2.Item1].Item1;
                index += /*'0'.RepeatChar(value - count) + */"1";
                count = value;
            }

            docspattern.Add((DocsFrecModal[item1.Item1], index));
        }

        SRIVectorDic<string/*term*/, ISRIVector<string/*doc*/, int>> resultTerms = new SRIVectorDic<string, ISRIVector<string, int>>();
        SRIVectorDic<int/*doc*/, ISRIVector<int/*term*/, int>> resultDocs = new SRIVectorDic<int, ISRIVector<int, int>>();

        // foreach (var item1 in MatrixStorage)
        // {
        //     var index = termsIndex[item1.Item1];
        //     var valueTerm = new SRIVectorDic<string, int>();

        //     foreach (var item2 in item1.Item2)
        //     {
        //         var docindex = DocsFrecModal[item2.Item1].Item1;
        //         var pattern = docspattern[docindex];

        //         if (!valueTerm.ContainsKey(pattern))
        //         {
        //             valueTerm.Add((pattern, 1));
        //         }
        //         else
        //         {
        //             valueTerm[pattern] += 1;
        //         }

        //         if (!resultDocs.ContainsKey(docindex))
        //         {
        //             var docs = new SRIVectorDic<int, int>();
        //             docs.Add((index, 1));
        //             resultDocs.Add(docindex, docs);
        //         }
        //         else
        //         {
        //             var docs = resultDocs[docindex];
        //             if (docs!.ContainsKey(index))
        //             {
        //                 docs.Add(index, 1);
        //             }
        //             else
        //             {
        //                 docs[index] += 1;
        //             }
        //         }
        //     }
        //     resultTerms.Add(item1.Item1, valueTerm);
        // }

        weightTerms = resultTerms;
        weightDocs = resultDocs;
        needUpdate = false;
    }
}
