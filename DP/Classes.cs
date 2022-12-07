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

public class Document : IDocument, IComparable
{
    private ParsedInfo? info;
    private DateTime modifiedDateTime;
    private Func<IEnumerable<char>, ParsedInfo> parser;

    public Document(string id, Func<IEnumerable<char>, ParsedInfo> parser)
    {
        Id = id;
        modifiedDateTime = default(DateTime);
        this.parser = parser;
    }

    public string Id { get; private set; }

    public IEnumerable<char> Name
    {
        get
        {
            if (info is null) info = parser(this);
            StreamReader reader = new StreamReader(Id);
            foreach (var item in GetChars(reader).Skip(info.TitleInit).Take(info.TitleLen))
                yield return item;
            reader.Close();
        }
    }

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

    public IEnumerable<char> GetSnippet(int snippetLen)
    {
        if (info is null) info = parser(this);
        StreamReader reader = new StreamReader(Id);
        int infoSnippetLen = info.SnippetLen < 0 ? int.MaxValue : info.SnippetLen;
        foreach (var item in GetChars(reader).Skip(info.SnippetInit).Take(Math.Min(infoSnippetLen, snippetLen)))
            yield return item;
        reader.Close();
    }

    public stateDoc GetState()
    {
        if (File.Exists(Id)) return (DateTime.Equals(modifiedDateTime, File.GetLastWriteTime(Id))) ? stateDoc.notchanged : stateDoc.changed;
        return stateDoc.deleted;
    }

    public void UpdateDateTime() => modifiedDateTime = File.GetLastWriteTime(Id);

    public IEnumerator<char> GetEnumerator() => GetEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerable().GetEnumerator();

    public override bool Equals(object? obj) => obj is Document document && modifiedDateTime == document.modifiedDateTime && Id == document.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? obj)
    {
        return obj is Document document ? document.Id.CompareTo(Id) : throw new InvalidCastException();
    }
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