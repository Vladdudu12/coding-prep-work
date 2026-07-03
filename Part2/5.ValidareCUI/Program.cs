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
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13) return false;
        if (string.IsNullOrEmpty(key) || key.Length != 12) return false;
        if (!cnp.All(char.IsDigit)) return false;

        var sumaTotala = 0;
        for (int i = 0; i < cnp.Length - 1; i++)
        {
            sumaTotala += (cnp[i] - '0') * (key[i] - '0');
        }

        var rest = sumaTotala % 11;
        rest = rest == 10 ? 1 : rest;

        return (cnp[12] - '0') == rest;
    }
}