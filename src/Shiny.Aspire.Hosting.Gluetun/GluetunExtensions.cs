using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker;
using Shiny.Aspire.Hosting.Gluetun.Internal;

namespace Aspire.Hosting;

public static class GluetunExtensions
{
    public static IResourceBuilder<GluetunResource> AddGluetun(
        this IDistributedApplicationBuilder builder,
        string name,
        int? httpProxyPort = null,
        int? shadowsocksPort = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var resource = new GluetunResource(name);
        var rb = builder.AddResource(resource)
            .WithImage(GluetunContainerDefaults.Image, GluetunContainerDefaults.Tag)
            .WithImageRegistry(GluetunContainerDefaults.Registry)
            .WithContainerRuntimeArgs("--cap-add", "NET_ADMIN")
            .WithContainerRuntimeArgs("--device", GluetunContainerDefaults.Device);

        if (httpProxyPort.HasValue)
            rb = rb.WithEndpoint(port: httpProxyPort.Value, targetPort: GluetunContainerDefaults.HttpProxyPort, name: "http-proxy");

        if (shadowsocksPort.HasValue)
            rb = rb.WithEndpoint(port: shadowsocksPort.Value, targetPort: GluetunContainerDefaults.ShadowsocksPort, name: "shadowsocks");

        rb.PublishAsDockerComposeService((context, service) =>
        {
            service.CapAdd ??= [];
            service.CapAdd.Add("NET_ADMIN");

            service.Devices ??= [];
            service.Devices.Add(GluetunContainerDefaults.Device);

            // Port transfer: move ports from routed containers to gluetun
            foreach (var annotation in resource.Annotations.OfType<GluetunRoutedResourceAnnotation>())
            {
                foreach (var endpoint in annotation.RoutedResource.Annotations.OfType<EndpointAnnotation>())
                {
                    var targetPort = endpoint.TargetPort;
                    if (targetPort.HasValue)
                    {
                        var hostPort = endpoint.Port;
                        service.Ports ??= [];
                        service.Ports.Add(hostPort.HasValue
                            ? $"{hostPort}:{targetPort}"
                            : $"{targetPort}");
                    }
                }
            }
        });

        return rb;
    }

    public static IResourceBuilder<GluetunResource> WithVpnProvider(
        this IResourceBuilder<GluetunResource> builder,
        string provider)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        return builder.WithEnvironment(GluetunContainerDefaults.VpnServiceProvider, provider);
    }

    public static IResourceBuilder<GluetunResource> WithOpenVpn(
        this IResourceBuilder<GluetunResource> builder,
        string user,
        string password)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return builder
            .WithEnvironment(GluetunContainerDefaults.VpnType, "openvpn")
            .WithEnvironment(GluetunContainerDefaults.OpenVpnUser, user)
            .WithEnvironment(GluetunContainerDefaults.OpenVpnPassword, password);
    }

    public static IResourceBuilder<GluetunResource> WithOpenVpn(
        this IResourceBuilder<GluetunResource> builder,
        IResourceBuilder<ParameterResource> user,
        IResourceBuilder<ParameterResource> password)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(password);

        return builder
            .WithEnvironment(GluetunContainerDefaults.VpnType, "openvpn")
            .WithEnvironment(GluetunContainerDefaults.OpenVpnUser, user)
            .WithEnvironment(GluetunContainerDefaults.OpenVpnPassword, password);
    }

    public static IResourceBuilder<GluetunResource> WithWireGuard(
        this IResourceBuilder<GluetunResource> builder,
        string privateKey)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);

        return builder
            .WithEnvironment(GluetunContainerDefaults.VpnType, "wireguard")
            .WithEnvironment(GluetunContainerDefaults.WireGuardPrivateKey, privateKey);
    }

    public static IResourceBuilder<GluetunResource> WithWireGuard(
        this IResourceBuilder<GluetunResource> builder,
        IResourceBuilder<ParameterResource> privateKey)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(privateKey);

        return builder
            .WithEnvironment(GluetunContainerDefaults.VpnType, "wireguard")
            .WithEnvironment(GluetunContainerDefaults.WireGuardPrivateKey, privateKey);
    }

    public static IResourceBuilder<GluetunResource> WithServerCountries(
        this IResourceBuilder<GluetunResource> builder,
        params string[] countries)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(countries);

        return builder.WithEnvironment(GluetunContainerDefaults.ServerCountries, string.Join(",", countries));
    }

    public static IResourceBuilder<GluetunResource> WithServerCities(
        this IResourceBuilder<GluetunResource> builder,
        params string[] cities)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(cities);

        return builder.WithEnvironment(GluetunContainerDefaults.ServerCities, string.Join(",", cities));
    }

    public static IResourceBuilder<GluetunResource> WithHttpProxy(
        this IResourceBuilder<GluetunResource> builder,
        bool enabled = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(GluetunContainerDefaults.HttpProxy, enabled ? "on" : "off");
    }

    public static IResourceBuilder<GluetunResource> WithShadowsocks(
        this IResourceBuilder<GluetunResource> builder,
        bool enabled = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(GluetunContainerDefaults.Shadowsocks, enabled ? "on" : "off");
    }

    public static IResourceBuilder<GluetunResource> WithFirewallOutboundSubnets(
        this IResourceBuilder<GluetunResource> builder,
        params string[] subnets)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(subnets);

        return builder.WithEnvironment(GluetunContainerDefaults.FirewallOutboundSubnets, string.Join(",", subnets));
    }

    public static IResourceBuilder<GluetunResource> WithTimezone(
        this IResourceBuilder<GluetunResource> builder,
        string timezone)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(timezone);

        return builder.WithEnvironment(GluetunContainerDefaults.Timezone, timezone);
    }

    public static IResourceBuilder<GluetunResource> WithGluetunEnvironment(
        this IResourceBuilder<GluetunResource> builder,
        string name,
        string value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return builder.WithEnvironment(name, value);
    }

    public static IResourceBuilder<GluetunResource> WithGluetunEnvironment(
        this IResourceBuilder<GluetunResource> builder,
        string name,
        IResourceBuilder<ParameterResource> value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        return builder.WithEnvironment(name, value);
    }

    public static IResourceBuilder<GluetunResource> WithRoutedContainer<T>(
        this IResourceBuilder<GluetunResource> builder,
        IResourceBuilder<T> resource) where T : ContainerResource
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(resource);

        var gluetun = builder.Resource;
        var routed = resource.Resource;

        // Add annotation to gluetun resource
        builder.WithAnnotation(new GluetunRoutedResourceAnnotation(gluetun, routed));

        // Add network args to the routed container for docker run
        resource.WithContainerRuntimeArgs("--network", $"container:{gluetun.Name}");

        // Docker Compose: set NetworkMode on the routed container
        resource.PublishAsDockerComposeService((context, service) =>
        {
            service.NetworkMode = $"service:{gluetun.Name}";
            service.Ports = []; // Ports are transferred to the gluetun service
        });

        return builder;
    }
}
