rem @echo off
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM example run-tests build 
REM will cause to build (and not rebuild) test solutions,
REM ********************************************************

IF "%JAVA_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

IF "%GH_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION
IF "%GHROOT%"=="" set GHROOT=%GH_HOME%

REM ********************************************************
REM Set parameters
REM ********************************************************

IF "%1"=="" (
	set BUILD_OPTION=rebuild
) ELSE (
	set BUILD_OPTION=%1
)
set OUTPUT_FILE_PREFIX=GH_TEST


REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%GH_HOME%\jgac\vmw4j2ee_110\
set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar;%JGAC_PATH%System.jar;%JGAC_PATH%System.Xml.jar;%JGAC_PATH%J2SE.Helpers.jar;
set NUNIT_OPTIONS=/fixture=MonoTests.W3C_xmlconf.CleanTests
set PROJECT_CONFIGURATION=Debug_Java20
set GH_OUTPUT_XML=W3C_nunit_results.xml
set NUNIT_PATH=..\..\..\..\..\nunit20
set NUNIT_CLASSPATH=%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.framework.jar;%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.util.jar;%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.core.jar;%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit-console.jar
set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"
set W3C_DIR=Test\System.Xml\W3C\

pushd %W3C_DIR%

REM ********************************************************
@echo Building NUnit solution...
REM ********************************************************

if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP
msbuild %NUNIT_PATH%\nunit20.java.sln /t:%BUILD_OPTION% /p:configuration=%PROJECT_CONFIGURATION% >build.log.txt 2<&1

goto NUNITREADY

:NUNITSKIP
echo Skipping NUnit Build...

:NUNITREADY
set NUNIT_BUILD=DONE

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\..\..\..\tools\mono-xmltool
msbuild %XML_TOOL_PATH%\XmlTool20.sln /p:configuration=Debug >>build.log.txt 2<&1
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
copy %XML_TOOL_PATH%\bin\Debug\xmltool.exe ..\..\..
copy %XML_TOOL_PATH%\nunit_transform.xslt ..\..\..

REM ********************************************************
@echo Building GH solution...
REM ********************************************************
del W3C.jar
msbuild W3c20.J2EE.sln /t:%BUILD_OPTION% /p:configuration=%PROJECT_CONFIGURATION% >>build.log.txt 2<&1
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Building test catalog...
REM ********************************************************
del xmlts20031210.zip
wget http://www.w3.org/XML/Test/xmlts20031210.zip
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

mkdir xmlconf
unzip -un xmlts20031210.zip
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION


REM ********************************************************
@echo Running GH tests...
REM ********************************************************
@echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi W3C.jar %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML%  >run.log.txt 2<&1
@echo off

popd

copy %W3C_DIR%\%GH_OUTPUT_XML% .

REM ********************************************************
@echo Analyze and print results
REM ********************************************************
@echo on
xmltool.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%
@echo off

:FINALLY
GOTO END

:ENVIRONMENT_EXCEPTION
@echo This test requires environment variables JAVA_HOME and GH_HOME to be defined
GOTO END

:BUILD_EXCEPTION
popd
@echo Error in building solutions. See build.log.txt for details...
GOTO END

:RUN_EXCEPTION
popd
@echo Error in running fixture. See run.log.txt for details...
GOTO END

:END
