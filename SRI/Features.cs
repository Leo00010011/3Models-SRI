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
    public static double Scalar_Mult<K>(this ISRIVector<K, double> item1, ISRIVector<K, double> item2) => item1.GetKeys().Sum(x => item1[x] * item2[x]);

    /// <summary>
    /// Método para calcular la norma 2 de un vector SRI
    /// </summary>
    /// <typeparam name="K">llave del vector SRI</typeparam>
    /// <param name="item">vector actual</param>
    /// <returns>retorna el valor de la norma calculada</returns>
    public static double Norma2<K>(this ISRIVector<K, double> item) => Math.Pow(item.Sum(x => Math.Pow(x, 2)), 0.5);
}