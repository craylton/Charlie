/*
 *	MAIN.C
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */

using System;

namespace CharlieChess
{
    public static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (Tscp tscp = new Tscp())
                {
                    // TSCP runs on its own
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
