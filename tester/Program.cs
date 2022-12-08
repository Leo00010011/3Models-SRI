﻿using System;
using System.Diagnostics;
using DP;
using DP.Interface;
using SRI;
using SRI.Interface;
using Utils;
using System.Text.RegularExpressions;
using System.Collections;



// namespace Test;
// public static class TestingMethods
// {

// }

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
// x[0]= true;
// x[1]= true;

// string corpus_path = "C:\\Users\\User\\Desktop\\Test Collections\\Test Collections\\20 Newsgroups\\20news-18828\\talk.religion.misc";
// IEnumerable<string> directories = Utils.Utils.ReadAllFiles(corpus_path);
// LinkedList<IDocument> docs = new LinkedList<IDocument>();
// foreach (var item in directories)
// {
//    docs.AddLast(new DP.Document(item, Parser.NewsgroupParser));
// }
// BSMTermDoc booleanModel = new BSMTermDoc(docs); 
// ISearchResult result = new SearchResult(booleanModel.Ranking(booleanModel.GetSearchItems(query,300)),""); 
       
// foreach (var item in result)
// {
//     System.Console.WriteLine($"{SearchItem.Convert(item.Title)} => {item.Score}");
// }
