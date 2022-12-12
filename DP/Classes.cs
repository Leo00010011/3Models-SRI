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


public class Document : IDocument, IComparable
{
    public virtual IEnumerable<char> Name
    {
        get
        {
            if (info is null) info = parser(this);
            StreamReader reader = new StreamReader(path);
            foreach (var item in GetChars(reader).Skip(info.TitleInit).Take(info.TitleLen))
                yield return item;
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
        reader.Close();
    }

    public virtual IEnumerable<char> GetSnippet(int snippetLen)
    {
        if (info is null) info = parser(this);
        StreamReader reader = new StreamReader(path);
        int infoSnippetLen = info.SnippetLen < 0 ? int.MaxValue : info.SnippetLen;
        foreach (var item in GetChars(reader).Skip(info.SnippetInit).Take(Math.Min(infoSnippetLen, snippetLen)))
            yield return item;
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

    public override bool Equals(object? obj) => obj is Document document && modifiedDateTime == document.modifiedDateTime && path == document.path;

    public override int GetHashCode() => path.GetHashCode();

    public int CompareTo(object? obj)
    {
        return obj is Document document ? document.path.CompareTo(path) : throw new InvalidCastException();
    }
}

public class CollectionSplitter : IEnumerable<IDocument>, IDisposable
{

    class CollectionSplitterEnumerator : IEnumerator<IDocument>
    {
        public IDocument Current => current;

        int index = 1;

        bool disposed = false;

        public EmbebedDocument current = null;

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

            if(current == null || (!current.EndOfFileReached && current.EndReached))
            {
                current = new EmbebedDocument(enumerable.collectionPath,index,enumerable.stream,enumerable.parser,enumerable.endMatcherCreator);
                index++;
                return true;
            }
            else
            {
                if(!current.EndReached)
                    throw new Exception("Tienes que leer el documento anterior hasta el final");
                //Solo se llega aquí si current.EndOfFileReached es true
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

    bool disposed = false;

    bool streamUsed = false;

    Func<IEnumerable<char>, ParsedInfo> parser;

    ICreator<ILazyMatcher> endMatcherCreator;

    Stream stream;
    public CollectionSplitter(string collectionPath, ICreator<ILazyMatcher> endMatcherCreator, Func<IEnumerable<char>, ParsedInfo> parser)
    {
        this.collectionPath = collectionPath;
        this.endMatcherCreator = endMatcherCreator;
        this.parser = parser;
        stream = new BufferedStream(File.Open(collectionPath,FileMode.Open));
    }

    public IEnumerator<IDocument> GetEnumerator()
    {
        if(disposed)
            throw new ObjectDisposedException("LLamaron Dispose() en este CollectionSplitter");
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
        if(!disposed)
        {
            stream.Dispose();
            stream = null;
            disposed = true;
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

        EmbebedDocument enumerable;

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
                enumerable.EndOfFileReached = true;
                enumerable.EndReached = true;
                return false;
            }
            Current = (char)item;
            matcher.MatchStep(Current);
            if(matcher.AtFinalState)
            {
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
            enumerable = null;
            matcher = null;
        }
    }

    public override IEnumerable<char> Name => throw new NotImplementedException();

    public override IEnumerable<char> GetSnippet(int snippetLen) => throw new NotImplementedException();
    
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

    Stream stream;

    ICreator<ILazyMatcher> endMatcherCreator;

    Func<IEnumerable<char>, ParsedInfo> parser;

    public EmbebedDocument(string id,int index,Stream stream, Func<IEnumerable<char>, ParsedInfo> parser, ICreator<ILazyMatcher> endMatcher) : base(id, parser)
    {
        path = id;
        Id = path + "\\" + index;
        this.stream = stream;
        this.endMatcherCreator = endMatcher;
        this.parser = parser;
    }

    public override  IEnumerator<char> GetEnumerator()
    {
        if(!enumeratorSended)
        {
            enumeratorSended = true;
            return new EmbebedDocumentEnumerator(this);
        }
        throw new InvalidOperationException("Solo se puede pedir un enumerator");
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