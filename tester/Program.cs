using DP;
using DP.Interface;
using System.Text.Json;
using SRI;
using System.Diagnostics;

// namespace Test;
// public static class TestingMethods
// {

// }


var directories = Utils.Utils.ReadAllFiles(@"contents\Reuters\");

IEnumerable<IDocument> temp = new CollectionSplitter(@"contents\Reuters\reut2-000.sgm",new EndReutersMatcherCreator(),Utils.Parser.ReutersParser);
foreach(var doc_path in directories.Skip(1))
{
    temp = temp.Concat(new CollectionSplitter(doc_path,new EndReutersMatcherCreator(),Utils.Parser.ReutersParser));
}
var list1 = new LinkedList<IDocument>();

foreach(var doc in  temp)
{
    var temp2 = doc;
    var temp3 = new ProcesedDocument(doc).Length;
    System.Console.WriteLine();
    list1.AddLast(doc);
}
System.Console.WriteLine("End of Doc");

Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
var list2 = new LinkedList<IDocument>();
foreach(var doc in  temp)
{
    var temp2 = doc;
    // string temp3 = String.Concat(doc);
    list2.AddLast(doc);
}
System.Console.WriteLine("End of Doc");

var current1 = list1.First;
var current2 = list2.First;
int count = 0;
System.Console.WriteLine(list1.Count);
System.Console.WriteLine(list2.Count);
while(current1 != null && current2 != null)
{
    if(current1.Value != current2.Value)
    {
        Console.WriteLine("distintos");
        break;
    }
    current1 = current1.Next;
    current2 = current2.Next;
    count++;
}

System.Console.WriteLine(count);
Console.WriteLine("termino");

// public class Program
// {
//     public static void Print(IEnumerable<char> text)
//     {
//         foreach (char c in text)
//         {
//             if (c == '\n')
//             {
//                 Console.WriteLine();
//             }
//             else
//             {
//                 Console.Write(c);
//             }
//         }
//     }

//     public static IEnumerable<string> ReadAllFiles(string path)
//     {
//         foreach (var item in Directory.EnumerateFiles(path))
//         {
//             yield return item;
//         }
//         foreach (var docs in Directory.EnumerateDirectories(path))
//         {
//             foreach (var item in ReadAllFiles(docs))
//             {
//                 yield return item;
//             }
//         }
//     }

//public static void Main(string[] args)
//{
// Stopwatch cloc = new Stopwatch();
// IEnumerable<string> docsID = ReadAllFiles(".\\contents\\20 Newsgroups\\20news-18828");

// LinkedList<IDocument> docs = new LinkedList<IDocument>();
// foreach (var item in docsID)
// {
//    docs.AddLast(new Document(item, Parser.NewsgroupParser));
// }

// cloc.Start();
// ReadTest(docs);
// cloc.Stop();

// Console.WriteLine($"leer documentos solamente cuesta: {cloc.Elapsed}");
// cloc.Reset();

// cloc.Start();
// WMTermDoc vectorial = new WMTermDoc(docs);
// cloc.Stop();

// Console.WriteLine($"construir el modelo cuesta: {cloc.Elapsed}");
// cloc.Reset();

// SRIVectorDic<string, IWeight> query = new SRIVectorDic<string, IWeight>();

// QueryVSMWeight hoW = new QueryVSMWeight(2, 2);
// query.Add("house", hoW);

// cloc.Start();
// SearchItem[] results = vectorial.Ranking(vectorial.GetSearchItems(query, 30));
// cloc.Stop();

// Console.WriteLine($"buscar en el modelo cuesta: {cloc.Elapsed}");

// foreach (var item in results.Select(x => x.Title))
// {
//     System.Console.WriteLine(SearchItem.Convert(item));
// }
//}

//     private static char ReadTest(IEnumerable<IDocument> docs)
//     {
//         char a = '0';
//         foreach (var doc in docs)
//         {
//             foreach (var item in doc)
//             {
//                 a = item;
//             }
//         }
//         return a;
//     }
// }

// var file = File.CreateText(@".\qrels_save");
// file.WriteLine(JsonSerializer.Serialize(qrels, typeof(Dictionary<string, Dictionary<string, double>>)));
// file.Close();

// string s ="From: hgomez@magnus.acs.ohio-state.edu (Humberto L Gomez)\nsubject: MULTISYNC 3D NEC MONITOR FOR SALE\n\n\nI have an NEC multisync 3d monitor for sale. great condition. looks new. it is\n.28 dot pitch\nSVGA monitor that syncs from 15-38khz\n\nit is compatible with all aga amiga graphics modes.\nleave message if interested. make an offer.\n-- ";
// ParsedInfo a = Parser.NewsgroupParser(s);

// System.Console.WriteLine("-----Newsgroup-----");
// System.Console.WriteLine($"Title init: {a.TitleInit}");
// System.Console.WriteLine($"Title Len: {a.TitleLen}");
// System.Console.WriteLine($"Text Init: {a.TextInit}");
// System.Console.WriteLine($"Snippet init: {a.SnippetInit}");

// s= "<title>BAHIA COCOA REVIEW</title><DATELINE>    SALVADOR, Feb 26 - </DATELINE><body>Showers continued throughout the week inthe Bahia cocoa zone, alleviating the drought since earlyJanuary and improving prospects for the coming temporao,although normal humidity levels have not been restored,Comissaria Smith said in its weekly review.";
// a = Parser.ReutersParser(s);

// System.Console.WriteLine("-----Routers-----");
// System.Console.WriteLine($"Title init: {a.TitleInit}");
// System.Console.WriteLine($"Title Len: {a.TitleLen}");
// System.Console.WriteLine($"Text Init: {a.TextInit}");
// System.Console.WriteLine($"Snippet init: {a.SnippetInit}");

// s= ".I 1.t\nexperimental investigation of the aerodynamics of awing in a slipstream .\n.a\nbrenckman,m..Bj. ae. scs. 25, 1958, 324. .w\nexperimental investigation of the aerodynamics of a wing in a slipstream . an experimental study of a wing in a propeller slipstream was made in order to determine the spanwise distribution of the lift increase due to slipstream at different angles of attack of the wing.";
// a = Parser.CranParser(s);

// System.Console.WriteLine("-----Cran-----");
// System.Console.WriteLine($"Title init: {a.TitleInit}");
// System.Console.WriteLine($"Title Len: {a.TitleLen}");
// System.Console.WriteLine($"Text Init: {a.TextInit}");
// System.Console.WriteLine($"Snippet init: {a.SnippetInit}");

// ISRIVector<string, int> query = BSMTermDoc.CreateQuery("tony & !homosexuality");
// bool[] x = new bool[2];
// x[0] = true;
// x[1] = true;

// string corpus_path = "C:\\Users\\User\\Desktop\\Test Collections\\Test Collections\\20 Newsgroups\\20news-18828\\talk.religion.misc";
// IEnumerable<string> directories = Utils.Utils.ReadAllFiles(corpus_path);
// LinkedList<IDocument> docs = new LinkedList<IDocument>();
// foreach (var item in directories)
// {
//     docs.AddLast(new DP.Document(item, Parser.NewsgroupParser));
// }
// BSMTermDoc booleanModel = new BSMTermDoc(docs);
// ISearchResult result = new SearchResult(booleanModel.Ranking(booleanModel.GetSearchItems(query, 300)), "");

// foreach (var item in result)
// {
//     System.Console.WriteLine($"{SearchItem.Convert(item.Title)} => {item.Score}");
// }


// Stopwatch cloc = new Stopwatch();
// IEnumerable<string> docsID = Utils.Utils.ReadAllFiles(@"D:\Studio\SRI\3Models-SRI\contents\20news");

// LinkedList<IDocument> docs1 = new LinkedList<IDocument>();
// foreach (var item in docsID)
// {
//     docs1.AddLast(new Document(item, Parser.NewsgroupParser));
// }

// IEnumerable<IDocument> docs = docs1.Concat(new CollectionSplitter(@"D:\Studio\SRI\3Models-SRI\contents\Cran\cran.all.1400", new EndCranMatcherCreator(), Parser.CranParser));

// docsID = Utils.Utils.ReadAllFiles(@"D:\Studio\SRI\3Models-SRI\contents\Reuters");

// foreach (var item in docsID)
// {
//     docs = docs.Concat(new CollectionSplitter(item, new EndReutersMatcherCreator(), Parser.ReutersParser));
// }

// cloc.Start();
// ReadTest(docs);
// cloc.Stop();

// Console.WriteLine($"leer documentos solamente cuesta: {cloc.Elapsed}");
// cloc.Reset();

// cloc.Start();
// VSMDocTerm vectorial = new VSMDocTerm(docs);
// cloc.Stop();

// Console.WriteLine($"construir el modelo cuesta: {cloc.Elapsed}");
// cloc.Reset();

// cloc.Start();
// SearchItem[] results = vectorial.Ranking(vectorial.GetSearchItems(vectorial.CreateQuery("tony gay"), 30));
// cloc.Stop();

// Console.WriteLine($"buscar en el modelo cuesta: {cloc.Elapsed}");

// foreach (var item in results)
// {
//     System.Console.WriteLine(item.Snippet);
//     System.Console.WriteLine(item.GetText());
// }

// char ReadTest(IEnumerable<IDocument> docs)
// {
//     char a = '0';
//     foreach (var doc in docs)
//     {
//         foreach (var item in doc)
//         {
//             a = item;
//         }
//     }
//     return a;
// }

// System.Console.ReadKey();

//var docs_file = JsonSerializer.Deserialize(File.ReadAllText(@".\docs_save"), typeof(Dictionary<string, Doc>)) as Dictionary<string, Doc>;
//var queries_file = JsonSerializer.Deserialize(File.ReadAllText(@".\queries_save"), typeof(Dictionary<string, Query>)) as Dictionary<string, Query>;
//
//LinkedList<IDocument> list = new LinkedList<IDocument>();
//foreach (var item in docs_file!.Values)
//{
//    list.AddLast(new CranJsonDocument(item.doc_id, item.title, item.text));
//}
//
//var model = new GVSMTermDoc(list);
//var qrels = new Dictionary<string, Dictionary<string, double>>();
//foreach (var query in queries_file!.Values)
//{
//    var query_dic = new Dictionary<string, double>();
//    var items = model.GetSearchItems(model.CreateQuery(query.text), 30);
//    foreach (var item in items)
//    {
//        query_dic.Add(item.URL, item.Score);
//    }
//    qrels.Add(query.query_id, query_dic);
//}
//
//var file = File.CreateText(@".\qrels_save");
//file.WriteLine(JsonSerializer.Serialize(qrels, typeof(Dictionary<string, Dictionary<string, double>>)));
//file.Close();

struct Doc
{
    public string doc_id { get; set; }
    public string title { get; set; }
    public string text { get; set; }

    public Doc(string doc_id, string title, string text)
    {
        this.doc_id = doc_id;
        this.title = title;
        this.text = text;
    }
}

struct Query
{
    public string query_id { get; set; }
    public string text { get; set; }

    public Query(string query_id, string text)
    {
        this.query_id = query_id;
        this.text = text;
    }
}