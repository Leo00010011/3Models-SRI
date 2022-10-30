using DP.Interface;
using SRI.Interface;

namespace SRI;
public class VSM : ISRIModel<string, string>
{
    public bool UpdateRequired { get; private set; }

    private IProcesedCorpus? corpus;
    private ISRIVector<string, ISRIVector<string, double>>? weightMatrix;

    public VSM(IProcesedCorpus? corpus = null)
    {
        UpdateRequired = !(corpus is null);
        this.corpus = corpus;
        weightMatrix = null;
    }

    public SearchItem[] GetSearchItems(IProcesedDocument query)
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        if(weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        int count = 0;
        SearchItem[] result = new SearchItem[weightMatrix.Count];
        foreach (var doc in weightMatrix)
        {
            result[count++] = new SearchItem("asd", "asd", "asd", query.TermFreqInDoc.Sum(x => (doc.GetKeys().Contains(x.Item1)) ? doc[x.Item1] : 0));
        }

        return result;
    }

    public ISRIVector<string, ISRIVector<string, double>> GetWeightMatrix() 
    {
        if(corpus is null) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        throw new NotImplementedException();
    }

    public ISRIVector<string, double> GetTermVector(string index) 
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        if(weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        int count = 0;
        double[] result = new double[weightMatrix.Count];
        foreach (var doc in weightMatrix)
        {
            result[count++] = doc[index];
        }

        return new SRIVector<string, double>(result);
    }

    public ISRIVector<string, double> GetDocVector(string index) 
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        if(weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        return weightMatrix[index];
    }

    public double SimilarityRate(ISRIVector<string, double> doc1, ISRIVector<string, double> doc2)
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");

        return doc1.Scalar_Mult<string>(doc2) / (doc1.Norma2() * doc2.Norma2());
    }

    public SearchItem[] Ranking(SearchItem[] searchResult)
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");

        return searchResult.OrderBy(x => x.Score).ToArray();
    }

    public bool UpdateProcesedCorpus(IProcesedCorpus corpus) => UpdateRequired = object.Equals(corpus, this.corpus);

    private bool CheckCorpus()
    {
        if(!UpdateRequired || corpus is null) return !UpdateRequired;

        weightMatrix = GetWeightMatrix();
        return true;
    }
}