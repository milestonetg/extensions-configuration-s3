﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MilestoneTG.Extensions.Configuration.S3.Json
{
    public class JsonS3ObjectParser : S3ObjectParser
    {
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;

        private JsonTextReader _reader;
        
        public override IDictionary<string, string> ParseAsync(Stream stream)
        {
            _data.Clear();
            _reader = new JsonTextReader(new StreamReader(stream));
            _reader.DateParseHandling = DateParseHandling.None;

            var jsonConfig = JObject.Load(_reader);

            VisitJObject(jsonConfig);

            return _data;
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    throw new FormatException(string.Format("Unsupported JSON token '{0}' was found. Path '{1}', line {2} position {3}.",
                        _reader.TokenType,
                        _reader.Path,
                        _reader.LineNumber,
                        _reader.LinePosition));
            }
        }

        private void VisitArray(JArray array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                throw new FormatException(string.Format("A duplicate key '{0}' was found.", key));
            }
            _data[key] = data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }
    }
}
