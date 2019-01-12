/*
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */
using System;

namespace CharlieChess
{
    public partial class Tscp
    {
        void Fen(string s)
        {
            long n = s.Length;
            int i, sq = 0, a = 0;
            long z;

            for (i = 0; i < 64; ++i)
            {
                color[i] = EMPTY;
                piece[i] = EMPTY;
            }

            for (i = 0, z = 0; i < n && z == 0; ++i)
            {
                switch (s[i])
                {
                    case '1': sq += 1; break;
                    case '2': sq += 2; break;
                    case '3': sq += 3; break;
                    case '4': sq += 4; break;
                    case '5': sq += 5; break;
                    case '6': sq += 6; break;
                    case '7': sq += 7; break;
                    case '8': sq += 8; break;
                    case 'p': color[sq] = DARK; piece[sq] = PAWN; ++sq; break;
                    case 'n': color[sq] = DARK; piece[sq] = KNIGHT; ++sq; break;
                    case 'b': color[sq] = DARK; piece[sq] = BISHOP; ++sq; break;
                    case 'r': color[sq] = DARK; piece[sq] = ROOK; ++sq; break;
                    case 'q': color[sq] = DARK; piece[sq] = QUEEN; ++sq; break;
                    case 'k': color[sq] = DARK; piece[sq] = KING; ++sq; break;
                    case 'P': color[sq] = LIGHT; piece[sq] = PAWN; ++sq; break;
                    case 'N': color[sq] = LIGHT; piece[sq] = KNIGHT; ++sq; break;
                    case 'B': color[sq] = LIGHT; piece[sq] = BISHOP; ++sq; break;
                    case 'R': color[sq] = LIGHT; piece[sq] = ROOK; ++sq; break;
                    case 'Q': color[sq] = LIGHT; piece[sq] = QUEEN; ++sq; break;
                    case 'K': color[sq] = LIGHT; piece[sq] = KING; ++sq; break;
                    case '/': break;
                    default: z = 1; break;
                }
                a = i;
            }

            side = -1;
            xside = -1;

            ++a;

            for (i = a, z = 0; i < n && z == 0; ++i)
            {
                switch (s[i])
                {
                    case 'w': side = LIGHT; xside = DARK; break;
                    case 'b': side = DARK; xside = LIGHT; break;
                    default: z = 1; break;
                }
                a = i;
            }

            castle = 0;

            for (i = a + 1, z = 0; i < n && z == 0; ++i)
            {
                switch (s[i])
                {
                    case 'K': castle |= 1; break;
                    case 'Q': castle |= 2; break;
                    case 'k': castle |= 4; break;
                    case 'q': castle |= 8; break;
                    case '-': break;
                    default: z = 1; break;
                }
                a = i;
            }

            ep = -1;

            for (i = a + 1, z = 0; i < n && z == 0; ++i)
            {
                switch (s[i])
                {
                    case '-': break;
                    case 'a': ep = 0; break;
                    case 'b': ep = 1; break;
                    case 'c': ep = 2; break;
                    case 'd': ep = 3; break;
                    case 'e': ep = 4; break;
                    case 'f': ep = 5; break;
                    case 'g': ep = 6; break;
                    case 'h': ep = 7; break;
                    case '1': ep += 56; break;
                    case '2': ep += 48; break;
                    case '3': ep += 40; break;
                    case '4': ep += 32; break;
                    case '5': ep += 24; break;
                    case '6': ep += 16; break;
                    case '7': ep += 8; break;
                    case '8': ep += 0; break;
                    default: z = 1; break;
                }
            }
        }

