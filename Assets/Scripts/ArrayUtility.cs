
public static class ArrayUtility
{
    /// <summary>
    /// Takes an array and shuffles it contents using the Fisher-Yates method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="seed">Specify which Seed to use for random operations</param>
    /// <returns></returns>
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++)
        {           
            int randomIndex = prng.Next(i, array.Length);

            T tempItem = array[randomIndex];

            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }
}