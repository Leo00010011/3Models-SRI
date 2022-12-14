using SRI.Interface;
using System.Collections.Generic;
using DP.Interface;

namespace UI;

public static class Date
{
    public static ISearchResult? results;
    public static IEnumerable<IDocument> docs = new LinkedList<IDocument>();
}