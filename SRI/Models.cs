namespace SRI;

using System.Collections;
using DP;
using DP.Interface;
using SRI.Interface;

public abstract class SRIModel<T1, T2, V, Q, D> : ISRIModel<T1, T2, V, Q, D>, ICollection<D> where T1 : notnull where T2 : notnull where Q : notnull
{
    protected virtual ICollection<D>? Storage { get; set; }

    public bool IsReadOnly => true;

    public int Count => Storage!.Count;

    public abstract SearchItem[] GetSearchItems(ISRIVector<Q, V> query, int snippetLen);

    public virtual SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();
    public abstract double SimilarityRate(ISRIVector<T2, V> doc1, ISRIVector<T2, V> doc2);

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
    public override double SimilarityRate(ISRIVector<T2, IWeight> doc1, ISRIVector<T2, IWeight> doc2)
    {
        double normaDoc1 = 0, scalarMul = 0;
        foreach (var item1 in doc1)
        {
            var value1 = item1.Item2.Weight;
            normaDoc1 += Math.Pow(value1, 2);

            if (doc2.ContainsKey(item1.Item1))
            {
                var item2 = doc2[item1.Item1];
                var value2 = item2.Weight;
                scalarMul += value1 * value2;
            }
        }
        normaDoc1 = Math.Sqrt(normaDoc1);

        double normaDoc2 = 0;
        foreach (var item in doc2)
        {
            var value = item.Item2.Weight;
            normaDoc2 += Math.Pow(value, 2);
        }
        normaDoc2 = Math.Sqrt(normaDoc2);

        return scalarMul / (normaDoc1 * normaDoc2);
    }

    public override SearchItem[] Ranking(SearchItem[] searchResult) => searchResult.OrderBy(x => x.Score).Reverse().ToArray();
}

public abstract class WMDocTerm : WModel<IDocument, string, IDocument>, ISRIModel<IDocument, string, IWeight, string, IDocument>, ICollection<IDocument>
{
    public WMDocTerm(IEnumerable<IDocument>? corpus = null) => Storage = new VSMStorageDT(corpus);

    public override SearchItem[] GetSearchItems(ISRIVector<string, IWeight> query, int snippetLen)
    {
        ((VSMStorageDT)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0; SearchItem[] result = new SearchItem[Storage.Count];
        foreach (var item in Storage)
            result[count++] = new SearchItem(item.Id, item.Name, item.GetSnippet(snippetLen), SimilarityRate(query, ((VSMStorageDT)Storage)[item]));
        return result;
    }
}

public class VSMDocTerm : WMDocTerm, ISRIModel<IDocument, string, IWeight, string, IDocument>, ICollection<IDocument>
{
    public VSMDocTerm(IEnumerable<IDocument>? corpus = null) : base(corpus) { }

    public ISRIVector<string, IWeight> CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);

        SRIVectorLinked<string, IWeight> query = new SRIVectorLinked<string, IWeight>();
        int modalFrec = results.MaxBy(x => x.Item2).Item2;

        foreach ((string, int) item in results)
            query.Add(item.Item1, new QueryVSMWeight(item.Item2, modalFrec));
        return query;
    }

}

public abstract class WMTermDoc : WModel<string, int, IDocument>, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public WMTermDoc(IEnumerable<IDocument>? corpus = null) => Storage = new VSMStorageTD(corpus);

    public override SearchItem[] GetSearchItems(ISRIVector<string, IWeight> query, int snippetLen)
    {
        ((VSMStorageTD)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0;
        SearchItem[] result = new SearchItem[Storage.Count];
        double[] score = new double[Storage.Count];

        double queryscore = 0;
        foreach (var item in query)
        {
            queryscore += Math.Pow(item.Item2.Weight, 2);
            foreach (var item1 in ((VSMStorageTD)Storage!)[item.Item1])
            {
                var index = ((VSMStorageTD)Storage!).DocsFrecModal[item1.Item1].Item1;
                score[index] += item.Item2.Weight * item1.Item2.Weight;
            }
        }
        queryscore = Math.Sqrt(queryscore);

        foreach (var item in ((VSMStorageTD)Storage).GetAllDocs())
        {
            result[count] = new SearchItem(item.Item1.Id, item.Item1.Name, item.Item1.GetSnippet(snippetLen), score[count++] / (queryscore * item.Item2));
        }
        return result;
    }
}

public class VSMTermDoc : WMTermDoc, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public VSMTermDoc(IEnumerable<IDocument>? corpus = null) : base(corpus) { }

    public static ISRIVector<string, IWeight> CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);

        SRIVectorLinked<string, IWeight> query = new SRIVectorLinked<string, IWeight>();
        int modalFrec = results.MaxBy(x => x.Item2).Item2;

        foreach ((string, int) item in results)
            query.Add(item.Item1, new QueryVSMWeight(item.Item2, modalFrec));
        return query;
    }
}

public class GVSMTermDoc : WMTermDoc, ISRIModel<string, int, IWeight, string, IDocument>, ICollection<IDocument>
{
    public GVSMTermDoc(IEnumerable<IDocument>? corpus = null) => Storage = new GVSMStorageDT(corpus);

