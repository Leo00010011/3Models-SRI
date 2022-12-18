using DP.Interface;
using Utils;
using System.Collections;
namespace DP;

#nullable disable

public enum stateDoc
{
    changed,
    deleted,
    notchanged
}

public class LazyKMP : ILazyMatcher
{

    public int IndexToMatch
    {
        get
        {
            return indexToMatch;
        }
    }

    public string Pattern
    {
        get
        {
            return pattern;
        }
    }

    public bool AtFinalState
    {
        get => indexToMatch == pattern.Length;
    }

    private int indexToMatch;

    readonly private string pattern;

    readonly int[] pi;

    public LazyKMP(string pattern)
    {
        this.pattern = pattern;
        pi = ComputePrefixFunction(pattern);
    }

    public static int[] ComputePrefixFunction(string pattern)
    {
        int[] pi = new int[pattern.Length];
        int k = 0;
        for (int i = 1; i < pattern.Length; i++)
        {
            while (k > 0 && pattern[k] != pattern[i])
                k = pi[k - 1];
            if (pattern[k] == pattern[i])
                k++;
            pi[i] = k;
        }
        return pi;
    }

    public bool MatchStep(char step)
    {
        if (indexToMatch == pattern.Length)
            indexToMatch = 0;

        bool result = false;
        while (indexToMatch > 0 && pattern[indexToMatch] != step)
            indexToMatch = pi[indexToMatch - 1];
        if (pattern[indexToMatch] == step)
            result = true;
        indexToMatch++;

        return result;
    }

    public bool PeekStep(char step)
    {
        bool result = false;
        int temp = indexToMatch;
        if (indexToMatch == pattern.Length)
            temp = 0;
        while (temp > 0 && pattern[temp] != step)
            temp = pi[temp - 1];
        if (pattern[indexToMatch] == step)
            result = true;
        return result;
    }

    public bool Match(IEnumerable<char> text)
    {
        bool last = false;
        foreach (var step in text)
        {
            last = this.MatchStep(step);
        }
        indexToMatch = 0;
        return AtFinalState;

    }

    public bool Match(string text)
    {
        if (text.Length < pattern.Length)
            return false;
        return this.Match((IEnumerable<char>)text);
    }

    public ILazyMatcher CloneParsing()
    {
        var result = new LazyKMP(pattern);
        result.indexToMatch = indexToMatch;
        return result;
    }

    public void ResetState()
    {
        indexToMatch = 0;
    }
}

public class ConsecutiveNumberMatcher : ILazyMatcher
{
    public bool AtFinalState
    {
        get;
        private set;
    }

    public ILazyMatcher CloneParsing()
    {
        return new ConsecutiveNumberMatcher();
    }

    public bool Match(IEnumerable<char> text)
    {
        foreach (char item in text)
        {
            if (!Char.IsDigit(item))
            {
                return false;
            }
        }
        return true;
    }

    public bool Match(string text)
    {
        return Match((IEnumerable<char>)text);
    }

    public bool MatchStep(char step)
    {
        AtFinalState = Char.IsDigit(step);
        return AtFinalState;
    }

    public bool PeekStep(char step)
    {
        return Char.IsDigit(step);
    }

    public void ResetState()
    {
        AtFinalState = false;
    }

}

public class EndCranMatcherCreator : ICreator<ILazyMatcher>
{
    public ILazyMatcher Create()
    {
        return new EndCranMatcher();
    }
}

public class EndReutersMatcherCreator : ICreator<ILazyMatcher>
{
    public ILazyMatcher Create()
    {
        return new LazyKMP("</REUTERS>");
    }
}

public class EndCranMatcher : ILazyMatcher
{
    public enum State
    {
        firstPattern,
        secondPattern,
        thirdPattern
    }

    State state = State.firstPattern;
    readonly string firstPattern = "\n.I ";
    readonly char thirdPattern = '\n';

    private int indexToMatch = 0;

    public bool AtFinalState
    {
        get;
        private set;
    }


