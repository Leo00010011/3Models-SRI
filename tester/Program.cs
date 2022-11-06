// See https://aka.ms/new-console-template for more information
using Utils;
using DP;

namespace Test;
public static class TestingMethods
{

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
    public static void Main(string[] args)
    {
        Console.WriteLine("###############################################################################");
        IEnumerable<char> doc = new Document("C:\\Users\\Leo pc\\Desktop\\SRI\\Test Collections\\20 Newsgroups\\20news-18828\\alt.atheism\\49960");
        var result = Utils.Parser.NewsgroupParser(doc);
        Print(doc.Skip(result.TitleInit).Take(result.TitleLen));
    }
}


// string s ="From: hgomez@magnus.acs.ohio-state.edu (Humberto L Gomez)\nSubject: MULTISYNC 3D NEC MONITOR FOR SALE\n\n\nI have an NEC multisync 3d monitor for sale. great condition. looks new. it is\n.28 dot pitch\nSVGA monitor that syncs from 15-38khz\n\nit is compatible with all aga amiga graphics modes.\nleave message if interested. make an offer.\n-- ";
// ParsedInfo a = Parser.NewsgroupParser(doc);


// System.Console.WriteLine(a.TitleInit);
// System.Console.WriteLine(a.TitleLen);
// System.Console.WriteLine(a.TextLen);
// System.Console.WriteLine(a.SnippetLen);


