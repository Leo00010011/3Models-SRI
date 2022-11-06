namespace DP;
using DP.Interface;
using Utils;
using System.Collections;

public enum stateDoc
{
    changed,
    deleted,
    notchanged
}

public class Document : IDocument
{
    private int maxSnippetSize;
    private DateTime modifiedDateTime;

    public Document(string id, int maxSnippetSize = 30)
    {
        Id = id;
        this.maxSnippetSize = maxSnippetSize;
        modifiedDateTime = default(DateTime);
        Name = id.Reverse().TakeWhile(x => x != '\\').Reverse();
    }

    public string Id { get; private set; }

    public IEnumerable<char> Name { get; private set; }

    public int ModalFrec { get; set; }

    private IEnumerable<char> GetChars(StreamReader reader)
    {
        while (!reader.EndOfStream)
            yield return char.ToLower((char)reader.Read());
    }

    private IEnumerable<char> GetEnumerable()
    {
        StreamReader reader = new StreamReader(Id);
        foreach (var item in GetChars(reader))
            yield return item;
        reader.Close();
    }

    public IEnumerable<char> GetSnippet()
    {
        StreamReader reader = new StreamReader(Id);
        foreach (var item in GetChars(reader).Take(maxSnippetSize))
            yield return item;
        reader.Close();
    }

    public stateDoc GetState()
    {
        try 
        { 
            return (DateTime.Equals(modifiedDateTime, File.GetLastWriteTime(Id))) ? stateDoc.notchanged : stateDoc.changed; 
        } catch { }
        return stateDoc.deleted;
    }

    public void UpdateDateTime() => modifiedDateTime = File.GetLastWriteTime(Id);

    public IEnumerator<char> GetEnumerator() => GetEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerable().GetEnumerator();
}

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