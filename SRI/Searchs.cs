using System.Collections;
using SRI.Interface;
using DP;
using DP.Interface;

namespace SRI;

public class MinTermWeight : IWeight
{
    public MinTermWeight(int index, double weight)
    {
        Index = index;
        Weight = weight;
    }

    public int Index { get; private set; }

    public double Weight { get; set; }

    public override bool Equals(object? obj) => obj is MinTermWeight weight && Index == weight.Index;

    public override int GetHashCode() => HashCode.Combine(Index);

    public static MinTermWeight operator +(MinTermWeight a, double b)
    {
        a.Weight += b;
        return a;
    }
}

public class MinTerm<T> : IEnumerable<T>
{
    private IEnumerable<T> terms;
    private int hashcode;

    public MinTerm(IEnumerable<T> terms)
    {
        this.terms = terms;
        hashcode = terms.Select(x => (x != null) ? x.GetHashCode() : 0).Sum();
    }

    public override bool Equals(object? obj) => obj is MinTerm<T> term && terms.Count() == term.terms.Count() &&
                                                terms.Zip(term.terms).All(x => object.Equals(x.First, x.Second));

    public IEnumerator<T> GetEnumerator() => terms.GetEnumerator();

    public override int GetHashCode() => hashcode;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class QueryVSMWeight : IWeight
{
    public double frec;
    public double modalFrec;

    public QueryVSMWeight(double frec, double modalFrec)
    {
        this.frec = frec;
        this.modalFrec = modalFrec;
    }

    public double Weight => frec / modalFrec;
}

public class VSMWeight : IWeight
{
    private double frec;

    public VSMWeight(double frec)
    {
        this.frec = frec;
    }

    public double Weight { get; private set; }

    public void Update(double modalFrec, double docsLength, double invFrec) => Weight = frec / modalFrec * Math.Log(docsLength / invFrec);
}

public class SRIVectorLinked<K, V> : ISRIVector<K, V> where K : notnull
{
    LinkedList<(K, V)> vector;

    public SRIVectorLinked() => this.vector = new LinkedList<(K, V)>();
    public SRIVectorLinked(IEnumerable<KeyValuePair<K, V>> colection) => this.vector = new LinkedList<(K, V)>(colection.Select(x => (x.Key, x.Value)));

    public V this[K index]
    {
        get
        {
            try { return vector.First(x => object.Equals(x.Item1, index)).Item2; }
            catch (System.Exception) { throw new IndexOutOfRangeException(); }
        }
        set
        {
            vector.Find(vector.First(x => object.Equals(x.Item1, index)))!.Value = (index, value);
        }
    }

    public int Count => vector.Count;

    public bool IsReadOnly => false;

    public void Add((K, V) item) => vector.AddLast(item);

    public void Add(K key, V value) => vector.AddLast((key, value));

    public void Clear() => vector.Clear();

    public bool Contains((K, V) item) => vector.Contains(item);

    public bool ContainsKey(K Key)
    {
        try
        {
            vector.First(x => object.Equals(x.Item1, Key));
            return true;
        }
        catch { return false; }
    }

    public void CopyTo((K, V)[] array, int arrayIndex) => vector.CopyTo(array, arrayIndex);

    public bool Remove((K, V) item) => vector.Remove(item);

    public IEnumerator<(K, V)> GetEnumerator() => vector.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class SRIVectorDic<K, V> : ISRIVector<K, V> where K : notnull
{
    Dictionary<K, V> vector;

    public SRIVectorDic() => this.vector = new Dictionary<K, V>();
    public SRIVectorDic(IEnumerable<KeyValuePair<K, V>> colection) => this.vector = new Dictionary<K, V>(colection);

    public V this[K index] { get { return vector[index]; } set { vector[index] = value; } }

    public int Count => vector.Count;

    public bool IsReadOnly => false;

    public void Add((K, V) item) => vector.Add(item.Item1, item.Item2);
    public void Add(K key, V value) => vector.Add(key, value);

    public void Clear() => vector.Clear();

    public bool Contains((K, V) item) => vector.ContainsKey(item.Item1) && object.Equals(vector[item.Item1], item.Item2);

    public bool ContainsKey(K key) => vector.ContainsKey(key);

    public void CopyTo((K, V)[] array, int arrayIndex)
    {
        int count = 0;
        foreach (var item in vector)
        {
            if (array.Length <= count) break;
            array[count] = (item.Key, item.Value);
            count++;
        }
    }

    public bool Remove((K, V) item) => vector.Remove(item.Item1);

    public bool Remove(K key) => vector.Remove(key);

    public IEnumerator<(K, V)> GetEnumerator() => vector.Select(x => (x.Key, x.Value)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Representa un documento recuperado a partir de una consulta
/// </summary>
public class SearchItem
{
    IDocument doc;
    int snippetLen;
    public SearchItem(IDocument doc, int snippetLen, double score)
    {
        this.doc = doc;
        this.Score = score;
        this.snippetLen = snippetLen;
    }

    /// <summary>
    /// Direcci??n del documento recuperado
    /// </summary>
    public string URL => doc.Id;

    /// <summary>
    /// T??tulo del documento recuperado
    /// </summary>
    public string Title => String.Concat(doc.Name);

    /// <summary>
    /// Peque??a representaci??n del documento recuperado
    /// </summary>
    public string Snippet => String.Concat(doc.GetSnippet(snippetLen));

    /// <summary>
    /// Peso del documento seg??n el modelo empleado
    /// </summary>
    public double Score { get; private set; }

    public string GetText() => doc.GetDocText();
}

/// <summary>
/// Representa el resultado de una consulta en un modelo de SRI
/// </summary>
public class SearchResult : ISearchResult, IEnumerable<SearchItem>
{
    SearchItem ISearchResult.this[int index] => items[index];

    private SearchItem[] items;

    public SearchResult(SearchItem[] items, string suggestion = "")
    {
        if (items == null) throw new ArgumentNullException("items");

        this.items = items;
        this.Suggestion = suggestion;
    }

    public SearchResult() : this(new SearchItem[0]) { }


    public string Suggestion { get; private set; }

    public int Count => items.Length;


    public IEnumerator<SearchItem> GetEnumerator() => ((IEnumerable<SearchItem>)items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<SearchItem>)items).GetEnumerator();
}