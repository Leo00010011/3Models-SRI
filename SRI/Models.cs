namespace SRI;

using DP;
using DP.Interface;
using SRI.Interface;

public class VSM : ISRIModel<string, string>
{
    private ISRIVector<IDocument, ISRIVector<string, IWeight>>? weightMatrix;
    private ISRIVector<string, double>? InvFrecTerms;

    public VSM(IEnumerable<IDocument>? corpus = null) => GenWeightMatrix(corpus);

    public SearchItem[] GetSearchItems(ISRIVector<string, double> query)
    {
        // if (!CheckCorpus() || corpus is null) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        if (weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        int count = 0;
        SearchItem[] result = new SearchItem[weightMatrix.Count];
        foreach (IDocument doc in weightMatrix.GetKeys())
        {
            result[count++] = new SearchItem(doc.Id, doc.Name, doc.GetSnippet(), SimilarityRate(query, GetDocVector(doc)));
        }

        return result;
    }

    public void GenWeightMatrix(IEnumerable<IDocument>? corpus = null)
    {
        if (corpus is null) return;

        LinkedList<KeyValuePair<IDocument, ISRIVector<string, IWeight>>> docs = new LinkedList<KeyValuePair<IDocument, ISRIVector<string, IWeight>>>();
        foreach (IDocument docname in corpus)
        {
            ProcesedDocument termsresult = new ProcesedDocument(docname);
            if (termsresult.Length == 0) continue;

            LinkedList<KeyValuePair<string, IWeight>> terms = new LinkedList<KeyValuePair<string, IWeight>>();
            foreach ((string, int) item in termsresult)
            {
                if (item.Item2 == 0) continue;
                // InvFrecTerms
                terms.AddLast(new KeyValuePair<string, IWeight>(item.Item1, new Weight(item.Item2)));
            }

            docs.AddLast(new KeyValuePair<IDocument, ISRIVector<string, IWeight>>(docname, new SRIVector<string, IWeight>(terms)));
        }

        weightMatrix = new SRIVector<IDocument, ISRIVector<string, IWeight>>(docs);
    }

    private double TFIDF(int term, int modalterm, int termdocs, int docs)
    {
        // if(corpus is null) throw new InvalidOperationException("no existe un corpus al que aplicarle el modelo, considere usar el método UpdateProcesedCorpus");
        return (term / modalterm) * Math.Log(docs / termdocs);
    }

    public ISRIVector<string, double> GetTermVector(string index)
    {
        if (weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        LinkedList<KeyValuePair<string, double>> values = new LinkedList<KeyValuePair<string, double>>();
        foreach (IDocument item in weightMatrix.GetKeys())
        {
            try
            {
                values.AddLast(new KeyValuePair<string, double>(item.Id, weightMatrix[item][index].GetWeight(item.ModalFrec, weightMatrix.Count)));
            }
            finally { }
        }

        return new SRIVector<string, double>(values);
    }

    public ISRIVector<string, double> GetDocVector(IDocument index)
    {
        if (weightMatrix is null) throw new ArgumentNullException("hubo un error inesperado, la matriz de pesos es null");

        return new SRIVector<string, double>(weightMatrix[index].GetKeys().Zip(weightMatrix[index]).Select(x => new KeyValuePair<string, double>(x.First, x.Second.GetWeight(index.ModalFrec, weightMatrix.Count))));
    }

    public double SimilarityRate(ISRIVector<string, double> doc1, ISRIVector<string, double> doc2)
    {
        if (InvFrecTerms is null) return -1;
        double normaDoc1 = 0, normaDoc2 = 0, scalarMul = 0;

        foreach (string item in InvFrecTerms.GetKeys())
        {
            try
            {
                normaDoc1 += Math.Pow(doc1[item], 2);
            } finally { }
            try
            {
                normaDoc2 += Math.Pow(doc2[item], 2);
                scalarMul += doc1[item] * doc2[item];
            } finally { }
        }

        normaDoc1 = Math.Pow(normaDoc1, 0.5);
        normaDoc2 = Math.Pow(normaDoc2, 0.5);

        return scalarMul / (normaDoc1 * normaDoc2);
    }

    public SearchItem[] Ranking(SearchItem[] searchResult)
    {

        return searchResult.OrderBy(x => x.Score).ToArray();
    }
}