//
// modifiers.cs: Modifier handling.
// 
using System;
using System.Reflection;

namespace CIR {
	public class Modifiers {

		//
		// The ordering of the following 4 constants
		// has been carefully done.
		//
		public const int PROTECTED = 0x0001;
		public const int PUBLIC    = 0x0002;
		public const int PRIVATE   = 0x0004;
		public const int INTERNAL  = 0x0008;
		public const int NEW       = 0x0010;
		public const int ABSTRACT  = 0x0020;
		public const int SEALED    = 0x0040;
		public const int STATIC    = 0x0080;
		public const int READONLY  = 0x0100;
		public const int VIRTUAL   = 0x0200;
		public const int OVERRIDE  = 0x0400;
		public const int EXTERN    = 0x0800;
		public const int TOP       = 0x0800;

		public const int Accessibility =
			PUBLIC | PROTECTED | INTERNAL | PRIVATE;
		
		static public string Name (int i)
		{
			string s = "";
			
			switch (i) {
			case Modifiers.NEW:
				s = "new"; break;
			case Modifiers.PUBLIC:
				s = "public"; break;
			case Modifiers.PROTECTED:
				s = "protected"; break;
			case Modifiers.INTERNAL:
				s = "internal"; break;
			case Modifiers.PRIVATE:
				s = "private"; break;
			case Modifiers.ABSTRACT:
				s = "abstract"; break;
			case Modifiers.SEALED:
				s = "sealed"; break;
			case Modifiers.STATIC:
				s = "static"; break;
			case Modifiers.READONLY:
				s = "readonly"; break;
			case Modifiers.VIRTUAL:
				s = "virtual"; break;
			case Modifiers.OVERRIDE:
				s = "override"; break;
			case Modifiers.EXTERN:
				s = "extern"; break;
			}

			return s;
		}

		public static TypeAttributes TypeAttr (int mod_flags, TypeContainer caller)
		{
			TypeAttributes t = 0;
			bool top_level = caller.IsTopLevel;
			
			if (top_level){
				if ((mod_flags & PUBLIC) != 0)
					t |= TypeAttributes.Public;
				if ((mod_flags & PRIVATE) != 0)
					t |= TypeAttributes.NotPublic;
			} else {
				if ((mod_flags & PUBLIC) != 0)
					t |= TypeAttributes.NestedPublic;
				if ((mod_flags & PRIVATE) != 0)
					t |= TypeAttributes.NestedPrivate;
				if ((mod_flags & PROTECTED) != 0 && (mod_flags & INTERNAL) != 0)
					t |= TypeAttributes.NestedFamORAssem;
				if ((mod_flags & PROTECTED) != 0)
					t |= TypeAttributes.NestedFamily;
				if ((mod_flags & INTERNAL) != 0)
					t |= TypeAttributes.NestedAssembly;
			}
			
			if ((mod_flags & SEALED) != 0)
				t |= TypeAttributes.Sealed;
			if ((mod_flags & ABSTRACT) != 0)
				t |= TypeAttributes.Abstract;

			// If we do not have static constructors, static methods
			// can be invoked without initializing the type.
			if (!caller.HaveStaticConstructor)
				t |= TypeAttributes.BeforeFieldInit;
				
			return t;
		}

		public static FieldAttributes FieldAttr (int mod_flags)
		{
			FieldAttributes fa = 0;

			if ((mod_flags & PUBLIC) != 0)
				fa |= FieldAttributes.Public;
			if ((mod_flags & PRIVATE) != 0)
				fa |= FieldAttributes.Private;
			if ((mod_flags & PROTECTED) != 0 && (mod_flags & INTERNAL) != 0)
				fa |= FieldAttributes.FamORAssem;
			if ((mod_flags & PROTECTED) != 0)
				fa |= FieldAttributes.Family;
			if ((mod_flags & INTERNAL) != 0)
				fa |= FieldAttributes.Assembly;
			
			if ((mod_flags & STATIC) != 0)
				fa |= FieldAttributes.Static;
			if ((mod_flags & READONLY) != 0)
				fa |= FieldAttributes.InitOnly;

			return fa;
		}

		public static MethodAttributes MethodAttr (int mod_flags)
		{
			MethodAttributes ma = 0;

			if ((mod_flags & PUBLIC) != 0)
				ma |= MethodAttributes.Public;
			if ((mod_flags & PRIVATE) != 0)
				ma |= MethodAttributes.Private;
			if ((mod_flags & PROTECTED) != 0 && (mod_flags & INTERNAL) != 0)
				ma |= MethodAttributes.FamORAssem;
			if ((mod_flags & PROTECTED) != 0)
				ma |= MethodAttributes.Family;
			if ((mod_flags & INTERNAL) != 0)
				ma |= MethodAttributes.Assembly;
			

			if ((mod_flags & STATIC) != 0)
				ma |= MethodAttributes.Static;
			if ((mod_flags & ABSTRACT) != 0){
				ma |= MethodAttributes.Abstract | MethodAttributes.Virtual |
					MethodAttributes.NewSlot;
			}
			if ((mod_flags & SEALED) != 0)
				ma |= MethodAttributes.Final;
			if ((mod_flags & VIRTUAL) != 0)
				ma |= MethodAttributes.Virtual;

			if ((mod_flags & OVERRIDE) != 0)
				ma |= MethodAttributes.Virtual;
			
			if ((mod_flags & NEW) != 0)
				ma |= MethodAttributes.HideBySig;
			
			return ma;
		}

		// <summary>
		//   Checks the object @mod modifiers to be in @allowed.
		//   Returns the new mask.  Side effect: reports any
		//   incorrect attributes. 
		// </summary>
		public static int Check (int allowed, int mod, int def_access)
		{
			int invalid_flags  = (~allowed) & mod;
			int i;

			if (invalid_flags == 0){
				int a = mod;

				//
				// If no accessibility bits provided
				// then provide the defaults.
				//
				if ((mod & Accessibility) == 0){
					mod |= def_access;
					return mod;
				}

				//
				// Make sure that no conflicting accessibility
				// bits have been set.  Protected+Internal is
				// allowed, that is why they are placed on bits
				// 1 and 4 (so the shift 3 basically merges them)
				//
				a &= 15;
				a |= (a >> 3);
				a = ((a & 2) >> 1) + (a & 5);
				a = ((a & 4) >> 2) + (a & 3);
				if (a > 1)
					CSharpParser.error (107, "More than one protection modifier specified");
				
				return mod;
			}
			
			for (i = 1; i < TOP; i <<= 1){
				if ((i & invalid_flags) == 0)
					continue;

				CSharpParser.error (106, "the modifier `" + Name (i) + "' is not valid for this item");
			}

			return allowed & mod;
		}
	}
}
