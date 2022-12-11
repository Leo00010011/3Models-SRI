using System.Collections;
public class QuerySet
{
    int QID {get;}
    string query {get;}
    int tolerance;
    HashSet<string> relevant_documentsID;

    public QuerySet(string query, int queryID, int tolerance)
    {
        this.query = query;  
        this.QID = queryID;
        this.tolerance = tolerance;
        relevant_documentsID = new HashSet<string>();
    }
    public void AddRelDocID(string DocId, float value)
    {
        if(value >= tolerance)
        {
            relevant_documentsID.Add(DocId);
        }
    }
}

public abstract class MRIEvaluation
{
    List<QuerySet> queries;

    public abstract double Evaluate(int queriesIndex);
    public abstract double MediaEvaluate();

}

public class Precision : MRIEvaluation
{
    public override double Evaluate(int queriesIndex)
    {
        throw new NotImplementedException();
    }

    public override double MediaEvaluate()
    {
        throw new NotImplementedException();
    }
}

public class Reward : MRIEvaluation
{
    public override double Evaluate(int queriesIndex)
    {
        throw new NotImplementedException();
    }

    public override double MediaEvaluate()
    {
        throw new NotImplementedException();
    }
}
