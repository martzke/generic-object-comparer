using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare.Tests.Entities
{
    public class SelfReferencingObject
    {
        public SelfReferencingObject()
        {
            this.SelfReference = this;
        }

        public SelfReferencingObject SelfReference { set; get; }
    }
}
