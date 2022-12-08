using System.Collections;

#nullable disable

namespace SRI;
public class TrieNode<T> : IEnumerable<TrieNode<T>> where T : notnull
{
    T Value { get; }
    public int index { get; set; }
    private Dictionary<T, TrieNode<T>> childs;

    public TrieNode(T val, int index = -1)
    {
        this.childs = new Dictionary<T, TrieNode<T>>();
        this.Value = val;
        this.index = index;
    }

    public void add_child(TrieNode<T> child)
    {

        if (this.childs.ContainsKey(child.Value)) return;
        this.childs.Add(child.Value, child);

    }

    public bool is_leaf_node() => this.childs == null || this.childs.Count == 0;

    public TrieNode<T> get_child(T key)
    {
        TrieNode<T> result;
        this.childs.TryGetValue(key, out result);
        return result;
    }

    public IEnumerator<TrieNode<T>> GetEnumerator() => this.childs.Select(x => x.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

public class Trie<T>
{
    TrieNode<T> root;
    int counter = 0;
    public Trie(T root_identifier)
    {
        root = new TrieNode<T>(root_identifier);
    }

    public int InsertTrie(IEnumerable<T> chain)
    {
        TrieNode<T> current = this.root;
        TrieNode<T> temp;
        foreach (var item in chain)
        {
            if (!current.is_leaf_node())
            {
                temp = current.get_child(item);
                if (temp != null)
                {
                    current = temp;
                    continue;
                }
            }
            temp = new TrieNode<T>(item);
            current.add_child(temp);
            current = temp;
        }
        current.index = current.index == -1 ? counter++ : current.index;
        return current.index;
    }
}
