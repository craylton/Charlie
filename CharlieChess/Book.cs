/*
 *	BOOK.C
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */

using System;
using System.IO;

namespace TSCP_Sharp_unsafe
{
    public partial class TSCP
    {
/* the opening book file */
        StreamReader book_file;
        Random randBook;

        void open_book()
        {
            randBook = new Random(DateTime.Now.Millisecond);
            if (File.Exists("book.txt"))
               book_file = File.OpenText("book.txt");
            else
               Console.WriteLine("Opening book missing."); 

        }

/* close_book() closes the book file. This is called when the program exits. */
        void close_book()
        {
            if (book_file != null)
            {
                book_file.Close();
                book_file = null;
            }
        }

/* book_move() returns a book move (in integer format) or -1 if there is no
book move. */

        long book_move()
        {
        string line = string.Empty;
        string book_line = string.Empty;
        long i, j, m;
        long[] move= new long[50];  /* the possible book moves */
        long[] count= new long[50];  /* the number of occurrences of each move */
        long moves = 0;
        long total_count = 0;

        if (book_file==null || hply > 25)
        return -1;

        /* line is a string with the current line, e.g., "e2e4 e7e5 g1f3 " */
        j = 0;
        for (i = 0; i < hply; ++i)
            line+= move_str(hist_dat[i].m.b)+' ';

        book_file.BaseStream.Seek(0, SeekOrigin.Begin);
        /* compare line to each line in the opening book */
        while ((book_line = book_file.ReadLine()) != null)   
        {
            if (book_match(line.Trim(), book_line)) 
            {

	            /* parse the book move that continues the line */
                m =  line.Length>book_line.Length ? -1: parse_move(book_line.Substring(line.Length));
	            if (m == -1)
		            continue;
	            m = gen_dat[m].m.u;

	            // add the book move to the move list, or update the move's
		        //    count */
	            for (j = 0; j < moves; ++j)
		            if (move[j] == m) {
			            ++count[j];
			            break;
		            }
	            if (j == moves) {
		            move[moves] = m;
		            count[moves] = 1;
		            ++moves;
	            }
	            ++total_count;
            }
        }

        /* no book moves? */
        if (moves == 0)
        return -1;

        // Think of total_count as the set of matching book lines.
        //Randomly pick one of those lines (j) and figure out which
        //move j "corresponds" to. */
        j = randBook.Next() % total_count;
        for (i = 0; i < moves; ++i) {
        j -= count[i];
        if (j < 0)
	        return move[i];
        }
        return -1;  /* shouldn't get here */
        }

/* book_match() returns TRUE if the first part of s2 matches s1. */
        bool book_match(string s1, string s2)
{
	int i;
	for (i = 0; i < s1.Length; ++i)
		if (i == s2.Length || s2[i] != s1[i])
			return false;
	return true;
}
    }
}
