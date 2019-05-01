using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

namespace Compare
{
    internal class ObjectComparisonData<T>
    {
        public ObjectComparisonData()
        {
            ObjectIdGenerator1 = new ObjectIDGenerator();
            ObjectIdGenerator2 = new ObjectIDGenerator();
            VisitedObjectList1 = new List<long>();
            VisitedObjectList2 = new List<long>();
            WhereAmI = Expression.Parameter(typeof(T));
        }

        public ObjectComparisonData(ObjectComparisonData<T> data)
        {
            VisitedObjectList1 = new List<long>(data.VisitedObjectList1);
            VisitedObjectList2 = new List<long>(data.VisitedObjectList2);
            ObjectIdGenerator1 =  data.ObjectIdGenerator1;
            ObjectIdGenerator2 = data.ObjectIdGenerator2;
            WhereAmI = WhereAmI;
        }

        public List<long> VisitedObjectList1 { set; get; }
        public List<long> VisitedObjectList2 { set; get; }
        public ObjectIDGenerator ObjectIdGenerator1 { set; get; }
        public ObjectIDGenerator ObjectIdGenerator2 { set; get; }
        public Expression WhereAmI { set; get; }
    }
}
