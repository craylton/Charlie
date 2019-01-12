/*
 *	BOARD.C
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */

using System;

namespace CharlieChess
{
    unsafe public partial class Tscp
    {
        Random rand;

        private void InitBoard(string s)
        {
            Fen(s);
            fifty = 0;
            ply = 0;
            hply = 0;
            SetHash();  /* InitHash() must be called before this function */
            first_move[0] = 0;
        }

        private void InitHash()
        {
            rand = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < 2; ++i)
                for (int j = 0; j < 6; ++j)
                    for (int k = 0; k < 64; ++k)
                        hash_piece[(i * 6 * 64) + (j * 64) + k] = HashRand();

            hash_side = HashRand();

            for (int i = 0; i < 64; ++i)
                hash_ep[i] = HashRand();
        }

        /* HashRand() XORs some shifted random numbers together to make sure
        we have good coverage of all 32 bits. (rand() returns 16-bit numbers
        on some systems.) */
        private long HashRand()
        {
            long r = 0;

            for (int i = 0; i < 32; ++i)
                r ^= rand.Next() << i;

            return r;
        }

        /* SetHash() uses the Zobrist method of generating a unique number (hash)
        for the current chess position. Of course, there are many more chess
        positions than there are 32 bit numbers, so the numbers generated are
        not really unique, but they're unique enough for our purposes (to detect
        repetitions of the position). 
        The way it works is to XOR random numbers that correspond to features of
        the position, e.g., if there's a black knight on B8, hash is XORed with
        hash_piece[BLACK][KNIGHT][B8]. All of the pieces are XORed together,
        hash_side is XORed if it's black's move, and the en passant square is
        XORed if there is one. (A chess technicality is that one position can't
        be a repetition of another if the en passant state is different.) */
        private void SetHash()
        {
            hash = 0;
            for (int i = 0; i < 64; ++i)
                if (col[i] != EMPTY)
                    hash ^= hp[(6 * 64 * col[i]) + (ptr[i] * 64) + i];

            if (side == DARK)
                hash ^= hash_side;

            if (ep != -1)
                hash ^= hash_ep[ep];
        }

        /* in_check() returns TRUE if side s is in check and FALSE
           otherwise. It just scans the board to find side s's king
           and calls attack() to see if it's being attacked. */
        private bool InCheck(long s)
        {
            for (int i = 0; i < 64; ++i)
                if (ptr[i] == KING && col[i] == s)
                    return Attack(i, s ^ 1);

            return true;  /* shouldn't get here */
        }

