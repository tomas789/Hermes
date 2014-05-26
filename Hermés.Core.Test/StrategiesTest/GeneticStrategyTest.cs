using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hermés.Core.Strategies;

namespace Hermés.Core.Test.StrategiesTest
{
    [TestClass]
    public class ChromozomeTest
    {
        [TestMethod]
        public void Chromozome_CheckTypesSingleNode()
        {
            Delegate node = (Func<int>) (() => 1);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());
            ch.Add(node);
            Assert.IsTrue(ch.CheckTypeConstraints());
            Assert.AreEqual(typeof(int), ch.GetReturnType());
        }

        [TestMethod]
        public void Chromozome_CheckTypesSingleNodeInconsistentInnerNode()
        {
            Delegate sub = (Func<int, double, int>)((int a, double b) => a - (int)b);
            Delegate lhs = (Func<int>)(() => 10);
            Delegate rhs = (Func<int>)(() => 5);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());
            ch.Add(sub);
            ch.Add(lhs);
            ch.Add(rhs);
            Assert.IsFalse(ch.CheckTypeConstraints());
        }

        [TestMethod]
        public void Chromozome_CheckTypesSingleNodeInconsistentTerminalNode()
        {
            Delegate sub = (Func<int, int, int>)((int a, int b) => a - b);
            Delegate lhs = (Func<double>)(() => 10);
            Delegate rhs = (Func<int>)(() => 5);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());
            ch.Add(sub);
            ch.Add(lhs);
            ch.Add(rhs);
            Assert.IsFalse(ch.CheckTypeConstraints());
        }

        [TestMethod]
        public void Chromozome_ChromozomeEvalSingleTerminalNode()
        {
            Delegate ccc = (Func<int>)(() => 1);
            var ch1 = new Chromozome<Delegate>(new DelegateGeneHelper());
            ch1.Add(ccc);

            var res = ch1.DynamicInvoke();
            Assert.AreEqual(typeof(int), res.GetType());
            Assert.AreEqual(1, (int)res);
        }

        [TestMethod]
        public void Chromozome_ChromozomeEvalMultipleNodes()
        {
            Delegate sub = (Func<int, int, int>)((int a, int b) => a - b);
            Delegate lhs = (Func<int>)(() => 6);
            Delegate rhs = (Func<int>)(() => 5);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());

            ch.Add(sub);
            ch.Add(lhs);
            ch.Add(rhs);

            Assert.IsTrue(ch.CheckTypeConstraints(), "Type Constraints.");

            var res = ch.DynamicInvoke();
            Assert.AreEqual(typeof(int), res.GetType());
            Assert.AreEqual(1, (int)res);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException),
         "Accessing out of bounds not throws exception.")]
        public void Chromozome_ChromozomeEvalIncompleteTree()
        {
            Delegate sub = (Func<int, int, int>)((int a, int b) => a - b);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());

            ch.Add(sub);

            ch.CheckTypeConstraints();
        }
    }

    [TestClass]
    public class FullTreeGeneratorTest
    {
        [TestMethod]
        public void FullTreeGenerator_TerminalTypeConstrained()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new[]
            {
                (Func<double>) (() => 0)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 0, 
                MaxDepth = 0,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_TerminalTypeConstrainedMultipleTerminals()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new[]
            {
                (Func<double>) (() => 0),
                (Func<double>) (() => 1),
                (Func<double>) (() => -1)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 0,
                MaxDepth = 0,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_TerminalTypeConstrainedMultipleTerminalsOfMultipleTypes()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new Delegate[]
            {
                (Func<double>) (() => 0),
                (Func<int>) (() => 1),
                (Func<double>) (() => -1),
                (Func<int>) (() => -1),
                (Func<double>) (() => -1),
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 0,
                MaxDepth = 0,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_FunctionTypeConstrained()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new Delegate[]
            {
                (Func<double>) (() => 0),
                (Func<double, double>) ((a) => a)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 1,
                MaxDepth = 1,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_FunctionTypeConstrainedMultipleTerminals()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new Delegate[]
            {
                (Func<double>) (() => 0),
                (Func<double>) (() => 1),
                (Func<double>) (() => 5),
                (Func<double>) (() => -1),
                (Func<double>) (() => -5),
                (Func<double, double>) ((a) => a)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 1,
                MaxDepth = 1,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_FunctionTypeConstrainedMultipleTerminalsMultipleTypes()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new Delegate[]
            {
                (Func<double>) (() => 0),
                (Func<int>) (() => 1),
                (Func<double>) (() => 5),
                (Func<int>) (() => -1),
                (Func<double>) (() => -5),
                (Func<double, double>) ((a) => a)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 1,
                MaxDepth = 1,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_FunctionTypeConstrainedMultipleTerminalsMultipleTypesMultipleFunctions()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new Delegate[]
            {
                (Func<double>) (() => 0),
                (Func<int>) (() => 1),
                (Func<double>) (() => 5),
                (Func<int>) (() => -1),
                (Func<double>) (() => -5),
                (Func<double, double>) ((a) => a),
                (Func<double, double, double>) ((a, b) => a + b),
                (Func<double, double, double>) ((a, b) => a - b),
                (Func<double, double, double>) ((a, b) => a * b)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 1,
                MaxDepth = 1,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_FunctionTypeConstrainedMultipleTerminalsMultipleTypesMultipleFunctionsMultipleTypes()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new Delegate[]
            {
                (Func<double>) (() => 0),
                (Func<int>) (() => 1),
                (Func<double>) (() => 5),
                (Func<int>) (() => -1),
                (Func<double>) (() => -5),
                (Func<double, double>) ((a) => a),
                (Func<double, int, double>) ((a, b) => a + b),
                (Func<int, double, double>) ((a, b) => a - b),
                (Func<double, double, double>) ((a, b) => a * b)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 1,
                MaxDepth = 1,
                ReturnType = typeof(double)
            };

            var chromozome = generator.Operator(null);
            Assert.AreEqual(1, chromozome.Count);
            Assert.IsTrue(chromozome[0].CheckTypeConstraints());
        }

        [TestMethod]
        public void FullTreeGenerator_UnreachableTypeToGenerate()
        {
            var geneHelper = new DelegateGeneHelper();
            var primitiveSet = new Delegate[]
            {
                (Func<double>) (() => 0),
                (Func<int>) (() => 1),
                (Func<double>) (() => 5),
                (Func<int>) (() => -1),
                (Func<double>) (() => -5),
                (Func<double, double>) ((a) => a),
                (Func<double, int, double>) ((a, b) => a + b),
                (Func<int, double, double>) ((a, b) => a - b),
                (Func<double, double, double>) ((a, b) => a * b)
            };

            var generator = new FullTreeGenerator<Delegate>(geneHelper, primitiveSet)
            {
                MinDepth = 1,
                MaxDepth = 1,
                ReturnType = typeof(string)
            };

            var chromozome = generator.Operator(null);
            Assert.IsNotNull(chromozome);
            Assert.AreEqual(0, chromozome.Count);
        }
    }
}
