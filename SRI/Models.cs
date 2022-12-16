namespace SRI;
using System.Collections;
using System.Diagnostics;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class SRIModel<T1, T2, V, Q, D> : ISRIModel<T1, T2, V, Q, D>, ICollection<D> where T1 : notnull where T2 : notnull where Q : notnull
{
    protected virtual ICollection<D>? Storage { get; set; }

    public bool IsReadOnly => true;

    public int Count => Storage!.Count;

    public abstract SearchItem[] GetSearchItems(IDictionary<Q, V> query, int snippetLen);

    public virtual SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();
    public abstract double SimilarityRate(IDictionary<T2, V> doc1, IDictionary<T2, V> doc2);

    public virtual void Add(D item) => Storage!.Add(item);
    public virtual void Clear() => Storage!.Clear();
    public virtual bool Contains(D item) => Storage!.Contains(item);
    public virtual void CopyTo(D[] array, int arrayIndex) => Storage!.CopyTo(array, arrayIndex);
    public virtual bool Remove(D item) => Storage!.Remove(item);

    public virtual IEnumerator<D> GetEnumerator() => Storage!.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public abstract class WModel<T1, T2, D> : SRIModel<T1, T2, IWeight, string, D>, ISRIModel<T1, T2, IWeight, string, D>, ICollection<D> where T1 : notnull where T2 : notnull
{
    public override double SimilarityRate(IDictionary<T2, IWeight> doc1, IDictionary<T2, IWeight> doc2)
    {
        double normaDoc1 = 0, scalarMul = 0;
        foreach (var item1 in doc1)
        {
            var value1 = item1.Value.Weight;
            normaDoc1 += Math.Pow(value1, 2);

            if (doc2.ContainsKey(item1.Key))
            {
                var item2 = doc2[item1.Key];
                var value2 = item2.Weight;
                scalarMul += value1 * value2;
            }
        }
        normaDoc1 = Math.Sqrt(normaDoc1);

        double normaDoc2 = 0;
        foreach (var item in doc2)
        {
            var value = item.Value.Weight;
            normaDoc2 += Math.Pow(value, 2);
        }
        normaDoc2 = Math.Sqrt(normaDoc2);

        return scalarMul / (normaDoc1 * normaDoc2);
    }

    public override SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();
}

public abstract class WMDocTerm : WModel<IDocument, string, IDocument>, ISRIModel<IDocument, string, IWeight, string, IDocument>, ICollection<IDocument>
{
    public WMDocTerm(IEnumerable<IDocument> corpus) => Storage = new VSMStorageDT(corpus);

    public override SearchItem[] GetSearchItems(IDictionary<string, IWeight> query, int snippetLen)
    {
        ((VSMStorageDT)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0; SearchItem[] result = new SearchItem[Storage.Count];
        foreach (var item in Storage)
            result[count++] = new SearchItem(item, snippetLen, SimilarityRate(query, ((VSMStorageDT)Storage)[item]));
        return result;
    }
}

public class VSMDocTerm : WMDocTerm, ISRIModel<IDocument, string, IWeight, string, IDocument>, ICollection<IDocument>
{
    public VSMDocTerm(IEnumerable<IDocument> corpus) : base(corpus) { }

    public IDictionary<string, IWeight> CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);

        Dictionary<string, IWeight> query = new Dictionary<string, IWeight>();
        int modalFrec = results.MaxBy(x => x.Item2).Item2;

        foreach ((string, int) item in results)
            query.Add(item.Item1, new QueryVSMWeight(item.Item2, modalFrec));
        return query;
    }

}

public abstract class WMTermDoc : WModel<string, int, IDocument>, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public WMTermDoc(IEnumerable<IDocument> corpus) => Storage = new VSMStorageTD(corpus);

    public override SearchItem[] GetSearchItems(IDictionary<string, IWeight> query, int snippetLen)
    {
        ((VSMStorageTD)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0;
        SearchItem[] result = new SearchItem[Storage.Count];
        double[] score = new double[Storage.Count];

        double queryscore = 0;
        foreach (var item in query)
        {
            queryscore += Math.Pow(item.Value.Weight, 2);
            if (!((VSMStorageTD)Storage!).ContainsKey(item.Key)) continue;
            foreach (var item1 in ((VSMStorageTD)Storage!)[item.Key])
            {
                var index = ((VSMStorageTD)Storage!).DocsFrecModal[item1.Key].Item1;
                score[index] += item.Value.Weight * item1.Value.Weight;
            }
        }
        queryscore = Math.Sqrt(queryscore);

        foreach (var item in ((VSMStorageTD)Storage).GetAllDocs())
        {
            result[count] = new SearchItem(item.Item1, snippetLen, score[count++] / (queryscore * item.Item2));
        }
        return result;
    }
}

public class VSMTermDoc : WMTermDoc, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public VSMTermDoc(IEnumerable<IDocument> corpus) : base(corpus) { }

    public IDictionary<string, IWeight> CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);

        Dictionary<string, IWeight> query = new Dictionary<string, IWeight>();
        int modalFrec = results.MaxBy(x => x.Item2).Item2;

        foreach ((string, int) item in results)
            query.Add(item.Item1, new QueryVSMWeight(item.Item2, modalFrec));
        return query;
    }
}

