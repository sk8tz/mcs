/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
/* Generated By:JavaCC: Do not edit this line. QueryParserTokenManager.java */
using System;
using Monodoc.Lucene.Net.Analysis;
using Monodoc.Lucene.Net.Documents;
using Term = Monodoc.Lucene.Net.Index.Term;
using Monodoc.Lucene.Net.Search;
using Searchable = Monodoc.Lucene.Net.Search.Searchable;
namespace Monodoc.Lucene.Net.QueryParsers
{
	
	public class QueryParserTokenManager : QueryParserConstants
	{
		private void  InitBlock()
		{
            System.IO.StreamWriter temp_writer;
            temp_writer = new System.IO.StreamWriter(System.Console.OpenStandardOutput(), System.Console.Out.Encoding);
            temp_writer.AutoFlush = true;
            debugStream = temp_writer;
        }
		public System.IO.StreamWriter debugStream;
		public virtual void  SetDebugStream(System.IO.StreamWriter ds)
		{
			debugStream = ds;
		}
		private int jjStopStringLiteralDfa_3(int pos, long active0)
		{
			switch (pos)
			{
				
				default: 
					return - 1;
				
			}
		}
        private int jjStartNfa_3(int pos, long active0)
        {
            return JjMoveNfa_3(jjStopStringLiteralDfa_3(pos, active0), pos + 1);
        }
        private int JjStopAtPos(int pos, int kind)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            return pos + 1;
        }
        private int JjStartNfaWithStates_3(int pos, int kind, int state)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return JjMoveNfa_3(state, pos + 1);
        }
        private int JjMoveStringLiteralDfa0_3()
        {
            switch (curChar)
            {
				
                case (char) (40): 
                    return JjStopAtPos(0, 12);
				
                case (char) (41): 
                    return JjStopAtPos(0, 13);
				
                case (char) (43): 
                    return JjStopAtPos(0, 10);
				
                case (char) (45): 
                    return JjStopAtPos(0, 11);
				
                case (char) (58): 
                    return JjStopAtPos(0, 14);
				
                case (char) (91): 
                    return JjStopAtPos(0, 21);
				
                case (char) (94): 
                    return JjStopAtPos(0, 15);
				
                case (char) (123): 
                    return JjStopAtPos(0, 22);
				
                default: 
                    return JjMoveNfa_3(0, 0);
				
            }
        }
        private void  JjCheckNAdd(int state)
        {
            if (jjrounds[state] != jjround)
            {
                jjstateSet[jjnewStateCnt++] = state;
                jjrounds[state] = jjround;
            }
        }
        private void  JjAddStates(int start, int end)
        {
            do 
            {
                jjstateSet[jjnewStateCnt++] = jjnextStates[start];
            }
            while (start++ != end);
        }
        private void  JjCheckNAddTwoStates(int state1, int state2)
        {
            JjCheckNAdd(state1);
            JjCheckNAdd(state2);
        }
        private void  JjCheckNAddStates(int start, int end)
        {
            do 
            {
                JjCheckNAdd(jjnextStates[start]);
            }
            while (start++ != end);
        }
        private void  JjCheckNAddStates(int start)
        {
            JjCheckNAdd(jjnextStates[start]);
            JjCheckNAdd(jjnextStates[start + 1]);
        }
        internal static readonly ulong[] jjbitVec0 = new ulong[]{0xfffffffffffffffeL, 0xffffffffffffffffL, 0xffffffffffffffffL, 0xffffffffffffffffL};
        internal static readonly ulong[] jjbitVec2 = new ulong[]{0x0L, 0x0L, 0xffffffffffffffffL, 0xffffffffffffffffL};
        private int JjMoveNfa_3(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 33;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop: 
                    do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
								if ((0x7bffd0f8ffffd9ffL & l) != (ulong) 0L)
								{
									if (kind > 17)
										kind = 17;
									JjCheckNAddStates(0, 6);
								}
								else if ((0x100002600L & l) != (ulong) 0L)
								{
									if (kind > 6)
										kind = 6;
								}
								else if (curChar == 34)
									JjCheckNAdd(15);
								else if (curChar == 33)
								{
									if (kind > 9)
										kind = 9;
								}
								if (curChar == 38)
									jjstateSet[jjnewStateCnt++] = 4;
								break;
							
							case 4: 
								if (curChar == 38 && kind > 7)
									kind = 7;
								break;
							
							case 5: 
								if (curChar == 38)
									jjstateSet[jjnewStateCnt++] = 4;
								break;
							
							case 13: 
								if (curChar == 33 && kind > 9)
									kind = 9;
								break;
							
							case 14: 
								if (curChar == 34)
									JjCheckNAdd(15);
								break;
							
							case 15: 
								if ((0xfffffffbffffffffL & l) != (ulong) 0L)
									JjCheckNAddTwoStates(15, 16);
								break;
							
							case 16: 
								if (curChar == 34 && kind > 16)
									kind = 16;
								break;
							
							case 18: 
								if ((0x3ff000000000000L & l) == (ulong) 0L)
									break;
								if (kind > 18)
									kind = 18;
								JjAddStates(7, 8);
								break;
							
							case 19: 
                                if (curChar == 46)
                                    JjCheckNAdd(20);
                                break;
							
							case 20: 
								if ((0x3ff000000000000L & l) == (ulong) 0L)
									break;
								if (kind > 18)
									kind = 18;
								JjCheckNAdd(20);
								break;
							
                            case 21: 
                                if ((0x7bffd0f8ffffd9ffL & l) == (ulong) 0L)
                                    break;
                                if (kind > 17)
                                    kind = 17;
                                JjCheckNAddStates(0, 6);
                                break;
							
                            case 22: 
								if ((0x7bfff8f8ffffd9ffL & l) == (ulong) 0L)
									break;
								if (kind > 17)
									kind = 17;
								JjCheckNAddTwoStates(22, 23);
								break;
							
							case 24: 
                                if ((0x84002f0600000000L & l) == (ulong) 0L)
                                    break;
                                if (kind > 17)
                                    kind = 17;
                                JjCheckNAddTwoStates(22, 23);
                                break;
							
                            case 25: 
                                if ((0x7bfff8f8ffffd9ffL & l) != (ulong) 0L)
                                    JjCheckNAddStates(9, 11);
                                break;
							
							case 26: 
                                if (curChar == 42 && kind > 19)
                                    kind = 19;
                                break;
							
                            case 28: 
                                if ((0x84002f0600000000L & l) != (ulong) 0L)
                                    JjCheckNAddStates(9, 11);
                                break;
							
							case 29: 
                                if ((0xfbfffcf8ffffd9ffL & l) == (ulong) 0L)
                                    break;
                                if (kind > 20)
                                    kind = 20;
                                JjCheckNAddTwoStates(29, 30);
                                break;
							
                            case 31: 
                                if ((0x84002f0600000000L & l) == (ulong) 0L)
                                    break;
                                if (kind > 20)
                                    kind = 20;
                                JjCheckNAddTwoStates(29, 30);
                                break;
							
                            default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else if (curChar < 128)
				{
					ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
								if ((0x97ffffff97ffffffL & l) != (ulong) 0L)
								{
									if (kind > 17)
										kind = 17;
									JjCheckNAddStates(0, 6);
								}
								else if (curChar == 126)
                                {
                                    if (kind > 18)
                                        kind = 18;
                                    jjstateSet[jjnewStateCnt++] = 18;
                                }
                                if (curChar == 92)
									JjCheckNAddStates(12, 14);
								else if (curChar == 78)
									jjstateSet[jjnewStateCnt++] = 11;
								else if (curChar == 124)
									jjstateSet[jjnewStateCnt++] = 8;
								else if (curChar == 79)
									jjstateSet[jjnewStateCnt++] = 6;
								else if (curChar == 65)
									jjstateSet[jjnewStateCnt++] = 2;
								break;
							
							case 1: 
								if (curChar == 68 && kind > 7)
									kind = 7;
								break;
							
							case 2: 
								if (curChar == 78)
									jjstateSet[jjnewStateCnt++] = 1;
								break;
							
							case 3: 
								if (curChar == 65)
									jjstateSet[jjnewStateCnt++] = 2;
								break;
							
							case 6: 
								if (curChar == 82 && kind > 8)
									kind = 8;
								break;
							
							case 7: 
								if (curChar == 79)
									jjstateSet[jjnewStateCnt++] = 6;
								break;
							
							case 8: 
								if (curChar == 124 && kind > 8)
									kind = 8;
								break;
							
							case 9: 
								if (curChar == 124)
									jjstateSet[jjnewStateCnt++] = 8;
								break;
							
							case 10: 
								if (curChar == 84 && kind > 9)
									kind = 9;
								break;
							
							case 11: 
								if (curChar == 79)
									jjstateSet[jjnewStateCnt++] = 10;
								break;
							
							case 12: 
								if (curChar == 78)
									jjstateSet[jjnewStateCnt++] = 11;
								break;
							
							case 15: 
								JjAddStates(13, 16);
								break;
							
							case 17: 
                                if (curChar != 126)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                jjstateSet[jjnewStateCnt++] = 18;
                                break;
							
							case 21: 
                                if ((0x97ffffff97ffffffL & l) == (ulong) 0L)
                                    break;
                                if (kind > 17)
                                    kind = 17;
                                JjCheckNAddStates(0, 6);
                                break;
							
							case 22: 
								if ((0x97ffffff97ffffffL & l) == (ulong) 0L)
									break;
								if (kind > 17)
									kind = 17;
								JjCheckNAddTwoStates(22, 23);
								break;
							
							case 23: 
                                if (curChar == 92)
                                    JjCheckNAddTwoStates(24, 24);
                                break;
							
                            case 24: 
                                if ((0x6800000078000000L & l) == (ulong) 0L)
                                    break;
                                if (kind > 17)
                                    kind = 17;
                                JjCheckNAddTwoStates(22, 23);
                                break;
							
							case 25: 
                                if ((0x97ffffff97ffffffL & l) != (ulong) 0L)
                                    JjCheckNAddStates(9, 11);
                                break;
							
							case 27: 
                                if (curChar == 92)
                                    JjCheckNAddTwoStates(28, 28);
                                break;
							
							case 28: 
                                if ((0x6800000078000000L & l) != (ulong) 0L)
                                    JjCheckNAddStates(9, 11);
                                break;
							
							case 29: 
                                if ((0x97ffffff97ffffffL & l) == (ulong) 0L)
                                    break;
                                if (kind > 20)
                                    kind = 20;
                                JjCheckNAddTwoStates(29, 30);
                                break;
							
							case 30: 
								if (curChar == 92)
									JjCheckNAddTwoStates(31, 31);
								break;
							
                            case 31: 
                                if ((0x6800000078000000L & l) == (ulong) 0L)
                                    break;
                                if (kind > 20)
                                    kind = 20;
                                JjCheckNAddTwoStates(29, 30);
                                break;
							
                            case 32: 
                                if (curChar == 92)
                                    JjCheckNAddStates(12, 14);
                                break;
							
                            default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else
				{
					int hiByte = (int) (curChar >> 8);
					int i1 = hiByte >> 6;
					ulong l1 = ((ulong) 1L) << (hiByte & 63);
					int i2 = (curChar & 0xff) >> 6;
					ulong l2 = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
								if (!jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									break;
								if (kind > 17)
									kind = 17;
								JjCheckNAddStates(0, 6);
								break;
							
							case 15: 
								if (jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									JjAddStates(15, 16);
								break;
							
							case 22: 
								if (!jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									break;
								if (kind > 17)
									kind = 17;
								JjCheckNAddTwoStates(22, 23);
								break;
							
							case 25: 
								if (jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									JjCheckNAddStates(9, 11);
								break;
							
							case 29: 
								if (!jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									break;
                                if (kind > 20)
                                    kind = 20;
                                JjCheckNAddTwoStates(29, 30);
                                break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				if (kind != 0x7fffffff)
				{
					jjmatchedKind = kind;
					jjmatchedPos = curPos;
					kind = 0x7fffffff;
				}
				++curPos;
				if ((i = jjnewStateCnt) == (startsAt = 33 - (jjnewStateCnt = startsAt)))
					return curPos;
				try
				{
					curChar = input_stream.ReadChar();
				}
				catch (System.IO.IOException)
				{
					return curPos;
				}
			}
		}
		private int JjStopStringLiteralDfa_1(int pos, ulong active0)
		{
			switch (pos)
			{
				
				case 0: 
					if ((active0 & 0x10000000L) != 0L)
					{
						jjmatchedKind = 31;
						return 4;
					}
					return - 1;
				
				default: 
					return - 1;
				
			}
		}
		private int JjStartNfa_1(int pos, ulong active0)
		{
			return JjMoveNfa_1(JjStopStringLiteralDfa_1(pos, (ulong) active0), pos + 1);
		}
		private int JjStartNfaWithStates_1(int pos, int kind, int state)
		{
			jjmatchedKind = kind;
			jjmatchedPos = pos;
			try
			{
				curChar = input_stream.ReadChar();
			}
			catch (System.IO.IOException)
			{
				return pos + 1;
			}
			return JjMoveNfa_1(state, pos + 1);
		}
		private int JjMoveStringLiteralDfa0_1()
		{
			switch (curChar)
			{
				
				case (char) (84): 
					return JjMoveStringLiteralDfa1_1((ulong) 0x10000000L);
				
				case (char) (125): 
					return JjStopAtPos(0, 29);
				
				default: 
					return JjMoveNfa_1(0, 0);
				
			}
		}
		private int JjMoveStringLiteralDfa1_1(ulong active0)
		{
			try
			{
				curChar = input_stream.ReadChar();
			}
			catch (System.IO.IOException)
			{
				JjStopStringLiteralDfa_1(0, (ulong) active0);
				return 1;
			}
			switch (curChar)
			{
				
				case (char) (79): 
					if ((active0 & 0x10000000L) != 0L)
						return JjStartNfaWithStates_1(1, 28, 4);
					break;
				
				default: 
					break;
				
			}
			return JjStartNfa_1(0, (ulong) active0);
		}
		private int JjMoveNfa_1(int startState, int curPos)
		{
			int[] nextStates;
			int startsAt = 0;
			jjnewStateCnt = 5;
			int i = 1;
			jjstateSet[0] = startState;
			int j, kind = 0x7fffffff;
			for (; ; )
			{
				if (++jjround == 0x7fffffff)
					ReInitRounds();
				if (curChar < 64)
				{
					ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
								if ((0xfffffffeffffffffL & l) != (ulong) 0L)
								{
									if (kind > 31)
										kind = 31;
									JjCheckNAdd(4);
								}
								if ((0x100002600L & l) != (ulong) 0L)
								{
									if (kind > 6)
										kind = 6;
								}
								else if (curChar == 34)
									JjCheckNAdd(2);
								break;
							
							case 1: 
								if (curChar == 34)
									JjCheckNAdd(2);
								break;
							
							case 2: 
								if ((0xfffffffbffffffffL & l) != (ulong) 0L)
									JjCheckNAddTwoStates(2, 3);
								break;
							
							case 3: 
								if (curChar == 34 && kind > 30)
									kind = 30;
								break;
							
							case 4: 
								if ((0xfffffffeffffffffL & l) == (ulong) 0L)
									break;
								if (kind > 31)
									kind = 31;
								JjCheckNAdd(4);
								break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else if (curChar < 128)
				{
					ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
							case 4: 
								if ((0xdfffffffffffffffL & l) == (ulong) 0L)
									break;
								if (kind > 31)
									kind = 31;
								JjCheckNAdd(4);
								break;
							
							case 2: 
								JjAddStates(17, 18);
								break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else
				{
					int hiByte = (int) (curChar >> 8);
					int i1 = hiByte >> 6;
					ulong l1 = ((ulong) 1L) << (hiByte & 63);
					int i2 = (curChar & 0xff) >> 6;
					ulong l2 = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
							case 4: 
								if (!jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									break;
								if (kind > 31)
									kind = 31;
								JjCheckNAdd(4);
								break;
							
							case 2: 
								if (jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									JjAddStates(17, 18);
								break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				if (kind != 0x7fffffff)
				{
					jjmatchedKind = kind;
					jjmatchedPos = curPos;
					kind = 0x7fffffff;
				}
				++curPos;
				if ((i = jjnewStateCnt) == (startsAt = 5 - (jjnewStateCnt = startsAt)))
					return curPos;
				try
				{
					curChar = input_stream.ReadChar();
				}
				catch (System.IO.IOException)
				{
					return curPos;
				}
			}
		}
		private int JjMoveStringLiteralDfa0_0()
		{
			return JjMoveNfa_0(0, 0);
		}
		private int JjMoveNfa_0(int startState, int curPos)
		{
			int[] nextStates;
			int startsAt = 0;
			jjnewStateCnt = 3;
			int i = 1;
			jjstateSet[0] = startState;
			int j, kind = 0x7fffffff;
			for (; ; )
			{
				if (++jjround == 0x7fffffff)
					ReInitRounds();
				if (curChar < 64)
				{
					ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
                                if ((0x3ff000000000000L & l) == (ulong) 0L)
									break;
                                if (kind > 23)
                                    kind = 23;
                                JjAddStates(19, 20);
                                break;
							
							case 1: 
								if (curChar == 46)
									JjCheckNAdd(2);
								break;
							
							case 2: 
								if ((0x3ff000000000000L & l) == (ulong) 0L)
									break;
                                if (kind > 23)
                                    kind = 23;
                                JjCheckNAdd(2);
                                break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else if (curChar < 128)
				{
					ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else
				{
					int hiByte = (int) (curChar >> 8);
					int i1 = hiByte >> 6;
					ulong l1 = ((ulong) 1L) << (hiByte & 63);
					int i2 = (curChar & 0xff) >> 6;
					ulong l2 = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				if (kind != 0x7fffffff)
				{
					jjmatchedKind = kind;
					jjmatchedPos = curPos;
					kind = 0x7fffffff;
				}
				++curPos;
				if ((i = jjnewStateCnt) == (startsAt = 3 - (jjnewStateCnt = startsAt)))
					return curPos;
				try
				{
					curChar = input_stream.ReadChar();
				}
				catch (System.IO.IOException e)
				{
					return curPos;
				}
			}
		}
		private int JjStopStringLiteralDfa_2(int pos, ulong active0)
		{
			switch (pos)
			{
				
				case 0: 
					if ((active0 & 0x1000000L) != (ulong) 0L)
					{
						jjmatchedKind = 27;
						return 4;
					}
					return - 1;
				
				default: 
					return - 1;
				
			}
		}
		private int JjStartNfa_2(int pos, ulong active0)
		{
			return JjMoveNfa_2(JjStopStringLiteralDfa_2(pos, (ulong) active0), pos + 1);
		}
		private int JjStartNfaWithStates_2(int pos, int kind, int state)
		{
			jjmatchedKind = kind;
			jjmatchedPos = pos;
			try
			{
				curChar = input_stream.ReadChar();
			}
			catch (System.IO.IOException e)
			{
				return pos + 1;
			}
			return JjMoveNfa_2(state, pos + 1);
		}
		private int JjMoveStringLiteralDfa0_2()
		{
			switch (curChar)
			{
				
				case (char) (84): 
					return JjMoveStringLiteralDfa1_2((ulong) 0x1000000L);
				
				case (char) (93): 
					return JjStopAtPos(0, 25);
				
				default: 
					return JjMoveNfa_2(0, 0);
				
			}
		}
		private int JjMoveStringLiteralDfa1_2(ulong active0)
		{
			try
			{
				curChar = input_stream.ReadChar();
			}
			catch (System.IO.IOException e)
			{
				JjStopStringLiteralDfa_2(0, (ulong) active0);
				return 1;
			}
			switch (curChar)
			{
				
				case (char) (79): 
					if ((active0 & 0x1000000L) != (ulong) 0L)
						return JjStartNfaWithStates_2(1, 24, 4);
					break;
				
				default: 
					break;
				
			}
			return JjStartNfa_2(0, (ulong) active0);
		}
		private int JjMoveNfa_2(int startState, int curPos)
		{
			int[] nextStates;
			int startsAt = 0;
			jjnewStateCnt = 5;
			int i = 1;
			jjstateSet[0] = startState;
			int j, kind = 0x7fffffff;
			for (; ; )
			{
				if (++jjround == 0x7fffffff)
					ReInitRounds();
				if (curChar < 64)
				{
					ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
								if ((0xfffffffeffffffffL & l) != (ulong) 0L)
								{
									if (kind > 27)
										kind = 27;
									JjCheckNAdd(4);
								}
								if ((0x100002600L & l) != (ulong) 0L)
								{
									if (kind > 6)
										kind = 6;
								}
								else if (curChar == 34)
									JjCheckNAdd(2);
								break;
							
							case 1: 
								if (curChar == 34)
									JjCheckNAdd(2);
								break;
							
							case 2: 
								if ((0xfffffffbffffffffL & l) != (ulong) 0L)
									JjCheckNAddTwoStates(2, 3);
								break;
							
							case 3: 
								if (curChar == 34 && kind > 26)
									kind = 26;
								break;
							
							case 4: 
								if ((0xfffffffeffffffffL & l) == (ulong) 0L)
									break;
								if (kind > 27)
									kind = 27;
								JjCheckNAdd(4);
								break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else if (curChar < 128)
				{
					ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
							case 4: 
								if ((0xffffffffdfffffffL & l) == (ulong) 0L)
									break;
								if (kind > 27)
									kind = 27;
								JjCheckNAdd(4);
								break;
							
							case 2: 
								JjAddStates(17, 16);
								break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				else
				{
					int hiByte = (int) (curChar >> 8);
					int i1 = hiByte >> 6;
					ulong l1 = ((ulong) 1L) << (hiByte & 63);
					int i2 = (curChar & 0xff) >> 6;
					ulong l2 = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
					do 
					{
						switch (jjstateSet[--i])
						{
							
							case 0: 
							case 4: 
								if (!jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									break;
								if (kind > 27)
									kind = 27;
								JjCheckNAdd(4);
								break;
							
							case 2: 
								if (jjCanMove_0(hiByte, i1, i2, (ulong) l1, (ulong) l2))
									JjAddStates(17, 18);
								break;
							
							default:  break;
							
						}
					}
					while (i != startsAt);
				}
				if (kind != 0x7fffffff)
				{
					jjmatchedKind = kind;
					jjmatchedPos = curPos;
					kind = 0x7fffffff;
				}
				++curPos;
				if ((i = jjnewStateCnt) == (startsAt = 5 - (jjnewStateCnt = startsAt)))
					return curPos;
				try
				{
					curChar = input_stream.ReadChar();
				}
				catch (System.IO.IOException e)
				{
					return curPos;
				}
			}
		}
        internal static readonly int[] jjnextStates = new int[]{22, 25, 26, 29, 30, 27, 23, 18, 19, 25, 26, 27, 24, 28, 31, 15, 16, 2, 3, 0, 1};
		private static bool jjCanMove_0(int hiByte, int i1, int i2, ulong l1, ulong l2)
		{
			switch (hiByte)
			{
				
				case 0: 
					return ((jjbitVec2[i2] & l2) != (ulong) 0L);
				
				default: 
					if ((jjbitVec0[i1] & l1) != (ulong) 0L)
						return true;
					return false;
				
			}
		}
		public static readonly System.String[] jjstrLiteralImages = new System.String[]{"", null, null, null, null, null, null, null, null, null, "\x002B", "\x002D", "\x0028", "\x0029", "\x003A", "\x005E", null, null, null, null, null, "\x005B", "\x007B", null, "\x0054\x004F", "\x005D", null, null, "\x0054\x004F", "\x007D", null, null};
		public static readonly System.String[] lexStateNames = new System.String[]{"Boost", "RangeEx", "RangeIn", "DEFAULT"};
		public static readonly int[] jjnewLexState = new int[]{- 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, 0, - 1, - 1, - 1, - 1, - 1, 2, 1, 3, - 1, 3, - 1, - 1, - 1, 3, - 1, - 1};
		internal static readonly ulong[] jjtoToken = new ulong[]{0xffffff81L};
		internal static readonly long[] jjtoSkip = new long[]{0x40L};
		protected internal CharStream input_stream;
		private uint[] jjrounds = new uint[33];
		private int[] jjstateSet = new int[66];
		protected internal char curChar;
		public QueryParserTokenManager(CharStream stream)
		{
			InitBlock();
			input_stream = stream;
		}
		public QueryParserTokenManager(CharStream stream, int lexState) : this(stream)
		{
			SwitchTo(lexState);
		}
		public virtual void  ReInit(CharStream stream)
		{
			jjmatchedPos = jjnewStateCnt = 0;
			curLexState = defaultLexState;
			input_stream = stream;
			ReInitRounds();
		}
		private void  ReInitRounds()
		{
			int i;
			jjround = 0x80000001;
			for (i = 33; i-- > 0; )
				jjrounds[i] = 0x80000000;
		}
		public virtual void  ReInit(CharStream stream, int lexState)
		{
			ReInit(stream);
			SwitchTo(lexState);
		}
		public virtual void  SwitchTo(int lexState)
		{
			if (lexState >= 4 || lexState < 0)
				throw new TokenMgrError("Error: Ignoring invalid lexical state : " + lexState + ". State unchanged.", TokenMgrError.INVALID_LEXICAL_STATE);
			else
				curLexState = lexState;
		}
		
		protected internal virtual Token JjFillToken()
		{
			Token t = Token.NewToken(jjmatchedKind);
			t.kind = jjmatchedKind;
			System.String im = jjstrLiteralImages[jjmatchedKind];
			t.image = (im == null) ? input_stream.GetImage() : im;
			t.beginLine = input_stream.GetBeginLine();
			t.beginColumn = input_stream.GetBeginColumn();
			t.endLine = input_stream.GetEndLine();
			t.endColumn = input_stream.GetEndColumn();
			return t;
		}
		
		internal int curLexState = 3;
		internal int defaultLexState = 3;
		internal int jjnewStateCnt;
		internal uint jjround;
		internal int jjmatchedPos;
		internal int jjmatchedKind;
		
		public virtual Token GetNextToken()
		{
			int kind;
			Token specialToken = null;
			Token matchedToken;
			int curPos = 0;
			
			for (; ; )
			{
				try
				{
					curChar = input_stream.BeginToken();
				}
				catch (System.IO.IOException e)
				{
					jjmatchedKind = 0;
					matchedToken = JjFillToken();
					return matchedToken;
				}
				
				switch (curLexState)
				{
					
					case 0: 
						jjmatchedKind = 0x7fffffff;
						jjmatchedPos = 0;
						curPos = JjMoveStringLiteralDfa0_0();
						break;
					
					case 1: 
						jjmatchedKind = 0x7fffffff;
						jjmatchedPos = 0;
						curPos = JjMoveStringLiteralDfa0_1();
						break;
					
					case 2: 
						jjmatchedKind = 0x7fffffff;
						jjmatchedPos = 0;
						curPos = JjMoveStringLiteralDfa0_2();
						break;
					
					case 3: 
						jjmatchedKind = 0x7fffffff;
						jjmatchedPos = 0;
						curPos = JjMoveStringLiteralDfa0_3();
						break;
					}
				if (jjmatchedKind != 0x7fffffff)
				{
					if (jjmatchedPos + 1 < curPos)
						input_stream.Backup(curPos - jjmatchedPos - 1);
					if ((jjtoToken[jjmatchedKind >> 6] & ((ulong) (1L << (jjmatchedKind & 63)))) != (ulong) 0L)
					{
						matchedToken = JjFillToken();
						if (jjnewLexState[jjmatchedKind] != - 1)
							curLexState = jjnewLexState[jjmatchedKind];
						return matchedToken;
					}
					else
					{
						if (jjnewLexState[jjmatchedKind] != - 1)
							curLexState = jjnewLexState[jjmatchedKind];
						goto EOFLoop;
					}
				}
				int error_line = input_stream.GetEndLine();
				int error_column = input_stream.GetEndColumn();
				System.String error_after = null;
				bool EOFSeen = false;
				try
				{
					input_stream.ReadChar(); input_stream.Backup(1);
				}
				catch (System.IO.IOException e1)
				{
					EOFSeen = true;
					error_after = curPos <= 1?"":input_stream.GetImage();
					if (curChar == '\n' || curChar == '\r')
					{
						error_line++;
						error_column = 0;
					}
					else
						error_column++;
				}
				if (!EOFSeen)
				{
					input_stream.Backup(1);
					error_after = curPos <= 1?"":input_stream.GetImage();
				}
				throw new TokenMgrError(EOFSeen, curLexState, error_line, error_column, error_after, curChar, TokenMgrError.LEXICAL_ERROR);

EOFLoop: ;
			}
		}
	}
}