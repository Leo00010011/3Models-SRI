using System.Collections;
using DP.Interface;

public class Document : IDocument
{
    private IEnumerable<char> snippet;

    public Document(string id, IEnumerable<char> snippet)
    {
        Id = id;
        Name = id.Reverse().TakeWhile(x => x != '\\').Reverse();
        ModifiedDateTime = File.GetLastWriteTime(id);
        this.snippet = snippet;
    }

    public string Id { get; private set; }

    public IEnumerable<char> Name { get; private set; }

    public DateTime ModifiedDateTime { get; private set; }

    public IEnumerator<char> GetEnumerator() => snippet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => snippet.GetEnumerator();
}