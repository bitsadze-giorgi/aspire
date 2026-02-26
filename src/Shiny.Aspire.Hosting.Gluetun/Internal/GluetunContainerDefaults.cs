namespace Shiny.Aspire.Hosting.Gluetun.Internal;

internal static class GluetunContainerDefaults
{
    public const string Registry = "docker.io";
    public const string Image = "qmcgaw/gluetun";
    public const string Tag = "latest";
    public const int HttpProxyPort = 8888;
    public const int ShadowsocksPort = 8388;
    public const string Device = "/dev/net/tun";

    // Environment variable names
    public const string VpnServiceProvider = "VPN_SERVICE_PROVIDER";
    public const string VpnType = "VPN_TYPE";
    public const string OpenVpnUser = "OPENVPN_USER";
    public const string OpenVpnPassword = "OPENVPN_PASSWORD";
    public const string WireGuardPrivateKey = "WIREGUARD_PRIVATE_KEY";
    public const string ServerCountries = "SERVER_COUNTRIES";
    public const string ServerCities = "SERVER_CITIES";
    public const string HttpProxy = "HTTPPROXY";
    public const string Shadowsocks = "SHADOWSOCKS";
    public const string FirewallOutboundSubnets = "FIREWALL_OUTBOUND_SUBNETS";
    public const string Timezone = "TZ";
}
