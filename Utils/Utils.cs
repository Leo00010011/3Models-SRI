namespace Utils;

public static class Utils
{
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
