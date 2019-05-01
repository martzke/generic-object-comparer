//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices.ComTypes;

//namespace Compare.Foo
//{
//    internal class ListComparer<T, T1> : IEqualityComparer<T> where T : ICollection<T>
//                                                              where T1 : class
//    {
//        private EqualityChecker<T1> _equalityChecker;
//        private ObjectComparisonData<T1> _data;

//        public ListComparer(EqualityChecker<T1> equalityChecker, ObjectComparisonData<T1> data)
//        {
//            _equalityChecker = equalityChecker;
//            _data = data;
//        }

//        public bool Equals(T x, T y)
//        {
//            var genericType = typeof(T).GetGenericArguments()[0];
//            if (x.Count != y.Count)
//            {
//                for (int i = 0; i < x.Count; i++)
//                {
//                    var equal = _equalityChecker.ClassesHaveEqualValues(x, y, _data, false, genericType);
//                    if (!equal)
//                    {
//                        return false;
//                    }
//                }
//            }

//            return true;
//        }

//        public int GetHashCode(T obj)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}