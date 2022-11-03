using System.Collections;
using DP.Interface;

public class Document : IDocument
{
    private int maxSnippetSize;

    public Document(string id, int maxSnippetSize = 30)
    {
        Id = id;
        this.maxSnippetSize = maxSnippetSize;
        ModifiedDateTime = File.GetLastWriteTime(id);
        Name = id.Reverse().TakeWhile(x => x != '\\').Reverse();
    }

    public string Id { get; private set; }

    public IEnumerable<char> Name { get; private set; }

    public DateTime ModifiedDateTime { get; private set; }

    private IEnumerable<char> GetChars(StreamReader reader)
    {
        while(!reader.EndOfStream)
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

    public IEnumerator<char> GetEnumerator() => GetEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerable().GetEnumerator();
}