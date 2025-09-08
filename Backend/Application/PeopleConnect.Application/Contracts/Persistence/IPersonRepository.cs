using PeopleConnect.Domain.Entities;

namespace PeopleConnect.Application.Contracts.Persistence;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Person?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default);
    Task<Person> UpdateAsync(Person person, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CpfExistsAsync(string cpf, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
