// Copyright (c) IEvangelist. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosEventSourcing.ChangeFeed;
using Microsoft.Azure.CosmosEventSourcing.Converters;
using Microsoft.Azure.CosmosEventSourcing.Events;
using Microsoft.Azure.CosmosEventSourcing.Items;
using Microsoft.Azure.CosmosEventSourcing.Options;
using Microsoft.Azure.CosmosEventSourcing.Projections;
using Microsoft.Azure.CosmosRepository.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.CosmosEventSourcing.Builders;

internal class DefaultCosmosEventSourcingBuilder : ICosmosEventSourcingBuilder
{
    private readonly IServiceCollection _services;

    public DefaultCosmosEventSourcingBuilder(IServiceCollection services) =>
        _services = services;

    public IEventItemProjectionBuilder<TEventItem, TProjectionKey> AddEventItemProjection<TEventItem,TProjectionKey, TProjection>(
        Action<EventSourcingProcessorOptions<TEventItem, TProjectionKey>>? optionsAction = null)
        where TEventItem : EventItem
        where TProjection : class, IEventItemProjection<TEventItem, TProjectionKey>
        where TProjectionKey : IProjectionKey
    {
        EventSourcingProcessorOptions<TEventItem, TProjectionKey> options = new();
        optionsAction?.Invoke(options);

        _services.AddSingleton(options);
        _services.AddSingleton<IEventItemProjection<TEventItem, TProjectionKey>, TProjection>();
        _services.AddSingleton<IEventSourcingProcessor, DefaultEventSourcingProcessor<TEventItem, TProjectionKey>>();

        return new EventItemProjectionBuilder<TEventItem, TProjectionKey>(
            _services,
            this);
    }


    public ICosmosEventSourcingBuilder AddDefaultDomainEventProjection<TEventItem, TProjectionKey>(
        Action<EventSourcingProcessorOptions<TEventItem, TProjectionKey>>? optionsAction = null)
        where TEventItem : EventItem where TProjectionKey : IProjectionKey
    {
        EventSourcingProcessorOptions<TEventItem, TProjectionKey> options = new();
        optionsAction?.Invoke(options);

        _services.AddSingleton(options);
        _services
            .AddSingleton<IEventItemProjection<TEventItem, TProjectionKey>, DefaultDomainEventProjection<TEventItem, TProjectionKey>>();
        _services.AddSingleton<IEventSourcingProcessor, DefaultEventSourcingProcessor<TEventItem, TProjectionKey>>();
        return this;
    }

    public ICosmosEventSourcingBuilder AddDomainEventTypes(
        params Assembly[] assemblies)
    {
        if (!assemblies.Any())
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        List<Type> types = assemblies
            .SelectMany(x => x.GetTypes()
                .Where(type => typeof(IDomainEvent).IsAssignableFrom(type)))
            .ToList();

        types.ForEach(t => DomainEventConverter.ConvertableTypes.Add(t));

        return this;
    }

    public ICosmosEventSourcingBuilder AddDomainEventProjectionHandlers(
        params Assembly[] assemblies)
    {
        if (!assemblies.Any())
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        _services.Scan(x => x.FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventProjection<,,>)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return this;
    }

    public ICosmosEventSourcingBuilder AddCosmosRepository(
        Action<RepositoryOptions>? setupAction = default,
        Action<CosmosClientOptions>? additionSetupAction = default)
    {
        _services.AddCosmosRepository(options =>
        {
            options.ContainerPerItemType = true;
            setupAction?.Invoke(options);
        }, additionSetupAction);

        return this;
    }
}