//
// modifiers.cs: Modifier handling.
// 
using System;
using System.Reflection;

namespace Mono.MonoBASIC {
	public class Modifiers {

		//
		// The ordering of the following 4 constants
		// has been carefully done.
		//
		public const int PROTECTED = 0x00001;
		public const int PUBLIC    = 0x00002;
		public const int PRIVATE   = 0x00004;
		public const int INTERNAL  = 0x00008;
		public const int NEW       = 0x00010;
		public const int ABSTRACT  = 0x00020;
		public const int SEALED    = 0x00040;
		public const int STATIC    = 0x00080;
		public const int READONLY  = 0x00100;
		public const int VIRTUAL   = 0x00200;
		public const int OVERRIDE  = 0x00400;
		public const int EXTERN    = 0x00800;
		public const int VOLATILE  = 0x01000;
		public const int UNSAFE    = 0x02000;
		public const int WRITEONLY = 0x04000;
		// Todo : Shadows needs implementation		
		public const int SHADOWS   = 0x08000;
		public const int DEFAULT   = 0x10000;
		public const int NONVIRTUAL= 0x20000;
		private const int TOP      = 0x20000;
		

		public const int Accessibility =
			PUBLIC | PROTECTED | INTERNAL | PRIVATE;
		
		static public string Name (int i)
		{
			string s = "";
			
			switch (i) {
			case Modifiers.NEW:
				s = "overloads"; break;
			case Modifiers.PUBLIC:
				s = "public"; break;
			case Modifiers.PROTECTED:
				s = "protected"; break;
			case Modifiers.INTERNAL:
				s = "friend"; break;
			case Modifiers.PRIVATE:
				s = "private"; break;
			case Modifiers.ABSTRACT:
				s = "mustinherit"; break;
			case Modifiers.SEALED:
				s = "notinheritable"; break;
			case Modifiers.STATIC:
				s = "shared"; break;
			case Modifiers.READONLY:
				s = "readonly"; break;
			case Modifiers.VIRTUAL:
				s = "overridable"; break;
			case Modifiers.OVERRIDE:
				s = "overrides"; break;
			case Modifiers.EXTERN:
				s = "extern"; break;
			case Modifiers.VOLATILE:
				s = "volatile"; break;
			case Modifiers.SHADOWS:
				s = "shadows"; break;
			case Modifiers.NONVIRTUAL:
				s = "notoveridable"; break;
			}

			return s;
		}

		public static TypeAttributes TypeAttr (int mod_flags, bool is_toplevel)
		{
			TypeAttributes t = 0;

			if (is_toplevel){
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

			return t;
		}
		
		public static TypeAttributes TypeAttr (int mod_flags, TypeContainer caller)
		{
			TypeAttributes t = TypeAttr (mod_flags, caller.IsTopLevel);

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
			if ((mod_flags & PROTECTED) != 0){
				if ((mod_flags & INTERNAL) != 0)
					fa |= FieldAttributes.FamORAssem;
				else 
					fa |= FieldAttributes.Family;
			} else {
				if ((mod_flags & INTERNAL) != 0)
					fa |= FieldAttributes.Assembly;
			}
			
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
			if ((mod_flags & PROTECTED) != 0){
				if ((mod_flags & INTERNAL) != 0)
					ma |= MethodAttributes.FamORAssem;
				else 
					ma |= MethodAttributes.Family;
			} else {
				if ((mod_flags & INTERNAL) != 0)
					ma |= MethodAttributes.Assembly;
			}

			if ((mod_flags & STATIC) != 0)
				ma |= MethodAttributes.Static;
			if ((mod_flags & ABSTRACT) != 0){
				ma |= MethodAttributes.Abstract | MethodAttributes.Virtual |
					MethodAttributes.HideBySig;
			}
			if ((mod_flags & NONVIRTUAL) != 0)
				ma |= MethodAttributes.Final;

			if ((mod_flags & VIRTUAL) != 0)
				ma |= MethodAttributes.Virtual;

			if ((mod_flags & OVERRIDE) != 0)
				ma |= MethodAttributes.Virtual | MethodAttributes.HideBySig;
			else {
				if ((ma & MethodAttributes.Virtual) != 0)
					ma |= MethodAttributes.NewSlot;
			}
			
			if ((mod_flags & NEW) != 0)
				ma |= MethodAttributes.HideBySig;

			//if ((mod_flags & SHADOWS) != 0)
				// needs to be fixed

			return ma;
		}

		// <summary>
		//   Checks the object @mod modifiers to be in @allowed.
		//   Returns the new mask.  Side effect: reports any
		//   incorrect attributes. 
		// </summary>
		public static int Check (int allowed, int mod, int def_access, Location l)
		{
			int invalid_flags  = (~allowed) & mod;
			int i;

			if (invalid_flags == 0){
				int a = mod;

				if ((mod & Modifiers.UNSAFE) != 0){
					if (!RootContext.Unsafe){
						Report.Error (227, l,
							      "Unsafe code requires the --unsafe command " +
							      "line option to be specified");
					}
				}
				
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
					Report.Error (30176, l, "More than one protection modifier specified");
				
				return mod;
			}

			for (i = 1; i < TOP; i <<= 1){
				if ((i & invalid_flags) == 0)
					continue;

				Error_InvalidModifier (l, Name (i));
			}

			return allowed & mod;
		}

		public static void Error_InvalidModifier (Location l, string name)
		{
			Report.Error (106, l, "the modifier " + name + " is not valid for this item");
		}
	}
}
