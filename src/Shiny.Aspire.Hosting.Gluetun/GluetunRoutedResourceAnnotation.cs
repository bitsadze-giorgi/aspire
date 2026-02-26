namespace Aspire.Hosting.ApplicationModel;

public sealed record GluetunRoutedResourceAnnotation(
    GluetunResource GluetunResource,
    ContainerResource RoutedResource) : IResourceAnnotation;
