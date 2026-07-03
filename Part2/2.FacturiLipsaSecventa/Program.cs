/*
Facturile au numerotare secvențială (ex: SOLO-0001, SOLO-0002...). Dându-se o listă de numere de facturi emise, găsește ce numere lipsesc (posibil semn de facturi șterse/needitate corect).
*/

/// Testează: manipulare de array/set, edge cases (secvențe goale, un singur element)
/// Hint: HashSet pentru O(1) lookup, iterează de la min la max

class Program
{
    static void Main(string[] args)
    {
        List<string> numereFacturi = new List<string>{"SOLO-0001","SOLO-0002","SOLO-0003","SOLO-0005","SOLO-0008","SOLO-0009"};
        HashSet<int> sequenceNumbers = new HashSet<int>();

        // Console.WriteLine(Int32.Parse(numereFacturi[numereFacturi.Count - 1].Split('-')[1]));
        for (int j = 0; j < numereFacturi.Count; j++)
        {
            sequenceNumbers.Add(Int32.Parse(numereFacturi[j].Split('-')[1]));
        }
        var missingNumbers = new List<int>();

        for (int i = sequenceNumbers.Min(); i <= sequenceNumbers.Max(); i++)
        {
            if(!sequenceNumbers.Contains(i))
            {
                missingNumbers.Add(i);
            }
        }
    
        foreach (var number in missingNumbers)
        {
            Console.WriteLine($"Missing {number}");
        }

    }
}