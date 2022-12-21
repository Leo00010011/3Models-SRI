namespace SRI;

using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using DP;
using DP.Interface;
using SRI.Interface;
using Utils;
using System.Text.Json;
using System.Runtime.InteropServices;

public abstract class Storage<T1, T2, V, D> : IStorage<T1, T2, V, D>, ICollection<D> where T1 : notnull where T2 : notnull
{
    protected Storage(IEnumerable<D> corpus)
    {
        this.corpus = corpus;
    }

    protected abstract IDictionary<T1, IDictionary<T2, V>> MatrixStorage { get; set; }
    public virtual IDictionary<T2, V> this[T1 index] => MatrixStorage[index];
    public virtual IEnumerable<D> corpus { get; }

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

    public VSMStorageDT(IEnumerable<IDocument> corpus, bool Is_Add = false) : base(corpus)
    {
        MatrixStorage = new Dictionary<IDocument, IDictionary<string, IWeight>>();
        DocsFrecModal = new Dictionary<IDocument, int>();
        InvFrecTerms = new Dictionary<string, (int, int)>();

        if (Is_Add) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

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
        foreach (var item in MatrixStorage.Keys)
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

    public virtual bool ContainsKey(string key) => InvFrecTerms.ContainsKey(key);

    public override IEnumerator<IDocument> GetEnumerator() => MatrixStorage.Keys.GetEnumerator();
}

public class VSMStorageTD : Storage<string, IDocument, IWeight, IDocument>, IStorage<string, IDocument, IWeight, IDocument>, ICollection<IDocument>
{
    public Dictionary<IDocument, (int, double)> DocsFrecModal;
    protected bool needUpdate;

    public VSMStorageTD(IEnumerable<IDocument> corpus, bool Is_Add = false) : base(corpus)
    {
        MatrixStorage = new Dictionary<string, IDictionary<IDocument, IWeight>>();
        DocsFrecModal = new Dictionary<IDocument, (int, double)>();

        if (Is_Add) return;

        foreach (var item in corpus)
            this.Add(item);
        UpdateDocs();
    }

    protected override IDictionary<string, IDictionary<IDocument, IWeight>> MatrixStorage { get; set; }

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
        doc.UpdateDateTime();
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

            //var value = DocsFrecModal[item.doc];
            (int, double) value;
            if (DocsFrecModal.TryGetValue(item.doc, out value))
                DocsFrecModal[item.doc] = (value.Item1, Math.Sqrt(norma2[value.Item1]));
        }
        needUpdate = false;
    }

    public IEnumerable<(IDocument, double)> GetAllDocs() => DocsFrecModal.Select(x => (x.Key, x.Value.Item2));

    public virtual bool ContainsKey(string key) => MatrixStorage.ContainsKey(key);

    public override IEnumerator<IDocument> GetEnumerator() => DocsFrecModal.Keys.GetEnumerator();
}


public class GVSMStorageDT : VSMStorageDT, IStorage<IDocument, string, IWeight, IDocument>, ICollection<IDocument>
{
    public new double[] this[IDocument index] => GetKey2Vector(index);
    public double[] this[int index] => GetKey2Vector(index);

    private const int saveSize = 3000;
    private double[][]? actualDocs;
    private int actualIndex;
    private Dictionary<IDocument, int> docs;
    private Dictionary<string, IDictionary<int, double>> weightTerms;
    private MinTerm<int>[]? docspattern;

    public GVSMStorageDT(IEnumerable<IDocument> corpus, bool Is_Add = false) : base(corpus, true)
    {
        weightTerms = new Dictionary<string, IDictionary<int, double>>();
        docs = new Dictionary<IDocument, int>();
        actualIndex = -1;

        if (Is_Add) return;

        foreach (var item in corpus)
        {
            this.Add(item);
        }
        UpdateDocs();
    }

    public int DocsLength => docspattern!.Length;

    public new IDictionary<int, double> GetKey1Vector(string index) => weightTerms[index];

