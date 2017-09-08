﻿using System;
using System.Linq;
using System.Reflection;
using Jasper.Bus.Runtime;
using Jasper.Util;

namespace Jasper.Bus.Model
{
    public static class MethodInfoExtensions
    {
        public static Type MessageType(this MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            var parameters = method.GetParameters();
            if (!parameters.Any())
            {
                return null;
            }

            if (parameters.Length == 1)
            {
                return parameters.First().ParameterType;
            }

            var first = parameters.FirstOrDefault(x => x.Name.IsIn("message", "input", "@event"));

            return first?.ParameterType;
        }
    }
}