using System.Collections;
using SRI.Interface;

namespace SRI;

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

    public V this[K index] { get { return vector[index]; } set { vector[index] = value; } }

    public int Count => vector.Count;

    public bool IsReadOnly => false;

    public void Add((K, V) item) => vector.Add(item.Item1, item.Item2);
    public void Add(K key, V value) => vector.Add(key, value);

    public void Clear() => vector.Clear();

    public bool Contains((K, V) item) => vector.ContainsKey(item.Item1) && object.Equals(vector[item.Item1], item.Item2);

    public bool ContainsKey(K key) => vector.ContainsKey(key);

    public void CopyTo((K, V)[] array, int arrayIndex) => throw new NotImplementedException();

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
    public SearchItem(string URL, IEnumerable<char> title, IEnumerable<char> snippet, double score)
    {
        this.URL = URL;
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
    }

    /// <summary>
    /// Dirección del documento recuperado
    /// </summary>
    public string URL { get; private set; }

    /// <summary>
    /// Título del documento recuperado
    /// </summary>
    public IEnumerable<char> Title { get; private set; }

    /// <summary>
    /// Pequeña representación del documento recuperado
    /// </summary>
    public IEnumerable<char> Snippet { get; private set; }

    /// <summary>
    /// Peso del documento según el modelo empleado
    /// </summary>
    public double Score { get; private set; }
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