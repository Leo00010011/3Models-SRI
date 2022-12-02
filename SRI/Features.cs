using SRI.Interface;

public static class Extension
{
    /// <summary>
    /// Método para calcular la multiplicación escalar de dos vectores SRI
    /// </summary>
    /// <typeparam name="K">llave del vector SRI</typeparam>
    /// <param name="item1">vector actual</param>
    /// <param name="item2">vector con el que se va a realizar el cálculo</param>
    /// <returns>retorna el valor de la múltiplicación</returns>
    public static double Scalar_Mult<K>(this IDictionary<K, double> item1, IDictionary<K, double> item2) where K : notnull => item1.Sum(x => x.Value * item2[x.Key]);

    /// <summary>
    /// Método para calcular la norma 2 de un vector SRI
    /// </summary>
    /// <typeparam name="K">llave del vector SRI</typeparam>
    /// <param name="item">vector actual</param>
    /// <returns>retorna el valor de la norma calculada</returns>
    public static double Norma2<K>(this IDictionary<K, double> item) where K : notnull => Math.Pow(item.Sum(x => Math.Pow(x.Value, 2)), 0.5);
}