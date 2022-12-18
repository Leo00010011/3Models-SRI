using System.Collections;

namespace Utils;

public static class Utils
{
    public static int Relevance(this IEnumerable<double> colection, double seed, double value, Func<double, double, bool> stop)
    {
        int index = 0;

        foreach (var item in colection)
        {
            seed = seed + item;
            if (stop(seed, value))
                break;
            index++;
        }

        return index == 0 ? -1 : index;
    }

    public static void Apply<T>(this IEnumerable<T> colection, Action<T> func)
    {
        foreach (var item in colection)
            func(item);
    }

    public static string RepeatChar(this char character, int count = 1)
    {
        string result = "";
        for (; count > 0; count--)
            result += character;
        return result;
    }

    public static IEnumerable<char> StreamToEnumerable(Stream sr)
    {
        while (!(sr.Position == sr.Length))
            yield return (char)sr.ReadByte();
    }

    public static IEnumerable<T> DisposeAtEnd<K,T>(K iter) where K : IEnumerable<T>,IDisposable
    {
        foreach(var item in  iter)
        {
            yield return item;
        }
        iter.Dispose();
    }

    public static int Peek(BufferedStream sr)
    {
        long prevPos = sr.UnderlyingStream.Position;
        var fd = sr.UnderlyingStream;
        fd.Position = sr.Position;
        int result = fd.ReadByte();
        fd.Position = prevPos;
        return result;
    }

    public static IEnumerable<string> ReadAllFiles(string path)
    {
        foreach (var item in Directory.EnumerateFiles(path))
        {
            yield return item;
        }
        foreach (var docs in Directory.EnumerateDirectories(path))
        {
            foreach (var item in ReadAllFiles(docs))
            {
                yield return item;
            }
        }
    }
    private static HashSet<string>? stopWords;

