using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Core.Contracts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class CreatureNode : ModuleNode
    {
        /// <summary>Relationship type to identify a creature object part (http://pack.guildsmanship.com/ikosa/creature)</summary>
        public const string CreatureRelation = @"http://pack.guildsmanship.com/ikosa/creature";

        private Creature _Creature;

        public CreatureNode(Description description, Creature creature)
            : base(description)
        {
            _Creature = creature;
        }

        public static CreatureNode GetCreatureNode(PackagePart part)
        {
            if (part != null)
            {
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _critter = (CreatureNode)_fmt.Deserialize(_ctxStream);
                _critter.SetPackagePart(part);

                // return
                return _critter;
            }
            return null;
        }

        public override string GroupName => @"Creatures";

        public Creature Creature => _Creature;

        protected override void OnSetPackagePart() { }

        public override string TypeName => typeof(CreatureNode).FullName;
        public override IEnumerable<ICorePart> Relationships { get { yield break; } }

        protected override string NodeExtension => @"creature";
        protected override string ContentType => @"ikosa/creature";
        protected override string RelationshipType => CreatureRelation;

        public override void Close() { }
        protected override void OnRefreshPart() { }
        protected override void OnDoSave(Uri baseUri) { }

        public override void Save(Package parent)
        {
            DoSaveFile(parent);
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            DoSaveFile(parent, baseUri);
        }
    }
}
