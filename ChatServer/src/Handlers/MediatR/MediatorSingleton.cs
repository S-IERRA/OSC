using MediatR;

namespace ChatServer.Handlers;

/*
public class MediatorSingleton
{
    
    private static readonly Lazy<IMediator> Lazy = new(() => new ServiceCollection()
        .AddMediatR(typeof(MediatorSingleton).Assembly)
        .BuildServiceProvider()
        .GetService<IMediator>());

    static MediatorSingleton()
    {
        Lazy = new Lazy<IMediator>(() => new Mediator(new SingleInstanceFactory(t => throw new InvalidOperationException("Cannot resolve an instance.")), new MultiInstanceFactory()));
    }

    public static IMediator Instance => Lazy.Value;
}

*/