// OpCodes.cs
// Mechanically generated  - DO NOT EDIT!
//
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Reflection.Emit;

namespace System.Reflection.Emit {


	public class OpCodes {
		public static readonly OpCode Add;
  		public static readonly OpCode Add_Ovf;
  		public static readonly OpCode Add_Ovf_Un;
  		public static readonly OpCode And;
  		public static readonly OpCode Arglist;
  		public static readonly OpCode Beq;
  		public static readonly OpCode Beq_S;
  		public static readonly OpCode Bge;
  		public static readonly OpCode Bge_S;
  		public static readonly OpCode Bge_Un;
  		public static readonly OpCode Bge_Un_S;
  		public static readonly OpCode Bgt;
  		public static readonly OpCode Bgt_S;
  		public static readonly OpCode Bgt_Un;
  		public static readonly OpCode Bgt_Un_S;
  		public static readonly OpCode Ble;
  		public static readonly OpCode Ble_S;
  		public static readonly OpCode Ble_Un;
  		public static readonly OpCode Ble_Un_S;
  		public static readonly OpCode Blt;
  		public static readonly OpCode Blt_S;
  		public static readonly OpCode Blt_Un;
  		public static readonly OpCode Blt_Un_S;
  		public static readonly OpCode Bne_Un;
  		public static readonly OpCode Bne_Un_S;
  		public static readonly OpCode Box;
#if NET_1_0
[Obsolete]	public static readonly OpCode Boxval = new OpCode("boxval", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop1, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x8C);
#endif
  		public static readonly OpCode Br;
  		public static readonly OpCode Br_S;
  		public static readonly OpCode Break;
  		public static readonly OpCode Brfalse;
  		public static readonly OpCode Brfalse_S;
  		public static readonly OpCode Brtrue;
  		public static readonly OpCode Brtrue_S;
  		public static readonly OpCode Call;
  		public static readonly OpCode Calli;
  		public static readonly OpCode Callvirt;
  		public static readonly OpCode Castclass;
  		public static readonly OpCode Ceq;
  		public static readonly OpCode Cgt;
  		public static readonly OpCode Cgt_Un;
  		public static readonly OpCode Ckfinite;
  		public static readonly OpCode Clt;
  		public static readonly OpCode Clt_Un;
  		public static readonly OpCode Conv_I;
  		public static readonly OpCode Conv_I1;
  		public static readonly OpCode Conv_I2;
  		public static readonly OpCode Conv_I4;
  		public static readonly OpCode Conv_I8;
  		public static readonly OpCode Conv_Ovf_I;
  		public static readonly OpCode Conv_Ovf_I_Un;
  		public static readonly OpCode Conv_Ovf_I1;
  		public static readonly OpCode Conv_Ovf_I1_Un;
  		public static readonly OpCode Conv_Ovf_I2;
  		public static readonly OpCode Conv_Ovf_I2_Un;
  		public static readonly OpCode Conv_Ovf_I4;
  		public static readonly OpCode Conv_Ovf_I4_Un;
  		public static readonly OpCode Conv_Ovf_I8;
  		public static readonly OpCode Conv_Ovf_I8_Un;
  		public static readonly OpCode Conv_Ovf_U;
  		public static readonly OpCode Conv_Ovf_U_Un;
  		public static readonly OpCode Conv_Ovf_U1;
  		public static readonly OpCode Conv_Ovf_U1_Un;
  		public static readonly OpCode Conv_Ovf_U2;
  		public static readonly OpCode Conv_Ovf_U2_Un;
  		public static readonly OpCode Conv_Ovf_U4;
  		public static readonly OpCode Conv_Ovf_U4_Un;
  		public static readonly OpCode Conv_Ovf_U8;
  		public static readonly OpCode Conv_Ovf_U8_Un;
  		public static readonly OpCode Conv_R_Un;
  		public static readonly OpCode Conv_R4;
  		public static readonly OpCode Conv_R8;
  		public static readonly OpCode Conv_U;
  		public static readonly OpCode Conv_U1;
  		public static readonly OpCode Conv_U2;
  		public static readonly OpCode Conv_U4;
  		public static readonly OpCode Conv_U8;
  		public static readonly OpCode Cpblk;
  		public static readonly OpCode Cpobj;
  		public static readonly OpCode Div;
  		public static readonly OpCode Div_Un;
  		public static readonly OpCode Dup;
  		public static readonly OpCode Endfilter;
  		public static readonly OpCode Endfinally;
  		public static readonly OpCode Initblk;
  		public static readonly OpCode Initobj;
  		public static readonly OpCode Isinst;
  		public static readonly OpCode Jmp;
  		public static readonly OpCode Ldarg;
  		public static readonly OpCode Ldarg_0;
  		public static readonly OpCode Ldarg_1;
  		public static readonly OpCode Ldarg_2;
  		public static readonly OpCode Ldarg_3;
  		public static readonly OpCode Ldarg_S;
  		public static readonly OpCode Ldarga;
  		public static readonly OpCode Ldarga_S;
  		public static readonly OpCode Ldc_I4;
  		public static readonly OpCode Ldc_I4_0;
  		public static readonly OpCode Ldc_I4_1;
  		public static readonly OpCode Ldc_I4_2;
  		public static readonly OpCode Ldc_I4_3;
  		public static readonly OpCode Ldc_I4_4;
  		public static readonly OpCode Ldc_I4_5;
  		public static readonly OpCode Ldc_I4_6;
  		public static readonly OpCode Ldc_I4_7;
  		public static readonly OpCode Ldc_I4_8;
  		public static readonly OpCode Ldc_I4_M1;
  		public static readonly OpCode Ldc_I4_S;
  		public static readonly OpCode Ldc_I8;
  		public static readonly OpCode Ldc_R4;
  		public static readonly OpCode Ldc_R8;
  		public static readonly OpCode Ldelem_I;
  		public static readonly OpCode Ldelem_I1;
  		public static readonly OpCode Ldelem_I2;
  		public static readonly OpCode Ldelem_I4;
  		public static readonly OpCode Ldelem_I8;
  		public static readonly OpCode Ldelem_R4;
  		public static readonly OpCode Ldelem_R8;
  		public static readonly OpCode Ldelem_Ref;
  		public static readonly OpCode Ldelem_U1;
  		public static readonly OpCode Ldelem_U2;
  		public static readonly OpCode Ldelem_U4;
  		public static readonly OpCode Ldelema;
#if NET_1_2
		public static readonly OpCode Ldelem_Any;
#endif
  		public static readonly OpCode Ldfld;
  		public static readonly OpCode Ldflda;
  		public static readonly OpCode Ldftn;
  		public static readonly OpCode Ldind_I;
  		public static readonly OpCode Ldind_I1;
  		public static readonly OpCode Ldind_I2;
  		public static readonly OpCode Ldind_I4;
  		public static readonly OpCode Ldind_I8;
  		public static readonly OpCode Ldind_R4;
  		public static readonly OpCode Ldind_R8;
  		public static readonly OpCode Ldind_Ref;
  		public static readonly OpCode Ldind_U1;
  		public static readonly OpCode Ldind_U2;
  		public static readonly OpCode Ldind_U4;
  		public static readonly OpCode Ldlen;
  		public static readonly OpCode Ldloc;
  		public static readonly OpCode Ldloc_0;
  		public static readonly OpCode Ldloc_1;
  		public static readonly OpCode Ldloc_2;
  		public static readonly OpCode Ldloc_3;
  		public static readonly OpCode Ldloc_S;
  		public static readonly OpCode Ldloca;
  		public static readonly OpCode Ldloca_S;
  		public static readonly OpCode Ldnull;
  		public static readonly OpCode Ldobj;
  		public static readonly OpCode Ldsfld;
  		public static readonly OpCode Ldsflda;
  		public static readonly OpCode Ldstr;
  		public static readonly OpCode Ldtoken;
  		public static readonly OpCode Ldvirtftn;
  		public static readonly OpCode Leave;
  		public static readonly OpCode Leave_S;
  		public static readonly OpCode Localloc;
  		public static readonly OpCode Mkrefany;
  		public static readonly OpCode Mul;
  		public static readonly OpCode Mul_Ovf;
  		public static readonly OpCode Mul_Ovf_Un;
  		public static readonly OpCode Neg;
  		public static readonly OpCode Newarr;
  		public static readonly OpCode Newobj;
  		public static readonly OpCode Nop;
  		public static readonly OpCode Not;
  		public static readonly OpCode Or;
  		public static readonly OpCode Pop;
  		public static readonly OpCode Prefix1;
  		public static readonly OpCode Prefix2;
  		public static readonly OpCode Prefix3;
  		public static readonly OpCode Prefix4;
  		public static readonly OpCode Prefix5;
  		public static readonly OpCode Prefix6;
  		public static readonly OpCode Prefix7;
  		public static readonly OpCode Prefixref;
  		public static readonly OpCode Refanytype;
  		public static readonly OpCode Refanyval;
  		public static readonly OpCode Rem;
  		public static readonly OpCode Rem_Un;
  		public static readonly OpCode Ret;
  		public static readonly OpCode Rethrow;
  		public static readonly OpCode Shl;
  		public static readonly OpCode Shr;
  		public static readonly OpCode Shr_Un;
  		public static readonly OpCode Sizeof;
  		public static readonly OpCode Starg;
  		public static readonly OpCode Starg_S;
  		public static readonly OpCode Stelem_I;
  		public static readonly OpCode Stelem_I1;
  		public static readonly OpCode Stelem_I2;
  		public static readonly OpCode Stelem_I4;
  		public static readonly OpCode Stelem_I8;
  		public static readonly OpCode Stelem_R4;
  		public static readonly OpCode Stelem_R8;
  		public static readonly OpCode Stelem_Ref;
#if NET_1_2
		public static readonly OpCode Stelem_Any;
#endif
  		public static readonly OpCode Stfld;
  		public static readonly OpCode Stind_I;
  		public static readonly OpCode Stind_I1;
  		public static readonly OpCode Stind_I2;
  		public static readonly OpCode Stind_I4;
  		public static readonly OpCode Stind_I8;
  		public static readonly OpCode Stind_R4;
  		public static readonly OpCode Stind_R8;
  		public static readonly OpCode Stind_Ref;
  		public static readonly OpCode Stloc;
  		public static readonly OpCode Stloc_0;
  		public static readonly OpCode Stloc_1;
  		public static readonly OpCode Stloc_2;
  		public static readonly OpCode Stloc_3;
  		public static readonly OpCode Stloc_S;
  		public static readonly OpCode Stobj;
  		public static readonly OpCode Stsfld;
  		public static readonly OpCode Sub;
  		public static readonly OpCode Sub_Ovf;
  		public static readonly OpCode Sub_Ovf_Un;
  		public static readonly OpCode Switch;
  		public static readonly OpCode Tailcall;
  		public static readonly OpCode Throw;
  		public static readonly OpCode Unaligned;
  		public static readonly OpCode Unbox;
#if NET_1_2
		public static readonly OpCode Unbox_Any;
#endif
  		public static readonly OpCode Volatile;
  		public static readonly OpCode Xor;
#if NET_1_2
		public static readonly OpCode Constrained;
#endif

