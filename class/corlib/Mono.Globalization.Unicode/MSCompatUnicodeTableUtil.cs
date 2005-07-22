using System;
using System.Globalization;
using System.Text;

namespace Mono.Globalization.Unicode
{
	internal /*static*/ class MSCompatUnicodeTableUtil
	{
		public const byte ResourceVersion = 1;

		public static readonly CodePointIndexer Ignorable;
		public static readonly CodePointIndexer Category;
		public static readonly CodePointIndexer Level1;
		public static readonly CodePointIndexer Level2;
		public static readonly CodePointIndexer Level3;
		public static readonly CodePointIndexer WidthCompat;
		public static readonly CodePointIndexer CjkCHS;
		public static readonly CodePointIndexer Cjk;

		static MSCompatUnicodeTableUtil ()
		{
			// FIXME: those ranges could be more compact, but since
			// I haven't filled all the table yet, I keep it safer.
			int [] ignoreStarts = new int [] {
				0, 0xA000, 0xF900};
			int [] ignoreEnds = new int [] {
				0x3400, 0xA500, 0x10000};
			int [] catStarts = new int [] {
				0, 0x1E00, 0x3000, 0x4E00, 0xAC00, 0xF900};
			int [] catEnds = new int [] {
				0x1200, 0x2800, 0x3400, 0xA000, 0xD7B0, 0x10000};
			int [] lv1Starts = new int [] {
				0, 0x1E00, 0x3000, 0x4E00, 0xAC00, 0xF900};
			int [] lv1Ends = new int [] {
				0x1200, 0x2800, 0x3400, 0xA000, 0xD7B0, 0x10000};
			int [] lv2Starts = new int [] {0, 0x1E00, 0x3000, 0xFB00};
			int [] lv2Ends = new int [] {0xF00, 0x2800, 0x3400, 0x10000};
			int [] lv3Starts = new int [] {0, 0x1E00, 0x3000, 0xFB00};
			int [] lv3Ends = new int [] {0x1200, 0x2800, 0x3400, 0x10000};
			int [] widthStarts = new int [] {0, 0x2000, 0x3100, 0xFF00};
			int [] widthEnds = new int [] {0x300, 0x2200, 0x3200, 0x10000};
			int [] chsStarts = new int [] {
				0x3100, 0x4E00, 0xE800}; // FIXME: really?
			int [] chsEnds = new int [] {
				0x3400, 0xA000, 0x10000};
			int [] cjkStarts = new int [] {0x3100, 0x4E00, 0xF900};
			int [] cjkEnds = new int [] {0x3400, 0xA000, 0xFB00};

			Ignorable = new CodePointIndexer (ignoreStarts, ignoreEnds, -1, -1);
			Category = new CodePointIndexer (catStarts, catEnds, 0, 0);
			Level1 = new CodePointIndexer (lv1Starts, lv1Ends, 0, 0);
			Level2 = new CodePointIndexer (lv2Starts, lv2Ends, 0, 0);
			Level3 = new CodePointIndexer (lv3Starts, lv3Ends, 0, 0);
			WidthCompat = new CodePointIndexer (widthStarts, widthEnds, 0, 0);
			CjkCHS = new CodePointIndexer (chsStarts, chsEnds, -1, -1);
			Cjk = new CodePointIndexer (cjkStarts, cjkEnds, -1, -1);
		}
	}
}
