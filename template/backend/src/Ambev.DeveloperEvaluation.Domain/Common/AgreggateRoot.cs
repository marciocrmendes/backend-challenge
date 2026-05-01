using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Common;

public abstract class AgreggateRoot : BaseEntity
{
    private readonly List<INotification> _domainEvents = [];

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearEvents() => _domainEvents.Clear();
}