    public static HashSet<string> GetStopWords()
    {
        if (stopWords == null)
            stopWords = new HashSet<string>(new string[] { "0o", "0s", "3a", "3b", "3d", "6b", "6o", "a", "a1", "a2", "a3", "a4", "ab",
             "able", "about", "above", "abst", "ac", "accordance", "according", "accordingly", "across", "act", "actually", "ad", "added",
             "adj", "ae", "af", "affected", "affecting", "affects", "after", "afterwards", "ag", "again", "against", "ah", "ain", "ain't",
             "aj", "al", "all", "allow", "allows", "almost", "alone", "along", "already", "also", "although", "always", "am", "among",
             "amongst", "amoungst", "amount", "an", "and", "announce", "another", "any", "anybody", "anyhow", "anymore", "anyone","anything",
             "anyway", "anyways", "anywhere", "ao", "ap", "apart", "apparently", "appear", "appreciate", "appropriate", "approximately", "ar",
             "are", "aren", "arent", "aren't", "arise", "around", "as", "a's", "aside", "ask", "asking", "associated", "at", "au", "auth", "av",
             "available", "aw", "away", "awfully", "ax", "ay", "az", "b", "b1", "b2", "b3", "ba", "back", "bc", "bd", "be", "became", "because",
             "become", "becomes", "becoming", "been", "before", "beforehand", "begin", "beginning", "beginnings", "begins", "behind", "being",
             "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "bi", "bill", "biol", "bj", "bk", "bl", "bn",
             "both", "bottom", "bp", "br", "brief", "briefly", "bs", "bt", "bu", "but", "bx", "by", "c", "c1", "c2", "c3", "ca", "call",
             "came", "can", "cannot", "cant", "can't", "cause", "causes", "cc", "cd", "ce", "certain", "certainly", "cf", "cg", "ch",
             "changes", "ci", "cit", "cj", "cl", "clearly", "cm", "c'mon", "cn", "co", "com", "come", "comes", "con", "concerning",
             "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could", "couldn", "couldnt",
             "couldn't", "course", "cp", "cq", "cr", "cry", "cs", "c's", "ct", "cu", "currently", "cv", "cx", "cy", "cz", "d", "d2", "da",
             "date", "dc", "dd", "de", "definitely", "describe", "described", "despite", "detail", "df", "di", "did", "didn", "didn't",
             "different", "dj", "dk", "dl", "do", "does", "doesn", "doesn't", "doing", "don", "done", "don't", "down", "downwards", "dp",
             "dr", "ds", "dt", "du", "due", "during", "dx", "dy", "e", "e2", "e3", "ea", "each", "ec", "ed", "edu", "ee", "ef", "effect",
             "eg", "ei", "eight", "eighty", "either", "ej", "el", "eleven", "else", "elsewhere", "em", "empty", "en", "end", "ending",
             "enough", "entirely", "eo", "ep", "eq", "er", "es", "especially", "est", "et", "et-al", "etc", "eu", "ev", "even", "ever",
             "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except", "ey", "f", "f2", "fa",
             "far", "fc", "few", "ff", "fi", "fifteen", "fifth", "fify", "fill", "find", "fire", "first", "five", "fix", "fj", "fl", "fn",
             "fo", "followed", "following", "follows", "for", "former", "formerly", "forth", "forty", "found", "four", "fr", "from", "front",
             "fs", "ft", "fu", "full", "further", "furthermore", "fy", "g", "ga", "gave", "ge", "get", "gets", "getting", "gi", "give",
             "given", "gives", "giving", "gj", "gl", "go", "goes", "going", "gone", "got", "gotten", "gr", "greetings", "gs", "gy", "h",
             "h2", "h3", "had", "hadn", "hadn't", "happens", "hardly", "has", "hasn", "hasnt", "hasn't", "have", "haven", "haven't", "having",
             "he", "hed", "he'd", "he'll", "hello", "help", "hence", "her", "here", "hereafter", "hereby", "herein", "heres", "here's",
             "hereupon", "hers", "herself", "hes", "he's", "hh", "hi", "hid", "him", "himself", "his", "hither", "hj", "ho", "home",
             "hopefully", "how", "howbeit", "however", "how's", "hr", "hs", "http", "hu", "hundred", "hy", "i", "i2", "i3", "i4",
             "i6", "i7", "i8", "ia", "ib", "ibid", "ic", "id", "i'd", "ie", "if", "ig", "ignored", "ih", "ii", "ij", "il", "i'll",
             "im", "i'm", "immediate", "immediately", "importance", "important", "in", "inasmuch", "inc", "indeed", "index", "indicate",
             "indicated", "indicates", "information", "inner", "insofar", "instead", "interest", "into", "invention", "inward", "io",
             "ip", "iq", "ir", "is", "isn", "isn't", "it", "itd", "it'd", "it'll", "its", "it's", "itself", "iv", "i've", "ix", "iy", "iz",
             "j", "jj", "jr", "js", "jt", "ju", "just", "k", "ke", "keep", "keeps", "kept", "kg", "kj", "km", "know", "known", "knows", "ko",
             "l", "l2", "la", "largely", "last", "lately", "later", "latter", "latterly", "lb", "lc", "le", "least", "les", "less",
             "lest", "let", "lets", "let's", "lf", "like", "liked", "likely", "line", "little", "lj", "ll", "ll", "ln", "lo", "look",
             "looking", "looks", "los", "lr", "ls", "lt", "ltd", "m", "m2", "ma", "made", "mainly", "make", "makes", "many", "may",
             "maybe", "me", "mean", "means", "meantime", "meanwhile", "merely", "mg", "might", "mightn", "mightn't", "mill", "million",
             "mine", "miss", "ml", "mn", "mo", "more", "moreover", "most", "mostly", "move", "mr", "mrs", "ms", "mt", "mu", "much",
             "mug", "must", "mustn", "mustn't", "my", "myself", "n", "n2", "na", "name", "namely", "nay", "nc", "nd", "ne", "near",
             "nearly", "necessarily", "necessary", "need", "needn", "needn't", "needs", "neither", "never", "nevertheless", "new",
             "next", "ng", "ni", "nine", "ninety", "nj", "nl", "nn", "no", "nobody", "non", "none", "nonetheless", "noone", "nor",
             "normally", "nos", "not", "noted", "nothing", "novel", "now", "nowhere", "nr", "ns", "nt", "ny", "o", "oa", "ob", "obtain",
             "obtained", "obviously", "oc", "od", "of", "off", "often", "og", "oh", "oi", "oj", "ok", "okay", "ol", "old", "om", "omitted",
             "on", "once", "one", "ones", "only", "onto", "oo", "op", "oq", "or", "ord", "os", "ot", "other", "others", "otherwise", "ou",
             "ought", "our", "ours", "ourselves", "out", "outside", "over", "overall", "ow", "owing", "own", "ox", "oz", "p", "p1", "p2",
             "p3", "page", "pagecount", "pages", "par", "part", "particular", "particularly", "pas", "past", "pc", "pd", "pe", "per",
             "perhaps", "pf", "ph", "pi", "pj", "pk", "pl", "placed", "please", "plus", "pm", "pn", "po", "poorly", "possible", "possibly",
             "potentially", "pp", "pq", "pr", "predominantly", "present", "presumably", "previously", "primarily", "probably", "promptly",
             "proud", "provides", "ps", "pt", "pu", "put", "py", "q", "qj", "qu", "que", "quickly", "quite", "qv", "r", "r2", "ra", "ran",
             "rather", "rc", "rd", "re", "readily", "really", "reasonably", "recent", "recently", "ref", "refs", "regarding", "regardless",
             "regards", "related", "relatively", "research", "research-articl", "respectively", "resulted", "resulting", "results", "rf",
             "rh", "ri", "right", "rj", "rl", "rm", "rn", "ro", "rq", "rr", "rs", "rt", "ru", "run", "rv", "ry", "s", "s2", "sa", "said",
             "same", "saw", "say", "saying", "says", "sc", "sd", "se", "sec", "second", "secondly", "section", "see", "seeing", "seem",
             "seemed", "seeming", "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "sf",
             "shall", "shan", "shan't", "she", "shed", "she'd", "she'll", "shes", "she's", "should", "shouldn", "shouldn't", "should've",
             "show", "showed", "shown", "showns", "shows", "si", "side", "significant", "significantly", "similar", "similarly", "since",
             "sincere", "six", "sixty", "sj", "sl", "slightly", "sm", "sn", "so", "some", "somebody", "somehow", "someone", "somethan",
             "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "sp", "specifically", "specified", "specify",
             "specifying", "sq", "sr", "ss", "st", "still", "stop", "strongly", "sub", "substantially", "successfully", "such", "sufficiently",
             "suggest", "sup", "sure", "sy", "system", "sz", "t", "t1", "t2", "t3", "take", "taken", "taking", "tb", "tc", "td", "te", "tell",
             "ten", "tends", "tf", "th", "than", "thank", "thanks", "thanx", "that", "that'll", "thats", "that's", "that've", "the", "their",
             "theirs", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "thered", "therefore", "therein", "there'll",
             "thereof", "therere", "theres", "there's", "thereto", "thereupon", "there've", "these", "they", "theyd", "they'd", "they'll",
             "theyre", "they're", "they've", "thickv", "thin", "think", "third", "this", "thorough", "thoroughly", "those", "thou", "though",
             "thoughh", "thousand", "three", "throug", "through", "throughout", "thru", "thus", "ti", "til", "tip", "tj", "tl", "tm", "tn",
             "to", "together", "too", "took", "top", "toward", "towards", "tp", "tq", "tr", "tried", "tries", "truly", "try", "trying", "ts",
             "t's", "tt", "tv", "twelve", "twenty", "twice", "two", "tx", "u", "u201d", "ue", "ui", "uj", "uk", "um", "un", "under",
             "unfortunately", "unless", "unlike", "unlikely", "until", "unto", "uo", "up", "upon", "ups", "ur", "us", "use", "used", "useful",
             "usefully", "usefulness", "uses", "using", "usually", "ut", "v", "va", "value", "various", "vd", "ve", "ve", "very", "via",
             "viz", "vj", "vo", "vol", "vols", "volumtype", "vq", "vs", "vt", "vu", "w", "wa", "want", "wants", "was", "wasn", "wasnt",
             "wasn't", "way", "we", "wed", "we'd", "welcome", "well", "we'll", "well-b", "went", "were", "we're", "weren", "werent",
             "weren't", "we've", "what", "whatever", "what'll", "whats", "what's", "when", "whence", "whenever", "when's", "where",
             "whereafter", "whereas", "whereby", "wherein", "wheres", "where's", "whereupon", "wherever", "whether", "which", "while",
             "whim", "whither", "who", "whod", "whoever", "whole", "who'll", "whom", "whomever", "whos", "who's", "whose", "why", "why's",
             "wi", "widely", "will", "willing", "wish", "with", "within", "without", "wo", "won", "wonder", "wont", "won't", "words", "world",
             "would", "wouldn", "wouldnt", "wouldn't", "www", "x", "x1", "x2", "x3", "xf", "xi", "xj", "xk", "xl", "xn", "xo", "xs",
             "xt", "xv", "xx", "y", "y2", "yes", "yet", "yj", "yl", "you", "youd", "you'd", "you'll", "your", "youre", "you're",
             "yours", "yourself", "yourselves", "you've", "yr", "ys", "yt", "z", "zero", "zi", "zz",});
        return stopWords;
    }

