namespace DP;
using DP.Interface;
using Utils;
using System.Collections;

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
            while(k > 0 && pattern[k] != pattern[i])
                k = pi[k - 1];
            if( pattern[k] == pattern[i])
                k++;
            pi[i] = k;
        }
        return pi;
    }

    public bool MatchStep(char step)
    {
        if(indexToMatch == pattern.Length)
            indexToMatch = 0;

        bool result = false;
        while(indexToMatch > 0 && pattern[indexToMatch] != step)
            indexToMatch = pi[indexToMatch - 1];
        if(pattern[indexToMatch] == step)
            result = true;
            indexToMatch++;
        
        return result;
    }

    public bool PeekStep(char step)
    {
        bool result = false;
        int temp = indexToMatch;
        if(indexToMatch == pattern.Length)
            temp = 0;
        while(temp > 0 && pattern[temp] != step)
            temp = pi[temp - 1];
        if(pattern[indexToMatch] == step)
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
        if(text.Length < pattern.Length)
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
        foreach(char item in text)
        {
            if(!Char.IsDigit(item))
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

public class EndCranMatcherCreator : ICreator<EndCranMatcher>
{
    public EndCranMatcher Create()
    {
        return new EndCranMatcher();
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
        if(text.Length < 4)
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
        foreach(char item in text)
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
        switch(state)
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
        switch(state)
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
        
        if(firstPattern[indexToMatch] == step)
        {
            indexToMatch++;
            if(indexToMatch == firstPattern.Length)
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
        if(Char.IsDigit(step))
        {
            return true;
        }
        else
        {
            if(PeekThirdPattern(step))
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
        if(thirdPattern == step)
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

    public virtual string Id { get; private set; }

    public virtual IEnumerable<char> Name
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

    public static IEnumerable<char> GetChars(StreamReader reader)
    {
        while (!reader.EndOfStream)
            yield return (char)reader.Read();
    }

    protected virtual IEnumerable<char> GetEnumerable()
    {
        StreamReader reader = new StreamReader(Id);
        foreach (var item in GetChars(reader))
            yield return item;
        reader.Close();
    }

    public virtual IEnumerable<char> GetSnippet(int snippetLen)
    {
        if (info is null) info = parser(this);
        StreamReader reader = new StreamReader(Id);
        int infoSnippetLen = info.SnippetLen < 0 ? int.MaxValue : info.SnippetLen;
        foreach (var item in GetChars(reader).Skip(info.SnippetInit).Take(Math.Min(infoSnippetLen, snippetLen)))
            yield return item;
        reader.Close();
    }

    public virtual stateDoc GetState()
    {
        if (File.Exists(Id)) return (DateTime.Equals(modifiedDateTime, File.GetLastWriteTime(Id))) ? stateDoc.notchanged : stateDoc.changed;
        return stateDoc.deleted;
    }

    public virtual void  UpdateDateTime() => modifiedDateTime = File.GetLastWriteTime(Id);

    public virtual IEnumerator<char> GetEnumerator() => GetEnumerable().GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public override bool Equals(object? obj)
    {
        return obj is Document document &&
               modifiedDateTime == document.modifiedDateTime &&
               Id == document.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}


public class CollectionSplitter : IEnumerable<IDocument>
{
    public string Path
    {
        get;
        private set;
    }

    ICreator<ILazyMatcher> matcherCreator;

    Func<IEnumerable<char>,ParsedInfo> parser;

    public CollectionSplitter(string collectionPath, ICreator<ILazyMatcher> matcherCreator, Func<IEnumerable<char>,ParsedInfo> parser)
    {
        Path = collectionPath;
        this.matcherCreator = matcherCreator;
        this.parser = parser;
    }

    public IEnumerator<IDocument> GetEnumerator()
    {
        EmbebedDoc previousDoc = null;
        EmbebedDoc current = null;
        while(previousDoc == null || !previousDoc.IsFinal)
        {
            current = new EmbebedDoc(Path,matcherCreator,parser);
            current.Server = previousDoc;
            yield return current;
            previousDoc = current;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class EmbebedDoc : Document
{
    class EmbebedDocEnumerator : IEnumerator<char>
    {
        public char Current 
        {
            get;
            private set;
        }

        object IEnumerator.Current => this.Current;

        bool finished = false;

        bool isFirstMoveNext = true;

        Stream stream;

        ILazyMatcher matcher;

        EmbebedDoc father;

        bool disposed = false;

        public EmbebedDocEnumerator(EmbebedDoc father, ILazyMatcher matcher)
        {
            this.father = father;
            this.matcher = matcher;
        }

        public void Dispose()
        {
            if(!disposed)
            {
                //No se pasa en el caso de terminado pq se usume que se pasó en el moveNext que se llegó al final
                if(!finished)
                {
                    father.RecieveNOTFinishedReader(stream,matcher.CloneParsing());
                }
                matcher = null;
                stream = null;
                father = null;
                disposed = true;
                finished = false;
                isFirstMoveNext = false;
            }
            else
            {
                throw new ObjectDisposedException("embebed document");
            }
        }

        public bool MoveNext()
        {
            if(!disposed)
            {
                if(isFirstMoveNext)
                {
                    stream = father.GetStreamReaderAtInitPos();
                    isFirstMoveNext = false;
                    finished = false;
                }
                int item = stream.ReadByte();
                if(item == -1)
                {
                    stream.Dispose();
                    isFirstMoveNext = true;
                    finished = true;
                    stream = null;
                    father.SetThisAsFinal();
                    return false;
                }
                matcher.MatchStep((char)item);
                if(matcher.AtFinalState)
                {
                    father.RecieveFinishedReader(stream);
                    isFirstMoveNext = true;
                    finished = true;
                    stream = null;
                    return false;
                }
                Current = (char)item;
                return true;
            }
            else
            {
                throw new ObjectDisposedException("embebed document");
            }
        }

        public void Reset()
        {
            if(!disposed)
            {
                if(!finished)
                {
                    father.RecieveNOTFinishedReader(stream,matcher.CloneParsing());
                }
                stream = null;
                isFirstMoveNext = true;
                finished = false;
            }
            else
            {
                throw new ObjectDisposedException("embebed document");
            }
        }

    }

    public override string Id 
    {
        get
        {
            return id;
        }
    }

    string id;


    public override IEnumerable<char> Name
    {
        get
        {
            if (info is null)
            {
                var text = Utils.StreamToEnumerable(GetStreamReaderAtInitPos());
                info = parser(text);
            }
            Stream reader = GetStreamReaderAtInitPos();
            foreach (var item in Utils.StreamToEnumerable(reader).Skip(info.TitleInit).Take(info.TitleLen))
                yield return item;
            reader.Close();
        }
    }

    Queue<(Stream,ILazyMatcher)> NotFinishedReader = new Queue<(Stream, ILazyMatcher)>();

    Queue<Stream> FinishedReader = new Queue<Stream>();

    readonly ICreator<ILazyMatcher> matcherCreator;

    readonly Func<IEnumerable<char>,ParsedInfo> parser;
    
    ParsedInfo info;

    public bool IsFinal
    {
        get;
        private set;
    }

    public long InitPos
    {
        get;
        private set;
    }

    public long LastPos
    {
        get;
        private set;
    }

    public EmbebedDoc Server
    {
        get;
        set;
    }

    public EmbebedDoc(string collectionPath, ICreator<ILazyMatcher> matcherCreator, Func<IEnumerable<char>,ParsedInfo> parser, int initPos = -1, int lastPos = -1) :base(collectionPath,parser)
    {
        
        this.id = collectionPath;
        this.matcherCreator = matcherCreator;
        this.InitPos = initPos;
        this.LastPos = lastPos;
        this.parser = parser;
    }

    void SetThisAsFinal()
    {
        IsFinal = true;
    }

    public Stream GetStreamForNextDoc()
    {

        if(IsFinal)
        {
            return null;
        }
        if(FinishedReader.Count > 0)
        {
            return FinishedReader.Dequeue();
        }
        else
        {
            if(NotFinishedReader.Count > 0)
            {
                var tuple = NotFinishedReader.Dequeue();
                var sr = tuple.Item1;
                var matcher = tuple.Item2;
                PrivateMoveToEnd(sr,matcher);
                return sr;
            }
            else
            {
                if(Server != null)
                {
                    var sr = Server.GetStreamForNextDoc();
                    if(sr == null)
                        throw new Exception("this document is beyond the final of the collection");
                    PrivateMoveToEnd(sr,matcherCreator.Create());
                    return sr;
                }
                else
                {
                    var fr = File.Open(id,FileMode.Open);
                    var sr = new BufferedStream(fr);
                    PrivateMoveToEnd(sr,matcherCreator.Create());
                    return sr;
                }
            }
        }
    }


    private void PrivateMoveToEnd(Stream sr, ILazyMatcher matcher)
    {
        if(LastPos != -1)
        {
            sr.Seek(LastPos,SeekOrigin.Begin);
        }
        else
        {
            MoveToEnd(sr,matcher);
            if(!IsFinal)
            {
                LastPos =  sr.Position;
            }
            
        }
    }

    private void RecieveFinishedReader(Stream reader)
    {
        FinishedReader.Enqueue(reader);
        LastPos = reader.Position;
    }

    private void RecieveNOTFinishedReader(Stream reader, ILazyMatcher matcherClon)
    {
        NotFinishedReader.Enqueue((reader,matcherClon));
    }

    public static void MoveToEnd(Stream reader, ILazyMatcher matcherClon)
    {
        foreach(char item in Utils.StreamToEnumerable(reader))
        {
            matcherClon.MatchStep(item);
            if(matcherClon.AtFinalState)
                break;
        }
    }

    public override IEnumerator<char> GetEnumerator()
    {   
        return new EmbebedDocEnumerator(this, matcherCreator.Create());
    }

    Stream GetStreamReaderAtInitPos()
    {
        if(Server == null)
        {
            var fr = File.Open(id,FileMode.Open);
            return new BufferedStream(fr);
        }
        else
        {
            var stream = Server.GetStreamForNextDoc();
            InitPos = stream.Position;
            return stream;
        }
    }

    public override IEnumerable<char> GetSnippet(int snippetLen)
    {
        if (info is null)
        {
            var text = Utils.StreamToEnumerable(GetStreamReaderAtInitPos());
            info = parser(text);
        }
        Stream reader = GetStreamReaderAtInitPos();
        int infoSnippetLen = info.SnippetLen < 0 ? int.MaxValue : info.SnippetLen;
        foreach (var item in Utils.StreamToEnumerable(reader).Skip(info.SnippetInit).Take(Math.Min(infoSnippetLen, snippetLen)))
            yield return item;
        reader.Close();
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

        foreach (string item in Utils.GetTermsToLower(Result))
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