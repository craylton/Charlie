/*
 *	SEARCH.C
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */

using System;

namespace CharlieChess
{
    public unsafe partial class Tscp
    {
        const bool GONULL = true;
        bool stop_search;

        /// <summary>
        /// Think() calls Search() iteratively. Search statistics are printed 
        /// depending on the value of output:
        /// 0 = no output
        /// 1 = normal output
        /// 2 = xboard format output
        /// </summary>
        private void Think(long output)
        {
            long i, j, x;

            /* try the opening book first */
            pvptr[0].u = BookMove();
            if (pvptr[0].u != -1)
                return;

            //some code that lets us longjmp back here and return
            //   from think() when our time is up */

            stop_search = false;
            start_time = Environment.TickCount;
            stop_time = start_time + max_time;

            ply = 0;
            nodes = 0;

            Array.Clear(pv, 0, pv.Length);
            Array.Clear(history, 0, history.Length);

            if (output == 1)
                Console.WriteLine("ply      nodes  score  pvptr");

            for (i = 1; i <= max_depth; ++i)
            {
                follow_pv = true;
                x = Search(-10000, 10000, i, true);
                if (stop_search)
                    break;

                if (output == 1)
                    Console.Write("{0,3}  {1,9}  {2,5} ", i, nodes, x);
                else if (output == 2)
                    Console.Write("{0} {1} {2} {3}",
                            i, x, (Environment.TickCount - start_time) / 10, nodes);
                if (output > 0)
                {
                    for (j = 0; j < pvl[0]; ++j)
                        Console.Write(" {0}", MoveStr(pvptr[0 * MAX_PLY + j].b));
                    Console.WriteLine();
                    Console.Out.Flush();
                }
                if (x > 9000 || x < -9000)
                    break;
            }
        }

        /// <summary>
        /// Search() does just that, in negamax fashion
        /// </summary>
        private long Search(long alpha, long beta, long depth, bool null_move)
        {
            long i, j, x;
            bool c, f;
            long nullmat;
            long o_side;
            long o_xside;
            long o_ep;
            long o_fifty;
            long o_hash;
            long o_castle;

            // we're as deep as we want to be; call quiesce() to get
            //   a reasonable score and return it. */
            if (depth <= 0)
                return Quiesce(alpha, beta);

            ++nodes;

            /* do some housekeeping every 1024 nodes */
            if ((nodes & 1023) == 0)
                Checkup();

            pvl[ply] = ply;

            // if this isn't the root of the search tree (where we have
            //   to pick a move and can't simply return 0) then check to
            //   see if the position is a repeat. if so, we can assume that
            //   this line is a draw and return 0. */
            if (ply > 0 && Reps() > 0)
                return 0;

            /* are we too deep? */
            if (ply >= MAX_PLY - 1)
                return Eval();
            if (hply >= HIST_STACK - 1)
                return Eval();

            /* are we in check? if so, we want to search deeper */
            c = InCheck(side);
            if (c) ++depth;

            /* null move */
            if (GONULL && !c && null_move && ply > 0)
            {
                nullmat = 0;
                for (i = 0; i < 64; ++i)
                {
                    if (ptr[i] != EMPTY && ptr[i] != PAWN && color[i] == side)
                        nullmat += piece_value[ptr[i]];
                }

                if (depth > (nullmat > 1500 ? 3 : 2))
                {
                    o_side = side;
                    o_xside = xside;
                    o_ep = ep;
                    o_fifty = fifty;
                    o_hash = hash;
                    o_castle = castle;
                    ep = -1;
                    fifty = 0;
                    side = xside;
                    xside = o_side;
                    x = -Search(-beta, -beta + 1, depth - 1 - (nullmat > 1500 ? 3 : 2), false);
                    side = o_side;
                    xside = o_xside;
                    ep = o_ep;
                    fifty = o_fifty;
                    hash = o_hash;
                    castle = o_castle;

                    if (stop_search)
                        return 0;

                    if (x >= beta)
                        return beta;
                }
            }

            Gen();
            if (follow_pv)  /* are we following the pvptr? */
                SortPV();

            f = false;

            /* loop through the moves */
            for (i = fm[ply]; i < fm[ply + 1]; ++i)
            {
                Sort(i);
                if (!MakeMove(ref gen_dat[i].m.b))
                    continue;

                f = true;
                x = -Search(-beta, -alpha, depth - 1, true);
                Takeback();
                if (stop_search)
                    return 0;

                if (x > alpha)
                {
                    //this move caused a cutoff, so increase the histo
                    //   value so it gets ordered high next time we can
                    //   search it */
                    histo[(long)gen_dat[i].m.b.from * 64 + gen_dat[i].m.b.to] += depth;
                    if (x >= beta)
                        return beta;
                    alpha = x;

                    /* update the pvptr */
                    pvptr[ply * MAX_PLY + ply] = gen_dat[i].m;
                    for (j = ply + 1; j < pvl[ply + 1]; ++j)
                        pvptr[ply * MAX_PLY + j] = pvptr[(ply + 1) * MAX_PLY + j];
                    pvl[ply] = pvl[ply + 1];
                }
            }

            /* no legal moves? then we're in checkmate or stalemate */
            if (!f)
            {
                if (c)
                    return -10000 + ply;
                else
                    return 0;
            }

            /* fifty move draw rule */
            if (fifty >= 100)
                return 0;

            return alpha;
        }

