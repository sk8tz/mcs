//
// System.Windows.Forms.FontDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@raytek.com)
//   Implemented by Jordi Mas i Hern�ndez (jmas@softcatala.org)
//
// (C) 2002-2003 Ximian, Inc
//
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing;
namespace System.Windows.Forms 
{	
        public class FontDialog : CommonDialog 
        {        	        	
	        private bool	bAllowScriptChange;
	        private bool	bAllowSimululations;							
		  	private bool	bAllowVectorForms;							
		  	private bool	bAllowVerticalForms;							
		  	private Color	color = Color.Black;							
		  	private bool	bFixedPitchOnly;							
		  	private Font	font;							
		  	private bool	bFontMustExist;							
		  	private int		nMaxSize;							
		  	private int		nMinSize;								  	
		  	private bool	bScriptsOnly;							
		  	private bool	bShowApply;								  	
		  	private bool	bShowColor;							
		  	private bool	bShowEffects;							
		  	private bool	bShowHelp;	
	
			//
			//  --- Constructor
			//		
			public FontDialog()
			{
				defaultValues();
			}
			
			internal void defaultValues()
			{		
				font = new Font("Microsoft Sans Serif", 8);
				bAllowScriptChange = true;
	        	bAllowSimululations  = true;							
		  		bAllowVectorForms  = true;							
		  		bAllowVerticalForms  = true;							
		  		color = Color.Black;							
		  		bFixedPitchOnly = false;								  		
		  		bFontMustExist = false;
		  		nMaxSize = 0 ;							
		  		nMinSize = 0;								  	
		  		bScriptsOnly = false;							
		  		bShowApply = true;								  	
		  		bShowColor = false;							
		  		bShowEffects = true;							
		  		bShowHelp = false;				
			}
	
			//
			//  --- Public Properties
			//		
			public virtual bool AllowScriptChange 
			{
				get { return bAllowScriptChange;  }
				set { bAllowScriptChange = value; }
			}
			
			public bool AllowSimululations 
			{
				get { return AllowSimululations;  }
				set { bAllowSimululations = value; }
			}
			
			public bool AllowVectorForms 
			{
				get { return AllowVectorForms;  }
				set { AllowVectorForms = value; }
			}
			
			public bool AllowVerticalForms 
			{
				get { return AllowVerticalForms;  }
				set { AllowVerticalForms = value; }
			}
			
			public Color Color 
			{
				get { return color;  }
				set { color = value; }
			}			
			
			public bool FixedPitchOnly 
			{
				get { return bFixedPitchOnly;  }
				set { bFixedPitchOnly = value; }
			}
			
			public Font Font 
			{
				get { return font;  }
				set { font = value; }
			}
			
			public bool FontMustExist 
			{
				get { return bFontMustExist;  }
				set { bFontMustExist = value; }
			}
			
			public int MaxSize 
			{
				get { return nMaxSize;  }
				set { nMaxSize = value; }
			}
			
			public int MinSize 
			{
				get { return nMinSize;  }
				set { nMinSize = value; }
			}
			
			public bool ScriptsOnly 
			{
				get { return bScriptsOnly;  }
				set { bScriptsOnly = value; }
			}
			
			public bool ShowApply 
			{
				get { return bShowApply;  }
				set { bShowApply = value; }
			}
			
			public bool ShowColor 
			{
				get { return bShowColor;  }
				set { bShowColor = value; }
			}
			
			public bool ShowEffects 
			{
				get { return bShowEffects;  }
				set { bShowEffects = value; }
			}
			
			public bool ShowHelp 
			{
				get { return bShowHelp;  }
				set { bShowHelp = value; }
			}
	
			
			//  --- Public Methods			
			public override void Reset()
			{				
				defaultValues();
			}
	
			//
			//  --- Public Events
			//
			[MonoTODO]
			public event EventHandler Apply;
	
			
			[MonoTODO]
			protected override IntPtr HookProc( IntPtr hWnd,  int msg,  IntPtr wparam,  IntPtr lparam )
			{
				return base.HookProc(hWnd, msg, wparam,lparam);					
			}
			protected override bool RunDialog(IntPtr hWndOwner)
			{						
				CHOOSEFONT cf = new CHOOSEFONT();
				LOGFONT lf = new LOGFONT();						
	      		
	      		cf.lStructSize  = (uint)Marshal.SizeOf(cf);
				cf.nSizeMin =  MinSize;
				cf.nSizeMax = MaxSize;		
							 
				lf.lfFaceName=Font.FontFamily.Name;			
				lf.lfWeight = (uint)400/* FW_NORMAL*/ ;
				lf.lfHeight = (uint) -((Font.Size *  96)/72); // TODO: Use Win32.GetDeviceCaps(0, LOGPIXELSY) when implemented
				
				//cf.lpfnHook = new Win32.FnHookProc(HookProc);			
				cf.Flags = (uint)(FontDlgFlags.CF_SCREENFONTS| /* FontDlgFlags.CF_ENABLEHOOK |*/  FontDlgFlags.CF_EFFECTS |FontDlgFlags.CF_INITTOLOGFONTSTRUCT);			
				
				// Flags			
				if (!AllowScriptChange) cf.Flags |= (int)FontDlgFlags.CF_NOSCRIPTSEL;
				if (ShowApply) cf.Flags |= (int)FontDlgFlags.CF_APPLY;			
				if (bShowEffects)  cf.Flags |= (int)FontDlgFlags.CF_EFFECTS;			
		  		if (bShowHelp)  cf.Flags |= (int)FontDlgFlags.CF_SHOWHELP;			
		  		if (bFixedPitchOnly) cf.Flags |= (int)FontDlgFlags.CF_SHOWHELP;			
	  		  	if (bFontMustExist) cf.Flags |= (int)FontDlgFlags.CF_FORCEFONTEXIST;										
	  		  	if (!(MaxSize==0) || !(MinSize==0)) cf.Flags |= (int)FontDlgFlags.CF_LIMITSIZE;										
				
				// Color			
				cf.rgbColors = (uint) (color.R | color.G<<8 |color.B <<16);					
				IntPtr lfBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(lf));
	      		Marshal.StructureToPtr(lf, lfBuffer, false);			
	      		cf.lpLogFont = lfBuffer;										        
				
				// Show dialog box
				if (Win32_WineLess.ChooseFont(ref cf))
				{				
					lf = (LOGFONT) Marshal.PtrToStructure(lfBuffer,  typeof(LOGFONT));			
			    	Marshal.FreeHGlobal(lfBuffer);
			    
			    	// Get font		    		    
			    	font = new Font(lf.lfFaceName, cf.iPointSize/10);		    
			    	Color = Color.FromArgb (0, (int)cf.rgbColors & 0x0FF, (int)(cf.rgbColors >> 8) & 0x0FF, (int) (cf.rgbColors >> 16) & 0x0FF);
			    	return true;
			    }
			    else
			    	return false;		   				
			}
		 }
}