public class GVSMDocTerm : WMTermDoc, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public GVSMDocTerm(IEnumerable<IDocument> corpus) : base(corpus) => Storage = new GVSMStorageDT(corpus);

    public SearchItem[] GetSearchItems(double[] query, int snippetLen)
    {
        ((GVSMStorageDT)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0;
        var result = new SearchItem[Storage.Count];

        foreach (var item in ((GVSMStorageDT)Storage))
        {
            result[count++] = new SearchItem(item, snippetLen, SimilarityRate(query, ((GVSMStorageDT)Storage)![item]));
        }
        return result;
    }

    protected double SimilarityRate(double[] doc1, double[] doc2)
    {
        double normaDoc1 = 0, normaDoc2 = 0, scalarMul = 0;
        for (int i = 0; i < doc1.Length; i++)
        {
            normaDoc1 += Math.Pow(doc1[i], 2);
            scalarMul += doc1[i] * doc2[i];
        }
        normaDoc1 = Math.Sqrt(normaDoc1);

        for (int i = 0; i < doc2.Length; i++)
        {
            normaDoc2 += Math.Pow(doc2[i], 2);
        }
        normaDoc2 = Math.Sqrt(normaDoc2);

        return (normaDoc1 != 0 && normaDoc2 != 0) ? scalarMul / (normaDoc1 * normaDoc2) : 0;
    }

    public double[] CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);
        double[] temp = new double[(Storage as GVSMStorageDT)!.DocsLength];
        foreach ((string, int) item1 in results)
        {
            if (!(Storage as GVSMStorageDT)!.ContainsKey(item1.Item1)) continue;
            foreach (var item2 in (Storage as GVSMStorageDT)!.GetKey1Vector(item1.Item1))
            {
                temp[item2.Key] += item2.Value;
            }
        }
        return temp;
    }
}

public class BSMTermDoc : WMTermDoc, ISRIModel<string, string, string, int, IDocument>, ICollection<IDocument>
{
    private static BooleanNode root;

