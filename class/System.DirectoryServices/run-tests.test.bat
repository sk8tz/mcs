@echo off
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM output files name prefix (mandratory) : prefix for naming output xml files
REM secure mode (mandratory): secure or non-secure mode indicator
REM test fixture name (optional) : if you want to run some particular test fixture
REM example run-tests build GhTests Test.Sys.Drawing  
REM will cause to build (and not rebuild) test solutions,
REM running Test.Sys.Drawing fixture 
REM with output files named GhTests.Net.xml and GhTests.GH.xml
REM ********************************************************


IF "%1"=="" GOTO USAGE

IF "%JAVA_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

IF "%GH_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

REM ********************************************************
REM Set parameters
REM ********************************************************

set BUILD_OPTION=%1
set SECURE_MODE=%2
set OUTPUT_FILE_PREFIX=%3
set RUNNING_FIXTURE=%4


REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%GH_HOME%\jgac\vmw4j2ee_110\

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar;%JGAC_PATH%System.jar;%JGAC_PATH%System.Xml.jar;%JGAC_PATH%System.DirectoryServices.jar;%JGAC_PATH%Novell.Directory.Ldap.jar;%JGAC_PATH%J2SE.Helpers.jar
set NUNIT_OPTIONS=/exclude=NotWorking

set GH_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.GH.%SECURE_MODE%.xml

set NUNIT_PATH=%BACK_TO_ROOT_DIR%..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.framework.jar;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.util.jar;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.core.jar;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit-console.jar;.
set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"


REM ********************************************************
REM @echo Building GH solution...
REM ********************************************************

devenv Test\System.DirectoryServices.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
REM @echo Building NUnit solution...
REM ********************************************************

devenv ..\..\nunit20\nunit.java.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
REM @echo Running GH tests...
REM ********************************************************

REM ********************************************************
REM @echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy Test\bin\Debug_Java\System.DirectoryServices.Test.jar .

IF "%SECURE_MODE%" NEQ "secure" (
	copy App.config nunit-console.exe.config 
) ELSE (
	copy Secure.config nunit-console.exe.config 
)

REM @echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi System.DirectoryServices.Test.jar /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUNNING_FIXTURE%_run.log.txt 2<&1
REM @echo off


REM ********************************************************
REM @echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool
devenv %XML_TOOL_PATH%\XmlTool.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

copy %XML_TOOL_PATH%\bin\Debug_Java\xmltool.exe .
copy %XML_TOOL_PATH%\nunit_transform.xslt .

REM ********************************************************
REM @echo Analyze and print results
REM ********************************************************
REM @echo on
xmltool.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%
REM @echo off

:FINALLY
GOTO END

:ENVIRONMENT_EXCEPTION
@echo This test requires environment variables JAVA_HOME and GH_HOME to be defined
GOTO END

:BUILD_EXCEPTION
@echo Error in building solutions. See %RUNNING_FIXTURE%_build.log.txt for details...
REM EXIT 1
GOTO END

:RUN_EXCEPTION
@echo Error in running fixture %RUNNING_FIXTURE%. See %RUNNING_FIXTURE%_run.log.txt for details...
REM EXIT 1
GOTO END

:USAGE
@echo Parameters: "[build|rebuild] <output_file_name_prefix> <test_fixture>"
GOTO END

:END
REM EXIT 0