    public bool Match(string text)
    {
        if (text.Length < 4)
        {
            return false;
        }
        return Match((IEnumerable<char>)text);
    }

    public bool Match(IEnumerable<char> text)
    {
        indexToMatch = 0;
        state = State.firstPattern;
        AtFinalState = false;
        foreach (char item in text)
        {
            MatchStep(item);
        }
        bool result = AtFinalState;
        indexToMatch = 0;
        state = State.firstPattern;
        AtFinalState = false;
        return result;
    }

    public bool PeekStep(char step)
    {
        switch (state)
        {
            case State.firstPattern:
                return PeekFirstPattern(step);
            case State.secondPattern:
                return PeekSecondPattern(step);
            case State.thirdPattern:
                return PeekThirdPattern(thirdPattern);
        }
        return false;
    }
    public bool MatchStep(char step)
    {
        switch (state)
        {
            case State.firstPattern:
                return MatchFirstPattern(step);
            case State.secondPattern:
                return MatchSecondPattern(step);
            case State.thirdPattern:
                return MatchThirdPattern(thirdPattern);
        }
        return false;
    }

    private bool PeekFirstPattern(char step)
    {
        return firstPattern[indexToMatch] == step;
    }

    private bool MatchFirstPattern(char step)
    {
        AtFinalState = false;

        if (firstPattern[indexToMatch] == step)
        {
            indexToMatch++;
            if (indexToMatch == firstPattern.Length)
            {
                state = State.secondPattern;
                indexToMatch = 0;
            }
            return true;
        }
        else
        {
            indexToMatch = 0;
            return false;
        }
    }

    private bool PeekSecondPattern(char step)
    {
        return Char.IsDigit(step);
    }

    private bool MatchSecondPattern(char step)
    {
        AtFinalState = false;
        if (Char.IsDigit(step))
        {
            return true;
        }
        else
        {
            if (PeekThirdPattern(step))
            {
                state = State.thirdPattern;
                indexToMatch = 0;
                return MatchStep(step);
            }
            else
            {
                state = State.firstPattern;
                indexToMatch = 0;
                return MatchStep(step);
            }
        }
    }

    private bool PeekThirdPattern(char step)
    {
        return thirdPattern == step;
    }

    private bool MatchThirdPattern(char step)
    {
        AtFinalState = false;
        if (thirdPattern == step)
        {
            indexToMatch = 0;
            state = State.firstPattern;
            AtFinalState = true;
            return true;
        }
        else
        {
            state = State.firstPattern;
            indexToMatch = 0;
            return MatchStep(step);
        }
    }

    public ILazyMatcher CloneParsing()
    {
        var result = new EndCranMatcher();
        result.state = state;
        result.AtFinalState = AtFinalState;
        result.indexToMatch = 0;
        return result;
    }

    public void ResetState()
    {
        state = State.firstPattern;
        AtFinalState = false;
        indexToMatch = 0;
    }
}

public class CranJsonDocument : IDocument, IComparable
{
    string doc_id;
    string title;
    string text;
    DateTime modifiedDateTime;

    public CranJsonDocument(string doc_id, string title, string text)
    {
        this.doc_id = doc_id;
        this.title = title;
        this.text = text;
    }

    public string Id => doc_id;

    public IEnumerable<char> Name => title;

    public IEnumerable<char> GetSnippet(int snippetLen) => text.Take(snippetLen);

    public virtual stateDoc GetState()
    {
        return (DateTime.Equals(modifiedDateTime, File.GetLastWriteTime(@".\contents\docs_save"))) ? stateDoc.notchanged : stateDoc.changed;
    }

    public virtual void UpdateDateTime() => modifiedDateTime = File.GetLastWriteTime(@".\contents\docs_save");

    public override bool Equals(object? obj) => obj is CranJsonDocument document && modifiedDateTime == document.modifiedDateTime && Id == document.Id;
    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? obj) => obj is CranJsonDocument document ? document.Id.CompareTo(Id) : throw new InvalidCastException();

    public IEnumerator<char> GetEnumerator() => (title as IEnumerable<char>).Concat(text).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public string GetDocText() => text;
}

