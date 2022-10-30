using DP.Interface;
using SRI.Interface;

namespace SRI;
public class VSM : ISRIModel<string , int>
{
    public bool UpdateRequired { get; private set; }

    private IProcesedCorpus? corpus;
    private ISRIVector<int, ISRIVector<string, double>>? weightMatrix;

    public VSM(IProcesedCorpus? corpus = null)
    {
        UpdateRequired = !(corpus is null);
        this.corpus = corpus;
        weightMatrix = null;
    }

    public SearchItem[] GetSearchItems()
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        throw new NotImplementedException();
    }

    public ISRIVector<int, ISRIVector<string, double>> GetWeightMatrix() 
    {
        if(corpus is null) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        throw new NotImplementedException();
    }

    public ISRIVector<int, double> GetTermVector(string index) 
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        throw new NotImplementedException();
    }

    public ISRIVector<string, double> GetDocVector(int index) 
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        throw new NotImplementedException();
    }

    public double SimilarityRate(ISRIVector<string, double> doc1, ISRIVector<string, double> doc2)
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        throw new NotImplementedException();
    }

    public SearchItem[] Ranking(SearchItem[] searchResult)
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        throw new NotImplementedException();
    }

    public bool UpdateProcesedCorpus(IProcesedCorpus corpus) => UpdateRequired = object.Equals(corpus, this.corpus);

    private bool CheckCorpus()
    {
        if(!UpdateRequired || corpus is null) return !UpdateRequired;

        weightMatrix = GetWeightMatrix();
        return true;
    }
}