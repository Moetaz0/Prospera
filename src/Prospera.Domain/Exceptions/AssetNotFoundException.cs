

namespace Prospera.Domain.Exceptions
{
    internal class AssetNotFoundException :DomainException

    {
        public AssetNotFoundException(Guid id) : base($"Asset with ID {id} was not found.")
        {
        }
    }
}
