using MongoDB.Driver;
using CaseStudy.Core.Entities;
using CaseStudy.DataAccess.Persistence;

namespace CaseStudy.DataAccess.Repositories.Impl;

/// <summary>
/// This class represents a holiday repository.
/// </summary>
public class HolidayRepository(DatabaseContext context, IMongoClient client)
    : BaseRepository<Holiday>(context, client),
        IHolidayRepository
{

}