    public new double[] GetKey2Vector(IDocument doc) => GetKey2Vector(docs[doc]);

    private double[] GetKey2Vector(int doc)
    {
        int index = (int)(doc / saveSize);
        if (index != actualIndex)
        {
            var binary = File.ReadAllBytes($@".\DocSave\save{index}");
            var save = MemoryMarshal.Cast<byte, double>(binary);
            actualDocs = new double[Math.Min(saveSize, save.Length / docspattern!.Length)][];
            for (int i = 0; i < Math.Min(saveSize, save.Length / docspattern!.Length); i++)
            {
                actualDocs[i] = save.Slice(i * docspattern!.Length, docspattern!.Length).ToArray();
            }
            actualIndex = index;
        }
        return actualDocs![doc % saveSize];
    }

    private IDictionary<string, IDictionary<IDocument, int>> Trasp(IDictionary<IDocument, IDictionary<string, IWeight>> matrix)
    {
        IDictionary<IDocument, int> vector = GenMinTerms();
        Dictionary<string, IDictionary<IDocument, int>> trasp = new Dictionary<string, IDictionary<IDocument, int>>();

        foreach (var doc in matrix)
        {
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
        if (!needUpdate) return;
        foreach (var item in MatrixStorage.Keys.Select((value, index) => (index, value)))
        {
            docs.Add(item.value, item.index);
        }

        if (File.Exists(@".\DocSave\SaveManager"))
        {
            var readsavefile = File.OpenText(@".\DocSave\SaveManager");
            var saves = JsonSerializer.Deserialize<string[]>(readsavefile.ReadToEnd());
            readsavefile.Close();
            if (saves!.Length != corpus.Count() || !saves!.All(x => corpus.Select(x => x.Id).Contains(x)))
                File.Delete(@".\DocSave\SaveManager");
        }

        base.UpdateAllWeight();
        needUpdate = true;

        var trasp = Trasp(MatrixStorage);
        Dictionary<string, IDictionary<int, double>> resultTerms = new();

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
            if (termVector.Count / docspattern!.Length > 0.80) continue;

            var correction = Math.Sqrt(termVector.Select(x => Math.Pow(x.Value, 2)).Sum());

            foreach (var item in termVector.Keys.ToArray())
            {
                termVector[item] /= correction;
            }
            resultTerms.Add(term.Key, termVector);
        }

        weightTerms = resultTerms;
        needUpdate = false;

        if (File.Exists(@".\DocSave\SaveManager")) return;

        var files = Directory.GetFiles(@".\DocSave");
        foreach (var item in files)
            File.Delete(item);
        var docindex = 0;
        var firstindex = 0;
        var save = new double[saveSize * docspattern!.Length];
        foreach (var doc in MatrixStorage)
        {
            foreach (var item in doc.Value)
            {
                if (!resultTerms.ContainsKey(item.Key)) continue;
                foreach (var minterm in resultTerms[item.Key])
                {
                    save[(docindex - firstindex) * saveSize + minterm.Key] += minterm.Value;
                }
            }
            if ((docindex != 0 && (docindex + 1) % saveSize == 0) || (docindex + 1) == MatrixStorage.Count)
            {
                firstindex = docindex + 1;
                int size = (firstindex / saveSize) - (firstindex == MatrixStorage.Count ? 0 : 1);
                using (var writer = new BinaryWriter(File.Open($@".\DocSave\save{size}", FileMode.Create)))
                {
                    var bytes = MemoryMarshal.Cast<double, byte>(save.AsSpan());
                    writer.Write(bytes);
                }
                save = new double[Math.Min(saveSize, MatrixStorage.Count - firstindex) * docspattern!.Length];
            }
            docindex++;
        }

        var writesavefile = File.CreateText(@".\DocSave\SaveManager");
        writesavefile.WriteLine(JsonSerializer.Serialize(corpus.Select(x => x.Id).ToArray()));
        writesavefile.Close();
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

    public override IEnumerator<IDocument> GetEnumerator() => MatrixStorage.Keys.GetEnumerator();
}