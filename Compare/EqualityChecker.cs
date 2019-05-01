using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Compare
{
    public class EqualityChecker<T> where T : class
    {
        private readonly Dictionary<Type, Func<object, object, bool>> _typeOverrides = new Dictionary<Type, Func<object, object, bool>>();
        private readonly Dictionary<string, Func<object, object, bool>> _equalityFunctionByExpression = new Dictionary<string, Func<object, object, bool>>();
        private readonly Func<object, object, bool> _ignoreEqualityExpression = (object o1, object o2) => true;

        public EqualityChecker<T> Ignore(Type type)
        {
            _typeOverrides[type] = _ignoreEqualityExpression;
            return this;
        }

        public EqualityChecker<T> Ignore<TReturn>(Expression<Func<T, TReturn>> memberSelectExpression)
        {
            if (!ValidateExpression(memberSelectExpression))
            {
                throw new ArgumentException("memberSelectExpression");
            }

            var expressionString = GetExpressionString(memberSelectExpression);
            _equalityFunctionByExpression[expressionString] = _ignoreEqualityExpression;
            return this;
        }

        public EqualityChecker<T> Override<T1>(Func<T1, T1, bool> equalityFunction)
        {
            _typeOverrides[typeof(T1)] = (object obj1, object obj2) => equalityFunction((T1)obj1, (T1)obj2);
            return this;
        }

        public EqualityChecker<T> Override<TMemberType>(Expression<Func<T, TMemberType>> memberSelectExpression, Func<TMemberType, TMemberType, bool> equalityFunction)
        {
            if (!ValidateExpression(memberSelectExpression))
            {
                throw new ArgumentException("memberSelectExpression");
            }

            var expressionString = GetExpressionString(memberSelectExpression);
            _equalityFunctionByExpression[expressionString] = (object obj1, object obj2) => equalityFunction((TMemberType)obj1, (TMemberType)obj1);
            return this;
        }

        public bool HaveEqualValues(T val1, T val2)
        {
            return ClassesHaveEqualValues(val1, val2, new ObjectComparisonData<T>(), typeof(T));
        }

        private string GetExpressionString(Expression expression)
        {
            var exprBody = expression.ToString();
            var index = exprBody.IndexOf('.');  // Remove the parameter from the start of the expression.
            return exprBody.Substring(index + 1);
        }

        private bool ClassesHaveEqualValues(object obj1, object obj2, ObjectComparisonData<T> data, Type memberType)
        {
            CheckForCircularReference(obj1, obj2, data);

            var members = GetPublicMembers(memberType);
            var enumerator = members.GetEnumerator();
            var objectsEqual = true;
            while (objectsEqual && enumerator.MoveNext())
            {
                var member = (MemberInfo)enumerator.Current;
                var propertyType = member.GetUnderlyingType();

                var val1 = member.GetValue(obj1);
                var val2 = member.GetValue(obj2);

                // Append the current member to the current expression to form a new expression.
                var currentPropertyExpression = Expression.PropertyOrField(data.WhereAmI, member.Name);

                // TODO: Collections are not implemented yet.
                // Rules for specific properties take precedence over rules for types.
                if (this._equalityFunctionByExpression.TryGetValue(GetExpressionString(currentPropertyExpression),
                    out Func<object, object, bool> func1))
                {
                    objectsEqual = func1(val1, val2);
                }
                else if (_typeOverrides.TryGetValue(propertyType, out Func<object, object, bool> func2))
                {
                    objectsEqual = func2(val1, val2);
                }
                else
                {
                    var objData = new ObjectComparisonData<T>(data) { WhereAmI = currentPropertyExpression };
                    objectsEqual = CompareMemberValues(objData, propertyType, val1, val2);
                }
            }

            return objectsEqual;
        }

        private static void CheckForCircularReference(object obj1, object obj2, ObjectComparisonData<T> data)
        {
            var id1 = data.ObjectIdGenerator1.GetId(obj1, out bool firstTime1);
            var id2 = data.ObjectIdGenerator2.GetId(obj2, out bool firstTime2);

            if (data.VisitedObjectList1.Contains(id1) || data.VisitedObjectList2.Contains(id2))
            {
                throw new ArgumentException("Circular reference");
            }
            else
            {
                data.VisitedObjectList1.Add(id1);
                data.VisitedObjectList2.Add(id2);
            }
        }

        private bool CompareMemberValues(ObjectComparisonData<T> data, Type memberType, object val1, object val2)
        {
            bool result = false;
            if (memberType == typeof(string) || memberType.IsValueType)
            {
                result = val1.Equals(val2);
            }
            else if (memberType.IsClass)
            {
                if (val1 != null && val2 != null)
                {
                    // TODO: look up specific implementation for lists, collections, etc.
                    if (memberType.GetInterfaces().Contains(typeof(IEnumerable)))
                    {
                        result = (val1 as IEnumerable).Count() == (val2 as IEnumerable).Count();
                    }
                    else
                    {
                        result = ClassesHaveEqualValues(val1, val2, data, memberType);
                    }
                }
                else
                {
                    result = (val1 == null) && (val2 == null);
                }
            }

            return result;
        }

        private static MemberInfo[] GetPublicMembers(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            MemberInfo[] members = type.GetFields(bindingFlags).Cast<MemberInfo>()
                .Concat(type.GetProperties(bindingFlags)).ToArray();
            return members;
        }

        private static bool ValidateExpression<TReturn>(Expression<Func<T, TReturn>> expression)
        {
            var par = expression.Parameters[0];
            Expression exp = expression.Body;
            while (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression me = (MemberExpression)exp;
                exp = me.Expression;
            }

            return exp == par;
        }
    }
}
