//
// Mono.ILASM.Report
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.IO;

namespace Mono.ILASM {

        public class Report {

                private int error_count;
                private int mark_count;

                public Report ()
                {
                        error_count = 0;
                }

                public int ErrorCount {
                        get { return error_count; }
                }

                public void AssembleFile (string file, string listing,
                                          string target, string output)
                {
                        Console.WriteLine ("Assembling '{0}' , {1}, to {2} --> '{3}'", file,
                                           GetListing (listing), target, output);
                }

                public void Error (string message)
                {
                        error_count++;
                        Console.WriteLine ("Error: " + message);
                }

                private string GetListing (string listing)
                {
                        if (listing == null)
                                return "no listing file";
                        return listing;
                }

        }

}

