// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// Driver.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

using System;
using System.Xml;

public class Driver
{
	public static void Main(string[] args)
	{
		XmlReader xmlReader = null;

		if (args.Length < 1)
		{
			xmlReader = new XmlTextReader(Console.In);
		}
		else
		{
			xmlReader = new XmlTextReader(args[0]);
		}

		while (xmlReader.Read())
		{
			Console.WriteLine("NodeType = {0}", xmlReader.NodeType);
			Console.WriteLine("  Name = {0}", xmlReader.Name);
			Console.WriteLine("  IsEmptyElement = {0}", xmlReader.IsEmptyElement);
			Console.WriteLine("  HasAttributes = {0}", xmlReader.HasAttributes);
			Console.WriteLine("  AttributeCount = {0}", xmlReader.AttributeCount);
			Console.WriteLine("  HasValue = {0}", xmlReader.HasValue);
			Console.WriteLine("  Value = {0}", xmlReader.Value);
		}
	}
}
