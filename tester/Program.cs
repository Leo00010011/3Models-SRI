// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using DP;
using DP.Interface;
using SRI;
using SRI.Interface;
using Utils;
using System.Text.RegularExpressions;
using System.Collections;

namespace Test;




public class CranDocSpliter : IEnumerable<IDocument>
{
    StreamReader reader;

    public CranDocSpliter(string cran_path)
    {
        
    }

    public IEnumerator<IDocument> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}

public class CranDocSpliterEnumerator : IEnumerator<IDocument>
{
    public IDocument Current => throw new NotImplementedException();

    object IEnumerator.Current => this.Current;

    StreamReader sharedReader = null;

    readonly string cran_path;

    public CranDocSpliterEnumerator(string cran_path)
    {
        this.cran_path = cran_path;
    }

    public void Dispose()
    {
        if(sharedReader != null)
        {
            sharedReader.Dispose();
            sharedReader = null;
        }
    }

    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    class CranDocument : IDocument
    {
        public string Id => throw new NotImplementedException();

        public IEnumerable<char> Name => throw new NotImplementedException();

        public IEnumerator<char> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<char> GetSnippet(int snippetLen)
        {
            throw new NotImplementedException();
        }

        public stateDoc GetState()
        {
            throw new NotImplementedException();
        }

        public void UpdateDateTime()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
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

    public static ParsedInfo DummyParser(IEnumerable<char> text)
    {
        return new ParsedInfo(0,0,0,0,0);
    }

    public static void shiftLeft(char[] arr)
    {
        for (int i = 0; i < arr.Length - 1; i++)
        {
            arr[i] = arr[i+1];
        }
    }

    public static void PrintTill(IEnumerable<char> text,int large)
    {
        Console.WriteLine(String.Concat(text.Take(large)));
    }

    public static IEnumerable<string> TestMatchStep(ILazyMatcher matcher,IEnumerable<char> text, int length)
    {
        char[] arr = new char[length];
        int index = 0;
        int iter = 0;
            foreach(char step in text)
            {
                iter++;
                Console.WriteLine(iter);
                if(matcher.MatchStep(step))
                {
                    arr[index] = step;
                    index++;
                    if(matcher.AtFinalState)
                    {
                        yield return new String(arr);
                    }
                }
                else
                {
                    index = 0;
                }
            }
    }

    public static void ProbeOfPattern(IEnumerable<char> cran)
    {
        int skip = 1042;
        int take = 1048;
        string text = String.Concat(cran.Take(take).Skip(skip));
        var temp = text.ToArray();
        Console.WriteLine(text);
    }

    public static void Main(string[] args)
    {
        using(var doc_reader = new StreamReader(@"C:\Users\Leo pc\Desktop\SRI\Test Collections\cran\cran.all.1400"))
        {
            Console.WriteLine(doc_reader.BaseStream);
        }
        
        //var doc = new Document("C:\\Users\\Leo pc\\Desktop\\SRI\\Test Collections\\cran\\cran.all.1400",DummyParser);        
        //var matcher = new EndCranMatcher();
        
    }


}


// string s ="From: hgomez@magnus.acs.ohio-state.edu (Humberto L Gomez)\nSubject: MULTISYNC 3D NEC MONITOR FOR SALE\n\n\nI have an NEC multisync 3d monitor for sale. great condition. looks new. it is\n.28 dot pitch\nSVGA monitor that syncs from 15-38khz\n\nit is compatible with all aga amiga graphics modes.\nleave message if interested. make an offer.\n-- ";
// ParsedInfo a = Parser.NewsgroupParser(doc);


// System.Console.WriteLine(a.TitleInit);
// System.Console.WriteLine(a.TitleLen);
// System.Console.WriteLine(a.TextLen);
// System.Console.WriteLine(a.SnippetLen);