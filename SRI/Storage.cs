namespace SRI;

using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using DP;
using DP.Interface;
using SRI.Interface;
using Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

public abstract class Storage<T1, T2, V, D> : IStorage<T1, T2, V, D>, ICollection<D> where T1 : notnull where T2 : notnull
{
    protected abstract IDictionary<T1, IDictionary<T2, V>> MatrixStorage { get; set; }
    public virtual IDictionary<T2, V> this[T1 index] => MatrixStorage[index];
    public abstract IEnumerable<D> corpus { get; }

    public abstract int Count { get; }
    public virtual bool IsReadOnly => MatrixStorage.IsReadOnly;

    public abstract IDictionary<T2, V> GetKey2Vector(T1 doc);
    public abstract IDictionary<T1, V> GetKey1Vector(T2 index);
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
    protected Dictionary<string, (int, int)> InvFrecTerms;
    protected bool needUpdate;

    public VSMStorageDT(IEnumerable<IDocument>? corpus)
    {
        MatrixStorage = new Dictionary<IDocument, IDictionary<string, IWeight>>();
        DocsFrecModal = new Dictionary<IDocument, int>();
        InvFrecTerms = new Dictionary<string, (int, int)>();
        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public override IEnumerable<IDocument> corpus => DocsFrecModal.Keys;

    public override int Count => DocsFrecModal.Count;

    protected override IDictionary<IDocument, IDictionary<string, IWeight>> MatrixStorage { get; set; }

    public override void Add(IDocument item)
    {
        needUpdate = true;

        int ModalFrec;
        var terms = GenWeightTerms(item, out ModalFrec);
        if (terms is null) return;

        MatrixStorage.Add(item, terms);
        DocsFrecModal.Add(item, ModalFrec);
    }

    public override IDictionary<IDocument, IWeight> GetKey1Vector(string index)
    {
        Dictionary<IDocument, IWeight> result = new Dictionary<IDocument, IWeight>();
        foreach (var item in MatrixStorage)
        {
            try
            {
                result.Add(item.Key, MatrixStorage[item.Key][index]);
            }
            catch { }
        }

        return result;
    }

    public override IDictionary<string, IWeight> GetKey2Vector(IDocument doc) => this[doc];

    public override bool Remove(IDocument item)
    {
        needUpdate = true;
        return (MatrixStorage as Dictionary<IDocument, IDictionary<string, IWeight>>)!.Remove(item);
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
                        (MatrixStorage as Dictionary<IDocument, IDictionary<string, IWeight>>)!.Remove(item);
                    else
                    {
                        (MatrixStorage as Dictionary<IDocument, IDictionary<string, IWeight>>)![item] = terms;
                        DocsFrecModal[item] = ModalFrec;
                    }
                    break;
                case stateDoc.deleted:
                    RemoveInvFrec(item);
                    (MatrixStorage as Dictionary<IDocument, IDictionary<string, IWeight>>)!.Remove(item);
                    break;
                case stateDoc.notchanged:
                    break;
                default:
                    throw new NotImplementedException("Se añadió otra variante de stateDocs");
            }
        }

        UpdateAllWeight();
    }

    protected virtual IDictionary<string, IWeight>? GenWeightTerms(IDocument doc, out int ModalFrec)
    {
        ModalFrec = 0;
        ProcesedDocument termsresult = new ProcesedDocument(doc);
        if (termsresult.Length == 0) return null;

        Dictionary<string, IWeight> result = new Dictionary<string, IWeight>();
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
            var value = InvFrecTerms![item.Key];
            InvFrecTerms![item.Key] = (value.Item1, value.Item2 - 1);
        }
    }

    public virtual void UpdateAllWeight()
    {
        if (!needUpdate) return;
        foreach (var doc in MatrixStorage)
            foreach (var item in doc.Value)
                (item.Value as VSMWeight)!.Update(DocsFrecModal[doc.Key], Count, InvFrecTerms![item.Key].Item2);
        needUpdate = false;
    }
}

public class VSMStorageTD : Storage<string, IDocument, IWeight, IDocument>, IStorage<string, IDocument, IWeight, IDocument>, ICollection<IDocument>
{
    public Dictionary<IDocument, (int, double)> DocsFrecModal;
    protected bool needUpdate;

