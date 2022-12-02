namespace SRI;

enum TokenLexem{
    and, or, not, xor, double_implies, implies, word, opar, cpar, EOF
}
public abstract class Node<T> 
{
    public List<Node<T>> childs;

    public Node()
    {
        this.childs = new List<Node<T>>();
    }
    public virtual void add_child(Node<T> child)
    {
        this.childs.Add(child);
    }
    public virtual void add_child(IEnumerable<Node<T>> childs)
    {
        foreach (var item in childs)
        {
            this.childs.Add(item);
        }
    }
}

public abstract class BinaryNode:BooleanNode
{
    public abstract bool evaluate(bool left, bool rigth);

}

public abstract class UnaryNode:BooleanNode
{
    public abstract bool evaluate(bool value);
}
public class BooleanNode: Node<bool>
{

}
public class AndNode : BinaryNode
{
    public override bool evaluate(bool left, bool rigth)
    {
       return left & rigth;
    }
}
public class OrNode : BinaryNode
{
    public override bool evaluate(bool left, bool rigth)
    {
       return left | rigth;
    }
}
public class XorNode : BinaryNode
{
    public override bool evaluate(bool left, bool rigth)
    {
       return left ^ rigth;
    }
}
public class DoubleImplicationNode : BinaryNode
{
    public override bool evaluate(bool left, bool rigth)
    {
       return (left & rigth) | !(left | rigth);
    }
}
public class ImplicationNode : BinaryNode
{
    public override bool evaluate(bool left, bool rigth)
    {
       return !left | rigth;
    }
}
public class NotNode : UnaryNode
{
    public override bool evaluate(bool value)
    {
       return !value;
    }
}

