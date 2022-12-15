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
var qrels = new Dictionary<string, Dictionary<string, double>>();
foreach (var query in queries_file!.Values)
{
    var query_dic = new Dictionary<string, double>();
    var items = model.GetSearchItems(model.CreateQuery(query.text), 30);
    var max = items.Max(x => x.Score);
    foreach (var item in items)
    {
        query_dic.Add(item.URL, item.Score);
    }
    qrels.Add(query.query_id, query_dic);
}

var file = File.CreateText(@".\qrels_save");
file.WriteLine(JsonSerializer.Serialize(qrels, typeof(Dictionary<string, Dictionary<string, double>>)));
file.Close();

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