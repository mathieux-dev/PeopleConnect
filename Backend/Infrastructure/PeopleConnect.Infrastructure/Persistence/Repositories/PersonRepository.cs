using Microsoft.EntityFrameworkCore;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Infrastructure.Persistence;

namespace PeopleConnect.Infrastructure.Persistence.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly DataContext _context;

    public PersonRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Persons
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Person?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Persons
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.CPF == cpf, cancellationToken);
    }

    public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Persons
            .Include(p => p.Contacts)
            .ToListAsync(cancellationToken);
    }

    public async Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default)
    {
        _context.Persons.Add(person);
        await _context.SaveChangesAsync(cancellationToken);
        return person;
    }

    public async Task<Person> UpdateAsync(Person person, CancellationToken cancellationToken = default)
    {
        _context.Persons.Update(person);
        await _context.SaveChangesAsync(cancellationToken);
        return person;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var person = await _context.Persons.FindAsync(new object[] { id }, cancellationToken);
        if (person != null)
        {
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Persons.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> CpfExistsAsync(string cpf, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Persons.Where(p => p.CPF == cpf);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
