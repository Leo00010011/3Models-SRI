using System.Collections;
using SRI.Interface;

namespace SRI;

public class SRIVector<T, K> : ISRIVector<T, K>
{
    public T this[K index] => throw new NotImplementedException();

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public class SearchItem
{
    public SearchItem(string title, string snippet, double score)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
    }

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