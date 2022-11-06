namespace DP;

using System.Collections;
using DP.Interface;

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