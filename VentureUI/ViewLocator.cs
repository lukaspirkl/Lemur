using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VentureUI;

public interface IViewModelCollection
{
    IViewModelCollection AddInterfaceViewModel<TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new();
    IViewModelCollection AddSingletonViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new();
    IViewModelCollection AddTransientViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new();
}

public class ViewModelCollection : IViewModelCollection
{
    private readonly IServiceCollection m_Services;

    public ViewModelCollection(IServiceCollection services)
    {
        m_Services = services;
    }

    public IViewModelCollection AddSingletonViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        if (typeof(TViewModel).IsInterface)
        {
            throw new InvalidOperationException("You are using interface as TViewModel. You should use the AddInterface method instead.");
        }

        m_Services.AddSingleton<TViewModel>();
        AddViewModelMapping<TViewModel, TView>();
        return this;
    }

    public IViewModelCollection AddTransientViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        if (typeof(TViewModel).IsInterface)
        {
            throw new InvalidOperationException("You are using interface as TViewModel. You should use the AddInterface method instead.");
        }

        m_Services.AddTransient<TViewModel>();
        AddViewModelMapping<TViewModel, TView>();
        return this;
    }

    public IViewModelCollection AddInterfaceViewModel<TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        if (!typeof(TViewModel).IsInterface)
        {
            throw new InvalidOperationException("You are not using interface as TViewModel. You should use different method.");
        }

        AddViewModelMapping<TViewModel, TView>();
        return this;
    }

    private IViewModelCollection AddViewModelMapping<TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        m_Services.AddSingleton(_ => new ViewModelToViewMapping(typeof(TViewModel), new ViewFactory<TView>()));

        if (typeof(IPeripheralTab).IsAssignableFrom(typeof(TViewModel)))
        {
            m_Services.AddSingleton<IPeripheralTab>(provider => (IPeripheralTab)provider.GetRequiredService<TViewModel>());
        }

        return this;
    }
}

public interface IViewFactory
{
    Control Create();
}


public class ViewFactory<TView> : IViewFactory where TView : Control, new()
{
    public Control Create()
    {
        return new TView();
    }
}

public record ViewModelToViewMapping(Type ViewModelType, IViewFactory ViewFactory);

public class ViewLocator : IDataTemplate
{
    private readonly ImmutableDictionary<Type, IViewFactory> m_ViewModelToView;
    private readonly ImmutableDictionary<Type, IViewFactory> m_ViewModelInterfaceToView;

    public ViewLocator(IEnumerable<ViewModelToViewMapping> mappings)
    {
        m_ViewModelToView = mappings.ToImmutableDictionary(map => map.ViewModelType, map => map.ViewFactory);
        m_ViewModelInterfaceToView = m_ViewModelToView.Where(x => x.Key.IsInterface).ToImmutableDictionary(x => x.Key, x => x.Value);
    }

    public Control Build(object? data)
    {
        if (TryGetViewType(data, out var viewFactory))
        {
            return viewFactory.Create();
        }

        return new TextBlock { Text = "View not found." };
    }

    public bool Match(object? data)
    {
        return TryGetViewType(data, out var _);
    }

    private bool TryGetViewType(object? data, [MaybeNullWhen(false)] out IViewFactory viewFactory)
    {
        viewFactory = null;

        if (data == null)
        {
            return false;
        }

        var type = data.GetType();
        if (m_ViewModelToView.TryGetValue(type, out viewFactory))
        {
            return true;
        }

        foreach (var item in m_ViewModelInterfaceToView)
        {
            if (type.IsAssignableTo(item.Key))
            {
                viewFactory = item.Value;
                return true;
            }
        }

        return false;
    }
}

public class ViewLocatorForPreviwer : IDataTemplate, IViewModelCollection
{
    private readonly Dictionary<Type, IViewFactory> m_ViewModelToView = new();

    public IViewModelCollection AddInterfaceViewModel<TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        m_ViewModelToView.Add(typeof(TViewModel), new ViewFactory<TView>());
        return this;
    }

    public IViewModelCollection AddSingletonViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        m_ViewModelToView.Add(typeof(TViewModel), new ViewFactory<TView>());
        return this;
    }

    public IViewModelCollection AddTransientViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        m_ViewModelToView.Add(typeof(TViewModel), new ViewFactory<TView>());
        return this;
    }

    public Control Build(object? data)
    {
        if (TryGetViewType(data, out var viewFactory))
        {
            return viewFactory.Create();
        }

        return new TextBlock { Text = "View not found." };
    }

    public bool Match(object? data)
    {
        return TryGetViewType(data, out var _);
    }

    private bool TryGetViewType(object? data, [MaybeNullWhen(false)] out IViewFactory viewFactory)
    {
        viewFactory = null;

        if (data == null)
        {
            return false;
        }

        var type = data.GetType();
        if (m_ViewModelToView.TryGetValue(type, out viewFactory))
        {
            return true;
        }

        // Use base type to support view models for previewer
        if (type.BaseType != null && m_ViewModelToView.TryGetValue(type.BaseType, out viewFactory))
        {
            return true;
        }

        // Search for interfaces
        foreach (var item in m_ViewModelToView.Where(x => x.Key.IsInterface))
        {
            if (type.IsAssignableTo(item.Key))
            {
                viewFactory = item.Value;
                return true;
            }
        }

        return false;
    }

}

public static class ViewLocatorForPreviwerExtension
{
    public static AppBuilder UseViewLocatorForPreviewer(this AppBuilder builder)
    {
        builder.AfterSetup(b =>
        {
            // When ApplicationLifetime is null we are running in the previewer
            if (b.Instance is App app && app.ApplicationLifetime == null)
            {
                var locator = new ViewLocatorForPreviwer();
                Program.RegisterViewModels(locator);
                app.DataTemplates.Add(locator);
            }
        });
        return builder;
    }
}
