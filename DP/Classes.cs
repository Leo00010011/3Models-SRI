using DP.Interface;
using Utils;
using System.Collections;
namespace DP;


public enum stateDoc
{
    changed,
    deleted,
    notchanged
}

public class Document : IDocument
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

    public override bool Equals(object? obj)
    {
        return obj is Document document &&
               modifiedDateTime == document.modifiedDateTime &&
               Id == document.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
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
        Utils.Porter2 porter_stem =  new Utils.Porter2();   
        frecs = new Dictionary<string, int>();

        foreach (string item in Utils.Utils.GetTerms(Result))
        {   
            string item1 = item.ToLower();
            if (!Utils.Utils.GetStopWords().Contains(item1))
            {
                item1 = porter_stem.stem(item1);
                int val;
                
                if (frecs.TryGetValue(item1, out val))
                {
                    frecs[item1] = val + 1;
                }
                else
                {
                    frecs.Add(item1, 1);
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