    public SearchItem[] GetSearchItems(double[] query, int snippetLen)
    {
        ((GVSMStorageDT)Storage!).UpdateDocs(); /*analizar si es null*/ int count = 0;
        SearchItem[] result = new SearchItem[Storage.Count];

        foreach (var item in ((GVSMStorageDT)Storage))
        {
            result[count++] = new SearchItem(item.Id, item.Name, item.GetSnippet(snippetLen), SimilarityRate(query, (Storage as GVSMStorageDT)![item]));
        }
        return result;
    }

    protected double SimilarityRate(double[] doc1, double[] doc2)
    {
        double normaDoc1 = 0, normaDoc2 = 0, scalarMul = 0;
        foreach (var tuple in doc1.Zip(doc2))
        {
            normaDoc1 += Math.Pow(tuple.First, 2);
            normaDoc2 += Math.Pow(tuple.Second, 2);

            scalarMul += tuple.First * tuple.Second;
        }
        normaDoc1 = Math.Sqrt(normaDoc1);
        normaDoc2 = Math.Sqrt(normaDoc2);

        return scalarMul / (normaDoc1 * normaDoc2);
    }

    public double[] CreateQuery(IEnumerable<char> docs)
    {
        ProcesedDocument results = new ProcesedDocument(docs);
        var temp = (new double[(Storage as GVSMStorageDT)!.DocsLength] as IEnumerable<double>);
        foreach ((string, int) item in results)
            temp = temp.Zip((Storage as GVSMStorageDT)!.GetKey1Vector(item.Item1)).Select(x => x.First + x.Second);
        return temp.ToArray();
    }
}

public class BSMTermDoc : WMTermDoc, ISRIModel<string, string, int, string, IDocument>, ICollection<IDocument>
{
    private static BooleanNode root;

    public BSMTermDoc(IEnumerable<IDocument>? corpus = null) : base(corpus) { }
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
        bool evaluation;
        if (values.Length <= 0) return false;
        if (node == null) return values[0];
        if (node is BinaryNode)
        {
            BinaryNode n2 = (BinaryNode)node;
            switch (n2.childs.Count)
            {
                case 0:
                    {
                        evaluation = n2.evaluate(values[index++], values[index++]);
                        break;
                    }
                case 1:
                    {
                        evaluation = n2.evaluate(values[index++], evaluate(values, ref index, (BooleanNode)n2.childs[0]));
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

    public static ISRIVector<string, int> CreateQuery(IEnumerable<char> docs)
    {
        SRIVectorLinked<string, int> query = new SRIVectorLinked<string, int>();
        List<TokenLexem> tokenized = new List<TokenLexem>();

        string token = "";
        foreach (char c in docs)
        {
            if (c == '<')
            {
                if (!string.IsNullOrEmpty(token))
                {
                    query.Add(token, 1);
                    tokenized.Add(TokenLexem.word);
                }
                token = "<";
            }
            else if (c == '=')
            {
                if (token == "<") token = "<=";
                else if (!string.IsNullOrEmpty(token))
                {
                    query.Add(token, 1);
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
                if (!string.IsNullOrEmpty(token)) { query.Add(token, 1); tokenized.Add(TokenLexem.word); token = ""; }
                if (c == '&') tokenized.Add(TokenLexem.and);
                if (c == '|') tokenized.Add(TokenLexem.or);
                if (c == '^') tokenized.Add(TokenLexem.xor);
                if (c == '!') tokenized.Add(TokenLexem.not);
                if (c == '(') tokenized.Add(TokenLexem.opar);
                if (c == ')') tokenized.Add(TokenLexem.cpar);
            }

        }
        if (!string.IsNullOrEmpty(token)) { query.Add(token, 1); tokenized.Add(TokenLexem.word); }
        tokenized.Add(TokenLexem.EOF);
        foreach (var item in tokenized)
        {
            System.Console.WriteLine(item);
        }
        BSMTermDoc.create_query_ast(tokenized);
        return query;
    }
    public SearchItem[] GetSearchItems(ISRIVector<string, int> query, int snippetLen = 30)
    {
        ((VSMStorageTD)Storage!).UpdateDocs(); /*analizar si es null*/
        SearchItem[] result = new SearchItem[Storage.Count];
        bool[][] score = new bool[Storage.Count][];
        int index = 0;
        foreach (var item in query)
        {
            foreach (var item1 in ((VSMStorageTD)Storage!)[item.Item1])
            {
                var value = (Storage as VSMStorageTD)!.DocsFrecModal[item1.Item1].Item1;
                if (score[value] == null) score[value] = new bool[query.Count];
                score[value][index] = true;
            }
            index++;
        }
        index = 0;
        foreach (var item in ((VSMStorageTD)Storage).GetAllDocs())
        {
            result[index] = new SearchItem(item.Item1.Id, item.Item1.Name, item.Item1.GetSnippet(snippetLen), (evaluate(score[index++], 0) ? 1 : 0));
        }
        return result;
    }

    public double SimilarityRate(ISRIVector<string, int> doc1, ISRIVector<string, int> doc2)
    {
        throw new NotImplementedException();
    }
}
