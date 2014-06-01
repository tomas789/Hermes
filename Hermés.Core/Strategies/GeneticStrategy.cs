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

        private readonly Random _random = new Random();

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

        private readonly List<Chromozome<Delegate>> _population = 
            new List<Chromozome<Delegate>>();

        private readonly Dictionary<GeneticSelector<Delegate>, int> _weightedSelectors = 
            new Dictionary<GeneticSelector<Delegate>, int>(); 

        private bool _initialized = false;

        private Kernel _kernel;

        private int _ticks = 0;

        private int _generation = 0;

        public void Initialize(Kernel kernel)
        {
            if (_initialized)
                throw new DoubleInitializationException();
            _initialized = true;

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

            _primitives.Add((Func<bool>)(() => true));
            _primitives.Add((Func<bool>)(() => false));

            _primitives.Add((Func<bool, SignalKind, SignalKind, SignalKind>)((cond, a, b) => cond ? a : b));
            _primitives.Add((Func<bool, double?, double?, double?>)((cond, a, b) => cond ? a : b));
            _primitives.Add((Func<bool, PriceGroup, PriceGroup, PriceGroup>)((cond, a, b) => cond ? a : b));

            var geneHelper = new DelegateGeneHelper();
            var random = new Random();

            var fullTreeGenerator = new FullTreeGenerator<Delegate>(geneHelper, _primitives)
            {
                ReturnType = typeof (SignalKind),
                Random = random
            };

            var trees = fullTreeGenerator.Operator(null);
            var tree = trees[0];
            Debug.WriteLine("output: {0}", tree.DynamicInvoke());

            AddGeneticOperator(fullTreeGenerator);
            AddGeneticOperator(new OnePointCrossover<Delegate>(geneHelper) {Random = random});

            AddGeneticSelector(new TournamentSelector<Delegate>(geneHelper, 3) {Random = random});

            Debug.WriteLine("GeneticStrategy inititalized.");
        }

        public void AddGeneticOperator(GeneticOperator<Delegate> op, int weight = 1)
        {
            if (weight <= 0)
                throw new ArgumentException(
                    "Genetic operator weight have to be positive number.");

            _weightedOperators.Add(op, weight);
        }

        public void AddGeneticSelector(GeneticSelector<Delegate> sel, int weight = 1)
        {
            if (weight <= 0)
                throw new ArgumentException(
                    "Genetic selector weight have to be positive number.");

            _weightedSelectors.Add(sel, weight);
        }

        private GeneticOperator<Delegate> GetRandomGeneticInitializerOperator()
        {
            var initializers = (from op in _weightedOperators
                                where op.Key.Arity == 0
                                select new { op = op.Key, weight = op.Value }).ToArray();

            if (initializers.Length == 0)
                throw new InvalidOperationException(
                    "Initializing population without initializer.");

            var weightSum = (from ini in initializers select ini.weight).Sum();

            GeneticOperator<Delegate> initializer = null;
            var p = _random.Next(weightSum + 1);
            foreach (var i in initializers)
            {
                if (p <= i.weight)
                {
                    initializer = i.op;
                    break;
                }

                p -= i.weight;
            }

            if (initializer == null)
                throw new ImpossibleException();

            return initializer;
        }

        private GeneticOperator<Delegate> GetRandomGeneticOperator()
        {
            var operators = (from op in _weightedOperators
                                where op.Key.Arity != 0
                                select new { op = op.Key, weight = op.Value }).ToArray();

            if (operators.Length == 0)
                throw new InvalidOperationException(
                    "Initializing population without initializer.");

            var weightSum = (from ini in operators select ini.weight).Sum();

            GeneticOperator<Delegate> geneticOp = null;
            var p = _random.Next(weightSum + 1);
            foreach (var i in operators)
            {
                if (p <= i.weight)
                    return i.op;

                p -= i.weight;
            }

            throw new ImpossibleException();
        }

        private GeneticSelector<Delegate> GetRandomGeneticSelector()
        {
            var selectors = (from op in _weightedSelectors
                             select new {op = op.Key, weight = op.Value}).ToArray();

            if (selectors.Length == 0)
                throw new InvalidOperationException(
                    "No genetic selector set.");

            var weightSum = (from ini in selectors select ini.weight).Sum();

            GeneticSelector<Delegate> selector = null;
            var p = _random.Next(weightSum + 1);
            foreach (var i in selectors)
            {
                if (p <= i.weight)
                {
                    selector = i.op;
                    break;
                }

                p -= i.weight;
            }

            if (selector == null)
                throw new ImpossibleException();

            return selector;
        }

        private void InitializePopulation()
        {
            _generation = 0;
            var initializer = GetRandomGeneticInitializerOperator();

            for (var i = 0; _population.Count < _config.PopulationSize; ++i)
            {
                _population.AddRange(initializer.Operator(null).Where(c => c.Count != 0));

                if (i > _config.PopulationSize * 10)
                    throw new InvalidOperationException("Unable to construct enough elements.");
            }
        }

        private void NextGeneration()
        {
            ++_generation;

            Debug.WriteLine("Doing generation {0}", _generation);

            if (_population.Count == 0)
                throw new InvalidOperationException(
                    "Initialize population before calling NextGeneration.");

            var next = new List<Chromozome<Delegate>>();
            while (next.Count < _config.PopulationSize)
            {
                var op = GetRandomGeneticOperator();
                var sel = GetRandomGeneticSelector();

                var chromozomes = new List<Chromozome<Delegate>>();
                while  (chromozomes.Count < op.Arity)
                    chromozomes.Add(sel.Operator(_population));

                if (chromozomes.Count != op.Arity)
                    throw new ImpossibleException();

                var offspring = op.Operator(chromozomes);
                next.AddRange(offspring);
            }

            Debug.WriteLine("End doing generation {0}", _generation);
        }

        private void EvaluateChromozome(MarketEvent ev, Chromozome<Delegate> chromozome)
        {
            Debug.WriteLine("GeneticStrategy: Evaluating individual: {0}", chromozome);
            var hold = 0;
            var miss = 0;

            var sizeInHold = 0.0;
            var value = 0.0;

            var priceGroup = ev.Market.GetHistoricalPriceGroup(_config.EvaluationLength); ;

            for (var i = _config.EvaluationLength - 1; i >= 1; --i)
            {
                var lastPriceGroup = priceGroup;
                priceGroup = ev.Market.GetHistoricalPriceGroup(i);

                if (priceGroup == null)
                {
                    continue;
                }

                value += (priceGroup.Close - lastPriceGroup.Close) * sizeInHold * ev.Market.PointPrice;

                _evaluationContext = priceGroup;
                var signal = (SignalKind)chromozome.DynamicInvoke();
                switch (signal)
                {
                    case SignalKind.Buy:
                        if (priceGroup.Close - lastPriceGroup.Close <= 0)
                            ++miss;
                        sizeInHold += 1;
                        break;
                    case SignalKind.Sell:
                        if (priceGroup.Close - lastPriceGroup.Close >= 0)
                            ++miss;
                        sizeInHold -= 1;
                        break;
                    case SignalKind.Hold:
                        ++hold;
                        break;
                }
            }

            chromozome.Fitness = new Fitness(new[] { 1.0, -1.0, -1.0 });
            chromozome.Fitness.Values[0] = value;
            chromozome.Fitness.Values[1] = miss;
            chromozome.Fitness.Values[2] = hold;
            chromozome.Fitness.Valid = true;

            Debug.WriteLine("GeneticStrategy: Chromozome value: {0}", value);
        }

        private void EvaluatePopulation(MarketEvent ev)
        {
            var tasks = (from chromozome in _population
                where chromozome.Fitness == null || !chromozome.Fitness.Valid
                select new Task(() => EvaluateChromozome(ev, chromozome))).ToArray();

            foreach (var task in tasks)
                task.Start();

            Task.WaitAll(tasks);
        }

        public void DispatchEvent(Event e)
        {
            var ts = new TypeSwitch()
                .Case((MarketEvent x) => DispatchConcrete(x));

            ts.Switch(e);
        }

        private void DispatchConcrete(MarketEvent ev)
        {
            ++_ticks;

            if (_ticks < _config.MinimumRequiredLength)
                return;

            if (_ticks == _config.MinimumRequiredLength)
            {
                InitializePopulation();
                for (var i = 0; i < _config.FirstUpdateGenerations; ++i)
                {
                    EvaluatePopulation(ev);
                    NextGeneration();
                }
            }
            else if ((_ticks - _config.MinimumRequiredLength) % _config.UpdateInterval == 0)
            {
                NextGeneration();
                for (var i = 0; i < _config.NextUpdateGenerations; ++i)
                {
                    EvaluatePopulation(ev);
                    NextGeneration();
                }
            }

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
            {
                return;
            }

            Debug.WriteLine("Best individual fitness: {0}", best.Fitness);

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

        public override string ToString()
        {
            if (Weights.Length == 0)
                return "empty fitness";

            var str = new StringBuilder();
            var items = Weights.Zip(Values, (a, b) => string.Format("{0} {1}", a, b)).ToArray();
            for (var i = 0; i < items.Length - 1; ++i)
                str.Append(items[i]).Append(", ");
            str.Append(items[items.Length - 1]);

            return string.Format("[{0}]({1})", GetWeightedSum(), str);
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

    public abstract class GeneticSelector<T>
    {
        public Random Random = new Random();

        public IGeneHelper<T> GeneHelper { get; private set; }

        protected GeneticSelector(IGeneHelper<T> geneHelper)
        {
            GeneHelper = geneHelper;
        }

        public Chromozome<T> Operator(IList<Chromozome<T>> chromozomes)
        {
            if (GeneHelper == null)
                throw new InvalidOperationException(
                    "GeneHelper is not set before using GeneticOperator.");

            return OperatorImpl(chromozomes);
        }

        protected abstract Chromozome<T> OperatorImpl(IList<Chromozome<T>> chromozomes);
    }

    public class TournamentSelector<T> : GeneticSelector<T>
    {
        private readonly int _tournamentSize;

        public TournamentSelector(IGeneHelper<T> geneHelper, int tournamentSize)
            : base(geneHelper)
        {
            if (tournamentSize < 1)
                throw new ArgumentException(
                    "Creating tournament selection of non-positive size.");

            _tournamentSize = tournamentSize;
        }

        protected override Chromozome<T> OperatorImpl(IList<Chromozome<T>> chromozomes)
        {
            if (chromozomes.Count == 0)
                return null;

            var tournament = new List<Chromozome<T>>();
            for (var i = 0; i < _tournamentSize; ++i)
                tournament.Add(chromozomes[Random.Next(chromozomes.Count)]);

            Chromozome<T> best = null;
            foreach (var chromozome in chromozomes)
            {
                if (best == null || !best.Fitness.Valid)
                {
                    best = chromozome;
                    continue;
                }

                var currentFitness = chromozome.Fitness.GetWeightedSum();
                var bestFitness = best.Fitness.GetWeightedSum();
                if (currentFitness > bestFitness)
                    best = chromozome;

            }

            return best;
        }
    }

    /// <summary>
    /// Abstract base class for every genetic operator used in simulation.
    /// </summary>
    /// <typeparam name="T">Type of gene.</typeparam>
    public abstract class GeneticOperator<T>
    {
        private Random _random = new Random();

        protected GeneticOperator(IGeneHelper<T> geneHelper)
        {
            GeneHelper = geneHelper;
        } 
        
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
            : base(geneHelper)
        {
            ReturnType = null;
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

        /// <summary>
        /// Return type of generated tree.
        /// </summary>
        public Type ReturnType;

        protected override int GetArity()
        {
            return 0;
        }

        protected override IList<Chromozome<T>> OperatorImpl(IList<Chromozome<T>> chromozomes)
        {
            if (ReturnType == null)
                throw new InvalidOperationException(
                    "Trying to generate chromozome before return type was set.");

            var depth = _random.Next(_minDepth, _maxDepth);
            var chromozome = new Chromozome<T>(GeneHelper);
            if (!GenerateSubtree(chromozome, ReturnType, depth))
                return new Chromozome<T>[] {};

            if (!chromozome.CheckTypeConstraints())
                throw new ImpossibleException();

            return new [] {chromozome};
        }

        private bool GenerateSubtree(Chromozome<T> chromozome, Type type, int depth)
        {
            if (depth == 0)
            {
                var allowedTerminals = (
                    from terminal in _terminalSet
                    where GeneHelper.ReturnType(terminal) == type
                    select terminal).ToArray();

                if (allowedTerminals.Length == 0)
                {
                    Debug.WriteLine("Type check failed at terminal; Type: {0}", type);
                    return false;
                }

                chromozome.Add(allowedTerminals[_random.Next(allowedTerminals.Length)]);
            }
            else
            {
                var allowedFunctions = (
                    from function in _functionSet
                    where GeneHelper.ReturnType(function) == type
                    select function).ToArray();

                if (allowedFunctions.Length == 0)
                {
                    Debug.WriteLine("Type check failed at function; Type: {0}", type);
                    return false;
                }

                var selected = allowedFunctions[_random.Next(allowedFunctions.Length)];
                chromozome.Add(selected);
                var arity = GeneHelper.Arity(selected);
                for (var i = 0; i < arity; ++i)
                {
                    var success = false;
                    for (var tries = 0; tries < 5; ++tries)
                    {
                        if (!GenerateSubtree(chromozome, GeneHelper.ArgumentType(selected, i), depth - 1))
                            continue;

                        success = true;
                        break;
                    }

                    if (success) 
                        continue;

                    Debug.WriteLine("Type check failed at recursive function; Type: {0}", type);
                    chromozome.RemoveAt(chromozome.Count - 1);
                    return false;
                }
            }

            return true;
        } 
    }

    /// <summary>
    /// Well-known one-point-subtree crossover.
    /// </summary>
    /// <typeparam name="T">Type of gene.</typeparam>
    public class OnePointCrossover<T> : GeneticOperator<T>
    {
        public OnePointCrossover(IGeneHelper<T> geneHelper)
            :base (geneHelper)
        {
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

                types.Add(chromozomeTypes);
            }

            var commonTypes = new HashSet<Type>();
            foreach (var chromozomeTypes in types)
                commonTypes.IntersectWith(chromozomeTypes.Keys);

            var lhsCommonItems = (from commonType in types[0] 
                               where commonTypes.Contains(commonType.Key) 
                               from cxPt in commonType.Value 
                               select new KeyValuePair<Type, int>(commonType.Key, cxPt)).ToArray();

            if (lhsCommonItems.Length == 0)
                return chromozomes;

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

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
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

            if (_items.Count == 0)
                return true;

            Type type;
            var i = 0;
            _typeValid = _checkInTreeTypeConstraints(ref i, out type);
            return _typeValid.Value;
        }

        private bool _checkInTreeTypeConstraints(ref int i, out Type type)
        {
            var myPos = i++;
            type = GeneHelper.ReturnType(_items[myPos]);
            
            for (var arg = 0; arg < GeneHelper.Arity(_items[myPos]); ++arg)
            {
                Type paramType;
                if (!_checkInTreeTypeConstraints(ref i, out paramType) ||
                    paramType != GeneHelper.ArgumentType(_items[myPos], arg))
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
