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

    public SearchItem[] GetSearchItems(IResult<string, string, int> query)
    {
        if(!CheckCorpus() || corpus is null) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        if(weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        // int count = 0;
        SearchItem[] result = new SearchItem[weightMatrix.Count];
        // foreach (IDocument doc in corpus.GetAllDocument())
        // {
        //     result[count++] = new SearchItem(doc.Id, doc.Id, (string)((IEnumerable<char>)doc), query.Sum(x => (weightMatrix.ContainsKey(x.Item1)) ? weightMatrix[doc.Id][x.Item1] : 0));
        // }

        return result;
    }

    public ISRIVector<string, ISRIVector<string, double>> GetWeightMatrix() 
    {
        if(corpus is null) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        if(!UpdateRequired && !(weightMatrix is null)) return weightMatrix;
        UpdateRequired = false;

        // return new SRIVector<string, ISRIVector<string, double>>(
        //     corpus.GetAllDocument().Select(
        //         docname => new KeyValuePair<string, ISRIVector<string, double>>(docname, new SRIVector<string, double>(
        //             corpus.GetProcesedDocument(docname).Select(
        //                 x => new KeyValuePair<string, double>(x.Item1, TFIDF(docname, x.Item1)))))));

        throw new NotImplementedException();
    }

    private double TFIDF(string doc, string term)
    {
        if(corpus is null) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        return corpus.Frequency(doc, term) * Math.Log(corpus.InvertedFrequency(term));
    }

    public ISRIVector<string, double> GetTermVector(string index) 
    {
        if(!CheckCorpus()) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        if(weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        return new SRIVector<string, double>(weightMatrix.GetKeys().Select(x => new KeyValuePair<string, double>(x, weightMatrix[x][index])));
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

        double normaDoc1 = 0, normaDoc2 = 0, scalarMul = 0;

        foreach (var item in doc1.GetKeys())
        {
            try
            {
                normaDoc1 += Math.Pow(doc1[item], 2);
                normaDoc2 += Math.Pow(doc2[item], 2);
                scalarMul += doc1[item] * doc2[item];
            }
            catch (System.Exception)
            {
                throw new ArgumentOutOfRangeException("los vectores tienen que ser del mismo espacio vectorial");
            }
        }

        normaDoc1 = Math.Pow(normaDoc1, 0.5);
        normaDoc2 = Math.Pow(normaDoc2, 0.5);

        return scalarMul / (normaDoc1 * normaDoc2);
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