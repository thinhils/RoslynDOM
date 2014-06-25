﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynDom;
using RoslynDom.Common;

// Common scenarios for loading the DOM are in RoslynDomLoad of this RoslynDomExampleTests project
namespace RoslynDomExampleTests
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// There are a set of scenarios that require either determining if a file matches a pattern
    /// (as in the code first work) or a subpart matches.
    /// <br/>
    /// In exploring this, I felt the concept of root classes was important. Just give me the root classes
    /// whether or not there are surrounding namespaces. 
    /// <br/>
    /// This is one of the scenarios where I think annotations/design time attributes will be important, and
    /// almost certainly public ones. 
    /// </remarks>
    [TestClass]
    public class Scenario_PatternMatchingSelection
    {
#region Syntax
        [TestMethod]
        public void Can_select_root_classes_by_attribute ()
        {
            var csharpCode = @"
using System
[A(), B]
public class Foo  {}
namespace Namespace1
{
   [C]
   [D, E]
   public class Foo1
   {
      [F]
      public class Foo3{}
   }
   public class Foo2{}
}
";

            var root = RDomFactory.GetRootFromString(csharpCode);
            var rootAttributes = GetRootAttributeNames(root).ToArray();
            Assert.AreEqual(5, rootAttributes.Count());
            Assert.AreEqual("A", rootAttributes[0]);
            Assert.AreEqual("B", rootAttributes[1]);
            Assert.AreEqual("C", rootAttributes[2]);
            Assert.AreEqual("D", rootAttributes[3]);
            Assert.AreEqual("E", rootAttributes[4]);
            Assert.IsTrue(HasAttribute(root, "A"));
            Assert.IsTrue(HasAttribute(root, "B"));
            Assert.IsTrue(HasAttribute(root, "C"));
            Assert.IsTrue(HasAttribute(root, "D"));
            Assert.IsTrue(HasAttribute(root, "E"));
            Assert.IsFalse(HasAttribute(root, "F"));

        }


        private IEnumerable<string> GetRootAttributeNames(IRoot root)
        {
                // This method may go away in favor of public annotations as soon as I work that out
                // I don't see this being used except as design time annotations
                var classAttributeNames = from x in root.RootClasses
                                          from a in x.Attributes
                                          select a.Name;
                return classAttributeNames;
        }

        private bool HasAttribute(IRoot root, string name)
        {
            var attributeNames = GetRootAttributeNames(root);
            var matches = attributeNames.Where(x => x == name);
            return (matches.Count() > 0);
        }
#endregion 
    }
}
