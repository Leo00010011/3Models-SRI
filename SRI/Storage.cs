namespace SRI;

using System.Diagnostics;
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

    public IEnumerable<(IDocument, double)> GetAllDocs() => DocsFrecModal.Select(x => (x.Key, x.Value.Item2));
}

public class GVSMStorageDT : VSMStorageDT, IStorage<IDocument, string, IWeight, IDocument>, ICollection<IDocument>
{
    public new double[] this[IDocument index] => weightDocs[index];
    private SRIVectorDic<string, double[]> weightTerms;
    private SRIVectorDic<IDocument, double[]> weightDocs;
    private MinTerm<int>[]? docspattern;

    public GVSMStorageDT(IEnumerable<IDocument>? corpus) : base(null)
    {
        weightTerms = new SRIVectorDic<string, double[]>();
        weightDocs = new SRIVectorDic<IDocument, double[]>();

        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public int DocsLength => docspattern!.Length;

    public new double[] GetKey1Vector(string index) => weightTerms[index];

    public new double[] GetKey2Vector(IDocument doc) => weightDocs[doc];

    private ISRIVector<string, ISRIVector<(IDocument, int), SortedSet<MinTermWeight>>> Trasp(ISRIVector<IDocument, ISRIVector<string, IWeight>> matrix)
    {
        ISRIVector<IDocument, int> vector = GenMinTerms();
        SRIVectorDic<string, ISRIVector<(IDocument, int), SortedSet<MinTermWeight>>> trasp = new SRIVectorDic<string, ISRIVector<(IDocument, int), SortedSet<MinTermWeight>>>();

        foreach (var doc in matrix)
        {
            SortedSet<MinTermWeight> docVector = new();
            foreach (var term in doc.Item2)
            {
                if (!trasp.ContainsKey(term.Item1))
                {
                    var termVector = new SRIVectorDic<(IDocument, int), SortedSet<MinTermWeight>>();
                    termVector.Add((doc.Item1, vector[doc.Item1]), docVector);
                    trasp.Add(term.Item1, termVector);
                }
                else
                {
                    trasp[term.Item1].Add((doc.Item1, vector[doc.Item1]), docVector);
                }
            }
        }

        return trasp;
    }

    public override void UpdateAllWeight()
    {
        base.UpdateAllWeight();
        needUpdate = true;

        var trasp = Trasp(MatrixStorage);
        // var resultTerms = new SRIVectorDic<string, double[]>();
        // foreach (var term in trasp)
        // {
        //     var termVector = new double[docspattern!.Length];
        //     foreach (var doc in term.Item2)
        //     {
        //         termVector[doc.Item1.Item2] += 1;
        //     }

        //     // var correction = Math.Sqrt(termVector.Select(value => Math.Pow(value, 2)).Sum());
        //     // termVector = correction != 0 ? termVector.Select(value => value == 0 ? value : value / correction).ToArray() : termVector;
            
        //     var temp = termVector.Select((value, index) => (value, index)).Where(x => x.value != 0).ToArray();
        //     foreach (var doc in term.Item2)
        //     {
        //         foreach (var item in temp)
        //         {
        //             doc.Item2[item.index] += item.value;
        //         }
        //     }

        //     resultTerms.Add(term.Item1, termVector);
        // }

        // var resultDocs = new SRIVectorDic<IDocument, double[]>();
        // foreach (var term in trasp)
        // {
        //     foreach (var doc in term.Item2)
        //     {
        //         if (!resultDocs.ContainsKey(doc.Item1.Item1)) continue;
        //         else
        //         {
        //             resultDocs.Add(doc.Item1.Item1, doc.Item2);
        //         }
        //     }
        // }

        // weightTerms = resultTerms;
        // weightDocs = resultDocs;
        needUpdate = false;
    }

    private SRIVectorDic<IDocument, int> GenMinTerms()
    {
        var trie = new Trie<int>(-1);
        var docsindex = new SRIVectorDic<IDocument, int>();
        var tempPattern = new SRIVectorDic<int, MinTerm<int>>();
        foreach (var doc in MatrixStorage)
        {
            var temp = new LinkedList<int>();
            foreach (var term in doc.Item2)
                temp.AddLast(InvFrecTerms[term.Item1].Item1);
            var index = trie.InsertTrie(temp);

            if (!tempPattern.ContainsKey(index))
                tempPattern.Add(index, new MinTerm<int>(temp));
            docsindex.Add(doc.Item1, index);
        }
        docspattern = tempPattern.OrderBy(x => x.Item1).Select(x => x.Item2).ToArray();
        return docsindex;
    }
}