		private OpCodes () {}

		static OpCodes ()
		{

			Nop = new OpCode("nop", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x0);
  			Arglist = new OpCode("arglist", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x0);
  			Break = new OpCode("break", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Break, 0xFF, 0x1);
  			Ceq = new OpCode("ceq", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x1);
  			Starg_S = new OpCode("starg.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x10);
  			Ldloc_S = new OpCode("ldloc.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x11);
  			Endfilter = new OpCode("endfilter", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Return, 0xFE, 0x11);
  			Ldloca_S = new OpCode("ldloca.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x12);
  			Unaligned = new OpCode("unaligned.", 2, OpCodeType.Prefix, OperandType.ShortInlineI, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x12);
  			Stloc_S = new OpCode("stloc.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x13);
  			Volatile = new OpCode("volatile.", 2, OpCodeType.Prefix, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x13);
  			Ldnull = new OpCode("ldnull", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x14);
  			Tailcall = new OpCode("tail.", 2, OpCodeType.Prefix, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x14);
  			Ldc_I4_M1 = new OpCode("ldc.i4.m1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x15);
  			Initobj = new OpCode("initobj", 2, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0x15);
  			Ldc_I4_0 = new OpCode("ldc.i4.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x16);
  			Ldc_I4_1 = new OpCode("ldc.i4.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x17);
  			Cpblk = new OpCode("cpblk", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0x17);
  			Ldc_I4_2 = new OpCode("ldc.i4.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x18);
  			Initblk = new OpCode("initblk", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0x18);
  			Ldc_I4_3 = new OpCode("ldc.i4.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x19);
  			Ldc_I4_4 = new OpCode("ldc.i4.4", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1A);
  			Rethrow = new OpCode("rethrow", 2, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Throw, 0xFE, 0x1A);
  			Ldc_I4_5 = new OpCode("ldc.i4.5", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1B);
  			Ldc_I4_6 = new OpCode("ldc.i4.6", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1C);
  			Sizeof = new OpCode("sizeof", 2, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x1C);
  			Ldc_I4_7 = new OpCode("ldc.i4.7", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1D);
  			Refanytype = new OpCode("refanytype", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x1D);
  			Ldc_I4_8 = new OpCode("ldc.i4.8", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1E);
  			Ldc_I4_S = new OpCode("ldc.i4.s", 1, OpCodeType.Macro, OperandType.ShortInlineI, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1F);
  			Ldarg_0 = new OpCode("ldarg.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x2);
  			Cgt = new OpCode("cgt", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x2);
  			Ldc_I4 = new OpCode("ldc.i4", 1, OpCodeType.Primitive, OperandType.InlineI, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x20);
  			Ldc_I8 = new OpCode("ldc.i8", 1, OpCodeType.Primitive, OperandType.InlineI8, StackBehaviour.Pop0, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x21);
  			Ldc_R4 = new OpCode("ldc.r4", 1, OpCodeType.Primitive, OperandType.ShortInlineR, StackBehaviour.Pop0, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x22);
  			Ldc_R8 = new OpCode("ldc.r8", 1, OpCodeType.Primitive, OperandType.InlineR, StackBehaviour.Pop0, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x23);
  			Dup = new OpCode("dup", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push1_push1, FlowControl.Next, 0xFF, 0x25);
  			Pop = new OpCode("pop", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x26);
  			Jmp = new OpCode("jmp", 1, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Call, 0xFF, 0x27);
  			Call = new OpCode("call", 1, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Varpop, StackBehaviour.Varpush, FlowControl.Call, 0xFF, 0x28);
  			Calli = new OpCode("calli", 1, OpCodeType.Primitive, OperandType.InlineSig, StackBehaviour.Varpop, StackBehaviour.Varpush, FlowControl.Call, 0xFF, 0x29);
  			Ret = new OpCode("ret", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Varpop, StackBehaviour.Push0, FlowControl.Return, 0xFF, 0x2A);
  			Br_S = new OpCode("br.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0x2B);
  			Brfalse_S = new OpCode("brfalse.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2C);
  			Brtrue_S = new OpCode("brtrue.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2D);
  			Beq_S = new OpCode("beq.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2E);
  			Bge_S = new OpCode("bge.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2F);
  			Ldarg_1 = new OpCode("ldarg.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x3);
  			Cgt_Un = new OpCode("cgt.un", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x3);
  			Bgt_S = new OpCode("bgt.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x30);
  			Ble_S = new OpCode("ble.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x31);
  			Blt_S = new OpCode("blt.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x32);
  			Bne_Un_S = new OpCode("bne.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x33);
  			Bge_Un_S = new OpCode("bge.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x34);
  			Bgt_Un_S = new OpCode("bgt.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x35);
  			Ble_Un_S = new OpCode("ble.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x36);
  			Blt_Un_S = new OpCode("blt.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x37);
  			Br = new OpCode("br", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0x38);
  			Brfalse = new OpCode("brfalse", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x39);
  			Brtrue = new OpCode("brtrue", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3A);
  			Beq = new OpCode("beq", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3B);
  			Bge = new OpCode("bge", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3C);
  			Bgt = new OpCode("bgt", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3D);
  			Ble = new OpCode("ble", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3E);
  			Blt = new OpCode("blt", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3F);
  			Ldarg_2 = new OpCode("ldarg.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x4);
  			Clt = new OpCode("clt", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x4);
  			Bne_Un = new OpCode("bne.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x40);
  			Bge_Un = new OpCode("bge.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x41);
  			Bgt_Un = new OpCode("bgt.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x42);
  			Ble_Un = new OpCode("ble.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x43);
  			Blt_Un = new OpCode("blt.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x44);
  			Switch = new OpCode("switch", 1, OpCodeType.Primitive, OperandType.InlineSwitch, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x45);
  			Ldind_I1 = new OpCode("ldind.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x46);
  			Ldind_U1 = new OpCode("ldind.u1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x47);
  			Ldind_I2 = new OpCode("ldind.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x48);
  			Ldind_U2 = new OpCode("ldind.u2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x49);
  			Ldind_I4 = new OpCode("ldind.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x4A);
  			Ldind_U4 = new OpCode("ldind.u4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x4B);
  			Ldind_I8 = new OpCode("ldind.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x4C);
  			Ldind_I = new OpCode("ldind.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x4D);
  			Ldind_R4 = new OpCode("ldind.r4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x4E);
  			Ldind_R8 = new OpCode("ldind.r8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x4F);
  			Ldarg_3 = new OpCode("ldarg.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5);
  			Clt_Un = new OpCode("clt.un", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x5);
  			Ldind_Ref = new OpCode("ldind.ref", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x50);
  			Stind_Ref = new OpCode("stind.ref", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x51);
  			Stind_I1 = new OpCode("stind.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x52);
  			Stind_I2 = new OpCode("stind.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x53);
  			Stind_I4 = new OpCode("stind.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x54);
  			Stind_I8 = new OpCode("stind.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x55);
  			Stind_R4 = new OpCode("stind.r4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popr4, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x56);
  			Stind_R8 = new OpCode("stind.r8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popr8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x57);
  			Add = new OpCode("add", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x58);
  			Sub = new OpCode("sub", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x59);
  			Mul = new OpCode("mul", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5A);
  			Div = new OpCode("div", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5B);
  			Div_Un = new OpCode("div.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5C);
  			Rem = new OpCode("rem", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5D);
  			Rem_Un = new OpCode("rem.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5E);
  			And = new OpCode("and", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5F);
  			Ldloc_0 = new OpCode("ldloc.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x6);
  			Ldftn = new OpCode("ldftn", 2, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x6);
  			Or = new OpCode("or", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x60);
  			Xor = new OpCode("xor", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x61);
  			Shl = new OpCode("shl", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x62);
  			Shr = new OpCode("shr", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x63);
  			Shr_Un = new OpCode("shr.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x64);
  			Neg = new OpCode("neg", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x65);
  			Not = new OpCode("not", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x66);
  			Conv_I1 = new OpCode("conv.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x67);
  			Conv_I2 = new OpCode("conv.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x68);
  			Conv_I4 = new OpCode("conv.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x69);
  			Conv_I8 = new OpCode("conv.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x6A);
  			Conv_R4 = new OpCode("conv.r4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x6B);
  			Conv_R8 = new OpCode("conv.r8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x6C);
  			Conv_U4 = new OpCode("conv.u4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x6D);
  			Conv_U8 = new OpCode("conv.u8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x6E);
  			Callvirt = new OpCode("callvirt", 1, OpCodeType.Objmodel, OperandType.InlineMethod, StackBehaviour.Varpop, StackBehaviour.Varpush, FlowControl.Call, 0xFF, 0x6F);
  			Ldloc_1 = new OpCode("ldloc.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x7);
  			Ldvirtftn = new OpCode("ldvirtftn", 2, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x7);
  			Cpobj = new OpCode("cpobj", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x70);
  			Ldobj = new OpCode("ldobj", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x71);
  			Ldstr = new OpCode("ldstr", 1, OpCodeType.Objmodel, OperandType.InlineString, StackBehaviour.Pop0, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x72);
  			Newobj = new OpCode("newobj", 1, OpCodeType.Objmodel, OperandType.InlineMethod, StackBehaviour.Varpop, StackBehaviour.Pushref, FlowControl.Call, 0xFF, 0x73);
  			Castclass = new OpCode("castclass", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x74);
  			Isinst = new OpCode("isinst", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x75);
  			Conv_R_Un = new OpCode("conv.r.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x76);
  			Unbox = new OpCode("unbox", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x79);
#if NET_1_2
  			Unbox_Any = new OpCode("unbox.any", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xa5);
#endif
  			Throw = new OpCode("throw", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref, StackBehaviour.Push0, FlowControl.Throw, 0xFF, 0x7A);
  			Ldfld = new OpCode("ldfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Popref, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x7B);
  			Ldflda = new OpCode("ldflda", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x7C);
  			Stfld = new OpCode("stfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Popref_pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x7D);
  			Ldsfld = new OpCode("ldsfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x7E);
  			Ldsflda = new OpCode("ldsflda", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x7F);
  			Ldloc_2 = new OpCode("ldloc.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x8);
  			Stsfld = new OpCode("stsfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x80);
  			Stobj = new OpCode("stobj", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Popi_pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x81);
  			Conv_Ovf_I1_Un = new OpCode("conv.ovf.i1.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x82);
  			Conv_Ovf_I2_Un = new OpCode("conv.ovf.i2.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x83);
  			Conv_Ovf_I4_Un = new OpCode("conv.ovf.i4.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x84);
  			Conv_Ovf_I8_Un = new OpCode("conv.ovf.i8.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x85);
  			Conv_Ovf_U1_Un = new OpCode("conv.ovf.u1.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x86);
  			Conv_Ovf_U2_Un = new OpCode("conv.ovf.u2.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x87);
  			Conv_Ovf_U4_Un = new OpCode("conv.ovf.u4.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x88);
  			Conv_Ovf_U8_Un = new OpCode("conv.ovf.u8.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x89);
  			Conv_Ovf_I_Un = new OpCode("conv.ovf.i.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8A);
  			Conv_Ovf_U_Un = new OpCode("conv.ovf.u.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8B);
  			Box = new OpCode("box", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop1, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x8C);
  			Newarr = new OpCode("newarr", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x8D);
  			Ldlen = new OpCode("ldlen", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8E);
  			Ldelema = new OpCode("ldelema", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8F);
#if NET_1_2
  			Ldelem_Any = new OpCode("ldelem.any", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref_popi, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xA3);
#endif
  			Ldloc_3 = new OpCode("ldloc.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x9);
  			Ldarg = new OpCode("ldarg", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFE, 0x9);
  			Ldelem_I1 = new OpCode("ldelem.i1", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x90);
  			Ldelem_U1 = new OpCode("ldelem.u1", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x91);
  			Ldelem_I2 = new OpCode("ldelem.i2", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x92);
  			Ldelem_U2 = new OpCode("ldelem.u2", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x93);
  			Ldelem_I4 = new OpCode("ldelem.i4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x94);
  			Ldelem_U4 = new OpCode("ldelem.u4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x95);
  			Ldelem_I8 = new OpCode("ldelem.i8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x96);
  			Ldelem_I = new OpCode("ldelem.i", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x97);
  			Ldelem_R4 = new OpCode("ldelem.r4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x98);
  			Ldelem_R8 = new OpCode("ldelem.r8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x99);
  			Ldelem_Ref = new OpCode("ldelem.ref", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x9A);
  			Stelem_I = new OpCode("stelem.i", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9B);
  			Stelem_I1 = new OpCode("stelem.i1", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9C);
  			Stelem_I2 = new OpCode("stelem.i2", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9D);
  			Stelem_I4 = new OpCode("stelem.i4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9E);
  			Stelem_I8 = new OpCode("stelem.i8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9F);
  			Stloc_0 = new OpCode("stloc.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA);
  			Ldarga = new OpCode("ldarga", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0xA);
  			Stelem_R4 = new OpCode("stelem.r4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popr4, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA0);
  			Stelem_R8 = new OpCode("stelem.r8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popr8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA1);
  			Stelem_Ref = new OpCode("stelem.ref", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popref, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA2);
#if NET_1_2
  			Stelem_Any = new OpCode("stelem.any", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref_popi_popref, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA4);
#endif
  			Stloc_1 = new OpCode("stloc.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xB);
  			Starg = new OpCode("starg", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0xB);
  			Conv_Ovf_I1 = new OpCode("conv.ovf.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB3);
  			Conv_Ovf_U1 = new OpCode("conv.ovf.u1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB4);
  			Conv_Ovf_I2 = new OpCode("conv.ovf.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB5);
  			Conv_Ovf_U2 = new OpCode("conv.ovf.u2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB6);
  			Conv_Ovf_I4 = new OpCode("conv.ovf.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB7);
  			Conv_Ovf_U4 = new OpCode("conv.ovf.u4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB8);
  			Conv_Ovf_I8 = new OpCode("conv.ovf.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0xB9);
  			Conv_Ovf_U8 = new OpCode("conv.ovf.u8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0xBA);
  			Stloc_2 = new OpCode("stloc.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xC);
  			Ldloc = new OpCode("ldloc", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFE, 0xC);
  			Refanyval = new OpCode("refanyval", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xC2);
  			Ckfinite = new OpCode("ckfinite", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0xC3);
  			Mkrefany = new OpCode("mkrefany", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xC6);
  			Stloc_3 = new OpCode("stloc.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xD);
  			Ldloca = new OpCode("ldloca", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0xD);
  			Ldtoken = new OpCode("ldtoken", 1, OpCodeType.Primitive, OperandType.InlineTok, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD0);
  			Conv_U2 = new OpCode("conv.u2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD1);
  			Conv_U1 = new OpCode("conv.u1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD2);
  			Conv_I = new OpCode("conv.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD3);
  			Conv_Ovf_I = new OpCode("conv.ovf.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD4);
  			Conv_Ovf_U = new OpCode("conv.ovf.u", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD5);
  			Add_Ovf = new OpCode("add.ovf", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD6);
  			Add_Ovf_Un = new OpCode("add.ovf.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD7);
  			Mul_Ovf = new OpCode("mul.ovf", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD8);
  			Mul_Ovf_Un = new OpCode("mul.ovf.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD9);
  			Sub_Ovf = new OpCode("sub.ovf", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xDA);
  			Sub_Ovf_Un = new OpCode("sub.ovf.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xDB);
  			Endfinally = new OpCode("endfinally", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Return, 0xFF, 0xDC);
  			Leave = new OpCode("leave", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0xDD);
  			Leave_S = new OpCode("leave.s", 1, OpCodeType.Primitive, OperandType.ShortInlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0xDE);
  			Stind_I = new OpCode("stind.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xDF);
  			Ldarg_S = new OpCode("ldarg.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xE);
  			Stloc = new OpCode("stloc", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0xE);
  			Conv_U = new OpCode("conv.u", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xE0);
  			Ldarga_S = new OpCode("ldarga.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xF);
  			Localloc = new OpCode("localloc", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0xF);
  			Prefix7 = new OpCode("prefix7", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xF8);
  			Prefix6 = new OpCode("prefix6", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xF9);
  			Prefix5 = new OpCode("prefix5", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFA);
  			Prefix4 = new OpCode("prefix4", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFB);
  			Prefix3 = new OpCode("prefix3", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFC);
  			Prefix2 = new OpCode("prefix2", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFD);
  			Prefix1 = new OpCode("prefix1", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFE);
  			Prefixref = new OpCode("prefixref", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFF);

#if NET_1_2
  			Constrained = new OpCode("constrained.", 2, OpCodeType.Prefix, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x16);
#endif
		}

		public static bool TakesSingleByteArgument (OpCode inst)
		{
			OperandType t = inst.OperandType;

			// check for short-inline instructions
			return (   t == OperandType.ShortInlineBrTarget
			        || t == OperandType.ShortInlineI
			        || t == OperandType.ShortInlineR
			        || t == OperandType.ShortInlineVar
			       );
		}
	}



  


} // namespace System.Reflection.Emit
