using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core.Strategies
{
    /// <summary>
    /// Real-time updating genetic programming (RTU-GP) strategy.
    /// </summary>
    public class GeneticStrategy : IStrategy
    {
        private GeneticStrategyConfiguration _config = 
            new GeneticStrategyConfiguration();

        private Random _random = new Random();

        public GeneticStrategyConfiguration Configuration
        {
            get { return _config; }
            set
            {
                if (_initialized)
                    throw new InvalidOperationException("Changing state after initialization.");

                _config = value;
            }
        }

        private readonly IList<Delegate> _primitives = 
            new List<Delegate>();

        private readonly Dictionary<GeneticOperator<Delegate>, int> _weightedOperators = 
            new Dictionary<GeneticOperator<Delegate>, int>(); 

        private PriceGroup _evaluationContext = null;

        private List<Chromozome<Delegate>> _population = 
            new List<Chromozome<Delegate>>();

        private bool _initialized = false;

        private Chromozome<Delegate> _selectedChromozome = null;

        private Kernel _kernel;

        public void Initialize(Kernel kernel)
        {
            if (_initialized)
                throw new DoubleInitializationException();

            _kernel = kernel;
            
            if (_config == null)
                throw new InvalidOperationException(
                    "Initializing genetic strategy without configuration.");

            if (_config.PopulationSize <= 0)
                throw new InvalidOperationException(
                    "Population size have to be positive.");

            if (_config.EvaluationLength > _config.MinimumRequiredLength)
                throw new InvalidOperationException(
                    "Evaluation length is longer then minimum required length.");

            _primitives.Add((Func<SignalKind>) (() => SignalKind.Buy));
            _primitives.Add((Func<SignalKind>) (() => SignalKind.Sell));
            _primitives.Add((Func<SignalKind>) (() => SignalKind.Hold));

            _primitives.Add((Func<double?>)(() => null));
            _primitives.Add((Func<double?>)(() => 0));
            _primitives.Add((Func<double?>)(() => 1));

            _primitives.Add((Func<PriceGroup>)(() => _evaluationContext));

            _primitives.Add((Func<int>)(() => 0));
            _primitives.Add((Func<int>)(() => 1));
            _primitives.Add((Func<int>)(() => -1));

            _primitives.Add((Func<int, int, int>)((a, b) => a + b));
            _primitives.Add((Func<int, int, int>)((a, b) => a - b));
            _primitives.Add((Func<int, int, int>)((a, b) => a * b));
            _primitives.Add((Func<int, int, int>) Math.Max);
            _primitives.Add((Func<int, int, int>) Math.Min);

            _primitives.Add((Func<int, int, bool>)((a, b) => a < b));
            _primitives.Add((Func<int, int, bool>)((a, b) => a <= b));
            _primitives.Add((Func<int, int, bool>)((a, b) => a > b));
            _primitives.Add((Func<int, int, bool>)((a, b) => a >= b));
            _primitives.Add((Func<int, int, bool>)((a, b) => a == b));

            _primitives.Add((Func<int, int>)Math.Abs);

            _primitives.Add((Func<double, int>)((a) => (int)Math.Round(a)));

            _primitives.Add((Func<PriceGroup, double>)((pg) => pg.Close));

            _primitives.Add((Func<PriceGroup, double?>)((pg) => pg.Open));
            _primitives.Add((Func<PriceGroup, double?>)((pg) => pg.High));
            _primitives.Add((Func<PriceGroup, double?>)((pg) => pg.Low));
            _primitives.Add((Func<PriceGroup, double?>)((pg) => pg.OpenInterenst));
            _primitives.Add((Func<PriceGroup, double?>)((pg) => pg.Volume));

            _primitives.Add((Func<double, double?>)((a) => a));

            _primitives.Add((Func<double?, double>)((a) => a.GetValueOrDefault()));

            _primitives.Add((Func<double?, double, double>)((a, b) => a.HasValue ? a.Value : b));

            _primitives.Add((Func<double>) (() => 0));
            _primitives.Add((Func<double>) (() => 1));
            _primitives.Add((Func<double>) (() => -1));

            _primitives.Add((Func<double, double, double>) ((a, b) => a + b));
            _primitives.Add((Func<double, double, double>) ((a, b) => a - b));
            _primitives.Add((Func<double, double, double>) ((a, b) => a * b));
            _primitives.Add((Func<double, double, double>) Math.Max);
            _primitives.Add((Func<double, double, double>) Math.Min);

            _primitives.Add((Func<double, double>)(Math.Abs));
            _primitives.Add((Func<double, double>)(Math.Sin));
            _primitives.Add((Func<double, double>)(Math.Cos));
            _primitives.Add((Func<double, double>)(Math.Tan));
            _primitives.Add((Func<double, double>)(Math.Log));

            _primitives.Add((Func<double, bool>) (Double.IsNaN));
            _primitives.Add((Func<double, bool>) (Double.IsInfinity));

            _primitives.Add((Func<double, double, bool>)((a, b) => a < b));
            _primitives.Add((Func<double, double, bool>)((a, b) => a <= b));
            _primitives.Add((Func<double, double, bool>)((a, b) => a > b));
            _primitives.Add((Func<double, double, bool>)((a, b) => a >= b));
            _primitives.Add((Func<double, double, bool>)((a, b) => a - b < 1e-6));

            _primitives.Add((Func<bool, SignalKind, SignalKind, SignalKind>)((cond, a, b) => cond ? a : b));
            _primitives.Add((Func<bool, double?, double?, double?>)((cond, a, b) => cond ? a : b));
            _primitives.Add((Func<bool, PriceGroup, PriceGroup, PriceGroup>)((cond, a, b) => cond ? a : b));

            var geneHelper = new DelegateGeneHelper();
            AddGeneticOperator(new FullTreeGenerator<Delegate>(geneHelper, _primitives));
            AddGeneticOperator(new OnePointCrossover<Delegate>(geneHelper));
        }

        public void AddGeneticOperator(GeneticOperator<Delegate> op, int weight = 1)
        {
            if (weight <= 0)
                throw new ArgumentException("Genetic operator weight have to be positive number.");

            _weightedOperators.Add(op, weight);
        }

        private void InitializePopulation()
        {
            var initializers = (from op in _weightedOperators 
                                where op.Key.Arity == 0 
                                select new { op = op.Key, weight = op.Value}).ToArray();
            var weightSum = (from ini in initializers select ini.weight).Sum();

            GeneticOperator<Delegate> initializer = null;
            var p = _random.Next(weightSum + 1);
            foreach (var i in initializers)
            {
                if (p < i.weight)
                {
                    initializer = i.op;
                    break;
                }

                p -= i.weight;
            }

            if (initializer == null)
                throw new ImpossibleException();

            while (_population.Count < _config.PopulationSize)
                _population.AddRange(initializer.Operator(new Chromozome<Delegate>[] { }));
        }

        private void NextGeneration()
        {
            var next = new List<Chromozome<Delegate>>();
            while (next.Count < _config.PopulationSize)
            {
                GeneticOperator<Delegate> op = null;
                var weightSum = (from weight in _weightedOperators.Values select weight).Sum();
                var p = _random.Next(weightSum);
                foreach (var i in _weightedOperators)
                {
                    if (p < i.Value)
                    {
                        op = i.Key;
                        break;
                    }

                    p -= i.Value;
                }

                if (op == null)
                    throw new ImpossibleException();

                var chromozomes = new List<Chromozome<Delegate>>();
                while  (chromozomes.Count < op.Arity)
                    chromozomes.Add(_population[_random.Next(_population.Count)]);

                if (chromozomes.Count != op.Arity)
                    throw new ImpossibleException();
                
                next.AddRange(op.Operator(chromozomes));
            }
        }

        private void EvaluatePopulation(MarketEvent ev)
        {
            foreach (var chromozome in _population)
            {
                var hold = 0;
                var miss = 0;

                var sizeInHold = 0.0;
                var value = 0.0;

                var priceGroup = ev.Market.GetHistoricalPriceGroup(_config.EvaluationLength);;

                for (var i = _config.EvaluationLength - 1; i >= 1; ++i)
                {
                    var lastPriceGroup = priceGroup;
                    priceGroup = ev.Market.GetHistoricalPriceGroup(i);

                    value += (priceGroup.Close - lastPriceGroup.Close)*sizeInHold*ev.Market.PointPrice;

                    _evaluationContext = priceGroup;
                    var signal = (SignalKind)chromozome.DynamicInvoke();
                    switch (signal)
                    {
                        case SignalKind.Buy:
                            sizeInHold += 1;
                            break;
                        case SignalKind.Sell:
                            sizeInHold -= 1;
                            break;
                    }
                }

                chromozome.Fitness = new Fitness(new [] { 1.0 });
                chromozome.Fitness.Values[0] = value;
                chromozome.Fitness.Valid = true;
            }
        }

        public void DispatchEvent(Event e)
        {
            var ts = new TypeSwitch()
                .Case((MarketEvent x) => DispatchConcrete(x));

            ts.Switch(e);
        }

        private void DispatchConcrete(MarketEvent ev)
        {
            if (ev.Market.Count < _config.MinimumRequiredLength)
                return;

            if (ev.Market.Count == _config.MinimumRequiredLength)
            {
                InitializePopulation();
                EvaluatePopulation(ev);
            }

            if ((ev.Market.Count - _config.MinimumRequiredLength) % _config.UpdateInterval == 0)
            {
                NextGeneration();
                EvaluatePopulation(ev);
            }

            EvaluatePopulation(ev);

            Chromozome<Delegate> best = null;
            foreach (var chromozome in _population.Where(chromozome => chromozome.Fitness.Valid))
            {
                if (best == null)
                {
                    best = chromozome;
                    continue;
                }

                if (chromozome.Fitness.GetWeightedSum() > best.Fitness.GetWeightedSum())
                    best = chromozome;
            }

            if (best == null)
                return;

            var evaluated = (SignalKind)best.DynamicInvoke();
            if (evaluated == SignalKind.Hold)
                return;
            var signal = new SignalEvent(_kernel.WallTime, ev.Market, evaluated);
            _kernel.AddEvent(signal);
        }

        public void Dispose()
        {
        }
    }

    public class GeneticStrategyConfiguration
    {
        public int UpdateInterval = 50;

        public int FirstUpdateGenerations = 50;
        public int NextUpdateGenerations = 20;

        public int MinimumRequiredLength = 300;

        public int PopulationSize = 100;

        public int EvaluationLength = 300;
    }

    public class Fitness : IComparable<Fitness>, IEquatable<Fitness>, ICloneable
    {
        public bool Valid = false;

        public readonly double[] Weights;
        public readonly ValuesHelper Values;

        public double Epsilon = 1e-6;

        public int Length { get { return Weights.Length; } }

        public Fitness(double[] weights)
        {
            Weights = weights;
            Values = new ValuesHelper(this);
        }

        public bool Equals(Fitness other)
        {
            if (Length != other.Length)
                throw new ArgumentException(
                    "Equality-testing Fitnesses of different length.");

            for (var i = 0; i < Length; ++i)
                if (Values[i] * Weights[i] - other.Values[i] * other.Weights[i] >= Epsilon)
                    return false;

            return true;
        }

        public int CompareTo(Fitness other)
        {
            if (Length != other.Length)
                throw new ArgumentException(
                    "Comparing Fitnesses of different length.");

            for (var i = 0; i < Length; ++i)
            {
                if (Values[i] * Weights[i] - other.Values[i] * other.Weights[i] < Epsilon)
                    continue;

                return (Values[i] * Weights[i]).CompareTo(other.Values[i] * other.Weights[i]);
            }

            return 0;
        }

        public bool Dominates(Fitness other)
        {
            if (Length != other.Length)
                throw new ArgumentException(
                    "Comparing Fitnesses of different length.");

            for (var i = 0; i < Length; ++i)
                if (Values[i] * Weights[i] - other.Values[i] * other.Weights[i] <= Epsilon)
                    return false;

            return true;
        }

        public object Clone()
        {
            return new Fitness(this.Weights);
        }

        public double GetWeightedSum()
        {
            return Weights.Zip(Values, (a, b) => a*b).Sum();
        }
    }
    public class ValuesHelper : IEnumerable<double>
    {
        private readonly Fitness _fitness;
        private readonly double[] _values;

        public ValuesHelper(Fitness fitness)
        {
            _fitness = fitness;
            _values = new double[fitness.Weights.Length];
        }

        public double this[int i]
        {
            get { return _values[i]; }
            set
            {
                _fitness.Valid = false;
                _values[i] = value;
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            return ((IEnumerable<double>) _values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }

    /// <summary>
    /// Abstract base class for every genetic operator used in simulation.
    /// </summary>
    /// <typeparam name="T">Type of gene.</typeparam>
    public abstract class GeneticOperator<T>
    {
        private Random _random = new Random();
        
        public Random Random
        {
            get { return _random; } 
            set { _random = value; }
        }

        public int Arity
        {
            get { return GetArity(); }
        }

        public IGeneHelper<T> GeneHelper;

        /// <summary>
        /// Get arity of this operator.
        /// </summary>
        /// <remarks>
        /// Arity is non-negative number.
        /// </remarks>
        /// <returns>Arity of this operator.</returns>
        protected abstract int GetArity();

        public IList<Chromozome<T>> Operator(IList<Chromozome<T>> chromozomes)
        {
            if (GeneHelper == null)
                throw new InvalidOperationException(
                    "GeneHelper is not set before using GeneticOperator.");

            if (Arity == 0)
            {
                if (chromozomes != null && chromozomes.Count != 0)
                    throw new ArgumentException(string.Format(
                            "OnePoint of arity 0, got {1}", chromozomes.Count));
            }
            else
            {
                if (chromozomes == null)
                    throw new ArgumentException(string.Format(
                            "OnePoint of arity {0}, got {{null}}", Arity));
                if (chromozomes.Count != Arity)
                    throw new ArgumentException(string.Format(
                            "OnePoint of arity {0}, got {1}", Arity, chromozomes.Count));
            }

            return OperatorImpl(chromozomes);
        }


        /// <summary>
        /// Implementation of genetic operator supplied by derived class.
        /// </summary>
        /// <param name="chromozomes">Chromozomes to perform operation at.</param>
        /// <returns>New chromozomes.</returns>
        protected abstract IList<Chromozome<T>> OperatorImpl(IList<Chromozome<T>> chromozomes);
    }

    public interface IGeneHelper<in T>
    {
        /// <summary>
        /// Get arity of <paramref name="gene"/>
        /// </summary>
        /// <param name="gene">Gene to be inspected.</param>
        /// <returns>Arity of <paramref name="gene"/></returns>
        int Arity(T gene);

        /// <summary>
        /// Get return type of <paramref name="gene"/>
        /// </summary>
        /// <param name="gene">Gene to be inspected.</param>
        /// <returns>Return type of <paramref name="gene"/></returns>
        Type ReturnType(T gene);
        
        /// <summary>
        /// Get type if <paramref name="argument" />th argument of <paramref name="gene" />. 
        /// Zero-based indexing.
        /// </summary>
        /// <remarks>
        /// <see cref="System.IndexOutOfRangeException"/> is thrown when 
        /// <paramref name="argument" /> is greater
        /// then arity of given <paramref name="gene" />.
        /// </remarks>
        /// <param name="gene">Gene to be inspected.</param>
        /// <param name="argument">Zero-based position of argument.</param>
        /// <returns>Type if <param name="argument" />th argument of <param name="gene" />.</returns>
        Type ArgumentType(T gene, int argument);

        /// <summary>
        /// Invoke gene with parameters.
        /// </summary>
        /// <param name="gene">Gene to invoke.</param>
        /// <param name="parameters">Parameters to invoke with.</param>
        /// <returns>Return value of gene.</returns>
        object DynamicInvoke(T gene, object[] parameters);
    }

    /// <summary>
    /// Genetic operator that generates full tree from given primitive set.
    /// </summary>
    /// <typeparam name="T">Type of gene.</typeparam>
    public class FullTreeGenerator<T> : GeneticOperator<T>
    {
        private readonly Random _random = new Random();

        private readonly IEnumerable<T> _functionSet;
        private readonly IEnumerable<T> _terminalSet;

        private readonly IEnumerable<Type> _allowedTypes; 

        private readonly IGeneHelper<T> _geneHelper; 

        public FullTreeGenerator(IGeneHelper<T> geneHelper, IEnumerable<T> primitiveSet)
        {
            _geneHelper = geneHelper;

            var terminals = new List<T>();
            var functions = new List<T>();
            foreach (var primitive in primitiveSet)
                (geneHelper.Arity(primitive) == 0 ? terminals : functions)
                    .Add(primitive);

            _functionSet = functions;
            _terminalSet = terminals;

            // TODO: Generate _allowedTypes;
        }

        private int _minDepth = 0;
        private int _maxDepth = 5;

        /// <summary>
        /// Mimimum depth of generated tree.
        /// </summary>
        public int MinDepth
        {
            get { return _minDepth; }
            set
            {
                if (value < 0)
                    throw new ArgumentException(
                        "MinDepth must be non-negative number.");

                if (value > _maxDepth)
                    throw new ArgumentException(
                        "MinDepth must be less then or equal to MaxDepth.");

                _minDepth = value;
            }
        }

        /// <summary>
        /// Maximum depth of generated tree.
        /// </summary>
        public int MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                if (value < _minDepth)
                    throw new ArgumentException(
                        "MaxDepth must be greater or equal to MinDepth.");

                _maxDepth = value;
            }
        }

        private Type _returnType;

        /// <summary>
        /// Return type of generated tree.
        /// </summary>
        public Type ReturnType
        {
            get { return _returnType; }
            set
            {
                if (_terminalSet.All(terminal => _geneHelper.ReturnType(terminal) != value))
                    throw new ArgumentException(
                        "ReturnType is not reachable by current terminal set.");

                _returnType = value;
            }
        }

        protected override int GetArity()
        {
            return 0;
        }

        protected override IList<Chromozome<T>> OperatorImpl(IList<Chromozome<T>> chromozomes)
        {
            var depth = _random.Next(_minDepth, _maxDepth);
            var chromozome = new Chromozome<T>(GeneHelper);
            GenerateSubtree(chromozome, ReturnType, depth);
            return new [] {chromozome};
        }

        private void GenerateSubtree(Chromozome<T> chromozome, Type type, int depth)
        {
            if (depth == 0)
            {
                var allowedTerminals = (from terminal in _terminalSet
                    where GeneHelper.ReturnType(terminal) == type
                    select terminal).ToArray();
                chromozome.Add(allowedTerminals[_random.Next(allowedTerminals.Length)]);
            }
            else
            {
                var allowedFunctions = (from function in _functionSet
                    where GeneHelper.ReturnType(function) == type
                    select function).ToArray();
                var selected = allowedFunctions[_random.Next(allowedFunctions.Length)];
                chromozome.Add(selected);
                var arity = GeneHelper.Arity(selected);
                for (var i = 0; i < arity; ++i)
                    GenerateSubtree(
                        chromozome, 
                        GeneHelper.ArgumentType(selected, i), 
                        depth - 1);
            }
        } 
    }

    /// <summary>
    /// Well-known one-point-subtree crossover.
    /// </summary>
    /// <typeparam name="T">Type of gene.</typeparam>
    public class OnePointCrossover<T> : GeneticOperator<T>
    {
        public OnePointCrossover(IGeneHelper<T> geneHelper)
        {
            GeneHelper = geneHelper;
        } 

        protected override IList<Chromozome<T>> OperatorImpl(IList<Chromozome<T>> chromozomes)
        {
            var types = new List<Dictionary<Type, List<int>>>();
            foreach (var chromozome in chromozomes)
            {
                var chromozomeTypes = new Dictionary<Type, List<int>>();
                for (var i = 0; i < chromozome.Count; ++i)
                {
                    List<int> nodeList;
                    var returnType = chromozome.GetSubtreeReturnType(i);
                    if (!chromozomeTypes.TryGetValue(returnType, out nodeList))
                        chromozomeTypes.Add(returnType, new List<int>() { i });
                }
            }

            var commonTypes = new HashSet<Type>();
            foreach (var chromozomeTypes in types)
                commonTypes.IntersectWith(chromozomeTypes.Keys);

            var lhsCommonItems = (from commonType in types[0] 
                               where commonTypes.Contains(commonType.Key) 
                               from cxPt in commonType.Value 
                               select new KeyValuePair<Type, int>(commonType.Key, cxPt)).ToArray();
            var lhsItemSelected = lhsCommonItems[Random.Next(lhsCommonItems.Length)];
            var cxType = lhsItemSelected.Key;
            var lhsCxPt = lhsItemSelected.Value;
            var lhsSubtreeEnd = chromozomes[0].GetSubtreeEndPosition(lhsCxPt);

            var rhsCommonPts = types[1][cxType];
            var rhsCxPt = rhsCommonPts[Random.Next(rhsCommonPts.Count)];
            var rhsSubtreeEnd = chromozomes[1].GetSubtreeEndPosition(rhsCxPt);

            var newLhs = new Chromozome<T>(GeneHelper);
            var lhsPos = 0;
            for (; lhsPos < lhsCxPt; ++lhsPos)
                newLhs.Add(chromozomes[0][lhsPos]);
            for (; lhsPos < rhsSubtreeEnd; ++lhsPos)
                newLhs.Add(chromozomes[1][lhsPos]);
            for (; lhsPos < chromozomes[0].Count; ++lhsPos)
                newLhs.Add(chromozomes[0][lhsPos]);

            var newRhs = new Chromozome<T>(GeneHelper);
            var rhsPos = 0;
            for (; rhsPos < rhsCxPt; ++rhsPos)
                newRhs.Add(chromozomes[1][rhsPos]);
            for (; rhsPos < lhsSubtreeEnd; ++rhsPos)
                newRhs.Add(chromozomes[0][rhsPos]);
            for (; rhsPos < chromozomes[1].Count; ++rhsPos)
                newRhs.Add(chromozomes[1][rhsPos]);

            return new[] {newLhs, newRhs};
        }

        protected override int GetArity()
        {
            return 2;
        }
    }

    public class DelegateGeneHelper : IGeneHelper<Delegate>
    {
        public int Arity(Delegate gene)
        {
            return gene.GetMethodInfo().GetParameters().Length;
        }

        public Type ReturnType(Delegate gene)
        {
            return gene.GetMethodInfo().ReturnType;
        }

        public Type ArgumentType(Delegate gene, int argument)
        {
            return gene.GetMethodInfo().GetParameters()[argument].ParameterType;
        }

        public object DynamicInvoke(Delegate gene, params object[] parameters)
        {
            return gene.DynamicInvoke(parameters);
        }
    }

    /// <summary>
    /// Implementation of typed genetic programming chromozome.
    /// </summary>
    /// <typeparam name="T">Type of gene.</typeparam>
    public class Chromozome<T>
    {
        bool? _typeValid = null;

        private readonly List<T> _items = new List<T>();

        public IGeneHelper<T> GeneHelper { get; private set; }

        public Fitness Fitness;

        public Chromozome(IGeneHelper<T> geneHelper)
        {
            GeneHelper = geneHelper;
        } 

        public T this[int i] 
        {
            get { return _items[i]; }
            set { 
                _typeValid = null; 
                _items[i] = value; 
            }
        }

        public void Add(T del)
        {
            _typeValid = null;
            _items.Add(del);
        }

        public int Count { get { return _items.Count; } }

        public Type GetReturnType()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Trying to get return type of empty chromozome.");

            return GeneHelper.ReturnType(_items[0]);
        }

        public Type GetSubtreeReturnType(int pos)
        {
            return GeneHelper.ReturnType(this[pos]);
        }

        public bool CheckTypeConstraints()
        {
            if (_typeValid.HasValue) 
                return _typeValid.Value;

            Type type;
            var i = 0;
            _typeValid = _checkInTreeTypeConstraints(ref i, out type) && typeof(T) == type;
            return _typeValid.Value;
        }

        private bool _checkInTreeTypeConstraints(ref int i, out Type type)
        {
            type = GeneHelper.ReturnType(_items[i]);
            i += 1;
            for (var arg = 0; arg < GeneHelper.Arity(_items[i]); ++arg)
            {
                Type paramType;
                if (!_checkInTreeTypeConstraints(ref i, out paramType) ||
                    paramType != GeneHelper.ArgumentType(_items[0], arg))
                    return false;
            }

            return true;
        }

        public object DynamicInvoke()
        {
            if (!CheckTypeConstraints())
                throw new InvalidOperationException("Tree is not type safe.");

            var i = 0;
            return _dynamicInvokeImpl(ref i);
        }

        private object _dynamicInvokeImpl(ref int i)
        {
            var myPos = i;
            var arity = GeneHelper.Arity(_items[myPos]);
            var parameters = new object[arity];
            i += 1;
            for (var pos = 0; pos < arity; ++pos)
                parameters[pos] = _dynamicInvokeImpl(ref i);

            return GeneHelper.DynamicInvoke(_items[myPos], parameters);
        }

        public int SubtreeLength(int beginPos)
        {
            var endPos = GetSubtreeEndPosition(beginPos);
            return endPos - beginPos;
        }

        public int GetSubtreeEndPosition(int beginPos)
        {
            var endPos = beginPos;
            GetSubtreeEndPositionImpl(ref endPos);
            return endPos;
        }

        private void GetSubtreeEndPositionImpl(ref int pos)
        {
            var parameters = GeneHelper.Arity(_items[pos]);
            ++pos;
            for (var i = 0; i < parameters; ++i)
                GetSubtreeEndPositionImpl(ref pos);
        }
    }
}
