using System.Collections;
using SRI.Interface;

namespace SRI;

/// <summary>
/// Representa un vector en el modelo de SRI
/// </summary>
/// <typeparam name="K">tipo de llave de dicho vector</typeparam>
/// <typeparam name="T">tipo de valor que le corresponde a una llave</typeparam>
public class SRIVector<K, T> : ISRIVector<K, T>
{
    public T this[K index] => throw new NotImplementedException();

    public SRIVector(double[] result)
    {
        IEnumerable<int> a = new List<int>();
    }

    public int Count => throw new NotImplementedException();


    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<K> GetKeys()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Representa un documento recuperado a partir de una consulta
/// </summary>
public class SearchItem
{
    public SearchItem(string URL, string title, string snippet, double score)
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
    public string Title { get; private set; }

    /// <summary>
    /// Pequeña representación del documento recuperado
    /// </summary>
    public string Snippet { get; private set; }

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

    public SearchResult(SearchItem[] items, string suggestion="")
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