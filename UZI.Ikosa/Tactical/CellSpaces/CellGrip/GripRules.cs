using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class GripRules
    {
        public GripRules()
        {
            _Rules = [];
        }

        private List<GripRule> _Rules;

        /// <summary>
        /// initialize grip rules for uniform material fill
        /// </summary>
        public void InitializeUniform(CellMaterial material)
        {
            Rules.Clear();
            if ((material?.Balance ?? material?.DangleGrip
                ?? material?.BaseGrip ?? material?.LedgeGrip) != null)
            {
                Rules.Add(new AnchorFaceGripRule
                {
                    AppliesTo = AnchorFaceList.All,
                    Balance = material.Balance,
                    Base = material.BaseGrip,
                    Dangling = material.DangleGrip,
                    Ledge = material.LedgeGrip
                });
            }
        }

        /// <summary>
        /// initialize grip rules for two materials
        /// </summary>
        public void InitializeMaterials(CellMaterial material, CellMaterial plusMaterial)
        {
            Rules.Clear();
            if ((material?.Balance ?? material?.DangleGrip
                ?? material?.BaseGrip ?? material?.LedgeGrip) != null)
            {
                Rules.Add(new MaterialGripRule
                {
                    Balance = material.Balance,
                    Dangling = material.DangleGrip,
                    Base = material.BaseGrip,
                    Ledge = material.LedgeGrip,
                    Disposition = GripDisposition.Full
                });
                Rules.Add(new MaterialGripRule
                {
                    Balance = material.Balance,
                    Dangling = material.DangleGrip,
                    Base = material.BaseGrip,
                    Disposition = GripDisposition.Internal
                });
                Rules.Add(new MaterialGripRule
                {
                    Balance = material.Balance,
                    Dangling = material.DangleGrip,
                    Base = material.BaseGrip,
                    Ledge = material.LedgeGrip,
                    Disposition = GripDisposition.Rectangular
                });
                Rules.Add(new MaterialGripRule
                {
                    Balance = material.Balance,
                    Dangling = material.DangleGrip,
                    Base = material.BaseGrip,
                    Ledge = material.LedgeGrip,
                    Disposition = GripDisposition.Irregular
                });
            }
            if ((plusMaterial?.Balance ?? plusMaterial?.DangleGrip
                ?? plusMaterial?.BaseGrip ?? plusMaterial?.LedgeGrip) != null)
            {
                Rules.Add(new MaterialGripRule
                {
                    Balance = plusMaterial.Balance,
                    Dangling = plusMaterial.DangleGrip,
                    Base = plusMaterial.BaseGrip,
                    Ledge = plusMaterial.LedgeGrip,
                    IsPlusMaterial = true,
                    Disposition = GripDisposition.Full
                });
                Rules.Add(new MaterialGripRule
                {
                    Balance = plusMaterial.Balance,
                    Dangling = plusMaterial.DangleGrip,
                    Base = plusMaterial.BaseGrip,
                    IsPlusMaterial = true,
                    Disposition = GripDisposition.Internal
                });
                Rules.Add(new MaterialGripRule
                {
                    Balance = plusMaterial.Balance,
                    Dangling = plusMaterial.DangleGrip,
                    Base = plusMaterial.BaseGrip,
                    Ledge = plusMaterial.LedgeGrip,
                    IsPlusMaterial = true,
                    Disposition = GripDisposition.Rectangular
                });
                Rules.Add(new MaterialGripRule
                {
                    Balance = plusMaterial.Balance,
                    Dangling = plusMaterial.DangleGrip,
                    Base = plusMaterial.BaseGrip,
                    Ledge = plusMaterial.LedgeGrip,
                    IsPlusMaterial = true,
                    Disposition = GripDisposition.Irregular
                });
            }
        }

        public List<GripRule> Rules { get { return _Rules; } }

        #region public int? GetInnerLedge(bool plusBlocks)
        public int? GetInnerLedge(bool plusBlocks)
        {
            return (from _mgr in Rules.OfType<MaterialGripRule>()
                    where _mgr.IsPlusMaterial == plusBlocks
                       && _mgr.Ledge.HasValue
                       && (_mgr.Disposition == GripDisposition.Internal)
                    select _mgr.Ledge).Min();
        }
        #endregion

        #region public int? GetInnerBase(bool plusBlocks)
        public int? GetInnerBase(bool plusBlocks)
        {
            return (from _mgr in Rules.OfType<MaterialGripRule>()
                    where _mgr.IsPlusMaterial == plusBlocks
                       && _mgr.Base.HasValue
                       && (_mgr.Disposition == GripDisposition.Internal)
                    select _mgr.Base).Min();
        }
        #endregion

        #region public int? GetInnerDangling(bool plusBlocks)
        public int? GetInnerDangling(bool plusBlocks)
        {
            return (from _mgr in Rules.OfType<MaterialGripRule>()
                    where _mgr.IsPlusMaterial == plusBlocks
                       && _mgr.Dangling.HasValue
                       && (_mgr.Disposition == GripDisposition.Internal)
                    select _mgr.Dangling).Min();
        }
        #endregion

        /// <summary>Dangling from a mixed material face</summary>
        public int? GetOuterDangling(GripDisposition disposition = GripDisposition.Rectangular)
        {
            return (from _gr in Rules.OfType<MaterialGripRule>()
                    where _gr.Dangling.HasValue
                       && _gr.Disposition == disposition
                    select _gr.Dangling).Min();
        }

        /// <summary>Dangling from a single material face</summary>
        public int? GetOuterDangling(bool isPlusMaterial)
        {
            return (from _gr in Rules.OfType<MaterialGripRule>()
                    where _gr.Dangling.HasValue
                       && _gr.Disposition == GripDisposition.Full
                       && _gr.IsPlusMaterial == isPlusMaterial
                    select _gr.Dangling).Min();
        }

        /// <summary>Clinging to a mixed material face</summary>
        public int? GetOuterBase(GripDisposition disposition = GripDisposition.Rectangular)
        {
            return (from _gr in Rules.OfType<MaterialGripRule>()
                    where _gr.Base.HasValue
                       && _gr.Disposition == disposition
                    select _gr.Base).Min();
        }

        /// <summary>Clinging to a single material face</summary>
        public int? GetOuterBase(bool isPlusMaterial)
        {
            return (from _gr in Rules.OfType<MaterialGripRule>()
                    where _gr.Base.HasValue
                       && _gr.Disposition == GripDisposition.Full
                       && _gr.IsPlusMaterial == isPlusMaterial
                    select _gr.Base).Min();
        }

        /// <summary>Clinging to a ledge on a mixed material face</summary>
        public int? GetOuterLedge(bool isPlusMaterial, GripDisposition disposition = GripDisposition.Rectangular)
        {
            return (from _gr in Rules.OfType<MaterialGripRule>()
                    where _gr.Ledge.HasValue
                       && _gr.Disposition == disposition
                       && _gr.IsPlusMaterial == isPlusMaterial
                    select _gr.Ledge).Min();
        }

    }
}