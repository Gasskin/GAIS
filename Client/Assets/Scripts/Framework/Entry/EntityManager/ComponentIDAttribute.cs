using System;

namespace Framework
{
    public class ComponentIDAttribute : Attribute
    {
        public int ComponentID;

        public ComponentIDAttribute(int id)
        {
            ComponentID = id;
        }
    }
}