public class Document : IDocument, IComparable
{
    public virtual IEnumerable<char> Name
    {
        get
        {
            StreamReader reader = new StreamReader(path);
            if (info is null) info = parser(GetChars(reader));
            reader.Dispose();
            reader = new StreamReader(path);
            

            foreach (var item in GetChars(reader).Skip(info.TitleInit).Take(info.TitleLen))
                yield return item;
            reader.Dispose();
            reader.Close();
        }
    }

    public virtual string Id
    {
        get;
        protected set;
    }

    protected string path;
    private ParsedInfo? info;
    private DateTime modifiedDateTime;
    private Func<IEnumerable<char>, ParsedInfo> parser;

    

    public Document(string id, Func<IEnumerable<char>, ParsedInfo> parser)
    {
        Id = id;
        path = id;
        modifiedDateTime = default(DateTime);
        this.parser = parser;
    }

    public static IEnumerable<char> GetChars(StreamReader reader)
    {
        while (!reader.EndOfStream)
            yield return (char)reader.Read();
    }

    protected virtual IEnumerable<char> GetEnumerable()
    {
        StreamReader reader = new StreamReader(path);
        foreach (var item in GetChars(reader))
            yield return item;
        reader.Dispose();
        reader.Close();
    }

    public virtual IEnumerable<char> GetSnippet(int snippetLen)
    {
        StreamReader reader = new StreamReader(path);
        if (info is null) info = parser(GetChars(reader));
        reader.Dispose();
        reader = new StreamReader(path);
        int infoSnippetLen = info.SnippetLen < 0 ? int.MaxValue : info.SnippetLen;
        foreach (var item in GetChars(reader).Skip(info.SnippetInit).Take(Math.Min(infoSnippetLen, snippetLen)))
            yield return item;
        reader.Dispose();
        reader.Close();
    }

    public virtual stateDoc GetState()
    {
        if (File.Exists(path)) return (DateTime.Equals(modifiedDateTime, File.GetLastWriteTime(path))) ? stateDoc.notchanged : stateDoc.changed;
        return stateDoc.deleted;
    }

    public virtual void  UpdateDateTime() => modifiedDateTime = File.GetLastWriteTime(path);

    public virtual IEnumerator<char> GetEnumerator() => GetEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public override bool Equals(object? obj) => obj is Document document && modifiedDateTime == document.modifiedDateTime && Id == document.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? obj)
    {
        return obj is Document document ? document.Id.CompareTo(Id) : throw new InvalidCastException();
    }

    public virtual string GetDocText()
    {
        string result;
        using(StreamReader sr = new StreamReader(File.Open(path,FileMode.Open)))
        {
            result = sr.ReadToEnd();
        }
        return result;
    }
}

public class CollectionSplitter : IEnumerable<IDocument>, IDisposable
{

    class CollectionSplitterEnumerator : IEnumerator<IDocument>
    {
        public IDocument Current => current.Value;

        int index = 1;

        bool disposed = false;

        LinkedListNode<EmbebedDocument> current = null;

        object IEnumerator.Current => this.Current;
        
        CollectionSplitter enumerable;

        public CollectionSplitterEnumerator(CollectionSplitter enumerable)
        {
            this.enumerable = enumerable;
        }

        public void Dispose()
        {
            if(disposed)
                return;
            enumerable.streamUsed = false;
            enumerable = null;
            disposed = true;
        }

        public bool MoveNext()
        {
            if(disposed)
                return false;
            
            if(current == null ||(!current.Value.EndOfFileReached && Utils.Utils.Peek(enumerable.stream) != -1 && (enumerable.parsedCompleted || current.Value.EndReached)))
            {

                if(index <= enumerable.createdDoc.Count)
                {
                    if(current == null)
                    {
                        current = enumerable.createdDoc.First;
                    }
                    else
                    {
                        current = current.Next;
                    }
                    current.Value.Reset();
                }
                else
                {
                    EmbebedDocument preview = null;
                    if(current != null)
                        preview = current.Value;
                    current = new LinkedListNode<EmbebedDocument>(new EmbebedDocument(enumerable.collectionPath,index,enumerable.stream,enumerable.parser,enumerable.endMatcherCreator,enumerable,preview));
                    enumerable.createdDoc.AddLast(current);
                }
                index++;
                return true;
            }
            else
            {
                if(!(enumerable.parsedCompleted || current.Value.EndReached))
                    throw new Exception("Tienes que leer el documento anterior hasta el final");
                //Solo se llega aquí si current.EndOfFileReached es true
                enumerable.parsedCompleted = true;
                Dispose();
                return false;
            }
        }

