namespace SRI.Interface;

using DP.Interface;

/// <summary>
/// Representa la interfaz del resultado de una consulta en un modelo de SRI
/// </summary>
public interface ISearchResult : IEnumerable<SearchItem>
{
    SearchItem this[int index] { get; }

    /// <summary>
    /// Es la palabra más cercana a la consulta realizada
    /// </summary>
    string Suggestion { get; }

    /// <summary>
    /// Cantidad de documentos recuperados por la consulta
    /// </summary>
    int Count { get; }
}

public interface IWeight
{
    
    double GetWeight();
}

/// <summary>
/// Representa la interfaz de un modelo genérico en un SRI
/// </summary>
/// <typeparam name="T">tipo de término en el modelo</typeparam>
/// <typeparam name="D">tipo de documento en el modelo</typeparam>
public interface ISRIModel<D, T> where T: notnull where D: notnull
{
    /// <summary>
    /// Este método es el encargado de, a partir del peso de los documentos, 
    /// devolver los documentos recuperados por esa consulta
    /// </summary>
    /// <param name="query">consulta realizada al corpus del modelo</param>
    /// <returns>retorna un array de SearchItem que representa los documentos recuperados</returns>
    SearchItem[] GetSearchItems(IDictionary<T, double> query);

    /// <summary>
    /// Este método se utiliza para comparar cuán semejantes son dos documentos dentro
    /// del modelo de recuperación de información
    /// </summary>
    /// <param name="doc1">documento a comparar</param>
    /// <param name="doc2">documento a comparar</param>
    /// <returns>retorna un valor entre 0 y 1 como indicador de semejanza</returns>
    double SimilarityRate(IDictionary<T, double> doc1, (IDictionary<T, IWeight>, int) doc2);

    /// <summary>
    /// Dado una serie de documentos recuperados, estos se ordenan siguiendo el criterio
    /// específico del modelo implementado
    /// </summary>
    /// <param name="searchResult"></param>
    /// <returns></returns>
    SearchItem[] Ranking(SearchItem[] searchResult);
}

public interface IStorage<D, T, V> : ICollection<IDocument> where D: notnull where T: notnull
{
    (IDictionary<T, V>, int) this[D index]
    {
        get;
    }

    // /// <summary>
    // /// Este método es el encargado de dar valores a la matriz de vectores de documentos
    // /// </summary>
    // /// <returns>retorna un vector de vectores de documentos</returns>
    // void GenWeightMatrix(IEnumerable<D>? corpus);

    /// <summary>
    /// Este método se utiliza para selecionar un vector de término específico
    /// </summary>
    /// <param name="index">es el identificador de dicho término en el modelo</param>
    /// <returns>retorna un vector término</returns>
    IDictionary<D, IWeight> GetTermVector(T index);

    // /// <summary>
    // /// Este método se utiliza para selecionar un vector de documento específico
    // /// </summary>
    // /// <param name="index">es el identificador de dicho documento en el modelo</param>
    // /// <returns>retorna un vector documento</returns>
    // IDictionary<T, double> GetDocVector(D index);

    IDictionary<T, V>? GenWeightTerms(D doc, out int ModalFrec);

    void UpdateDocs();
}