namespace Uzi.Core
{
    public class ActionResult
    {
        #region private data
        private IActionProvider _Provider;
        private CoreAction _Action;
        private bool _External;
        #endregion

        public IActionProvider Provider { get { return _Provider; } set { _Provider = value; } }
        public CoreAction Action { get { return _Action; } set { _Action = value; } }
        public bool IsExternal { get { return _External; } set { _External = value; } }
    }
}
