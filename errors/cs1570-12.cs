// cs1570.cs: XML comment on `T:Test' has non-well-formed XML (Cannot insert specified type of node as a child of this node.)
// Line: 13
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

/// Text goes here.
///
/// <?xml version = "1.0" encoding = "utf-8" ?>
/// <configuration>
///     <appSettings>
///         <add key = "blah" value = "blech"/>
///     </appSettings>
/// </configuration> 
public class Test
{    
    static void Main ()
    {
    }
}