        /// <summary>
        /// Quiesce() is a recursive minimax search function with alpha-beta 
        /// cutoffs.In other words, negamax.It basically only searches capture 
        /// sequences and allows the evaluation function to cut the search off 
        /// (and set alpha). The idea is to find a position where there isn't a 
        /// lot going on so the static evaluation function will work.
        /// </summary>
        private long Quiesce(long alpha, long beta)
        {
            long i, j, x;

            ++nodes;

            /* do some housekeeping every 1024 nodes */
            if ((nodes & 1023) == 0)
                Checkup();

            pvl[ply] = ply;

            /* are we too deep? */
            if (ply >= MAX_PLY - 1)
                return Eval();
            if (hply >= HIST_STACK - 1)
                return Eval();

            /* check with the evaluation function */
            x = Eval();
            if (x >= beta)
                return beta;
            if (x > alpha)
                alpha = x;

            GenCaps();
            if (follow_pv)  /* are we following the pvptr? */
                SortPV();

            /* loop through the moves */
            for (i = fm[ply]; i < fm[ply + 1]; ++i)
            {
                Sort(i);
                if (!MakeMove(ref gen_dat[i].m.b))
                    continue;

                x = -Quiesce(-beta, -alpha);
                Takeback();
                if (stop_search)
                    return 0;

                if (x > alpha)
                {
                    if (x >= beta)
                        return beta;

                    alpha = x;

                    /* update the pvptr */
                    pvptr[ply * MAX_PLY + ply] = gen_dat[i].m;
                    for (j = ply + 1; j < pvl[ply + 1]; ++j)
                        pvptr[ply * MAX_PLY + j] = pvptr[(ply + 1) * MAX_PLY + j];

                    pvl[ply] = pvl[ply + 1];
                }
            }

            return alpha;
        }

        /// <summary>
        /// Returns the number of times the current position has been repeated.
        /// It compares the current value of hash to previous values.
        /// </summary>
        private long Reps()
        {
            long i;
            long r = 0;

            for (i = hply - fifty; i < hply; ++i)
                if (histdat[i].hash == hash)
                    ++r;
            return r;
        }

        /// <summary>
        /// SortPV() is called when the search function is following the 
        /// pvptr(Principal Variation). It looks through the current ply's move 
        /// list to see if the pvptr move is there. If so, it adds 10,000,000 to 
        /// the move's score so it's played first by the search function.If not, 
        /// follow_pv remains FALSE and Search() stops calling SortPV().
        /// </summary>
        private void SortPV()
        {
            long i;

            follow_pv = false;
            for (i = fm[ply]; i < fm[ply + 1]; ++i)
                if (gen_dat[i].m.u == pvptr[0 * MAX_PLY + ply].u)
                {
                    follow_pv = true;
                    gen_dat[i].score += 10000000;
                    return;
                }
        }

        /// <summary>
        /// Searches the current ply's move list from 'from' to the end to find 
        /// the move with the highest score. Then it swaps that move and the 
        /// 'from' move so the move with the highest score gets searched next, 
        /// and hopefully produces a cutoff.
        /// </summary>
        private void Sort(long from)
        {
            long i;
            long bs;  /* best score */
            long bi;  /* best i */
            GenT g;

            bs = -1;
            bi = from;
            for (i = from; i < fm[ply + 1]; ++i)
                if (gen_dat[i].score > bs)
                {
                    bs = gen_dat[i].score;
                    bi = i;
                }
            g = gen_dat[from];
            gen_dat[from] = gen_dat[bi];
            gen_dat[bi] = g;
        }

        /// <summary>
        /// Called once in a while during the search.
        /// </summary>
        private void Checkup()
        {
            //is the engine's time up? if so, longjmp back to the
            //   beginning of think() */
            if (Environment.TickCount >= stop_time)
                stop_search = true;
        }
    }
}
