/*
 *	MAIN.C
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */
using System;

namespace TSCP_Sharp_unsafe
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
              using (TSCP tscp = new TSCP());
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }    
        }
    }
}
