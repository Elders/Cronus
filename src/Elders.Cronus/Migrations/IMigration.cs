namespace Elders.Cronus.Migrations;

public interface IMigration { }

public interface IMigration<in T, out V> : IMigration
    where T : class
    where V : class
{
    bool ShouldApply(T current);
    V Apply(T current);
}

public interface IMigration<T> : IMigration<T, T>
    where T : class
{
}
