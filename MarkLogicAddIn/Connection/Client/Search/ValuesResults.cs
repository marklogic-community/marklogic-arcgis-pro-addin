using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public abstract class ValuesResultBase
    {
        protected ValuesResultBase() { }

        public JToken Token { get; private set; }
        
        public virtual void Set(ValuesResults results, JToken token)
        {
            Token = token;
        }

        public int Frequency { get; protected set; }

        public double Lat { get; protected set; }

        public double Long { get; protected set; }

        public virtual string GetTupleValue(string tupleName) { return string.Empty; }
    }

    public class ValuesResult : ValuesResultBase
    {
        public ValuesResult() { }

        public override void Set(ValuesResults results, JToken token)
        {
            var coordsValue = (string)token["_value"];
            Debug.Assert(coordsValue != null);

            var coords = coordsValue.Split(',').Select(c => double.Parse(c)).ToArray();
            var longFirst = results.Type == "xs:long-lat-point";
            Frequency = (int)token["frequency"];
            // TODO: investigate indexing if its correct
            //Lat = coords[longFirst ? 1 : 0];
            //Long = coords[longFirst ? 0 : 1];
            Lat = coords[0];
            Long = coords[1];

            base.Set(results, token);
        }
    }

    public class TuplesResult : ValuesResultBase
    {
        private string[] _tupleValues;

        public TuplesResult() { }

        public override void Set(ValuesResults results, JToken token)
        {
            var values = (JArray)token["distinct-value"];
            var coordsValue = (string)values[0];
            Debug.Assert(coordsValue != null);

            var coords = coordsValue.Split(',').Select(c => double.Parse(c)).ToArray();
            var longFirst = results.Type == "xs:long-lat-point";
            Frequency = (int)token["frequency"];
            Lat = coords[longFirst ? 1 : 0];
            Long = coords[longFirst ? 0 : 1];

            _tupleValues = values.Values<string>().Skip(1).ToArray();

            base.Set(results, token);
        }

        public override string GetTupleValue(string tupleName)
        {
            if (_tupleValues.Length >= 1)
            {
                return _tupleValues[0];
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class ValuesResults : IEnumerable<ValuesResultBase>
    {
        public class Enumerator<T> : IEnumerator<ValuesResultBase> where T : ValuesResultBase
        {
            private ValuesResults _valuesResults;
            private JToken _resultsArray, _current;

            public Enumerator(ValuesResults valuesResults, JToken resultsArray)
            {
                _valuesResults = valuesResults;
                _resultsArray = resultsArray;
            }

            public ValuesResultBase Current
            {
                get
                {
                    var r = Activator.CreateInstance<T>();
                    r.Set(_valuesResults, _current);
                    return r;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (_current == null) // initial state (before first)
                    _current = _resultsArray.First;
                else
                    _current = _current.Next;
                return _current != null; // return false if zero elements or end of list
            }

            public void Reset()
            {
                _current = null;
            }
        }

        private JObject _response, _valuesResponse;
        private JToken _resultsArray;
        private bool _isTuples;

        public string RawContent { get; private set; }

        public ValuesResults(string responseContent)
        {
            RawContent = responseContent;
            var json = JsonConvert.DeserializeObject(responseContent);
            Debug.Assert(json != null && json.GetType() == typeof(JObject));
            _response = (JObject)json;
            _valuesResponse = _response["values-response"] as JObject;
            _resultsArray = (_valuesResponse["distinct-value"] ?? _valuesResponse["tuple"]) ?? JToken.Parse("[]");
            _isTuples = _valuesResponse["tuple"] != null;
        }

        public IEnumerator<ValuesResultBase> GetEnumerator()
        {
            var resultType = _isTuples ? typeof(TuplesResult) : typeof(ValuesResult); // change for tuples
            var enumeratorType = typeof(Enumerator<>);
            return (IEnumerator<ValuesResultBase>)Activator.CreateInstance(enumeratorType.MakeGenericType(resultType), new object[] { this, _resultsArray });
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Name => _valuesResponse.Value<string>("name");

        public string Type => _valuesResponse.Value<string>("type");

        public int Count
        {
            get
            {
                if (_resultsArray == null || (_resultsArray != null && _resultsArray.GetType() != typeof(JArray)))
                    return 0;
                var results = (JArray)_resultsArray;
                return results.Count;
            }
        } 

        public bool IsTuples { get { return _isTuples; } }

        public string[] TupleNames { get { return new[] { "sentiment" }; } }
    }
}
