using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Shiny.Aspire.Hosting.Gluetun.Internal;
using Shouldly;

namespace Shiny.Aspire.Hosting.Gluetun.Tests;

public class GluetunExtensionsTests
{
    [Fact]
    public void AddGluetun_CreatesGluetunResource()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");

        vpn.Resource.ShouldBeOfType<GluetunResource>();
        vpn.Resource.Name.ShouldBe("vpn");
    }

    [Fact]
    public void AddGluetun_SetsCorrectImage()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");

        var imageAnnotation = vpn.Resource.Annotations
            .OfType<ContainerImageAnnotation>()
            .ShouldHaveSingleItem();

        imageAnnotation.Image.ShouldBe(GluetunContainerDefaults.Image);
        imageAnnotation.Tag.ShouldBe(GluetunContainerDefaults.Tag);
        imageAnnotation.Registry.ShouldBe(GluetunContainerDefaults.Registry);
    }

    [Fact]
    public async Task AddGluetun_SetsCapAddAndDeviceRuntimeArgs()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");

        var args = await GetContainerRuntimeArgs(vpn.Resource);

        args.ShouldContain("--cap-add");
        args.ShouldContain("NET_ADMIN");
        args.ShouldContain("--device");
        args.ShouldContain(GluetunContainerDefaults.Device);
    }

    [Fact]
    public async Task WithVpnProvider_SetsEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithVpnProvider("mullvad");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars.ShouldContainKey(GluetunContainerDefaults.VpnServiceProvider);
        envVars[GluetunContainerDefaults.VpnServiceProvider].ShouldBe("mullvad");
    }

    [Fact]
    public async Task WithOpenVpn_SetsVpnTypeAndCredentials()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithOpenVpn("user1", "pass1");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.VpnType].ShouldBe("openvpn");
        envVars[GluetunContainerDefaults.OpenVpnUser].ShouldBe("user1");
        envVars[GluetunContainerDefaults.OpenVpnPassword].ShouldBe("pass1");
    }

    [Fact]
    public async Task WithWireGuard_SetsVpnTypeAndPrivateKey()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithWireGuard("my-private-key");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.VpnType].ShouldBe("wireguard");
        envVars[GluetunContainerDefaults.WireGuardPrivateKey].ShouldBe("my-private-key");
    }

    [Fact]
    public async Task WithServerCountries_SetsCommaJoinedEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithServerCountries("US", "Canada");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.ServerCountries].ShouldBe("US,Canada");
    }

    [Fact]
    public async Task WithServerCities_SetsCommaJoinedEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithServerCities("New York", "Toronto");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.ServerCities].ShouldBe("New York,Toronto");
    }

    [Fact]
    public async Task WithHttpProxy_SetsOnOff()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithHttpProxy(true);

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.HttpProxy].ShouldBe("on");
    }

    [Fact]
    public async Task WithHttpProxy_DisabledSetsOff()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithHttpProxy(false);

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.HttpProxy].ShouldBe("off");
    }

    [Fact]
    public async Task WithShadowsocks_SetsOnOff()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithShadowsocks(true);

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.Shadowsocks].ShouldBe("on");
    }

    [Fact]
    public async Task WithFirewallOutboundSubnets_SetsCommaJoinedEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithFirewallOutboundSubnets("10.0.0.0/8", "192.168.0.0/16");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.FirewallOutboundSubnets].ShouldBe("10.0.0.0/8,192.168.0.0/16");
    }

    [Fact]
    public async Task WithTimezone_SetsTimezoneEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithTimezone("America/New_York");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars[GluetunContainerDefaults.Timezone].ShouldBe("America/New_York");
    }

    [Fact]
    public async Task WithGluetunEnvironment_SetsCustomEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn")
            .WithGluetunEnvironment("CUSTOM_VAR", "custom-value");

        var envVars = await GetEnvironmentVariables(builder, vpn.Resource);
        envVars["CUSTOM_VAR"].ShouldBe("custom-value");
    }

    [Fact]
    public void WithRoutedContainer_AddsAnnotationToGluetunResource()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");
        var scraper = builder.AddContainer("scraper", "my-scraper");

        vpn.WithRoutedContainer(scraper);

        var annotation = vpn.Resource.Annotations
            .OfType<GluetunRoutedResourceAnnotation>()
            .ShouldHaveSingleItem();

        annotation.GluetunResource.ShouldBe(vpn.Resource);
        annotation.RoutedResource.ShouldBe(scraper.Resource);
    }

    [Fact]
    public async Task WithRoutedContainer_AddsNetworkRuntimeArgsToRoutedContainer()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");
        var scraper = builder.AddContainer("scraper", "my-scraper");

        vpn.WithRoutedContainer(scraper);

        var args = await GetContainerRuntimeArgs(scraper.Resource);
        args.ShouldContain("--network");
        args.ShouldContain("container:vpn");
    }

    [Fact]
    public void WithRoutedContainer_MultipleContainersCreateMultipleAnnotations()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");
        var scraper1 = builder.AddContainer("scraper1", "my-scraper");
        var scraper2 = builder.AddContainer("scraper2", "my-scraper");

        vpn.WithRoutedContainer(scraper1);
        vpn.WithRoutedContainer(scraper2);

        var annotations = vpn.Resource.Annotations
            .OfType<GluetunRoutedResourceAnnotation>()
            .ToList();

        annotations.Count.ShouldBe(2);
        annotations.ShouldContain(a => a.RoutedResource.Name == "scraper1");
        annotations.ShouldContain(a => a.RoutedResource.Name == "scraper2");
    }

    [Fact]
    public void AddGluetun_NullBuilder_ThrowsArgumentNullException()
    {
        IDistributedApplicationBuilder builder = null!;
        Should.Throw<ArgumentNullException>(() => builder.AddGluetun("vpn"));
    }

    [Fact]
    public void AddGluetun_EmptyName_ThrowsArgumentException()
    {
        var builder = DistributedApplication.CreateBuilder();
        Should.Throw<ArgumentException>(() => builder.AddGluetun(""));
    }

    [Fact]
    public void WithVpnProvider_EmptyProvider_ThrowsArgumentException()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");
        Should.Throw<ArgumentException>(() => vpn.WithVpnProvider(""));
    }

    [Fact]
    public void WithOpenVpn_EmptyUser_ThrowsArgumentException()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");
        Should.Throw<ArgumentException>(() => vpn.WithOpenVpn("", "pass"));
    }

    [Fact]
    public void WithWireGuard_EmptyKey_ThrowsArgumentException()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");
        Should.Throw<ArgumentException>(() => vpn.WithWireGuard(""));
    }

    [Fact]
    public void WithRoutedContainer_NullResource_ThrowsArgumentNullException()
    {
        var builder = DistributedApplication.CreateBuilder();
        var vpn = builder.AddGluetun("vpn");
        Should.Throw<ArgumentNullException>(() => vpn.WithRoutedContainer<ContainerResource>(null!));
    }

    private static async Task<List<string>> GetContainerRuntimeArgs(IResource resource)
    {
        var args = new List<object>();
        var context = new ContainerRuntimeArgsCallbackContext(args, CancellationToken.None);

        foreach (var annotation in resource.Annotations.OfType<ContainerRuntimeArgsCallbackAnnotation>())
        {
            if (annotation.Callback is Func<ContainerRuntimeArgsCallbackContext, Task> asyncCallback)
                await asyncCallback(context);
        }

        return args.Select(a => a.ToString()!).ToList();
    }

    private static async Task<Dictionary<string, string>> GetEnvironmentVariables(
        IDistributedApplicationBuilder builder,
        IResource resource)
    {
        var envVars = new Dictionary<string, object>();
        var envContext = new EnvironmentCallbackContext(
            builder.ExecutionContext,
            envVars,
            CancellationToken.None);

        foreach (var annotation in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
        {
            await annotation.Callback(envContext);
        }

        return envVars.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "");
    }
}
