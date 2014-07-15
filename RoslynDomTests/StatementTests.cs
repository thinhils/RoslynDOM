﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynDom;
using RoslynDom.Common;

namespace RoslynDomTests
{
    [TestClass]
    public class StatementTests
    {
        private const string CodeLoadCategory = "CodeLoad";

        [TestMethod, TestCategory(CodeLoadCategory)]
        public void Can_load_misc_statements_for_method()
        {
            var csharpCode = @"
            public class Bar
            {
                public void Foo()
                {
                  if (true) {}
                  var x = "", "";
                  x = lastName + x + firstName;
                  Foo2();
                  return true;
                }
            }           
            ";
            var root = RDomFactory.GetRootFromString(csharpCode);
            var output = RDomFactory.BuildSyntax(root);
            var method = root.RootClasses.First().Methods.First();
            var statements = method.Statements.ToArray();
            Assert.AreEqual(5, statements.Count());
            Assert.IsInstanceOfType(statements[0], typeof(RDomIfStatement));
            Assert.IsInstanceOfType(statements[1], typeof(RDomDeclarationStatement));
            Assert.IsInstanceOfType(statements[2], typeof(RDomAssignmentStatement));
            Assert.IsInstanceOfType(statements[3], typeof(RDomInvocationStatement));
            Assert.IsInstanceOfType(statements[4], typeof(RDomReturnStatement));
            var expectedString = "public class Bar\r\n{\r\n    public Void Foo()\r\n    {\r\n        if (true)\r\n        {\r\n        }\r\n\r\n        var x = \", \";\r\n        x = lastName + x + firstName;\r\n        Foo2();\r\n        return true;\r\n    }\r\n}";
            Assert.AreEqual(expectedString, output.ToString());
        }

        [TestMethod, TestCategory(CodeLoadCategory)]
        public void Can_load_declaration_statements_for_method()
        {
            var csharpCode = @"
            public class Bar
            {
                public void Foo()
                {
                  var w = "", "";
                  string x = "", "";
                  var y = new Bar(4, ""Fred"");
                  XYZ xyz = new XYZ();
                  Bar z = Bar(w, x);
                }
            }           
            ";
            var root = RDomFactory.GetRootFromString(csharpCode);
            var output = RDomFactory.BuildSyntax(root);
            var method = root.RootClasses.First().Methods.First();
            var statements = method.Statements.ToArray();
            Assert.AreEqual(5, statements.Count());
            Assert.IsInstanceOfType(statements[0], typeof(RDomDeclarationStatement));
            Assert.IsInstanceOfType(statements[1], typeof(RDomDeclarationStatement));
            Assert.IsInstanceOfType(statements[2], typeof(RDomDeclarationStatement));
            Assert.IsInstanceOfType(statements[3], typeof(RDomDeclarationStatement));
            Assert.IsInstanceOfType(statements[4], typeof(RDomDeclarationStatement));
            // TODO: Solve simplification problem.
            var actual = RoslynUtilities.Simplify(output);
            var expectedString = "public class Bar\r\n{\r\n    public Void Foo()\r\n    {\r\n        var w = \", \";\r\n        String x = \", \";\r\n        var y = new Bar(4, \"Fred\");\r\n        XYZ xyz = new XYZ();\r\n        Bar z = Bar(w, x);\r\n    }\r\n}";
            Assert.AreEqual(expectedString, actual);
        }


        [TestMethod, TestCategory(CodeLoadCategory)]
        public void Can_load_if_statements_for_method()
        {
            var csharpCode = @"
            public class Bar
            {
                public void Foo()
                {
                    if (z = 1)
                    {
                        var x = 42;
                    }
                    else if (z=2)
                    { var x = 43;  y = x + x; }
                    else
                    { Console.WriteLine(); }
                    if (z = 1) Console.WriteLine();
                    if (z = 2) Console.Write();
                }
            }           
            ";

            var root = RDomFactory.GetRootFromString(csharpCode);
            var output = RDomFactory.BuildSyntax(root);
            var method = root.RootClasses.First().Methods.First();
            var statements = method.Statements.ToArray();
            Assert.AreEqual(3, statements.Count());
            Assert.IsInstanceOfType(statements[0], typeof(RDomIfStatement));
            Assert.IsInstanceOfType(statements[1], typeof(RDomIfStatement));
            Assert.IsInstanceOfType(statements[2], typeof(RDomIfStatement));
            var ifStatement = statements[0] as IIfStatement;
            Assert.AreEqual(1, ifStatement.ElseIfs.Count());
            Assert.IsInstanceOfType(ifStatement.Statements.First(), typeof(RDomDeclarationStatement));
            Assert.IsInstanceOfType(ifStatement.ElseIfs.First().Statements.Last(), typeof(RDomAssignmentStatement));
            Assert.IsInstanceOfType((statements[0] as IIfStatement).ElseStatements.First(), typeof(RDomInvocationStatement));

            // TODO: Solve simplification problem.
            var actual = RoslynUtilities.Simplify(output);
            var expectedString = "public class Bar\r\n{\r\n    public Void Foo()\r\n    {\r\n        if (z = 1)\r\n        {\r\n            var x = 42;\r\n        }\r\n        else if (z = 2)\r\n        {\r\n            var x = 43;\r\n            y = x + x;\r\n        }\r\n        else\r\n        {\r\n            Console.WriteLine();\r\n        }\r\n\r\n        if (z = 1)\r\n            Console.WriteLine();\r\n        if (z = 2)\r\n            Console.Write();\r\n    }\r\n}";
            Assert.AreEqual(expectedString, actual);
        }


