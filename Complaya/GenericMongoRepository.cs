using Microsoft.Extensions.Options;
using MongoDbGenericRepository;

namespace Complaya
{
    public class GenericMongoRepository : BaseMongoRepository
    {
        public GenericMongoRepository(IOptions<MongoConfigurationOptions> optionsAccessor) : base(optionsAccessor.Value.ConnectionString, optionsAccessor.Value.DatabaseName)
        {
        }

    }
}