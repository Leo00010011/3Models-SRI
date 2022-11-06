namespace DP;

using Utils;
using System.Collections;
using DP.Interface;

public class ProcesedDocument : IResult<IEnumerable<char>, string, int>
{
    Dictionary<string, int>? frecs;

    public int this[string index] => getFrecs()[index];

    public int Length => getFrecs().Count;

    public IEnumerable<char> Result
    {
        get;
        private set;
    }

    public ProcesedDocument(IEnumerable<char> rawDoc)
    {
        Result = rawDoc;
    }

    Dictionary<string, int> getFrecs()
    {
        if (frecs == null)
            CompFrecs();
        return frecs;
    }

    void CompFrecs()
    {
        frecs = new Dictionary<string, int>();

        foreach (string item in Utils.GetTerms(Result))
        {
            if (!Utils.GetStopWords().Contains(item))
            {
                int val;
                if (frecs.TryGetValue(item, out val))
                {
                    frecs[item] = val + 1;
                }
                else
                {
                    frecs.Add(item, 1);
                }
            }

        }
    }

    public IEnumerator<(string, int)> GetEnumerator()
    {
        return getFrecs().Select(x => (x.Key, x.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return getFrecs().Select(x => (x.Key, x.Value)).GetEnumerator();
    }
}