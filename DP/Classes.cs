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

    
}

public class EndCranMatcher : ILazyMatcher
{
    readonly string firstPattern = "\n.i ";
    readonly char thirdPattern = '\n';

    private int indexToMatch = 0;

    private Func<char,bool> matchFunc;
    private Func<char,bool> peekFunc;

    public bool AtFinalState
    {
        get;
        private set;
    }

    public EndCranMatcher()
    {
        matchFunc = MatchFirstPattern;
        peekFunc = PeekFirstPattern;
    }

    public bool Match(string text)
    {
        if(text.Length < 4)
        {
            return false;
        }
        return Match(text);
    }

    public bool Match(IEnumerable<char> text)
    {
        indexToMatch = 0;
        matchFunc = MatchFirstPattern;
        peekFunc = PeekFirstPattern;
        AtFinalState = false;
        foreach(char item in text)
        {
            MatchStep(item);
        }
        bool result = AtFinalState;
        indexToMatch = 0;
        matchFunc = MatchFirstPattern;
        peekFunc = PeekStep;
        AtFinalState = false;
        return result;
    }

    public bool PeekStep(char step)
    {
        return peekFunc(step);
    }
    public bool MatchStep(char step)
    {
        return matchFunc(step);
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
                matchFunc = MatchSecondPattern;
                peekFunc = PeekSecondPattern;
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
                peekFunc = PeekThirdPattern;
                matchFunc = MatchThirdPattern;
                indexToMatch = 0;
                return matchFunc(step);
            }
            else
            {
                peekFunc = PeekFirstPattern;
                matchFunc = MatchFirstPattern;
                indexToMatch = 0;
                return matchFunc(step);
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
            matchFunc = MatchFirstPattern;
            peekFunc = PeekFirstPattern;
            AtFinalState = true;
            return true;
        }
        else
        {
            peekFunc = PeekFirstPattern;
            matchFunc = MatchFirstPattern;
            indexToMatch = 0;
            return matchFunc(step);
        }
    }

    public ILazyMatcher CloneParsing()
    {
        var result = new EndCranMatcher();
        result.matchFunc = matchFunc;
        result.peekFunc = peekFunc;
        result.AtFinalState = AtFinalState;
        return result;
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