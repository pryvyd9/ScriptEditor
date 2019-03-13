using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Windows;
using ImpromptuInterface;
using System.Reflection;

using System.Linq;

namespace ScriptEditor
{
    internal sealed class DocumentChangeProxy : DynamicObject
    {
        private static readonly Type type = typeof(Document);
        private readonly Document origin;

        private static readonly HashSet<string> CommitableMethods =new HashSet<string>
        {
            "Replace",
            "Insert",
            "InsertLineAfter",
            "Delete",
            "BreakLine"
        };



        public void RollbackChanges() => origin.RollbackChanges();

        private bool TryGetMethod(
            object container, 
            InvokeMemberBinder binder, 
            object[] args,
            out MethodInfo method)
        {
            method = container.GetType().GetMethods().SingleOrDefault(n =>
            {
                var nn = n.Name == binder.Name;

                if (nn)
                {
                    var aargs = args.Select((m, i) => m?.GetType()).ToArray();

                    var targs = n.GetParameters().Select(m => m.ParameterType).ToArray();

                    if (aargs.Length != targs.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < aargs.Length; i++)
                    {
                        var isAssignable = targs[i].IsAssignableFrom(aargs[i]);
                        var isOptional = n.GetParameters().ElementAt(i).IsOptional;

                        if (aargs[i] == null && isOptional)
                        {
                            continue;
                        }
                        else if (!isAssignable)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });

            if (method != null)
            {
                return true;
            }

            return false;
        }

        private bool Invoke(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (TryGetMethod(this, binder, args, out var method))
            {
                result = method.Invoke(this, args);
                return true;
            }
            else
            {
                if (TryGetMethod(origin, binder, args, out method))
                {
                    result = method.Invoke(origin, args);
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (CommitableMethods.Contains(binder.Name))
            {
                origin.StartChanges();
                var res = Invoke(binder, args, out result);
                origin.CommitChanges();

                return res;
            }
            else
            {
                return Invoke(binder, args, out result);
            }

        }


        private class TypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.AssemblyQualifiedName == y.AssemblyQualifiedName;
            }

            public int GetHashCode(Type obj)
            {
                return obj.GetHashCode();
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                result = type.GetProperty(binder.Name).GetValue(origin);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        internal DocumentChangeProxy(Document document)
        {
            this.origin = document;
        }

        internal static IDocument AsIDocument(IDocument document)
        {
            return new DocumentChangeProxy((Document)document).ActLike<IDocument>();
        }

    }


}
