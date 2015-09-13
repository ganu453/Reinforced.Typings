﻿using System;
using System.Reflection;
using Reinforced.Typings.Attributes;

namespace Reinforced.Typings.Generators
{
    /// <summary>
    /// Default typescript code generator for method
    /// </summary>
    public class MethodCodeGenerator : ITsCodeGenerator<MethodInfo>
    {
        /// <summary>
        /// Retrieves return type for specified method. Fell free to override it.
        /// </summary>
        /// <param name="element">Method</param>
        /// <returns>Types which is being returned by this method</returns>
        protected virtual Type GetReturnFunctionType(MethodInfo element)
        {
            Type t = element.ReturnType;
            var fa = element.GetCustomAttribute<TsFunctionAttribute>();
            if (fa != null)
            {
                if (fa.StrongType != null) t = fa.StrongType;
            }
            return t;
        }

        /// <summary>
        /// Retrieves function name corresponding to method and return type. Fell free to override it.
        /// </summary>
        /// <param name="element">Method info</param>
        /// <param name="resolver">Type resolver</param>
        /// <param name="name">Resulting method name</param>
        /// <param name="type">Resulting return type name</param>
        protected virtual void GetFunctionNameAndReturnType(MethodInfo element, TypeResolver resolver, out string name, out string type)
        {
            name = element.Name;
            var fa = element.GetCustomAttribute<TsFunctionAttribute>();
            if (fa != null)
            {
                if (!string.IsNullOrEmpty(fa.Name)) name = fa.Name;

                if (!string.IsNullOrEmpty(fa.Type)) type = fa.Type;
                else if (fa.StrongType != null) type = resolver.ResolveTypeName(fa.StrongType);
                else type = resolver.ResolveTypeName(element.ReturnType);
            }
            else
            {
                type = resolver.ResolveTypeName(element.ReturnType);
            }

        }


        /// <summary>
        /// Writes all method's parameters to output writer.
        /// </summary>
        /// <param name="element">Method info</param>
        /// <param name="resolver">Type resolver</param>
        /// <param name="sw">Output writer</param>
        protected virtual void WriteMethodParameters(MethodInfo element, TypeResolver resolver, WriterWrapper sw)
        {
            ParameterInfo[] p = element.GetParameters();
            for (int index = 0; index < p.Length; index++)
            {
                var param = p[index];
                if (param.IsIgnored()) continue;
                var generator = resolver.GeneratorFor(param);
                generator.Generate(param, resolver, sw);
                if (index != p.Length - 1 && !p[index + 1].IsIgnored())
                {
                    sw.Write(", ");
                }
            }
        }

        /// <summary>
        /// Writes method body to output writer
        /// </summary>
        /// <param name="element">Method info</param>
        /// <param name="resolver">Type resolver</param>
        /// <param name="sw">Output writer</param>
        protected virtual void GenerateBody(MethodInfo element, TypeResolver resolver, WriterWrapper sw)
        {
            if (element.ReturnType != typeof(void))
            {
                sw.WriteLine();
                sw.WriteIndented(@"{ 
    return null; 
}");
            }
            else
            {
                sw.Write("{{ }}");
                sw.WriteLine();
            }
        }

        /// <summary>
        /// Writes method name, accessor and opening brace to output writer
        /// </summary>
        /// <param name="isStatic">Is method static or not</param>
        /// <param name="name">Method name</param>
        /// <param name="sw">Output writer</param>
        /// <param name="isInterfaceDecl">Is this method interface declaration or not (access modifiers prohibited on interface declaration methods)</param>
        protected void WriteFunctionName(bool isStatic, string name, WriterWrapper sw, bool isInterfaceDecl = false)
        {
            sw.Tab();
            sw.Indent();
            if (!isInterfaceDecl)
            {
                sw.Write("public ");
                if (isStatic) sw.Write("static ");
            }

            sw.Write("{0}(", name);
        }

        /// <summary>
        /// Writes rest of method declaration to output writer (after formal parameters list)
        /// </summary>
        /// <param name="type">Returning type name</param>
        /// <param name="sw">Output writer</param>
        protected void WriteRestOfDeclaration(string type, WriterWrapper sw)
        {
            sw.Write(") : {0}", type);
        }

        public virtual void Generate(MethodInfo element, TypeResolver resolver, WriterWrapper sw)
        {
            if (element.IsIgnored()) return;

            var isInterfaceMethod = element.DeclaringType.IsExportingAsInterface();
            string name, type;

            GetFunctionNameAndReturnType(element, resolver, out name, out type);
            WriteFunctionName(element.IsStatic, name, sw, isInterfaceMethod);

            WriteMethodParameters(element, resolver, sw);

            WriteRestOfDeclaration(type, sw);

            if (isInterfaceMethod) { sw.Write(";"); sw.WriteLine(); }
            else GenerateBody(element, resolver, sw);
            sw.UnTab();
        }



    }
}