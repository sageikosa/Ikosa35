using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ikosa.Packaging;

namespace Uzi.Visualize.IkosaPackaging
{
    [Serializable]
    public abstract class ResourceReference
    {
        protected ResourceReference(ResourceReferenceManager parent, string name, string fileName, string internalPath)
        {
            _Name = name;
            _File = fileName;
            _Internal = internalPath;
            _Parent = parent;
        }

        #region private data
        private string _Name;
        private string _File;
        private string _Internal;
        private ResourceReferenceManager _Parent;
        #endregion

        protected abstract void RefreshResolver();

        public string Name { get { return _Name; } }
        public string FileName { get { return _File; } set { _File = value; RefreshResolver(); } }
        public string InternalPath { get { return _Internal; } set { _Internal = value; RefreshResolver(); } }

        public ResourceReferenceManager Parent { get { return _Parent; } }

        public abstract IBasePart Part { get; }

        public CorePackage Package { get { return Parent.GetPackage(this).Item1; } }
    }
}