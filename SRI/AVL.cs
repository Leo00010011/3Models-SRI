using System.Collections;
using System.Diagnostics.CodeAnalysis;

class AVLNode<K, V> : IEnumerable<KeyValuePair<K, V>> where K : notnull, IComparable<K>
{
    public AVLNode(KeyValuePair<K, V> pair)
    {
        this.Key = pair.Key;
        this.Value = pair.Value;
        this.Left = null;
        this.Right = null;
    }

    public AVLNode((K Key, V Value) pair)
    {
        this.Key = pair.Key;
        this.Value = pair.Value;
        this.Left = null;
        this.Right = null;
    }

    public AVLNode(K Key, V Value)
    {
        this.Key = Key;
        this.Value = Value;
        this.Left = null;
        this.Right = null;
    }

    public K Key { get; private set; }

    public V Value { get; private set; }

    public AVLNode<K, V>? Left { get; set; }

    public AVLNode<K, V>? Right { get; set; }

    public int Count => (Left != null ? 1 : 0) + (Right != null ? 1 : 0);

    public int Valanced
    {
        get
        {
            int count = 0;
            if (Left != null)
                count -= Left.Valanced;
            if (Right != null)
                count += Right.Valanced;
            return count;
        }
    }

    private IEnumerable<KeyValuePair<K, V>> iter()
    {
        if (Left != null)
        {
            foreach (var item in Left.iter())
            {
                yield return item;
            }
        }

        yield return new KeyValuePair<K, V>(this.Key, this.Value);

        if (Right != null)
        {
            foreach (var item in Right.iter())
            {
                yield return item;
            }
        }
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => iter().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

class AVL<K, V> : IDictionary<K, V>, IEnumerable<KeyValuePair<K, V>> where K : notnull, IComparable<K>
{
    AVLNode<K, V>? root;

    public ICollection<K> Keys => throw new NotImplementedException();

    public ICollection<V> Values => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public V this[K key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public AVL(AVLNode<K, V> root) => this.root = root;

    public AVL((K, V) root) => this.root = new(root);

    public AVL(K Key, V Value) => this.root = new(Key, Value);

    public AVL() => this.root = null;

    // public bool Add(KeyValuePair<K, V> pair) => Add(pair.Key, pair.Value);

    // public bool Add((K Key, V Value) pair) => Add(pair.Key, pair.Value);

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(K key, V value)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(K key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(K key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
    {
        throw new NotImplementedException();
    }

    void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<K, V> item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<K, V> item)
    {
        throw new NotImplementedException();
    }
}