    public static IEnumerable<string> GetTerms(IEnumerable<char> text)
    {
        LinkedList<char>? currentTerm = null;

        foreach (char item in text)
        {
            if (Char.IsLetterOrDigit(item))
            {
                if (currentTerm == null)
                    currentTerm = new LinkedList<char>();
                currentTerm.AddLast(item);
            }
            else
            {
                if (currentTerm != null)
                {
                    yield return String.Concat(currentTerm);
                    currentTerm = null;
                }
            }
        }
        if (currentTerm != null)
            yield return String.Concat(currentTerm);
    }

    public static IEnumerable<string> GetTermsToLower(IEnumerable<char> text)
    {
        LinkedList<char>? currentTerm = null;
        foreach (char item in text)
        {
            if (Char.IsLetterOrDigit(item))
            {
                if (currentTerm == null)
                    currentTerm = new LinkedList<char>();
                currentTerm.AddLast(Char.ToLower(item));
            }
            else
            {
                if (currentTerm != null)
                {
                    yield return String.Concat(currentTerm);
                    currentTerm = null;
                }
            }
        }
        if (currentTerm != null)
            yield return String.Concat(currentTerm);
    }
}

public class ExceptIndexEnumerable<T> : IEnumerable<T>
{
    int currentIndex = 0;

