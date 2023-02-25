namespace Uzi.Core
{
    public readonly struct ActivityResponse
    {
        public ActivityResponse(bool success)
        {
            _Success = success;
        }

        private readonly bool _Success;
        public bool Success => _Success;
    }
}