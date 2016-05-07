using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    public class OutputTargetSectionViewModel: ViewModelBase, IPropertyPageContext
    {
        readonly Manifest manifest;

        private IOutputTargetViewModel library;
        public IOutputTargetViewModel Library
        {
            get { return library; }
            set
            {
                Set(ref library, value);
                Libraries = new IOutputTargetViewModel[] { library };
            }
        }

        IOutputTargetViewModel[] libraries;
        public IOutputTargetViewModel[] Libraries
        {
            get { return libraries; }
            set { Set(ref libraries, value); }
        }

        private ObservableCollection<IOutputTargetViewModel> binaries;
        public ObservableCollection<IOutputTargetViewModel> Binaries
        {
            get { return binaries; }
            set { Set(ref binaries, value); }
        }

        private ObservableCollection<IOutputTargetViewModel> benchmarks;
        public ObservableCollection<IOutputTargetViewModel> Benchmarks
        {
            get { return benchmarks; }
            set { Set(ref benchmarks, value); }
        }

        private ObservableCollection<IOutputTargetViewModel> tests;
        public ObservableCollection<IOutputTargetViewModel> Tests
        {
            get { return tests; }
            set { Set(ref tests, value); }
        }

        private ObservableCollection<IOutputTargetViewModel> examples;
        public ObservableCollection<IOutputTargetViewModel> Examples
        {
            get { return examples; }
            set { Set(ref examples, value); }
        }

        public OutputTargetSectionViewModel(Manifest m)
        {
            this.manifest = m;
            Load();
        }

        private bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                var temp = DirtyChanged;
                if(temp != null)
                    temp(null, null);
            }
        }

        public event EventHandler DirtyChanged;

        public void Apply()
        {
            IsDirty = false;
            throw new NotImplementedException();
        }

        private void Load()
        {
            var lookup = manifest.OutputTargets.ToLookup(t => t.Type);
            var rawLibrary = lookup[OutputTargetType.Library].FirstOrDefault();
            if(rawLibrary != null)
                Library = new OutputTargetViewModel(rawLibrary);
            else 
                Library = new LibraryAutoOutputTargetViewModel(manifest);
            binaries =  LoadTargets(lookup[OutputTargetType.Binary], () => new BinaryAutoOutputTargetViewModel(manifest));
            benchmarks =  LoadTargets(lookup[OutputTargetType.Benchmark], () => new BenchmarkAutoOutputTargetViewModel(manifest));
            tests =  LoadTargets(lookup[OutputTargetType.Test], () => new TestAutoOutputTargetViewModel(manifest));
            examples =  LoadTargets(lookup[OutputTargetType.Example], () => new ExampleAutoOutputTargetViewModel(manifest));
        }

        static ObservableCollection<IOutputTargetViewModel> LoadTargets(IEnumerable<OutputTarget> targets, Func<IOutputTargetViewModel> ctor)
        {
            List<OutputTargetViewModel> vms = targets.Select(t => new OutputTargetViewModel(t)).ToList();
            if(vms.Count == 0)
                return new ObservableCollection<IOutputTargetViewModel> { ctor() };
            return new ObservableCollection<IOutputTargetViewModel>();
        }
    }
}
