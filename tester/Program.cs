using DP;
using DP.Interface;
using System.Text.Json;
using SRI;
using Utils;

var docs_file = JsonSerializer.Deserialize(File.ReadAllText(@".\docs_save"), typeof(Dictionary<string, Doc>)) as Dictionary<string, Doc>;
var queries_file = JsonSerializer.Deserialize(File.ReadAllText(@".\queries_save"), typeof(Dictionary<string, Query>)) as Dictionary<string, Query>;

LinkedList<IDocument> list = new LinkedList<IDocument>();
foreach (var item in docs_file!.Values)
{
    list.AddLast(new CranJsonDocument(item.doc_id, item.title, item.text));
}

var model = new VSMTermDoc(list);
var relevance = new double[] { 0.122, 0.70, 0.210, 0.400, 0.198 };
var qrels = new Dictionary<string, Dictionary<string, int>>();
foreach (var query in queries_file!.Values)
{
    var query_dic = new Dictionary<string, int>();
    foreach (var item in model.GetSearchItems(model.CreateQuery(query.text), 30))
    {
        // -1 -> 0.122
        //  1 -> 0.70
        //  2 -> 0.210
        //  3 -> 0.400
        //  4 -> 0.198
        query_dic.Add(item.URL, relevance.Relevance(0, item.Score, (x, y) => x > y));
    }
    qrels.Add(query.query_id, query_dic);
}

JsonSerializer.Serialize(File.Create(@".\qrels_save"), qrels, typeof(Dictionary<string, Dictionary<string, int>>));

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