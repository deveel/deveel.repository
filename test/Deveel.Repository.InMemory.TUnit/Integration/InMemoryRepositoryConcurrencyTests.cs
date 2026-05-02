using System.Collections.Concurrent;

namespace Deveel.Data;

[Category("Integration")]
public class InMemoryRepositoryConcurrencyTests
{
    private const int ThreadCount = 100;

    private static readonly Faker<Person> PersonFaker = new Faker<Person>("en")
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.Email, f => f.Internet.Email())
        .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber())
        .RuleFor(x => x.DateOfBirth, f => f.Date.Past(30));

    #region AddAsync — concurrent writers

    [Test]
    public async Task Should_AddAllEntities_When_ManyThreadsWriteConcurrently(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();
        var people = PersonFaker.Generate(ThreadCount);

        var tasks = people.Select(p => repository.AddAsync(p, cancellationToken));
        await Task.WhenAll(tasks);

        var count = await repository.CountAsync(QueryFilter.Empty, cancellationToken);
        await Assert.That(count).IsEqualTo(ThreadCount);
    }

    [Test]
    public async Task Should_NotCorruptState_When_WritersAndReadersRunSimultaneously(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();

        var seedPeople = PersonFaker.Generate(50);
        foreach (var p in seedPeople)
            await repository.AddAsync(p, cancellationToken);

        var newPeople = PersonFaker.Generate(ThreadCount);
        var exceptions = new ConcurrentBag<Exception>();

        var writeTasks = newPeople.Select(async p =>
        {
            try { await repository.AddAsync(p, cancellationToken); }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        var readTasks = Enumerable.Range(0, ThreadCount).Select(async _ =>
        {
            try
            {
                var list = await repository.FindAllAsync(Query.Empty, cancellationToken);
                await Assert.That(list).IsNotNull();
            }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        await Task.WhenAll(writeTasks.Concat(readTasks));

        await Assert.That(exceptions.Count).IsEqualTo(0);
        var total = await repository.CountAsync(QueryFilter.Empty, cancellationToken);
        await Assert.That(total >= 50).IsTrue();
    }

    #endregion

    #region FindAsync — concurrent readers

    [Test]
    public async Task Should_ReturnCorrectEntity_When_ManyThreadsReadConcurrently(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();
        var people = PersonFaker.Generate(ThreadCount);
        foreach (var p in people)
            await repository.AddAsync(p, cancellationToken);

        var tasks = people.Select(async p =>
        {
            var found = await repository.FindAsync(p.Id!, cancellationToken);
            return (Expected: p, Actual: found);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (expected, actual) in results)
        {
            await Assert.That(actual).IsNotNull();
            await Assert.That(actual!.Id).IsEqualTo(expected.Id);
        }
    }

    [Test]
    public async Task Should_NeverThrow_When_ManyThreadsCallFindAllConcurrently(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();
        var people = PersonFaker.Generate(50);
        foreach (var p in people)
            await repository.AddAsync(p, cancellationToken);

        var exceptions = new ConcurrentBag<Exception>();

        var tasks = Enumerable.Range(0, ThreadCount).Select(async _ =>
        {
            try
            {
                var list = await repository.FindAllAsync(Query.Empty, cancellationToken);
                await Assert.That(list).IsNotNull();
            }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        await Task.WhenAll(tasks);

        await Assert.That(exceptions.Count).IsEqualTo(0);
    }

    #endregion

    #region UpdateAsync — concurrent writers

    [Test]
    public async Task Should_PreserveAllUpdates_When_ManyThreadsUpdateConcurrently(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();
        var people = PersonFaker.Generate(ThreadCount);
        foreach (var p in people)
            await repository.AddAsync(p, cancellationToken);

        var tasks = people.Select(async p =>
        {
            p.Email = new Faker().Internet.Email();
            var updated = await repository.UpdateAsync(p, cancellationToken);
            return updated;
        });

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
            await Assert.That(result).IsTrue();
    }

    #endregion

    #region RemoveAsync — concurrent writers

    [Test]
    public async Task Should_RemoveAllTargetedEntities_When_ManyThreadsRemoveConcurrently(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();
        var people = PersonFaker.Generate(ThreadCount);
        foreach (var p in people)
            await repository.AddAsync(p, cancellationToken);

        var tasks = people.Select(p => repository.RemoveAsync(p, cancellationToken));
        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
            await Assert.That(result).IsTrue();

        var count = await repository.CountAsync(QueryFilter.Empty, cancellationToken);
        await Assert.That(count).IsEqualTo(0);
    }

    #endregion

    #region Mixed-workload

    [Test]
    public async Task Should_MaintainConsistency_When_ReadersWritersAndUpdatersCombine(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();

        var seed = PersonFaker.Generate(50);
        foreach (var p in seed)
            await repository.AddAsync(p, cancellationToken);

        var exceptions = new ConcurrentBag<Exception>();

        var writers = PersonFaker.Generate(ThreadCount).Select(async p =>
        {
            try { await repository.AddAsync(p, cancellationToken); }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        var readers = Enumerable.Range(0, ThreadCount).Select(async _ =>
        {
            try
            {
                var count = await repository.CountAsync(QueryFilter.Empty, cancellationToken);
                await Assert.That(count >= 0).IsTrue();
            }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        var updaters = seed.Select(async p =>
        {
            try
            {
                p.PhoneNumber = new Faker().Phone.PhoneNumber();
                await repository.UpdateAsync(p, cancellationToken);
            }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        await Task.WhenAll(writers.Concat(readers).Concat(updaters));

        await Assert.That(exceptions.Count).IsEqualTo(0);
        var finalCount = await repository.CountAsync(QueryFilter.Empty, cancellationToken);
        await Assert.That(finalCount >= 50).IsTrue();
    }

    #endregion

    #region GetEntityKey — concurrent key discovery

    [Test]
    public async Task Should_ResolveEntityKey_When_ManyThreadsCallGetEntityKeyConcurrently(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();
        var people = PersonFaker.Generate(ThreadCount);
        foreach (var p in people)
            await repository.AddAsync(p, cancellationToken);

        var tasks = people.Select(p => Task.Run(() => repository.GetEntityKey(p), cancellationToken));
        var keys = await Task.WhenAll(tasks);

        for (int i = 0; i < people.Count; i++)
        {
            await Assert.That(keys[i]).IsNotNull();
            await Assert.That(keys[i]).IsEqualTo(people[i].Id);
        }
    }

    #endregion

    #region Entities snapshot

    [Test]
    public async Task Should_ReturnAtLeastSeedCount_When_SnapshotTakenDuringConcurrentWrites(CancellationToken cancellationToken)
    {
        using var repository = new InMemoryRepository<Person, string>();
        const int seedCount = 20;
        const int extraCount = ThreadCount;

        var seed = PersonFaker.Generate(seedCount);
        foreach (var p in seed)
            await repository.AddAsync(p, cancellationToken);

        var extra = PersonFaker.Generate(extraCount);

        var writeTask = Task.WhenAll(extra.Select(p => repository.AddAsync(p, cancellationToken)));
        var snapshot = repository.Entities;

        await writeTask;

        await Assert.That(snapshot.Count >= seedCount).IsTrue();

        var finalCount = await repository.CountAsync(QueryFilter.Empty, cancellationToken);
        await Assert.That(finalCount).IsEqualTo(seedCount + extraCount);
    }

    #endregion
}

