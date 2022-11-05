namespace Utils;

public static class Utils
{
    private static HashSet<string>? stopWords;

    public static HashSet<string> GetStopWords()
    {
        if(stopWords == null)
            stopWords = new HashSet<string>(new string[]{"pronombres","palabras","..."});
        return stopWords;
    }

    public static IEnumerable<string> GetTerms(IEnumerable<char> text)
    {
        LinkedList<char>? currentTerm = null;
        
        foreach(char item in text)
        {
            if(Char.IsLetterOrDigit(item))
            {
                if(currentTerm == null)
                    currentTerm = new LinkedList<char>();
                currentTerm.AddLast(item);
            }
            else
            {
                if(currentTerm != null)
                {
                    yield return String.Concat(currentTerm);
                    currentTerm = null;
                }
            }
        }
        if(currentTerm != null)
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
    public int TextLen {get; private set;}

    public ParsedInfo(int si,int sl, int ti, int tl, int tei, int tel)
    {
        SnippetInit=si;
        SnippetLen=sl;
        TitleInit=ti;
        TitleLen=tl;
        TextInit=tei;
        TextLen=tel;
    }

}

public static class Parser
{
    public static ParsedInfo NewsgroupParser(IEnumerable<char> file)
    {
        int ti=0;
        int tl=0;
        int count = 0;
        int title_match_count=0;
        char[] matching_machine = new char[]{'S', 'u','b','j','e','c','t',':',' '};
        IEnumerator<char> file_enumerator = file.GetEnumerator();

        while(file_enumerator.MoveNext())
        {   
            if(title_match_count >= matching_machine.Length)
            {
                ti = count++;
                break;
            }
            if(file_enumerator.Current==matching_machine[title_match_count]) title_match_count++;
            else title_match_count=0;
            count++;
        } 
        while(file_enumerator.MoveNext() && file_enumerator.Current != '\n') count++;
        tl = (count++)-ti;
        while(file_enumerator.MoveNext()) count++;
        int si = ti+tl +2;
        return new ParsedInfo(si,Math.Min(300, count- si),ti,tl,ti,count - ti);
    }
}