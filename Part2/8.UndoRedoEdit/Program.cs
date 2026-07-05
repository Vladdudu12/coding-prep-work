/*
Utilizatorul poate edita o factură (linii, sume, client) și trebuie să poată face undo/redo pe modificări
*/

/// Testează: two-stack pattern (undo stack + redo stack), design de API curat
/// Hint: fiecare acțiune salvează starea anterioară (sau un "diff"); discută trade-off snapshot complet vs diff


// undo stack
// redo stack

// incepem cu ambele goale



// stareaCurenta
// metoda undo - returneaza undo state
// metoda undo modifica 
// redo.addFirst stareaCurenta 
// stareaCurenta = primul element din stack de undo. 


// metoda redo - returneaza redo state
// metoda redo modifica 
// undo.addFirst stareaCurenta 
// stareaCurenta = primul element din stack de redo. 
public record Factura(string Linii, decimal Sume, string ClientId);

public class VersionSystem
{
    private Factura _currentState;
    private Stack<Factura> _undoStack = new();
    private Stack<Factura> _redoStack = new();

    public Factura RedoAction()
    {
        if (_redoStack.Count > 0)
        {
            _undoStack.Push(_currentState);
            _currentState = _redoStack.Pop();
        }

        return _currentState;
    }

    public Factura UndoAction()
    {
        if (_undoStack.Count > 0)
        {
            _redoStack.Push(_currentState);
            _currentState = _undoStack.Pop();
        }

        return _currentState;
    }

    public void AddFactura(Factura factura) // Echivalent POST
    {
        if (_currentState == null)
        {
            _undoStack.Push(_currentState);
            _currentState = factura;
        }
    }

    public void EditFactura(Factura factura) //Echivalent PUT
    {
        _undoStack.Push(_currentState);
        _currentState = factura;
        _redoStack.Clear();
    }

    public Factura GetFactura() // Echivalent GET (ar trebui cu ClientId, dar am optat sa existe doar o singura factura in tot sistemul)
    {
        return _currentState;
    }
}
class Program
{
    static void Main(string[] args)
    {
        VersionSystem versionSystem = new VersionSystem();
        versionSystem.AddFactura(new Factura("dasdhdasldasdsalksadhas", 100m, "C1"));
        versionSystem.EditFactura(new Factura("aaaa", 500m, "C2"));

        Console.WriteLine(versionSystem.GetFactura());
        Console.WriteLine();
        versionSystem.UndoAction();
        Console.WriteLine(versionSystem.GetFactura());
        Console.WriteLine();
 
        versionSystem.RedoAction();
        Console.WriteLine(versionSystem.GetFactura());
        Console.WriteLine();
    }
}