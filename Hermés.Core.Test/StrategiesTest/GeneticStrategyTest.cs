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
    public class GeneticStrategyTest
    {
        [TestMethod]
        public void GeneticStrategy_CheckTypesSingleNode()
        {
            Delegate node = (Func<int>) (() => 1);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());
            ch.Add(node);
            Assert.IsTrue(ch.CheckTypeConstraints());
        }

        [TestMethod]
        public void GeneticStrategy_CheckTypesSingleNodeInconsistentReturnTypes()
        {
            Delegate node = (Func<int>)(() => 1);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());
            ch.Add(node);
            Assert.IsFalse(ch.CheckTypeConstraints());
        }

        [TestMethod]
        public void GeneticStrategy_CheckTypesSingleNodeInconsistentInnerNode()
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
        public void GeneticStrategy_CheckTypesSingleNodeInconsistentTerminalNode()
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
        public void GeneticStrategy_ChromozomeEvalSingleTerminalNode()
        {
            Delegate ccc = (Func<int>)(() => 1);
            var ch1 = new Chromozome<Delegate>(new DelegateGeneHelper());
            ch1.Add(ccc);

            var res = ch1.DynamicInvoke();
            Assert.AreEqual(typeof(int), res.GetType());
            Assert.AreEqual(1, (int)res);
        }

        [TestMethod]
        public void GeneticStrategy_ChromozomeEvalMultipleNodes()
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
        public void GeneticStrategy_ChromozomeEvalIncompleteTree()
        {
            Delegate sub = (Func<int, int, int>)((int a, int b) => a - b);
            var ch = new Chromozome<Delegate>(new DelegateGeneHelper());

            ch.Add(sub);

            ch.CheckTypeConstraints();
        }
    }
}
