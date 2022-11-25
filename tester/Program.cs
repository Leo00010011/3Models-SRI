// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using DP;
using DP.Interface;
using SRI;
using SRI.Interface;
using Utils;
using System.Text.RegularExpressions;

namespace Test;

public class LazyKMP
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
        while(indexToMatch > 0 && pattern[indexToMatch] != step)
            indexToMatch = pi[indexToMatch - 1];
        if(pattern[indexToMatch] == step)
            indexToMatch++;
        if(indexToMatch == pattern.Length)
        {
            indexToMatch = 0;
            return true;
        }
        return false;
    }

    public bool Match(IEnumerable<char> text)
    {
        bool last = false;
        foreach (var step in text)
        {
            last = this.MatchStep(step);
        }
        indexToMatch = 0;
        return last;

    }
}




public class CranDocSpliter 
{
    StreamReader reader;

    public CranDocSpliter(string path)
    {
        
    }

    
}

public class Program
{
    public static void Print(IEnumerable<char> text)
    {
        foreach (char c in text)
        {
            if (c == '\n')
            {
                Console.WriteLine();
            }
            else
            {
                Console.Write(c);
            }
        }
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

    private static char ReadTest(IEnumerable<IDocument> docs)
    {
        char a = '0';
        foreach (var doc in docs)
        {
            foreach (var item in doc)
            {
                a = item;
            }
        }
        return a;
    }


    private static void SpeedTest()
    {
        Stopwatch cloc = new Stopwatch();
        IEnumerable<string> docsID = ReadAllFiles(".\\contents\\20 Newsgroups\\20news-18828");

        LinkedList<IDocument> docs = new LinkedList<IDocument>();
        foreach (var item in docsID)
        {
           docs.AddLast(new Document(item, Parser.NewsgroupParser));
        }

        cloc.Start();
        ReadTest(docs);
        cloc.Stop();

        Console.WriteLine($"leer documentos solamente cuesta: {cloc.Elapsed}");
        cloc.Reset();

        cloc.Start();
        VSM vectorial = new VSM(docs);
        cloc.Stop();

        Console.WriteLine($"construir el modelo cuesta: {cloc.Elapsed}");
        cloc.Reset();

        SRIVectorDic<string, IWeight> query = new SRIVectorDic<string, IWeight>();

        QueryVSMWeight hoW = new QueryVSMWeight(2, 2);
        query.Add("house", hoW);

        cloc.Start();
        SearchItem[] results = vectorial.Ranking(vectorial.GetSearchItems(query, 30));
        cloc.Stop();

        Console.WriteLine($"buscar en el modelo cuesta: {cloc.Elapsed}");

        foreach (var item in results.Select(x => x.Title))
        {
            System.Console.WriteLine(SearchItem.Convert(item));
        }
    }

    

    public static void Main(string[] args)
    {
        string pattern = "abdeabcf";
        foreach (var item in LazyKMP.ComputePrefixFunction(pattern))
        {
            Console.Write(item.ToString() + " ,");
        }
        Console.WriteLine();

        var matcher = new LazyKMP(pattern);
        while(true)
        {
            string text = Console.ReadLine();
            Console.WriteLine(matcher.Match(text));
            
        }

    }


}


// string s ="From: hgomez@magnus.acs.ohio-state.edu (Humberto L Gomez)\nSubject: MULTISYNC 3D NEC MONITOR FOR SALE\n\n\nI have an NEC multisync 3d monitor for sale. great condition. looks new. it is\n.28 dot pitch\nSVGA monitor that syncs from 15-38khz\n\nit is compatible with all aga amiga graphics modes.\nleave message if interested. make an offer.\n-- ";
// ParsedInfo a = Parser.NewsgroupParser(doc);


// System.Console.WriteLine(a.TitleInit);
// System.Console.WriteLine(a.TitleLen);
// System.Console.WriteLine(a.TextLen);
// System.Console.WriteLine(a.SnippetLen);