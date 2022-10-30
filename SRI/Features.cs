using SRI.Interface;

public static class Extension
{
    public static double Scalar_Mult<K>(this ISRIVector<K, double> item1, ISRIVector<K, double> item2) => item1.GetKeys().Sum(x => item1[x] * item2[x]);

    public static double Norma2<K>(this ISRIVector<K, double> item) => Math.Pow(item.Sum(x => Math.Pow(x, 2)), 0.5);
}