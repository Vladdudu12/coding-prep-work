/*
Implementează algoritmul de validare a cifrei de control pentru CNP-ul românesc (sau CUI, dacă vrei ceva mai simplu — CUI are propriul algoritm de validare cu cheia 753217532).
*/

/// Testează: manipulare de string/digit, atenție la edge cases (lungime greșită, caractere non-numerice)
/// Hint: înmulțește fiecare cifră cu constanta corespunzătoare din cheie, suma modulo 11

class Program
{
    static void Main(string[] args)
    {
        const string sablon = "279146358279";
        const string CNP = "299021946900";
        var result = validareCifraControl(CNP, sablon);

        Console.WriteLine(result);
    }

    static bool validareCifraControl(string cnp, string key)
    {
        if (cnp.Length != 13 && key.Length != 12) return false;

        var sumaTotala = 0;
        for (int i = 0; i < cnp.Length - 1; i++)
        {
            sumaTotala += Int32.Parse(cnp[i].ToString()) * Int32.Parse(key[i].ToString());
        }
        
        var rest = sumaTotala % 11;
        rest = rest == 10 ? 1 : rest;

        if (Int32.Parse(cnp[cnp.Length - 1].ToString()) == rest) return true;

        return false;
        
    }
}