    public BSMTermDoc(IEnumerable<IDocument> corpus) : base(corpus) { }
    private static void create_query_ast(List<TokenLexem> tokenized)
    {
        BSMTermDoc.root = null;
        IEnumerator<TokenLexem> t = tokenized.GetEnumerator();
        t.MoveNext();
        S(t, ref BSMTermDoc.root);

        void S(IEnumerator<TokenLexem> tokenized, ref BooleanNode current)
        {

            switch (tokenized.Current)
            {
                case TokenLexem.opar:
                case TokenLexem.word:
                    {
                        P(tokenized, ref current);
                        X(tokenized, ref current);
                        break;
                    }
                case TokenLexem.not:
                    {
                        if (tokenized.MoveNext())
                        {

                            BooleanNode not = new NotNode();
                            if (current != null) current.add_child(not);
                            else current = not;

                            if (tokenized.Current == TokenLexem.opar)
                            {
                                P(tokenized, ref not);
                                X(tokenized, ref not);
                            }
                            else
                            {
                                P(tokenized, ref current);
                                X(tokenized, ref current);
                            }

                        }//! bad EOF            
                        break;
                    }
                default:
                    {
                        //! unexpected token

                        break;
                    }
            }
        }

        void P(IEnumerator<TokenLexem> tokenized, ref BooleanNode current)
        {
            switch (tokenized.Current)
            {
                case TokenLexem.opar:
                    {
                        if (tokenized.MoveNext())
                        {
                            BooleanNode temp = null;
                            S(tokenized, ref temp);
                            if (current != null) current.add_child(temp);
                            else current = temp;

                            if (tokenized.Current != TokenLexem.cpar) { } //! missing cpar
                            tokenized.MoveNext();

                        }//! bad EOF
                        break;
                    }
                case TokenLexem.word:
                    {
                        if (current != null) current.add_child(new WordNode());
                        else current = new WordNode();
                        tokenized.MoveNext();
                        break;
                    }
                default:
                    {
                        //! notificar error de query
                        break;
                    }
            }
        }

        void X(IEnumerator<TokenLexem> tokenized, ref BooleanNode current)
        {
            BooleanNode temp;
            switch (tokenized.Current)
            {
                case TokenLexem.or:
                    {
                        temp = new OrNode();
                        break;
                    }
                case TokenLexem.and:
                    {
                        temp = new AndNode();
                        break;
                    }
                case TokenLexem.xor:
                    {
                        temp = new XorNode();
                        break;
                    }
                case TokenLexem.implies:
                    {
                        temp = new ImplicationNode();
                        break;
                    }
                case TokenLexem.double_implies:
                    {
                        temp = new DoubleImplicationNode();
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
            tokenized.MoveNext();
            if (current != null) temp.add_child(current);
            current = temp;
            S(tokenized, ref current);
        }
    }

    public static bool evaluate(bool[] values, int index)
    {
        return evaluate(values, ref index, BSMTermDoc.root);
    }
    private static bool evaluate(bool[] values, ref int index, BooleanNode node)
    {
        bool evaluation = false;
        if (values.Length <= 0) return false;
        if (node == null) return values[0];
        if (node is BinaryNode)
        {
            BinaryNode n2 = (BinaryNode)node;
            switch (n2.childs.Count)
            {
                case 0:
                case 1:
                    {
                        //! exception bad format
                        break;
                    }
                default:
                    {
                        evaluation = n2.evaluate(evaluate(values, ref index, (BooleanNode)n2.childs[0]), evaluate(values, ref index, (BooleanNode)n2.childs[1]));
                        break;
                    }
            }
        }
        else
        {
            UnaryNode n1 = (UnaryNode)node;
            switch (n1.childs.Count)
            {
                case 0:
                    {
                        evaluation = n1.evaluate(values[index++]);
                        break;
                    }
                default:
                    {
                        evaluation = n1.evaluate(evaluate(values, ref index, (BooleanNode)n1.childs[0]));
                        break;
                    }
            }
        }
        return evaluation;
    }

    public IDictionary<int, string> CreateQuery(IEnumerable<char> docs)
    {
        Utils.Porter2 porter_stem = new Utils.Porter2();
        Dictionary<int, string> query = new Dictionary<int, string>();
        List<TokenLexem> tokenized = new List<TokenLexem>();

        string token = "";
        int count = 0;
        foreach (char c in docs)
        {
            if (c == '<')
            {
                if (!string.IsNullOrEmpty(token))
                {
                    query.Add(count++, porter_stem.stem(token.ToLower()));
                    tokenized.Add(TokenLexem.word);
                }
                token = "<";
            }
            else if (c == '=')
            {
                if (token == "<") token = "<=";
                else if (!string.IsNullOrEmpty(token))
                {
                    query.Add(count++, porter_stem.stem(token.ToLower()));
                    tokenized.Add(TokenLexem.word);
                    token = "=";
                }
            }

            else if (char.IsLetter(c) || char.IsNumber(c)) token += c.ToString();
            else
            {
                if (c == '>')
                {
                    if (token == "=") tokenized.Add(TokenLexem.implies);
                    else if (token == "<=") tokenized.Add(TokenLexem.double_implies);
                    token = "";
                }
                if (!string.IsNullOrEmpty(token)) { query.Add(count++, porter_stem.stem(token.ToLower())); tokenized.Add(TokenLexem.word); token = ""; }
                if (c == '&') tokenized.Add(TokenLexem.and);
                if (c == '|') tokenized.Add(TokenLexem.or);
                if (c == '^') tokenized.Add(TokenLexem.xor);
                if (c == '!') tokenized.Add(TokenLexem.not);
                if (c == '(') tokenized.Add(TokenLexem.opar);
                if (c == ')') tokenized.Add(TokenLexem.cpar);
            }

        }
        if (!string.IsNullOrEmpty(token)) { query.Add(count++, porter_stem.stem(token.ToLower())); tokenized.Add(TokenLexem.word); }
        tokenized.Add(TokenLexem.EOF);
        BSMTermDoc.create_query_ast(tokenized);
        return query;
    }
    public SearchItem[] GetSearchItems(IDictionary<int, string> query, int snippetLen = 30)
    {
        ((VSMStorageTD)Storage!).UpdateDocs(); /*analizar si es null*/
        SearchItem[] result = new SearchItem[Storage.Count];
        bool[][] score = new bool[Storage.Count][];
        int index = 0;
        foreach (var item in query)
        {
            if (Utils.Utils.GetStopWords().Contains(item.Value))
            {
                for (int i = 0; i < score.Length; i++)
                {
                    if (score[i] == null) score[i] = new bool[query.Count];
                    score[i][index] = true;
                }
            }
            else
            {
                if (!((VSMStorageTD)Storage!).ContainsKey(item.Value)) continue;
                foreach (var item1 in ((VSMStorageTD)Storage!)[item.Value])
                {
                    var value = (Storage as VSMStorageTD)!.DocsFrecModal[item1.Key].Item1;
                    if (score[value] == null) score[value] = new bool[query.Count];
                    score[value][index] = true;
                }
            }
            index++;
        }
        index = 0;
        foreach (var item in ((VSMStorageTD)Storage).GetAllDocs())
        {
            result[index] = new SearchItem(item.Item1, snippetLen, (evaluate(score[index++] != null ? score[index - 1] : new bool[query.Count], 0) ? 1 : 0));
        }
        return result;
    }

    public double SimilarityRate(IDictionary<string, string> doc1, IDictionary<string, string> doc2)
    {
        throw new NotImplementedException();
    }

    public static string ConvertToBooleanQuery(string query)
    {
        var procesed = new ProcesedDocument(query).GetEnumerator();
        procesed.MoveNext();

        string result = procesed.Current.Item1;
        for (; procesed.MoveNext(); result += "&" + procesed.Current) ;
        return result;
    }
}
