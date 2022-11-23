namespace Utils;

public static class Utils
{
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
            stopWords = new HashSet<string>(new string[] { "pronombres", "palabras", "..." });
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
}

public class ParsedInfo
{
    public int SnippetInit {get; private set;}
    public int SnippetLen {get; private set;}
    public int TitleInit {get; private set;}
    public int TitleLen {get; private set;}
    public int TextInit {get; private set;}

    public ParsedInfo(int si,int sl, int ti, int tl, int tei)
    {
        SnippetInit=si;
        SnippetLen=sl;
        TitleInit=ti;
        TitleLen=tl;
        TextInit=tei;
    }

}

public static class Parser
{
    public static ParsedInfo NewsgroupParser(IEnumerable<char> file)
    {
        int count = 0;
        char[] matching_machine = new char[]{'s', 'u','b','j','e','c','t',':',' '};
        IEnumerator<char> file_enumerator = file.GetEnumerator();
        int ti = Parser.MatchIndex(file_enumerator, matching_machine,out count,count);
        while(file_enumerator.MoveNext() && file_enumerator.Current != '\n') count++;
        int  tl = (count++)-ti;
        int si = ti+tl +2;
        return new ParsedInfo(si,-1,ti,tl,ti);
    }

    public static ParsedInfo CranParser(IEnumerable<char> file)
    {
        int count = 0;
        char[] matching_title = new char[]{'.', 't','\n'};
        char[] matching_end_title = new char[]{'.', 'a','\n'};
        char[] matching_text = new char[]{'.', 'w','\n'};
        IEnumerator<char> file_enumerator = file.GetEnumerator();
        int ti= Parser.MatchIndex(file_enumerator,matching_title,out count,count);
        int tl= Parser.MatchIndex(file_enumerator,matching_end_title,out count,count) - ti -3;
        int texi = Parser.MatchIndex(file_enumerator,matching_text,out count,count);
        return new ParsedInfo(texi, -1, ti, tl, texi);

    }
    
    public static ParsedInfo ReutersParser(IEnumerable<char> file)
    {
        int count = 0;
        char[] matching_title = new char[]{'<', 't','i','t','l','e','>'};
        char[] matching_end_title = new char[]{'<','/', 't','i','t','l','e','>'};
        char[] matching_text = new char[]{'<', 'b','o','d','y','>'};
        IEnumerator<char> file_enumerator = file.GetEnumerator();
        int ti= Parser.MatchIndex(file_enumerator,matching_title,out count,count);
        int tl= Parser.MatchIndex(file_enumerator,matching_end_title,out count,count) - ti -8;
        int texi = Parser.MatchIndex(file_enumerator,matching_text,out count,count);
        return new ParsedInfo(texi, -1, ti, tl, texi);

    }

    private static int MatchIndex(IEnumerator<char> file_enumerator, char[] matching,out int current_index, int initial)
    {
        current_index = initial;
        int match_count = 0;
        int index_result = 0;
        while(file_enumerator.MoveNext())
        {   
            if(match_count >= matching.Length)
            {
                index_result = current_index++;
                break;
            }
            if(file_enumerator.Current==matching[match_count]) match_count++;
            else match_count=0;
            current_index++;
        }
        return index_result;
    }

}