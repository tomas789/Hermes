using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace Hermés.Core.Strategies
{
    /// <summary>
    /// Real-time updating genetic programming (RTU-GP) strategy.
    /// </summary>
    public class GeneticStrategy : IStrategy
    {
        private int _updateInterval = 50;

        private int _maximumLookback = 300;

        private int _firstUpdateGenerations = 50;
        private int _nextUpdateGenerations = 20;

        private int _minimumRequiredLength = 300;

        private int _populationSize = 100;

        public void Initialize(Kernel kernel)
        {
            throw new NotImplementedException();
        }

        public void DispatchEvent(Event e)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class Chromozome<T>
    {
        bool? _typeValid = null;

        private List<Delegate> _items = new List<Delegate>();

        public Delegate this[int i] 
        {
            get { return _items[i]; }
            set { 
                _typeValid = null; 
                _items[i] = value; 
            }
        }

        public void Add(Delegate del)
        {
            _typeValid = null;
            _items.Add(del);
        }

        public Type GetReturnType()
        {
            return typeof(T);
        }

        public bool CheckTypeConstraints()
        {
            if (_typeValid.HasValue) 
                return _typeValid.Value;

            Type type;
            int i = 0;
            _typeValid = _checkInTreeTypeConstraints(ref i, out type) && typeof(T) == type;
            return _typeValid.Value;
        }

        private bool _checkInTreeTypeConstraints(ref int i, out Type type)
        {
            var mi = _items[i].GetMethodInfo();
            var miParams = mi.GetParameters();
            type = mi.ReturnType;
            i += miParams.Length;
            foreach (var param in miParams)
            {
                Type paramType;
                if (!_checkInTreeTypeConstraints(ref i, out paramType) || paramType != param.ParameterType)
                    return false;
            }

            return true;
        }

        public T DynamicInvoke()
        {
            if (!CheckTypeConstraints())
                throw new InvalidOperationException("Tree is not type safe.");

            var i = 0;
            return (T)_dynamicInvokeImpl(ref i);
        }

        private object _dynamicInvokeImpl(ref int i)
        {
            var myPos = i;
            var arity = _items[myPos].GetMethodInfo().GetParameters().Length;
            var parameters = new object[arity];
            i += 1;
            for (var pos = 0; pos < arity; ++pos)
                parameters[pos] = _dynamicInvokeImpl(ref i);

            return _items[myPos].DynamicInvoke(parameters);
        }
    }
}
