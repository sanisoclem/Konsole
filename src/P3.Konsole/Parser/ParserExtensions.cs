﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace P3.Konsole.Parser
{
    public static class ParserExtensions
    {
        public static T ParseCommandArgs<T>(this string[] args) {
            if (args is T)
                return (T)(object)args;

            var pars = CommandParameter.GetParameters<T>();
            var values = new Dictionary<CommandParameter, string>();
            CommandParameter current = null;
            for (int i = 0; i < args.Length; i++) {
                var parm = pars.SingleOrDefault(p => p.ShortName == args[i] || p.LongName == args[i]);

                if (current != null) {
                    // -- expression  is a parameter, not value 
                    if (parm != null)
                        throw new MissingValueParseException();

                    // -- if we are expecting a value and the exp is not a parameter, get value and commit to list
                    values.Add(current, args[i]);
                    current = null;
                }
                else {
                    if (parm == null)
                        throw new UnknownParameterParseException();

                    if (parm.HasValue) {
                        current = parm;
                    }
                    else {
                        values.Add(parm, true.ToString());
                    }
                }
            }
            if (current != null)
                throw new MissingValueParseException();

            var falseFlags = pars.Where(p => !p.HasValue).Where(p => !values.ContainsKey(p)).ToList();
            foreach (var item in falseFlags) {
                values.Add(item, false.ToString());
            }

            if (pars.Where(p => p.IsRequired).Any(p => !values.ContainsKey(p)))
                throw new MissingParameterParseException();

            // -- hack for now until we have a real parser
            return new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(p => p.Key.PropertyName, p => p.Value)).Build().Get<T>();
        }
    }
}