        public void Reset()
        {
            throw new InvalidOperationException("No se puede resetear este enumerator");
        }
    }

    
    string collectionPath;

    public bool Disposed
    {
        get; 
        private set;
    }

    bool streamUsed = false;

    bool parsedCompleted = false;

    Func<IEnumerable<char>, ParsedInfo> parser;

    ICreator<ILazyMatcher> endMatcherCreator;

    LinkedList<EmbebedDocument> createdDoc = new LinkedList<EmbebedDocument>();

    BufferedStream stream;
    public CollectionSplitter(string collectionPath, ICreator<ILazyMatcher> endMatcherCreator, Func<IEnumerable<char>, ParsedInfo> parser)
    {
        this.collectionPath = collectionPath;
        this.endMatcherCreator = endMatcherCreator;
        this.parser = parser;
        stream = new BufferedStream(File.Open(collectionPath,FileMode.Open));
    }

    public IEnumerator<IDocument> GetEnumerator()
    {
        if(Disposed)
        {
            stream = new BufferedStream(File.Open(collectionPath, FileMode.Open));
            Disposed = false;
        }  
        if(!streamUsed)
        {
            streamUsed = true;
            stream.Seek(0,SeekOrigin.Begin);
            return new CollectionSplitterEnumerator(this);
        }
        else
            throw new InvalidOperationException("Otro enumerator está usando este stream");
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public void Dispose()
    {
        if(!Disposed)
        {
            stream.Dispose();
            streamUsed = false;
            stream = null;
            Disposed = true;
        }
    }
}

public class EmbebedDocument : Document
{
    class EmbebedDocumentEnumerator : IEnumerator<char>
    {
        public char Current
        {
            get;
            private set;
        }

        object IEnumerator.Current => Current;

        bool lastReached = false;

        long count = 0;

        EmbebedDocument enumerable;

        ParsedInfo info = null;

        ILazyMatcher matcher;

        public EmbebedDocumentEnumerator(EmbebedDocument enumerable)
        {
            this.enumerable = enumerable;
            matcher = enumerable.endMatcherCreator.Create();
        }

        public bool MoveNext()
        {
            if(lastReached)
            {
                return false;
            }
                
            int item = enumerable.stream.ReadByte();

            if(item == -1)
            {
                enumerable.length = count;
                enumerable.EndOfFileReached = true;
                enumerable.EndReached = true;
                return false;
            }
            Current = (char)item;
            matcher.MatchStep(Current);
            count++;
            if(matcher.AtFinalState)
            {
                enumerable.length = count;
                lastReached = true;
                enumerable.EndReached = true;
                Current = (char)item;
            }
            return true;
        }

        public void Reset()
        {
            throw new InvalidOperationException("Cannot do Reset in this enumerator");
        }

        public void Dispose()
        {
            enumerable.enumeratorSended = false;
            enumerable = null;
            matcher = null;
        }
    }

    public override IEnumerable<char> Name// => throw new Exception();
    {
        get
        {
            var streamInfo = GetLocalStream();
            BufferedStream localStream = streamInfo.Item1;
            bool openedHere = streamInfo.Item2;
            long prevPos = streamInfo.Item3;
            
            if(info == null)
                info = parser(Utils.Utils.StreamToEnumerable(localStream));
            localStream.Seek(initPos,SeekOrigin.Begin);

            foreach (var item in Utils.Utils.StreamToEnumerable(localStream).Skip(info.TitleInit).Take(info.TitleLen))
                yield return item;
            
            DevolverStream(localStream,openedHere,prevPos);
        }
    }