    public VSMStorageTD(IEnumerable<IDocument>? corpus)
    {
        MatrixStorage = new Dictionary<string, IDictionary<IDocument, IWeight>>();
        DocsFrecModal = new Dictionary<IDocument, (int, double)>();
        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    protected override IDictionary<string, IDictionary<IDocument, IWeight>> MatrixStorage { get; set; }
    public override IEnumerable<IDocument> corpus => DocsFrecModal.Select(x => x.Key);

    public override int Count => DocsFrecModal.Count;

    public override void Add(IDocument item)
    {
        needUpdate = true;

        int num = DocsFrecModal.Count;
        GenWeightTerms(item, in num);
    }

    public override IDictionary<string, IWeight> GetKey1Vector(IDocument index)
    {
        Dictionary<string, IWeight> result = new Dictionary<string, IWeight>();
        foreach (var item in MatrixStorage)
        {
            if (item.Value.ContainsKey(index))
                result.Add(item.Key, item.Value[index]);
        }

        return result;
    }

    public override IDictionary<IDocument, IWeight> GetKey2Vector(string doc) => MatrixStorage[doc];

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
                var docs = new Dictionary<IDocument, IWeight>();
                docs.Add(doc, new VSMWeight(item.Item2));
                MatrixStorage.Add(item.Item1, docs);
            }
            else
                MatrixStorage[item.Item1].Add(doc, new VSMWeight(item.Item2));
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
            foreach (var item in doc.Value)
            {
                var frecModal = DocsFrecModal[item.Key].Item2;
                (item.Value as VSMWeight)!.Update(frecModal, DocsFrecModal.Count, doc.Value.Count);
                norma2[DocsFrecModal[item.Key].Item1] += Math.Pow(item.Value.Weight, 2);
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

    [JsonInclude]
    private Dictionary<string, double[]> weightTerms;
    [JsonInclude]
    private Dictionary<IDocument, double[]> weightDocs;
    private MinTerm<int>[]? docspattern;

    public GVSMStorageDT(IEnumerable<IDocument>? corpus) : base(null)
    {
        weightTerms = new Dictionary<string, double[]>();
        weightDocs = new Dictionary<IDocument, double[]>();

        if (corpus is null) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    public int DocsLength => docspattern!.Length;

    public new double[] GetKey1Vector(string index) => weightTerms[index];

    public new double[] GetKey2Vector(IDocument doc) => weightDocs[doc];

    private IDictionary<string, IDictionary<IDocument, int>> Trasp(IDictionary<IDocument, IDictionary<string, IWeight>> matrix)
    {
        IDictionary<IDocument, int> vector = GenMinTerms();
        Dictionary<string, IDictionary<IDocument, int>> trasp = new Dictionary<string, IDictionary<IDocument, int>>();

        foreach (var doc in matrix)
        {
            // Dictionary<int, double> docVector = new();
            foreach (var term in doc.Value)
            {
                if (!trasp.ContainsKey(term.Key))
                {
                    var termVector = new Dictionary<IDocument, int>();
                    termVector.Add(doc.Key, vector[doc.Key]);
                    trasp.Add(term.Key, termVector);
                }
                else
                {
                    trasp[term.Key].Add(doc.Key, vector[doc.Key]);
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
        Dictionary<string, IDictionary<int, double>> resultTerms = new();
        Dictionary<IDocument, IDictionary<int, double>> resultDocs = new();
        Stopwatch prueba = new Stopwatch();
        var last = prueba.Elapsed;
        var lastfor = prueba.Elapsed;

        prueba.Start();
        foreach (var term in trasp)
        {
            SortedDictionary<int, double> termVector = new();
            foreach (var doc in term.Value)
            {
                if (!termVector.ContainsKey(doc.Value))
                    termVector.Add(doc.Value, 1);
                else
                    termVector[doc.Value] += 1;
            }
            if(termVector.Count / docspattern!.Length > 0.80) continue;

            var correction = Math.Sqrt(termVector.Select(x => Math.Pow(x.Value, 2)).Sum());

            foreach (var item in termVector.Keys.ToArray())
            {
                termVector[item] /= correction;
            }
            resultTerms.Add(term.Key, termVector);
        }
        var a = resultTerms.Values.Select(x => x.Count).Max();

        System.Console.WriteLine($"end terms: time = {lastfor = last = prueba.Elapsed}");
        var count = 0;
        var count2 = 1;
        foreach (var doc in MatrixStorage)
        {

            if (count == 1000)
            {
                System.Console.WriteLine($"docs {count2 * count}: time = {prueba.Elapsed - last}");
                last = prueba.Elapsed;
                count = 0;
                count2++;
            }

            double[] docVector = new double[docspattern!.Length];
            foreach (var item in doc.Value)
            {
                if (!resultTerms.ContainsKey(item.Key)) continue;
                foreach (var minterm in resultTerms[item.Key])
                {
                    docVector[minterm.Key] += minterm.Value;
                }
            }

            SortedDictionary<int, double> newDocVector = new();
            foreach (var item in docVector.Select((Value, Key) => (Key, Value)).Where(x => x.Value > 0))
            {
                newDocVector.Add(item.Key, item.Value);
            }

            resultDocs.Add(doc.Key, newDocVector);
            count++;
        }

        prueba.Stop();
        System.Console.WriteLine($"end docs: time = {prueba.Elapsed - lastfor}");

        // weightTerms = resultTerms;
        // weightDocs = resultDocs;
        needUpdate = false;
    }

    private Dictionary<IDocument, int> GenMinTerms()
    {
        var trie = new Trie<int>(-1);
        var docsindex = new Dictionary<IDocument, int>();
        var tempPattern = new Dictionary<int, MinTerm<int>>();
        foreach (var doc in MatrixStorage)
        {
            var temp = new LinkedList<int>();
            foreach (var term in doc.Value)
                temp.AddLast(InvFrecTerms[term.Key].Item1);
            var index = trie.InsertTrie(temp);

            if (!tempPattern.ContainsKey(index))
                tempPattern.Add(index, new MinTerm<int>(temp));
            docsindex.Add(doc.Key, index);
        }
        docspattern = tempPattern.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
        return docsindex;
    }
}