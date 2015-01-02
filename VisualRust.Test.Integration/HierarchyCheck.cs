using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test.Integration
{
    class HierarchyCheck
    {
        private ProjectNode root;
        private HierarchyCheck parent;
        private string caption;
        private Type type;
        private Action<HierarchyNode> validator;
        private List<HierarchyCheck> children = new List<HierarchyCheck>();

        public HierarchyCheck(ProjectNode root)
        {
            this.root = root;
        }

        private HierarchyCheck(string cap, Type t, Action<HierarchyNode> validator)
        {
            this.caption = cap;
            this.type = t;
            this.validator = validator ?? (_ => {});
        }

        public HierarchyCheck Child<U>(string caption) where U : HierarchyNode
        {
            var check = new HierarchyCheck(caption, typeof(U), null);
            check.parent = this;
            children.Add(check);
            return check;
        }

        public HierarchyCheck Child<U>(string caption, params Action<U>[] ac) where U : HierarchyNode
        {
            Action<U> combined = (Action<U>)Action<U>.Combine(ac);
            var check = new HierarchyCheck(caption, typeof(U), n => combined((U)n));
            check.parent = this;
            children.Add(check);
            return check;
        }

        public HierarchyCheck Sibling<U>(string caption) where U : HierarchyNode
        {
            return parent.Child<U>(caption).parent;
        }

        public HierarchyCheck Sibling<U>(string caption, params Action<U>[] ac) where U : HierarchyNode
        {
            return parent.Child<U>(caption, ac).parent;
        }

        public HierarchyCheck Parent()
        {
            return parent;
        }

        private void Run(HierarchyNode node)
        {
            Assert.ReferenceEquals(this.root, node);
            var projChildren = node.AllChildren.ToList();
            Assert.AreEqual<int>(
                children.Count,
                node.AllChildren.Count(),
                "Wrong number of children in node <{0}> of type <{1}>.",
                node.Caption,
                node.GetType());
            string allNodes = String.Join(", ", projChildren.Select(n => String.Format("<{0} ({1})>", n.Caption, n.GetType())));
            foreach(var child in children)
            {
                HierarchyNode childNode = projChildren.FirstOrDefault(n => child.type.IsAssignableFrom(n.GetType()) && String.Equals(n.Caption, child.caption));
                Assert.IsNotNull(childNode, "Node <{0} ({1})> has no subnode <{2} ({3})>. All subnodes: [{4}].", node.Caption, node.GetType(), child.caption, child.type, allNodes);
                child.Run(childNode);
            }
        }

        public void Run()
        {
            HierarchyCheck current = this;
            while (current.parent != null)
                current = current.parent;
            current.Run(current.root);
        }
    }
}
