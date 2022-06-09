using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Utilities
{
    public interface IConsoleTreeNode
    {
        string Text { get; }
        IReadOnlyList<IConsoleTreeNode> Children { get; }

        string Prefix { get; set; }
        bool IsLastChild { get; set; }
    }

    public class ConsoleTree
    {
        private const string cross = " ├─";
        private const string corner = " └─";
        private const string vertical = " │ ";
        private const string space = "   ";

        private readonly IConsoleTreeNode root;

        public ConsoleTree(IConsoleTreeNode root)
        {
            this.root = root;
        }

        public void WriteTo(StringBuilder builder)
        {
            string prefix = "";
            var stack = new Stack<IConsoleTreeNode>();
            stack.Push(root);

            while (stack.TryPop(out var node))
            {
                prefix = "";
                if (node != root)
                {
                    builder.Append(node.Prefix);
                    prefix = node.Prefix + (node.IsLastChild ? space : vertical);
                    builder.Append(node.IsLastChild ? corner : cross);
                }
                builder.AppendLine(node.Text);

                bool lastChild = true;
                for (int i = node.Children.Count - 1; i >= 0; i--)
                {
                    var child = node.Children[i];
                    child.Prefix = prefix;
                    child.IsLastChild = lastChild;
                    stack.Push(child);
                    lastChild = false;
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            WriteTo(builder);
            return builder.ToString();
        }
    }
}