        /* Attack() returns TRUE if square sq is being attacked by side
        s and FALSE otherwise. */
        private unsafe bool Attack(long sq, long s)
        {
            for (int i = 0; i < 64; ++i)
            {
                if (col[i] == s)
                {
                    if (ptr[i] == PAWN)
                    {
                        if (s == LIGHT)
                        {
                            if ((i & 7) != 0 && i - 9 == sq)
                                return true;
                            if ((i & 7) != 7 && i - 7 == sq)
                                return true;
                        }
                        else
                        {
                            if ((i & 7) != 0 && i + 7 == sq)
                                return true;
                            if ((i & 7) != 7 && i + 9 == sq)
                                return true;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < ofss[ptr[i]]; ++j)
                        {
                            long n = i;
                            while (true)
                            {
                                n = mb[mb64[n] + ofs[ptr[i] * 8 + j]];
                                if (n == -1)
                                    break;
                                if (n == sq)
                                    return true;
                                if (col[n] != EMPTY)
                                    break;
                                if (!sl[ptr[i]])
                                    break;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /* gen() generates pseudo-legal moves for the current position.
        It scans the board to find friendly pieces and then determines
        what squares they attack. When it finds a ptr/square
        combination, it calls gen_push to put the move on the "move
        stack." */
        private void Gen()
        {
            /* so far, we have no moves for the current ply */
            fm[ply + 1] = fm[ply];

            for (int i = 0; i < 64; ++i)
            {
                if (col[i] == side)
                {
                    if (ptr[i] == PAWN)
                    {
                        if (side == LIGHT)
                        {
                            if ((i & 7) != 0 && col[i - 9] == DARK)
                                GenPush(i, i - 9, 17);
                            if ((i & 7) != 7 && col[i - 7] == DARK)
                                GenPush(i, i - 7, 17);

                            if (col[i - 8] == EMPTY)
                            {
                                GenPush(i, i - 8, 16);
                                if (i >= 48 && col[i - 16] == EMPTY)
                                    GenPush(i, i - 16, 24);
                            }
                        }
                        else
                        {
                            if ((i & 7) != 0 && col[i + 7] == LIGHT)
                                GenPush(i, i + 7, 17);
                            if ((i & 7) != 7 && col[i + 9] == LIGHT)
                                GenPush(i, i + 9, 17);

                            if (col[i + 8] == EMPTY)
                            {
                                GenPush(i, i + 8, 16);
                                if (i <= 15 && col[i + 16] == EMPTY)
                                    GenPush(i, i + 16, 24);
                            }
                        }
                    }
                    else
                    {
                        for (long j = 0; j < ofss[ptr[i]]; ++j)
                        {
                            long n = i;
                            while (true)
                            {
                                n = mb[mb64[n] + ofs[ptr[i] * 8 + j]];
                                if (n == -1)
                                    break;
                                if (col[n] != EMPTY)
                                {
                                    if (col[n] == xside)
                                        GenPush(i, n, 1);
                                    break;
                                }
                                GenPush(i, n, 0);
                                if (!sl[ptr[i]])
                                    break;
                            }
                        }
                    }
                }
            }

            /* generate castle moves */
            if (side == LIGHT)
            {
                if ((castle & 1) != 0)
                    GenPush(E1, G1, 2);
                if ((castle & 2) != 0)
                    GenPush(E1, C1, 2);
            }
            else
            {
                if ((castle & 4) != 0)
                    GenPush(E8, G8, 2);
                if ((castle & 8) != 0)
                    GenPush(E8, C8, 2);
            }

            /* generate en passant moves */
            if (ep != -1)
            {
                if (side == LIGHT)
                {
                    if ((ep & 7) != 0 && col[ep + 7] == LIGHT && ptr[ep + 7] == PAWN)
                        GenPush(ep + 7, ep, 21);
                    if ((ep & 7) != 7 && col[ep + 9] == LIGHT && ptr[ep + 9] == PAWN)
                        GenPush(ep + 9, ep, 21);
                }
                else
                {
                    if ((ep & 7) != 0 && col[ep - 9] == DARK && ptr[ep - 9] == PAWN)
                        GenPush(ep - 9, ep, 21);
                    if ((ep & 7) != 7 && col[ep - 7] == DARK && ptr[ep - 7] == PAWN)
                        GenPush(ep - 7, ep, 21);
                }
            }
        }

        /* GenCaps() is basically a copy of gen() that's modified to
        only generate capture and promote moves. It's used by the
        quiescence search. */
        private void GenCaps()
        {
            fm[ply + 1] = fm[ply];
            for (int i = 0; i < 64; ++i)
            {
                if (col[i] == side)
                {
                    if (ptr[i] == PAWN)
                    {
                        if (side == LIGHT)
                        {
                            if ((i & 7) != 0 && col[i - 9] == DARK)
                                GenPush(i, i - 9, 17);
                            if ((i & 7) != 7 && col[i - 7] == DARK)
                                GenPush(i, i - 7, 17);
                            if (i <= 15 && col[i - 8] == EMPTY)
                                GenPush(i, i - 8, 16);
                        }
                        if (side == DARK)
                        {
                            if ((i & 7) != 0 && col[i + 7] == LIGHT)
                                GenPush(i, i + 7, 17);
                            if ((i & 7) != 7 && col[i + 9] == LIGHT)
                                GenPush(i, i + 9, 17);
                            if (i >= 48 && col[i + 8] == EMPTY)
                                GenPush(i, i + 8, 16);
                        }
                    }
                    else
                    {
                        for (long j = 0; j < ofss[ptr[i]]; ++j)
                        {
                            long n = i;
                            while (true)
                            {
                                n = mb[mb64[n] + ofs[ptr[i] * 8 + j]];
                                if (n == -1)
                                    break;

                                if (col[n] != EMPTY)
                                {
                                    if (col[n] == xside)
                                        GenPush(i, n, 1);
                                    break;
                                }

                                if (!sl[ptr[i]])
                                    break;
                            }
                        }
                    }
                }
            }

            if (ep != -1)
            {
                if (side == LIGHT)
                {
                    if ((ep & 7) != 0 && col[ep + 7] == LIGHT && ptr[ep + 7] == PAWN)
                        GenPush(ep + 7, ep, 21);
                    if ((ep & 7) != 7 && col[ep + 9] == LIGHT && ptr[ep + 9] == PAWN)
                        GenPush(ep + 9, ep, 21);
                }
                else
                {
                    if ((ep & 7) != 0 && col[ep - 9] == DARK && ptr[ep - 9] == PAWN)
                        GenPush(ep - 9, ep, 21);
                    if ((ep & 7) != 7 && col[ep - 7] == DARK && ptr[ep - 7] == PAWN)
                        GenPush(ep - 7, ep, 21);
                }
            }
        }

        /* GenPush() puts a move on the move stack, unless it's a
        pawn promotion that needs to be handled by gen_promote().
        It also assigns a score to the move for alpha-beta move
        ordering. If the move is a capture, it uses MVV/LVA
        (Most Valuable Victim/Least Valuable Attacker). Otherwise,
        it uses the move's history heuristic value. Note that
        1,000,000 is added to a capture move's score, so it
        always gets ordered above a "normal" move. */
        private void GenPush(long from, long to, long bits)
        {
            GenT g;

            if ((bits & 16) != 0)
            {
                if (side == LIGHT)
                {
                    if (to <= H8)
                    {
                        GenPromote(from, to, bits);
                        return;
                    }
                }
                else if (to >= A1)
                {
                    GenPromote(from, to, bits);
                    return;
                }
            }

            g = gen_dat[fm[ply + 1]++];
            g.m.b.from = (sbyte)from;
            g.m.b.to = (sbyte)to;
            g.m.b.promote = 0;
            g.m.b.bits = (sbyte)bits;

            if (col[to] != EMPTY)
                g.score = 1000000 + (ptr[to] * 10) - ptr[from];
            else
                g.score = histo[from * 64 + to];
        }

        /* GenPromote() is just like gen_push(), only it puts 4 moves
        n the move stack, one for each possible promotion ptr */
        private void GenPromote(long from, long to, long bits)
        {
            GenT g;

            for (long i = KNIGHT; i <= QUEEN; ++i)
            {
                g = gen_dat[fm[ply + 1]++];
                g.m.b.from = (sbyte)from;
                g.m.b.to = (sbyte)to;
                g.m.b.promote = (sbyte)i;
                g.m.b.bits = (sbyte)(bits | 32);
                g.score = 1000000 + (i * 10);
            }
        }

        /* MakeMove() makes a move. If the move is illegal, it
        undoes whatever it did and returns FALSE. Otherwise, it
        returns TRUE. */
        private bool MakeMove(ref MoveBytes m)
        {
            // test to see if a castle move is legal and move the rook
            //(the king is moved with the usual move code later) */
            if ((m.bits & 2) != 0)
            {
                long from, to;

                if (InCheck(side)) return false;

                switch (m.to)
                {
                    case 62:
                        if (col[F1] != EMPTY || col[G1] != EMPTY ||
                                Attack(F1, xside) || Attack(G1, xside))
                            return false;
                        from = H1;
                        to = F1;
                        break;
                    case 58:
                        if (col[B1] != EMPTY || col[C1] != EMPTY || col[D1] != EMPTY ||
                                Attack(C1, xside) || Attack(D1, xside))
                            return false;
                        from = A1;
                        to = D1;
                        break;
                    case 6:
                        if (col[F8] != EMPTY || col[G8] != EMPTY ||
                                Attack(F8, xside) || Attack(G8, xside))
                            return false;
                        from = H8;
                        to = F8;
                        break;
                    case 2:
                        if (col[B8] != EMPTY || col[C8] != EMPTY || col[D8] != EMPTY ||
                                Attack(C8, xside) || Attack(D8, xside))
                            return false;
                        from = A8;
                        to = D8;
                        break;
                    default:  /* shouldn't get here */
                        from = -1;
                        to = -1;
                        break;
                }
                col[to] = col[from];
                ptr[to] = ptr[from];
                col[from] = EMPTY;
                ptr[from] = EMPTY;
            }

            /* back up information so we can take the move back later. */
            histdat[hply].m.b = m;
            histdat[hply].capture = ptr[m.to];
            histdat[hply].castle = castle;
            histdat[hply].ep = ep;
            histdat[hply].fifty = fifty;
            histdat[hply].hash = hash;
            ++ply;
            ++hply;

            //update the castle, en passant, and
            //fifty-move-draw variables */
            castle &= castle_mask[m.from] & castle_mask[m.to];
            if ((m.bits & 8) != 0)
            {
                if (side == LIGHT)
                    ep = m.to + 8;
                else
                    ep = m.to - 8;
            }
            else
                ep = -1;

            if ((m.bits & 17) != 0)
                fifty = 0;
            else
                ++fifty;

            /* move the ptr */
            col[m.to] = side;

            if ((m.bits & 32) != 0)
                ptr[m.to] = m.promote;
            else
                ptr[m.to] = ptr[m.from];

            col[m.from] = EMPTY;
            ptr[m.from] = EMPTY;

            /* erase the pawn if this is an en passant move */
            if ((m.bits & 4) != 0)
            {
                if (side == LIGHT)
                {
                    col[m.to + 8] = EMPTY;
                    ptr[m.to + 8] = EMPTY;
                }
                else
                {
                    col[m.to - 8] = EMPTY;
                    ptr[m.to - 8] = EMPTY;
                }
            }

            //switch sides and test for legality (if we can capture
            //the other guy's king, it's an illegal position and
            //we need to take the move back) */
            side ^= 1;
            xside ^= 1;
            if (InCheck(xside))
            {
                Takeback();
                return false;
            }
            SetHash();
            return true;
        }

        /* Takeback() is very similar to makemove(), only backwards :)  */

        private void Takeback()
        {
            MoveBytes m;

            side ^= 1;
            xside ^= 1;
            --ply;
            --hply;
            m = histdat[hply].m.b;
            castle = histdat[hply].castle;
            ep = histdat[hply].ep;
            fifty = histdat[hply].fifty;
            hash = histdat[hply].hash;
            col[m.from] = side;
            if ((m.bits & 32) != 0)
                ptr[m.from] = PAWN;
            else
                ptr[m.from] = ptr[m.to];

            if (histdat[hply].capture == EMPTY)
            {
                col[m.to] = EMPTY;
                ptr[m.to] = EMPTY;
            }
            else
            {
                col[m.to] = xside;
                ptr[m.to] = histdat[hply].capture;
            }

            if ((m.bits & 2) != 0)
            {
                long from, to;

                switch (m.to)
                {
                    case 62:
                        from = F1;
                        to = H1;
                        break;
                    case 58:
                        from = D1;
                        to = A1;
                        break;
                    case 6:
                        from = F8;
                        to = H8;
                        break;
                    case 2:
                        from = D8;
                        to = A8;
                        break;
                    default:  /* shouldn't get here */
                        from = -1;
                        to = -1;
                        break;
                }

                col[to] = side;
                ptr[to] = ROOK;
                col[from] = EMPTY;
                ptr[from] = EMPTY;
            }

            if ((m.bits & 4) != 0)
            {
                if (side == LIGHT)
                {
                    col[m.to + 8] = xside;
                    ptr[m.to + 8] = PAWN;
                }
                else
                {
                    col[m.to - 8] = xside;
                    ptr[m.to - 8] = PAWN;
                }
            }

        }
    }
}
