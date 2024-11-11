namespace Aiursoft.InMemoryKvDb.Tests;

internal class Player(Guid id, string nickName)
{
    public Guid Id { get; } = id;
    public string NickName { get; } = nickName;
}