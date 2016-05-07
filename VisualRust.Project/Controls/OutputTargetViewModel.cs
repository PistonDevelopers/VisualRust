using System;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    public class OutputTargetViewModel : ViewModelBase, IOutputTargetViewModel
    {
        public OutputTargetType Type { get; private set; }

        public string TabName { get { return this.Name; } }

        private string name;
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }
        
        private string path;
        public string Path
        {
            get { return path; }
            set { Set(ref path, value); }
        }
        
        private bool test;
        public bool Test
        {
            get { return test; }
            set { Set(ref test, value); }
        }
        
        private bool doctest;
        public bool Doctest
        {
            get { return doctest; }
            set { Set(ref doctest, value); }
        }
        
        private bool bench;
        public bool Bench
        {
            get { return bench; }
            set { Set(ref bench, value); }
        }
        
        private bool doc;
        public bool Doc
        {
            get { return doc; }
            set { Set(ref doc, value); }
        }
        
        private bool plugin;
        public bool Plugin
        {
            get { return plugin; }
            set { Set(ref plugin, value); }
        }

        private bool harness;
        public bool Harness
        {
            get { return harness; }
            set { Set(ref harness, value); }
        }

        public bool IsReadOnly { get { return false; } }

        public OutputTargetViewModel(OutputTarget t)
        {
            Type = t.Type;
            name = t.Name;
            path = t.Path;
            test = t.Test;
            doctest = t.Doctest;
            bench = t.Bench;
            doc = t.Doc;
            plugin = t.Plugin;
            harness = t.Harness;
        }
    }
}