@echo off
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM output files name prefix (mandratory) : prefix for naming output xml files
REM test fixture name (optional) : if you want to run some particular test fixture
REM directory to run tests (optional)
REM path back to root directory (opposite to previous param)
REM example run-tests build GhTests Test.Sys.Drawing Test\DrawingTest\Test ..\..\..\
REM will cause to build (and not rebuild) test solutions,
REM running Test.Sys.Drawing fixture in directory Test\DrawingTest\Test
REM with output files named GhTests.Net.xml and GhTests.GH.xml
REM ********************************************************

IF "%1"=="" GOTO USAGE

IF "%VMW_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION



IF "%1"=="" (
	set BUILD_OPTION=rebuild
) ELSE (
	set BUILD_OPTION=%1
)

REM ********************************************************
REM Set parameters
REM ********************************************************

set BUILD_OPTION=%1
set OUTPUT_FILE_PREFIX=System.Data.OracleClient
set RUNNING_FIXTURE=MonoTests.System.Data.OracleClient

set TEST_SOLUTION=Test\System.Data.OracleClient.Tests20.J2EE.sln
set TEST_ASSEMBLY=System.Data.OracleClient.Tests20.J2EE.jar
set PROJECT_CONFIGURATION=Debug_Java20
set APP_CONFIG_FILE=Test\System.Data.OracleClient.J2EE.config

set OUTPUT_FILE_PREFIX=%OUTPUT_FILE_PREFIX%

REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%VMW_HOME%\jgac\vmw4j2ee_110\
set JAVA_HOME=%VMW_HOME%\jre5

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Xml.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Data.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%J2SE.Helpers.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Data.OracleClient.jar

set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%GHROOT%\jgac\jdbc\ojdbc14.jar

set NUNIT_OPTIONS=/exclude=NotWorking

set DATEL=%date:~4,2%_%date:~7,2%_%date:~10,4%
set TIMEL=%time:~0,2%_%time:~3,2%
set TIMESTAMP=%DATEL%_%TIMEL%

set GH_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.%RUNNING_FIXTURE%.GH.%TIMESTAMP%.xml
set BUILD_LOG=%OUTPUT_FILE_PREFIX%.%RUNNING_FIXTURE%_build.log.%TIMESTAMP%.txt
set RUN_LOG=%OUTPUT_FILE_PREFIX%.%RUNNING_FIXTURE%_run.log.%TIMESTAMP%.txt

set NUNIT_PATH=..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.framework.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.util.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.core.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit-console.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;.
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%TEST_ASSEMBLY%

set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"

REM ********************************************************
@echo Building GH solution...
REM ********************************************************

REM devenv %TEST_SOLUTION% /%BUILD_OPTION% %PROJECT_CONFIGURATION% >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %TEST_SOLUTION% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Building NUnit solution...
REM ********************************************************

REM devenv ..\..\nunit20\nunit.java.sln /%BUILD_OPTION% %PROJECT_CONFIGURATION% >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild ..\..\nunit20\nunit20.java.sln /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Running GH tests...
REM ********************************************************

REM ********************************************************
@echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy Test\bin\%PROJECT_CONFIGURATION%\%TEST_ASSEMBLY% .
copy %APP_CONFIG_FILE% nunit-console.exe.config


REM @echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUN_LOG% 2<&1
REM @echo off

REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool
REM devenv %XML_TOOL_PATH%\XmlTool.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %XML_TOOL_PATH%\XmlTool20.vmwcsproj /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

copy %XML_TOOL_PATH%\bin\%PROJECT_CONFIGURATION%\xmltool.exe .
copy %XML_TOOL_PATH%\nunit_transform.xslt .

REM ********************************************************
@echo Analyze and print results
REM ********************************************************
@echo on
xmltool.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%
@echo off

:FINALLY

copy %RUN_LOG% ..\
copy %BUILD_LOG% ..\
copy %GH_OUTPUT_XML% ..\

GOTO END

:ENVIRONMENT_EXCEPTION
@echo This test requires environment variable VMW_HOME to be defined
GOTO END

:BUILD_EXCEPTION
@echo Error in building solutions. See %BUILD_LOG% for details...
REM EXIT 1
GOTO END

:RUN_EXCEPTION
@echo Error in running fixture %RUNNING_FIXTURE%. See %RUN_LOG% for details...
REM EXIT 1
GOTO END

:USAGE
@echo Parameters: "[build|rebuild]"
GOTO END

:END
REM EXIT 0
