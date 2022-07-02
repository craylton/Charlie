using Charlie.BoardRepresentation;
using Charlie.Hash;
using System.Threading.Tasks;

namespace Charlie;

public static class Program
{
    private static async Task Main(string[] args)
    {
        Zobrist.Initialise();
        Magics.Initialise();

        var uci = new Uci();
        uci.Initialise();
        await uci.Loop();
    }
}
