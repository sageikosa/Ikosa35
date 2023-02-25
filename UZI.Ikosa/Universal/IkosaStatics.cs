using Uzi.Core;

namespace Uzi.Ikosa.Universal
{
    public class IkosaStatics
    {
        #region private static data
        private static IInteractProvider _InteractProvider;
        private static ICreatureProvider _Provider;
        private static IkosaProcessManager _Manager;
        #endregion

        public static ICreatureProvider CreatureProvider { get => _Provider; set => _Provider = value; }

        #region public static IInteractProvider InteractProvider { get; set; }
        public static IInteractProvider InteractProvider
        {
            get { return _InteractProvider; }
            set { _InteractProvider = value; }
        }
        #endregion

        #region public static IkosaProcessManager ProcessManager { get; set; }
        public static IkosaProcessManager ProcessManager
        {
            get { return _Manager; }
            set { _Manager = value; }
        }
        #endregion
    }
}