        [TestMethod, TestCategory(CodeLoadCategory)]
        public void Can_load_block_statements_for_method()
        {
            var csharpCode = @"
            public class Bar
            {
                public void Foo()
                {
                    {
                    var z;
                    var z;
                    {
                    z = 43;
                    z = x + y;
                    z = x + y;
                    z = x + y;
                    }
                    }
                    z = Console.WriteLine();
                    {}
                }
            }           
            ";
            var root = RDomFactory.GetRootFromString(csharpCode);
            var output = RDomFactory.BuildSyntax(root);
            var method = root.RootClasses.First().Methods.First();
            var statements = method.Statements.ToArray();
            Assert.AreEqual(3, statements.Count());
            Assert.IsInstanceOfType(statements[0], typeof(RDomBlockStatement));
            Assert.IsInstanceOfType(statements[1], typeof(RDomAssignmentStatement));
            Assert.IsInstanceOfType(statements[2], typeof(RDomBlockStatement));
            Assert.AreEqual(3, ((IBlockStatement)statements[0]).Statements.Count());
            Assert.AreEqual(4, ((IBlockStatement)((IBlockStatement)statements[0]).Statements.Last()).Statements.Count());
            // TODO: Solve simplification problem.
            var actual = RoslynUtilities.Simplify(output);
            var expectedString = "public class Bar\r\n{\r\n    public Void Foo()\r\n    {\r\n        {\r\n            var z;\r\n            var z;\r\n            {\r\n                z = 43;\r\n                z = x + y;\r\n                z = x + y;\r\n                z = x + y;\r\n            }\r\n        }\r\n\r\n        z = Console.WriteLine();\r\n        {\r\n        }\r\n    }\r\n}";
            Assert.AreEqual(expectedString, actual);
        }


        [TestMethod, TestCategory(CodeLoadCategory)]
        public void Can_load_invocation_statements_for_method()
        {
            var csharpCode = @"
            public class Bar
            {
                public void Foo()
                {
                    Console.WriteLine();
                    Math.Pow(4, 2);
                }
            }           
            ";
            var root = RDomFactory.GetRootFromString(csharpCode);
            var output = RDomFactory.BuildSyntax(root);
            var method = root.RootClasses.First().Methods.First();
            var statements = method.Statements.ToArray();
            Assert.AreEqual(2, statements.Count());
            Assert.IsInstanceOfType(statements[0], typeof(RDomInvocationStatement));
            Assert.IsInstanceOfType(statements[1], typeof(RDomInvocationStatement));
            // TODO: Solve simplification problem.
            var actual = RoslynUtilities.Simplify(output);
            var expectedString = "public class Bar\r\n{\r\n    public Void Foo()\r\n    {\r\n        Console.WriteLine();\r\n        Math.Pow(4, 2);\r\n    }\r\n}";
            Assert.AreEqual(expectedString, actual);
        }


        [TestMethod, TestCategory(CodeLoadCategory)]
        public void Can_load_return_statements_for_method()
        {
            var csharpCode = @"
            public class Bar
            {
                public void Foo()
                {
                  return;
                }
                public int Foo()
                {
                  return 42;
                }
            }           
            ";
            var root = RDomFactory.GetRootFromString(csharpCode);
            var output = RDomFactory.BuildSyntax(root);
            var method = root.RootClasses.First().Methods.First();
            var statements = method.Statements.ToArray();
            Assert.AreEqual(1, statements.Count());
            Assert.IsInstanceOfType(statements[0], typeof(RDomReturnStatement));
            // TODO: Solve simplification problem.
            var actual = RoslynUtilities.Simplify(output);
            var expectedString = "public class Bar\r\n{\r\n    public Void Foo()\r\n    {\r\n        return;\r\n    }\r\n\r\n    public Int32 Foo()\r\n    {\r\n        return 42;\r\n    }\r\n}";
            Assert.AreEqual(expectedString, actual);
        }


    }
}