    LinkedListNode<int> currentIndexToExcept;

    IEnumerable<T> iter;

    LinkedList<int> index;

    public ExceptIndexEnumerable(IEnumerable<T> iter,LinkedList<int> index )
    {
        this.iter = iter;
        this.index = index;
        currentIndexToExcept = index.First;
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach(var item in iter)
        {
            if(currentIndexToExcept != null && currentIndex == currentIndexToExcept.Value)
            {
                currentIndexToExcept = currentIndexToExcept.Next;
            }
            else
            {
                yield return item;
            }
            currentIndex++;
        }
        currentIndex = 0;
        currentIndexToExcept = index.First;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public interface ICreator<out T>
{
    T Create();
}

public class ParsedInfo
{
    public int SnippetInit { get; private set; }
    public int SnippetLen { get; private set; }
    public int TitleInit { get; private set; }
    public int TitleLen { get; private set; }
    public int TextInit { get; private set; }

    public ParsedInfo(int si, int sl, int ti, int tl, int tei)
    {
        SnippetInit = si;
        SnippetLen = sl;
        TitleInit = ti;
        TitleLen = tl;
        TextInit = tei;
    }

}

public static class Parser
{
    public static ParsedInfo NewsgroupParser(IEnumerable<char> file)
    {
        int count = 0;
        char[] matching_machine = new char[] { 'S', 'u', 'b', 'j', 'e', 'c', 't', ':', ' ' };
        IEnumerator<char> file_enumerator = file.GetEnumerator();
        int ti = Parser.MatchIndex(file_enumerator, matching_machine, out count, count);
        while (file_enumerator.MoveNext() && file_enumerator.Current != '\n') count++;
        int tl = (count++) - ti;
        int si = ti + tl + 2;
        return new ParsedInfo(si, -1, ti, tl, ti);
    }

    public static ParsedInfo CranParser(IEnumerable<char> file)
    {
        int count = 0;
        char[] matching_title = new char[] { '.', 'T', '\n' };
        char[] matching_end_title = new char[] { '.', 'A', '\n' };
        char[] matching_text = new char[] { '.', 'W', '\n' };
        IEnumerator<char> file_enumerator = file.GetEnumerator();
        int ti = Parser.MatchIndex(file_enumerator, matching_title, out count, count);
        int tl = Parser.MatchIndex(file_enumerator, matching_end_title, out count, count) - ti - 3;
        int texi = Parser.MatchIndex(file_enumerator, matching_text, out count, count);
        return new ParsedInfo(texi, -1, ti, tl, texi);

    }

    public static ParsedInfo ReutersParser(IEnumerable<char> file)
    {
        int count = 0;
        char[] matching_title = new char[] { '<', 'T', 'I', 'T', 'L', 'E', '>' };
        char[] matching_end_title = new char[] { '<', '/', 'T', 'I', 'T', 'L', 'E', '>' };
        char[] matching_text = new char[] { '<', 'B', 'O', 'D', 'Y', '>' };
        IEnumerator<char> file_enumerator = file.GetEnumerator();
        int ti = Parser.MatchIndex(file_enumerator, matching_title, out count, count);
        int tl = Parser.MatchIndex(file_enumerator, matching_end_title, out count, count) - ti - 8;
        int texi = Parser.MatchIndex(file_enumerator, matching_text, out count, count);
        return new ParsedInfo(texi, -1, ti, tl, texi);

    }

    private static int MatchIndex(IEnumerator<char> file_enumerator, char[] matching, out int current_index, int initial)
    {
        current_index = initial;
        int match_count = 0;
        int index_result = 0;
        while (file_enumerator.MoveNext())
        {
            if (match_count >= matching.Length)
            {
                index_result = current_index++;
                break;
            }
            if (file_enumerator.Current == matching[match_count]) match_count++;
            else match_count = 0;
            current_index++;
        }
        return index_result;
    }

}