﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Baseline;
using Jasper.Bus;

namespace Jasper.Util
{
    /// <summary>
    /// Used to override Jasper's default behavior for identifying a message type.
    /// Useful for integrating with other services without having to share a DTO
    /// type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeAliasAttribute : Attribute
    {
        public string Alias { get; }

        public TypeAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

    public static class TypeExtensions
    {
        private static readonly Regex _aliasSanitizer = new Regex("<|>", RegexOptions.Compiled);

        public static string GetPrettyName(this Type t)
        {
            if (!t.GetTypeInfo().IsGenericType)
                return t.Name;

            var sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.Ordinal)));
            sb.Append(t.GetGenericArguments().Aggregate("<", (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetPrettyName(type)));
            sb.Append(">");

            return sb.ToString();
        }

        public static string ToTypeAlias(this Type type)
        {
            if (type.HasAttribute<TypeAliasAttribute>())
            {
                return type.GetAttribute<TypeAliasAttribute>().Alias;
            }

            var nameToAlias = type.FullName;
            if (type.GetTypeInfo().IsGenericType)
            {
                nameToAlias = _aliasSanitizer.Replace(type.GetPrettyName(), string.Empty);
            }

            var parts = new List<string> {nameToAlias};
            if (type.IsNested)
            {
                parts.Insert(0, type.DeclaringType.Name);
            }

            return string.Join("_", parts);
        }

        public static string ToVersion(this Type messageType)
        {
            return messageType.HasAttribute<VersionAttribute>()
                ? messageType.GetAttribute<VersionAttribute>().Version
                : "V1";
        }

        public static string ToContentType(this Type messageType, string format)
        {
            var alias = messageType.ToTypeAlias().ToLowerInvariant();
            var version = messageType.ToVersion().ToLower();

            return $"application/vnd.{alias}.{version}+{format}";

        }
    }
}