    public override IEnumerable<char> GetSnippet(int snippetLen)
    {
        
        var streamInfo = GetLocalStream();
        BufferedStream localStream = streamInfo.Item1;
        bool openedHere = streamInfo.Item2;
        long prevPos = streamInfo.Item3;
        
        if(info == null)
            info = parser(Utils.Utils.StreamToEnumerable(localStream));
        localStream.Seek(initPos,SeekOrigin.Begin);
        
        int infoSnippetLen = info.SnippetLen < 0 ? int.MaxValue : info.SnippetLen;
        foreach (var item in Utils.Utils.StreamToEnumerable(localStream).Skip(info.SnippetInit).Take(Math.Min(infoSnippetLen, snippetLen)))
            yield return item;
        
        DevolverStream(localStream,openedHere,prevPos);
    }
    
    public bool EndReached
    {
        get;
        private set;
    }

    public bool EndOfFileReached
    {
        get;
        private set;
    }

    bool enumeratorSended = false;
    
    public long InitPos
    {
        get => initPos;
    }

    public long Length
    {
        get => length;
    }

    long initPos = -1;

    long length = -1;

    EmbebedDocument preview = null;

    ParsedInfo info = null;

    int index;

    BufferedStream stream;

    ICreator<ILazyMatcher> endMatcherCreator;

    Func<IEnumerable<char>, ParsedInfo> parser;

    CollectionSplitter splitter;

    public EmbebedDocument(string id,int index,BufferedStream stream, Func<IEnumerable<char>, ParsedInfo> parser, ICreator<ILazyMatcher> endMatcher, CollectionSplitter splitter, EmbebedDocument preview) : base(id, parser)
    {
        this.index = index;
        this.preview = preview;
        this.splitter = splitter;
        path = id;
        Id = path + "\\" + index;
        this.stream = stream;
        this.initPos = stream.Position;
        this.endMatcherCreator = endMatcher;
        this.parser = parser;
    }

    (BufferedStream,bool,long) GetLocalStream()
    {
        bool openedHere = false;
        BufferedStream localStream;
        long prevPos = -1;
        if(splitter.Disposed)
        {
            openedHere = true;
            localStream = new BufferedStream(File.Open(path,FileMode.Open));
        }
        else
        {
            prevPos = stream.UnderlyingStream.Position;
            stream.UnderlyingStream.Position = 0;
            localStream = new BufferedStream(stream.UnderlyingStream);
        }
        localStream.Seek(initPos,SeekOrigin.Begin);

        return (localStream,openedHere,prevPos);
    }

    public void Reset()
    {
        EndReached = false;
    }

    void DevolverStream(BufferedStream localStream, bool openedHere, long prevPos)
    {
        if(openedHere)
        {
            localStream.Dispose();
        }
        else
        {
            localStream.Flush();
            localStream.UnderlyingStream.Position = prevPos;
        }
    }
    public override string GetDocText()
    {
        string result;
        var streamInfo = GetLocalStream();
        BufferedStream localStream = streamInfo.Item1;
        bool openedHere = streamInfo.Item2;
        long prevPos = streamInfo.Item3;
        byte[] arr = new byte[length];
        localStream.Read(arr,0,(int)length);
        result = String.Concat(arr.Select((c,index) => (char)c));
        DevolverStream(localStream,openedHere,prevPos);
        return result;

    }
    public override  IEnumerator<char> GetEnumerator()
    {
        if(!enumeratorSended && ( preview == null || preview.EndReached ))
        {
            enumeratorSended = true;
            return new EmbebedDocumentEnumerator(this);
        }
        else
        {
            if(enumeratorSended)
                throw new InvalidOperationException("Solo se puede pedir un enumerator");
            else
                throw new InvalidOperationException("Hay que leer el documento anterior(" + preview.index + ") hasta el final para poder leer este");
        }
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
        Utils.Porter2 porter_stem = new Utils.Porter2();
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

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    
}