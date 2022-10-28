using DP.Interface;

namespace SRI.Interface;
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

public interface ISRIVector<T, K> : IEnumerable<T>
{
    T this[K index] { get; }
}

public interface ISRIModel
{
    /// <summary>
    /// Este método es el encargado de, a partir del peso de los documentos, 
    /// devolver los documentos recuperados por esa consulta
    /// </summary>
    /// <returns>retorna un array de SearchItem que representa los documentos recuperados</returns>
    SearchItem[] GetSearchItems();

    /// <summary>
    /// Este método es el encargado de dar valores a la matriz de vectores de documentos
    /// </summary>
    /// <returns>retorna un vector de vectores de documentos</returns>
    ISRIVector<ISRIVector<double, int>, int> GetWeightMatrix();

    /// <summary>
    /// Este método se utiliza para selecionar un vector de término específico
    /// </summary>
    /// <param name="index">es el identificador de dicho término en el modelo</param>
    /// <returns>retorna un vector término</returns>
    ISRIVector<double, int> GetTermVector(int index);

    /// <summary>
    /// Este método se utiliza para selecionar un vector de documento específico
    /// </summary>
    /// <param name="index">es el identificador de dicho documento en el modelo</param>
    /// <returns>retorna un vector documento</returns>
    ISRIVector<double, int> GetDocVector(int index);

    /// <summary>
    /// Este método se utiliza para comparar cuán semejantes son dos documentos dentro
    /// del modelo de recuperación de información
    /// </summary>
    /// <param name="doc1">documento a comparar</param>
    /// <param name="doc2">documento a comparar</param>
    /// <returns>retorna un valor entre 0 y 1 como indicador de semejanza</returns>
    double SimilarityRate(ISRIVector<double, int> doc1, ISRIVector<double, int> doc2);

    /// <summary>
    /// Dado una serie de documentos recuperados, estos se ordenan siguiendo el criterio
    /// específico del modelo implementado
    /// </summary>
    /// <param name="searchResult"></param>
    /// <returns></returns>
    SearchItem[] Ranking(SearchItem[] searchResult);

    /// <summary>
    /// Se usa para actualizar los valores del corpus del modelo en caso de que el contenido
    /// de dichos documentos cambie
    /// </summary>
    /// <param name="corpus">el nuevo corpus que se va a utilizar</param>
    /// <returns>retorna true si se reemplazó dicho corpus con éxito</returns>
    bool UpdateProcesedCorpus(IProcesedCorpus corpus);
}