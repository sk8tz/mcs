//
// StringFormatFlags class testing unit
//
// Author:
//
// 	 Jordi Mas i Hern�ndez (jordi@ximian.com)
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class StringFormatTest {
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
		
		[Test]
		public void TestSpecialConstructors() 
		{				
			StringFormat smf = StringFormat.GenericDefault;			
			smf = StringFormat.GenericTypographic;											
		}	
		
		[Test]
		public void TestClone() 
		{						
			StringFormat smf = new StringFormat();						
			StringFormat smfclone = (StringFormat) smf.Clone();			
			
			Assert.AreEqual (smf.LineAlignment, smfclone.LineAlignment);			
			Assert.AreEqual (smf.FormatFlags, smfclone.FormatFlags);			
			Assert.AreEqual (smf.LineAlignment, smfclone.LineAlignment);			
			Assert.AreEqual (smf.Alignment, smfclone.Alignment);			
			Assert.AreEqual (smf.Trimming, smfclone.Trimming);			
		}
			
		[Test]
		public void TestAlignment() 
		{					
			StringFormat	smf = new StringFormat ();
			
			smf.LineAlignment = StringAlignment.Center;									
			Assert.AreEqual (StringAlignment.Center, smf.LineAlignment);			
			
			smf.Alignment = StringAlignment.Far;									
			Assert.AreEqual (StringAlignment.Far, smf.Alignment);						 
		}		
			
		[Test]
		public void TestFormatFlags() 
		{				
			StringFormat	smf = new StringFormat ();
			
			smf.FormatFlags = StringFormatFlags.DisplayFormatControl;									
			Assert.AreEqual (StringFormatFlags.DisplayFormatControl, smf.FormatFlags);						 
		}		
		
		[Test]
		public void TabsStops() 
		{				
			StringFormat	smf = new StringFormat ();
			
			float firstTabOffset;			
			float[] tabsSrc = {100, 200, 300, 400};
			float[] tabStops;
			
			smf.SetTabStops(200, tabsSrc);
			tabStops = smf.GetTabStops(out firstTabOffset);
			
			Assert.AreEqual (200, firstTabOffset);						 
			Assert.AreEqual (tabsSrc.Length, tabStops.Length);						 
			Assert.AreEqual (tabsSrc[0], tabStops[0]);					
			Assert.AreEqual (tabsSrc[1], tabStops[1]);					
			Assert.AreEqual (tabsSrc[2], tabStops[2]);					
			Assert.AreEqual (tabsSrc[3], tabStops[3]);					
		}	
		
	}
}
