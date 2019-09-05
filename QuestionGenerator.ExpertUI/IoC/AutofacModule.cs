namespace QuestionGenerator.ExpertUI.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using Autofac;
    using Autofac.Util;

    using Caliburn.Micro;
    using QuestionGenerator.Library.Data;
    using QuestionGenerator.Library.Interfaces;
    using Module = Autofac.Module;

    public class AutofacModule : Module
    {
        /// <summary>
        /// The view model view mapping
        /// </summary>
        private static readonly IDictionary<Type, Type> ViewModelViewMapping = new Dictionary<Type, Type>();

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            
            var loadableTypes = this.ThisAssembly.GetLoadableTypes();
            builder.RegisterTypes(loadableTypes.Where(this.SatisfyViewModelFilter).ToArray()).AsSelf().InstancePerDependency().PropertiesAutowired();
            builder.RegisterTypes(loadableTypes.Where(this.StatisfiesViewFilter).ToArray()).AsSelf().InstancePerDependency().PropertiesAutowired();
            builder.RegisterAssemblyTypes(this.ThisAssembly).Where(t => t.Name.EndsWith("DataModel")).AsImplementedInterfaces().PropertiesAutowired();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<MessageService>().As<IMessageService>().SingleInstance();
            builder.RegisterType<DataManager>().AsSelf();

            this.RegisterLocators();
        }

        /// <summary>
        /// Registers the locators.
        /// </summary>
        private void RegisterLocators()
        {
            // Override the view model => view locator to allow a view to be passed in the first place.
            var defaultLocateForModel = ViewLocator.LocateForModel;
            ViewLocator.LocateForModel = (model, displayLocation, context) =>
            {
                if (model is UIElement uiElement)
                {
                    return uiElement;
                }

                return defaultLocateForModel(model, displayLocation, context);
            };

            // Override the view model type => view type locator to allow finding a view based on a base view model.
            var defaultTypeForModelTypeLocator = ViewLocator.LocateTypeForModelType;
            ViewLocator.LocateTypeForModelType = (modelType, displayLocation, context) =>
            {
                var result = defaultTypeForModelTypeLocator(modelType, displayLocation, context);
                var typeToCheck = modelType?.BaseType;

                while (result == null && typeToCheck != null)
                {
                    result = defaultTypeForModelTypeLocator(typeToCheck, displayLocation, context);
                    typeToCheck = typeToCheck.BaseType;
                }

                return result;
            };

            // Override the view model type => view instance locator to allow us to use the custom view model mappings.
            var defaultLocator = ViewLocator.LocateForModelType;
            ViewLocator.LocateForModelType = (modelType, displayLocation, context) =>
            {
                if (!ViewModelViewMapping.TryGetValue(modelType, out var viewType))
                {
                    foreach (var vmMapping in ViewModelViewMapping)
                    {
                        if (vmMapping.Key.IsAssignableFrom(modelType))
                        {
                            viewType = vmMapping.Value;
                            break;
                        }
                    }
                }

                return viewType != null ? ViewLocator.GetOrCreateViewType(viewType) : defaultLocator(modelType, displayLocation, context);
            };
        }

        /// <summary>
        /// Satisfies the view model filter.
        /// </summary>
        /// <param name="viewModelType">Type of the view model.</param>
        /// <returns>True if the conditions satisfy, False otherwise.</returns>
        private bool SatisfyViewModelFilter(Type viewModelType) => viewModelType.Name.EndsWith("ViewModel", StringComparison.Ordinal);

        /// <summary>
        /// Statisfieses the view filter.
        /// </summary>
        /// <param name="viewType">Type of the view.</param>
        /// <returns>True if the conditions satisfy, False otherwise.</returns>
        private bool StatisfiesViewFilter(Type viewType)
        {
            if (!viewType.Name.EndsWith("View", System.StringComparison.Ordinal))
            {
                return false;
            }

            int viewTypePosition = -1;
            int viewsNamespacePosition = -1;
            if ((viewsNamespacePosition = viewType.Namespace.IndexOf(".Views", StringComparison.OrdinalIgnoreCase)) + 6 == viewType.Namespace.Length &&
                viewsNamespacePosition > 0 &&
                (viewTypePosition = viewType.Name.IndexOf("View", StringComparison.OrdinalIgnoreCase)) + 4 == viewType.Name.Length &&
                viewTypePosition > 0)
            {
                var viewModelTypeName = $"{viewType.Namespace.Substring(0, viewsNamespacePosition)}.ViewModels.{viewType.Name.Substring(0, viewTypePosition)}ViewModel";
                var viewModelType = this.ThisAssembly.GetType(viewModelTypeName);
                if (viewModelType != null)
                {
                    ViewModelViewMapping[viewModelType] = viewType;
                }
            }

            return true;
        }
    }
}
