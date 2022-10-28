using DP.Interface;

namespace SRI.Interface;
public interface ISearchResult : IEnumerable<SearchItem>
{
    SearchItem this[int index] { get; }

    string Suggestion { get; }

    int Count { get; }
}

public interface ISRIVector<T, K> : IEnumerable<T>
{
    T this[K index] { get; }
}

public interface ISRIModel
{
    ISRIVector<double, int> GetDocVector(int index);

    SearchItem[] GetSearchItems();

    ISRIVector<double, int> GetTermVector(int index);

    ISRIVector<ISRIVector<double, int>, int> GetWeightMatrix();

    SearchItem[] Ranking(SearchItem[] searchResult);

    double SimilarityRate(ISRIVector<double, int> doc1, ISRIVector<double, int> doc2);

    bool UpdateProcesedCorpus(IProcesedCorpus corpus);
}