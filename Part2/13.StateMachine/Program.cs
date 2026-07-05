/*
O factură trece prin stări: Draft → Trimisă → Plătită sau Draft → Trimisă → Anulată. Scrie o funcție/clasă care validează dacă o tranziție e permisă și respinge tranzițiile invalide (ex: din Plătită direct în Draft).
*/

/// Testează: modelare de stare (enum + matrice/dicționar de tranziții valide), gândire defensivă
/// Hint: Dictionary<Status, HashSet<Status>> cu tranzițiile valide per stare curentă; discută unde pui validarea (domain layer, nu doar UI)


// Available states: Draft, Trimis, Platit, Anulat
// Draft Trimis Platit
// Draft Trimis Anulat

// din Draft merge doar Trimis
// din Trimis merge doar Platit sau Anulat
// din Platit nu merge nimic
// din Anulat nu merge nimic 

public enum StatusFactura { DRAFT, TRIMIS, PLATIT, ANULAT } //as putea sa fac in functie de valoarea din enum. 
// Draft fiind cel mai mic, inseamna ca orice actiune are voie doar sa treaca la ceva mai mare, nu la ceva mai mic decat valoarea curenta. 
// Edge case: pe regula asta Platit poate merge in Anulat, dar nu si invers.
// Dar cand cresc, inseamna ca pot creste de la draft la platit si anulat fara sa trec prin trimis, deci nu e corect
public class SistemFacturi
{
    private readonly Dictionary<StatusFactura, HashSet<StatusFactura>> _allowedTransitions = new();

    public SistemFacturi()
    {
        _allowedTransitions.Add(StatusFactura.DRAFT, new HashSet<StatusFactura>());
        _allowedTransitions[StatusFactura.DRAFT].Add(StatusFactura.TRIMIS);

        _allowedTransitions.Add(StatusFactura.TRIMIS, new HashSet<StatusFactura>());
        _allowedTransitions[StatusFactura.TRIMIS].Add(StatusFactura.PLATIT);
        _allowedTransitions[StatusFactura.TRIMIS].Add(StatusFactura.ANULAT);

        _allowedTransitions.Add(StatusFactura.PLATIT, new HashSet<StatusFactura>());

        _allowedTransitions.Add(StatusFactura.ANULAT, new HashSet<StatusFactura>());
    }

    public IReadOnlySet<StatusFactura> GetAllowedTransitions(StatusFactura statusFactura)
    {
        return _allowedTransitions[statusFactura];
    }

}

public class Factura
{
    private readonly SistemFacturi _sistemFacturi;
    private string _clientId;
    public StatusFactura Status { get; private set; } = StatusFactura.DRAFT;

    public Factura(string clientId, SistemFacturi sistemFacturi)
    {
        _clientId = clientId;
        _sistemFacturi = sistemFacturi;
    }

    public bool EsteTranzitiaPermisa(StatusFactura statusNou) => _sistemFacturi.GetAllowedTransitions(Status).Contains(statusNou);

    public void Tranzitioneaza(StatusFactura statusNou)
    {
        if(!EsteTranzitiaPermisa(statusNou))
        {
            throw new InvalidOperationException($"Tranzitia din {Status} in {statusNou} nu este permisa");
        }

        Status = statusNou;
    }


}

// trebuie modelata cumva legatura uni directionala dintre stari
// o stare poate sa tranzitioneze la mai multe stari, dar acele stari sunt unice in acel set


class Program
{
    static void Main(string[] args)
    {
        SistemFacturi sistemFacturi = new SistemFacturi();
        Factura factura1 = new Factura("C1", sistemFacturi);
        try
        {
            factura1.Tranzitioneaza(StatusFactura.TRIMIS);
            factura1.Tranzitioneaza(StatusFactura.PLATIT);
            factura1.Tranzitioneaza(StatusFactura.ANULAT);
        } 
        catch(Exception exc)
        {
            Console.WriteLine(exc.Message);
        }
    }
}