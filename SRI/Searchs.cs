using System.Collections;
using SRI.Interface;

namespace SRI;

public class Weight : IWeight
{
    public Weight(int item2)
    {
        Frec = item2;
    }

    public int Frec { get; private set; }
    public int ModalFrec { get; private set; }
    public int DocsLength { get; private set; }
    public int InvFrec { get; private set; }

    public void Update(int ModalFrec, int DocsLength, int InvFrec)
    {
        this.ModalFrec = ModalFrec;
        this.DocsLength = DocsLength;
        this.InvFrec = InvFrec;
    }

    public double GetWeight() => Frec / ModalFrec * Math.Log(DocsLength / InvFrec);
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