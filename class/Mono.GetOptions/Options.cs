using System;

namespace Mono.GetOptions
{
	public class Options
	{
		public OptionsParsingMode ParsingMode;
		public bool EndOptionProcessingWithDoubleDash;

		private OptionList optionParser;

		public string[] RemainingArguments;

		public Options()
		{
			ParsingMode = OptionsParsingMode.Both;
			EndOptionProcessingWithDoubleDash = true;
		}

		public void ProcessArgs(string[] args)
		{
			optionParser = new OptionList(this);
			RemainingArguments =  optionParser.ProcessArgs(args);
		}

		[Option("Show this help list", '?',"help")]
		public virtual WhatToDoNext DoHelp()
		{
			return optionParser.DoHelp();
		}

		[Option("Display version and licensing information", 'V', "version")]
		public virtual WhatToDoNext DoAbout()
		{
			return optionParser.DoAbout();
		}

		[Option("Show usage syntax and exit", ' ',"usage")]
		public virtual WhatToDoNext DoUsage()
		{
			return optionParser.DoUsage();
		}


	}
	
}
