﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Collections;
using NUnit.Framework;
using Microsoft.Build.Utilities;
using CanonicalError = Microsoft.Build.Shared.CanonicalError;
using System.Text.RegularExpressions;

namespace Microsoft.Build.UnitTests
{
    [TestFixture]
    public class CanonicalErrorTest
    {
        [Test]
        public void EmptyOrigin()
        {
            ValidateToolError(@"error CS0006: Metadata file 'C:\WINDOWS\Microsoft.NET\Framework\v1.2.21213\System.dll' could not be found", "", CanonicalError.Parts.Category.Error, "CS0006", @"Metadata file 'C:\WINDOWS\Microsoft.NET\Framework\v1.2.21213\System.dll' could not be found");
        }

        [Test]
        public void Alink()
        {
            // From AL.EXE
            ValidateToolError(@"ALINK: error AL1017: No target filename was specified", "ALINK", CanonicalError.Parts.Category.Error, "AL1017", @"No target filename was specified");
        }

        [Test]
        public void CscWithFilename()
        {
            // From CSC.EXE
            ValidateFileNameLineColumnError(@"foo.resx(2,1): error CS0116: A namespace does not directly contain members such as fields or methods", @"foo.resx", 2, 1, CanonicalError.Parts.Category.Error, "CS0116", "A namespace does not directly contain members such as fields or methods");
            ValidateFileNameLineColumnError(@"Main.cs(17,20): warning CS0168: The variable 'foo' is declared but never used", @"Main.cs", 17, 20, CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");
        }

        [Test]
        public void VbcWithFilename()
        {
            // From VBC.EXE
            ValidateFileNameLineError(@"C:\WINDOWS\Microsoft.NET\Framework\v1.2.x86fre\foo.resx(2) : error BC30188: Declaration expected.", @"C:\WINDOWS\Microsoft.NET\Framework\v1.2.x86fre\foo.resx", 2, CanonicalError.Parts.Category.Error, "BC30188", "Declaration expected.");
        }

        [Test]
        public void ClWithFilename()
        {
            // From CL.EXE
            ValidateFileNameLineError(@"foo.cpp(1) : error C2143: syntax error : missing ';' before '++'", @"foo.cpp", 1, CanonicalError.Parts.Category.Error, "C2143", "syntax error : missing ';' before '++'");
        }

        [Test]
        public void JscWithFilename()
        {
            // From JSC.EXE
            ValidateFileNameLineColumnError(@"foo.resx(2,1) : error JS1135: Variable 'blech' has not been declared", @"foo.resx", 2, 1, CanonicalError.Parts.Category.Error, "JS1135", "Variable 'blech' has not been declared");
        }

        [Test]
        public void LinkWithFilename()
        {
            // From Link.exe 
            // Note that this is impossible to distinguish from a tool error without
            // actually looking at the disk to see if the given file is there.
            ValidateFileNameError(@"foo.cpp : fatal error LNK1106: invalid file or disk full: cannot seek to 0x5361", @"foo.cpp", CanonicalError.Parts.Category.Error, "LNK1106", "invalid file or disk full: cannot seek to 0x5361");
        }

        [Test]
        public void BscMake()
        {
            // From BSCMAKE.EXE
            ValidateToolError(@"BSCMAKE: error BK1510 : corrupt .SBR file 'foo.cpp'", "BSCMAKE", CanonicalError.Parts.Category.Error, "BK1510", @"corrupt .SBR file 'foo.cpp'");
        }

        [Test]
        public void CvtRes()
        {
            // From CVTRES.EXE
            ValidateToolError(@"CVTRES : warning CVT4001: machine type not specified; assumed X86", "CVTRES", CanonicalError.Parts.Category.Warning, "CVT4001", @"machine type not specified; assumed X86");
            ValidateToolError(@"CVTRES : fatal error CVT1103: cannot read file", "CVTRES", CanonicalError.Parts.Category.Error, "CVT1103", @"cannot read file");
        }

        [Test]
        public void DumpBinWithFilename()
        {
            // From DUMPBIN.EXE (notice that an 'LNK' error is returned).
            ValidateFileNameError(@"foo.cpp : warning LNK4048: Invalid format file; ignored", @"foo.cpp", CanonicalError.Parts.Category.Warning, "LNK4048", "Invalid format file; ignored");
        }


        [Test]
        public void LibWithFilename()
        {
            // From LIB.EXE
            ValidateFileNameError(@"foo.cpp : fatal error LNK1106: invalid file or disk full: cannot seek to 0x5361", @"foo.cpp", CanonicalError.Parts.Category.Error, "LNK1106", "invalid file or disk full: cannot seek to 0x5361");
        }

        [Test]
        public void MlWithFilename()
        {
            // From ML.EXE
            ValidateFileNameLineError(@"bar.h(2) : error A2008: syntax error : lksdflksj", @"bar.h", 2, CanonicalError.Parts.Category.Error, "A2008", "syntax error : lksdflksj");
            ValidateFileNameLineError(@"bar.h(2) : error A2088: END directive required at end of file", @"bar.h", 2, CanonicalError.Parts.Category.Error, "A2088", "END directive required at end of file");
        }

        [Test]
        public void VcDeployWithFilename()
        {
            // From VCDEPLOY.EXE
            ValidateToolError(@"vcdeploy : error VCD0041: IIS must be installed on this machine in order for this program to function correctly.", "vcdeploy", CanonicalError.Parts.Category.Error, "VCD0041", @"IIS must be installed on this machine in order for this program to function correctly.");
        }

        [Test]
        public void VCBuildError()
        {
            // From VCBUILD.EXE
            ValidateFileNameLineError(@"1>c:\temp\testprefast\testprefast\testprefast.cpp(12) : error C4996: 'sprintf' was declared deprecated", @"c:\temp\testprefast\testprefast\testprefast.cpp", 12, CanonicalError.Parts.Category.Error, "C4996", "'sprintf' was declared deprecated");
            ValidateFileNameLineError(@"1234>c:\temp\testprefast\testprefast\testprefast.cpp(12) : error C4996: 'sprintf' was declared deprecated", @"c:\temp\testprefast\testprefast\testprefast.cpp", 12, CanonicalError.Parts.Category.Error, "C4996", "'sprintf' was declared deprecated");
        }

        [Test]
        public void FileNameLine()
        {
            ValidateFileNameMultiLineColumnError("foo.cpp(1):error TST0000:Text", "foo.cpp", 1, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.Category.Error, "TST0000", "Text");
        }

        [Test]
        public void FileNameLineLine()
        {
            ValidateFileNameMultiLineColumnError("foo.cpp(1-5):error TST0000:Text", "foo.cpp", 1, CanonicalError.Parts.numberNotSpecified, 5, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.Category.Error, "TST0000", "Text");
        }

        [Test]
        public void FileNameLineCol()
        {
            ValidateFileNameMultiLineColumnError("foo.cpp(1,15):error TST0000:Text", "foo.cpp", 1, 15, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.Category.Error, "TST0000", "Text");
        }

        [Test]
        public void FileNameLineColCol()
        {
            ValidateFileNameMultiLineColumnError("foo.cpp(1,15-25):error TST0000:Text", "foo.cpp", 1, 15, CanonicalError.Parts.numberNotSpecified, 25, CanonicalError.Parts.Category.Error, "TST0000", "Text");
        }

        [Test]
        public void FileNameLineColLineCol()
        {
            ValidateFileNameMultiLineColumnError("foo.cpp(1,15,2,25):error TST0000:Text", "foo.cpp", 1, 15, 2, 25, CanonicalError.Parts.Category.Error, "TST0000", "Text");
        }

        [Test]
        public void PathologicalFileNameWithParens()
        {
            // Pathological case, there is actually a file with () at the end (Doesn't work, treats the (1) as a line number anyway).
            ValidateFileNameMultiLineColumnError("PathologicalFile.txt(1):error TST0000:Text", "PathologicalFile.txt", 1, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.Category.Error, "TST0000", "Text");
        }

        [Test]
        public void OverflowTrimmingShouldNotDropChar()
        {
            // A devdiv build produced a huge message like this!
            string message = @"The name 'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX' does not exist in the current context";
            string error = @"test.cs(1,32): error CS0103: " + message;

            CanonicalError.Parts parts = CanonicalError.Parse(error);

            Helpers.VerifyAssertLineByLine(message, parts.text);
        }

        [Test]
        public void ValidateErrorMessageWithFileName()
        {
            ValidateFileNameError("error CS2011: Error opening response file 'e:\foo\test.rsp' -- 'The device is not ready. '",
                "", CanonicalError.Parts.Category.Error, "CS2011", "Error opening response file 'e:\foo\test.rsp' -- 'The device is not ready. '");
        }

        [Test]
        public void ValidateErrorMessageWithFileName2()
        {
            ValidateFileNameError(@"BUILDMSG: error: Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.",
                "BUILDMSG", CanonicalError.Parts.Category.Error, "", @"Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");

            ValidateFileNameError(@"BUILDMSG: error : Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.",
                "BUILDMSG", CanonicalError.Parts.Category.Error, "", @"Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");
        }

        [Test]
        public void ValidateErrorMessageWithFileName3()
        {
            ValidateNormalMessage(@"BUILDMSG: errorgarbage: Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");

            ValidateNormalMessage(@"BUILDMSG: errorgarbage : Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");
        }

        [Test]
        public void ValidateErrorMessageVariableNotUsed()
        {
            //      (line)
            ValidateFileNameMultiLineColumnError("Main.cs():Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            // This one actually falls under the (line-line) category. I'm not going to tweak the regex for this incorrect input just so we can 
            // pretend -3 == 0, and just leaving it here for completeness
            ValidateFileNameMultiLineColumnError("Main.cs(-3):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, 3, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            //      (line-line)
            ValidateFileNameMultiLineColumnError("Main.cs(-):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(-2):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, 2, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(1-):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", 1, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            //      (line,col)
            ValidateFileNameMultiLineColumnError("Main.cs(,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,2):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, 2, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(1,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", 1, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            // Similarly to the previous odd case, this really falls under (line,col-col). Included for completeness, even if results are 
            // not intuitive
            ValidateFileNameMultiLineColumnError("Main.cs(,-2):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, 2,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(-1,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            //      (line,col-col)
            ValidateFileNameMultiLineColumnError("Main.cs(,-):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(2,-):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", 2, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,4-):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, 4, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,-6):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, 6,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(-1,-):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            //      (line,col,line,col)
            ValidateFileNameMultiLineColumnError("Main.cs(,,,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(2,,,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", 2, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,3,,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, 3, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,,4,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, 4, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,,,5):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, 5,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            // negative numbers are not matched at all for this format and I don't think we should tweak regexes to accept invalid input
            // in that form
            ValidateFileNameMultiLineColumnError("Main.cs(-2,,1,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,-3,,2):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(3,,-4,):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");

            ValidateFileNameMultiLineColumnError("Main.cs(,4,,-5):Command line warning CS0168: The variable 'foo' is declared but never used",
                "Main.cs", CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified,
                CanonicalError.Parts.Category.Warning, "CS0168", "The variable 'foo' is declared but never used");
        }

        #region Support functions.        
        private static void AssertEqual(string str1, string str2)
        {
            if (str1 != str2)
            {
                str1 = null == str1 ? "{null}" : str1;
                str2 = null == str2 ? "{null}" : str2;

                string message = "Regression: string compare '" + str1 + "'!='" + str2 + "'";

                Assert.IsTrue(str1 == str2, message);
            }
        }

        private static void AssertEqual(int int1, int int2)
        {
            if (int1 != int2)
            {
                string message = "Regression: int compare '" + int1.ToString() + "'!='" + int2.ToString() + "'";

                Assert.IsTrue(int1 == int2, message);
            }
        }

        private static void AssertEqual(CanonicalError.Parts.Category cat1, CanonicalError.Parts.Category cat2)
        {
            if (cat1 != cat2)
            {
                string message = "Regression: category compare '" + cat1.ToString() + "'!='" + cat2.ToString() + "'";

                Assert.IsTrue(cat1 == cat2, message);
            }
        }

        private static void ValidateToolError(string message, string tool, CanonicalError.Parts.Category severity, string code, string text)
        {
            CanonicalError.Parts errorParts = CanonicalError.Parse(message);

            Assert.IsNotNull(errorParts, "The message '" + message + "' could not be interpretted.");
            AssertEqual(errorParts.origin, tool);
            AssertEqual(errorParts.category, severity);
            AssertEqual(errorParts.code, code);
            AssertEqual(errorParts.text, text);
            AssertEqual(errorParts.line, CanonicalError.Parts.numberNotSpecified);
            AssertEqual(errorParts.column, CanonicalError.Parts.numberNotSpecified);
            AssertEqual(errorParts.endLine, CanonicalError.Parts.numberNotSpecified);
            AssertEqual(errorParts.endColumn, CanonicalError.Parts.numberNotSpecified);
        }

        private static void ValidateFileNameMultiLineColumnError(string message, string filename, int line, int column, int endLine, int endColumn, CanonicalError.Parts.Category severity, string code, string text)
        {
            CanonicalError.Parts errorParts = CanonicalError.Parse(message);

            Assert.IsNotNull(errorParts, "The message '" + message + "' could not be interpretted.");
            AssertEqual(errorParts.origin, filename);
            AssertEqual(errorParts.category, severity);
            AssertEqual(errorParts.code, code);
            AssertEqual(errorParts.text, text);
            AssertEqual(errorParts.line, line);
            AssertEqual(errorParts.column, column);
            AssertEqual(errorParts.endLine, endLine);
            AssertEqual(errorParts.endColumn, endColumn);
        }

        private static void ValidateFileNameLineColumnError(string message, string filename, int line, int column, CanonicalError.Parts.Category severity, string code, string text)
        {
            ValidateFileNameMultiLineColumnError(message, filename, line, column, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, severity, code, text);
        }

        private static void ValidateFileNameLineError(string message, string filename, int line, CanonicalError.Parts.Category severity, string code, string text)
        {
            ValidateFileNameMultiLineColumnError(message, filename, line, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, severity, code, text);
        }

        private static void ValidateFileNameError(string message, string filename, CanonicalError.Parts.Category severity, string code, string text)
        {
            ValidateFileNameMultiLineColumnError(message, filename, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, CanonicalError.Parts.numberNotSpecified, severity, code, text);
        }

        private static void ValidateNormalMessage(string message)
        {
            CanonicalError.Parts errorParts = CanonicalError.Parse(message);

            Assert.IsNull(errorParts, "The message '" + message + "' is an error/warning message");
        }
    }
    #endregion
}