        void PrintFen()
        {
            long i;
            long p;
            long c;
            long halfmove;
            char[] str = new char[128];

            long cnt = 0;
            sbyte es = 0;

            for (i = 0; i < 64; i++)
            {
                if (i != 0 && (i % 8 == 0))
                {
                    if (es != 0)
                    {
                        str[cnt++] = num[es];
                        es = 0;
                    }
                    str[cnt++] = '/';
                }
                p = piece[i];
                c = color[i];
                switch (p)
                {
                    case PAWN:
                        if (es != 0)
                        {
                            str[cnt++] = num[es];
                            es = 0;
                        }
                        if (c == LIGHT)
                        {
                            str[cnt++] = 'P';
                            es = 0;
                        }
                        else
                        {
                            str[cnt++] = 'p';
                            es = 0;
                        }
                        break;
                    case KNIGHT:
                        if (es != 0)
                        {
                            str[cnt++] = num[es];
                            es = -1;
                        }
                        if (c == LIGHT)
                        {
                            str[cnt++] = 'N';
                            es = 0;
                        }
                        else
                        {
                            str[cnt++] = 'n';
                            es = 0;
                        }
                        break;
                    case BISHOP:
                        if (es != 0)
                        {
                            str[cnt++] = num[es];
                            es = 0;
                        }
                        if (c == LIGHT)
                        {
                            str[cnt++] = 'B';
                            es = 0;
                        }
                        else
                        {
                            str[cnt++] = 'b';
                            es = 0;
                        }
                        break;
                    case ROOK:
                        if (es != 0)
                        {
                            str[cnt++] = num[es];
                            es = 0;
                        }
                        if (c == LIGHT)
                        {
                            str[cnt++] = 'R';
                            es = 0;
                        }
                        else
                        {
                            str[cnt++] = 'r';
                            es = 0;
                        }
                        break;
                    case QUEEN:
                        if (es != 0)
                        {
                            str[cnt++] = num[es];
                            es = 0;
                        }
                        if (c == LIGHT)
                        {
                            str[cnt++] = 'Q';
                            es = 0;
                        }
                        else
                        {
                            str[cnt++] = 'q';
                            es = 0;
                        }
                        break;
                    case KING:
                        if (es != 0)
                        {
                            str[cnt++] = num[es];
                            es = 0;
                        }
                        if (c == LIGHT)
                        {
                            str[cnt++] = 'K';
                            es = 0;
                        }
                        else
                        {
                            str[cnt++] = 'k';
                            es = 0;
                        }
                        break;
                    default:
                        es++;
                        if (es > 8)
                        {
                            es = 0;
                        }
                        break;
                }
            }

            if (es != 0)
                str[cnt++] = num[es];

            str[cnt++] = ' ';

            if (side == LIGHT)
                str[cnt++] = 'w';
            else
                str[cnt++] = 'b';

            str[cnt++] = ' ';

            if ((castle & 1) != 0)
                str[cnt++] = 'K';

            if ((castle & 2) != 0)
                str[cnt++] = 'Q';

            if ((castle & 4) != 0)
                str[cnt++] = 'k';

            if ((castle & 8) != 0)
                str[cnt++] = 'q';

            if (castle != 0)
                str[cnt++] = '-';

            str[cnt++] = ' ';

            if (ep == -1)
            {
                str[cnt++] = '-';
            }
            else
            {
                str[cnt++] = square_name[ep][0];
                str[cnt++] = square_name[ep][1];
            }

            str[cnt++] = ' ';

            if (fifty < 10)
            {
                str[cnt++] = num[(int)fifty];
            }
            else
            {
                long x = (fifty / 10);
                str[cnt++] = num[(int)x];
                x = (fifty % 10);
                str[cnt++] = num[(int)x];
            }

            str[cnt++] = ' ';

            halfmove = (hply / 2) + 1;

            if (halfmove < 10)
            {
                str[cnt++] = num[(int)halfmove];
            }
            else if (halfmove < 100)
            {
                long x = (halfmove / 10);
                str[cnt++] = num[(int)x];
                x = (halfmove % 10);
                str[cnt++] = num[(int)x];
            }
            else if (halfmove < 1000)
            {
                long x = (halfmove / 100);
                str[cnt++] = num[(int)x];
                halfmove = halfmove % 100;
                x = (halfmove / 10);
                str[cnt++] = num[(int)x];
                x = (halfmove % 10);
                str[cnt++] = num[(int)x];
            }

            str[cnt] = '\0';

            Console.WriteLine(str);
        }
    }


}
