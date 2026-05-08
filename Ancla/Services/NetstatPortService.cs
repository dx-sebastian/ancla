using System.Diagnostics;
using Ancla.Models;

namespace Ancla.Services;

public sealed class NetstatPortService
{
    private readonly ProcessProtectionService _protectionService = new();
    private readonly DevelopmentPortCatalog _developmentPortCatalog = new();

    public IReadOnlyList<PortProcessEntry> GetEntries()
    {
        var tcpRows = ParseTcp(RunNetstat("-ano -p tcp"));
        var udpRows = ParseUdp(RunNetstat("-ano -p udp"));
        var rawRows = tcpRows.Concat(udpRows).ToList();

        var detailsByPid = rawRows
            .Select(row => row.ProcessId)
            .Distinct()
            .ToDictionary(pid => pid, pid => _protectionService.GetProcessDetails(pid));

        return rawRows
            .Select(row =>
            {
                var details = detailsByPid[row.ProcessId];
                var profile = _developmentPortCatalog.Match(row.Port, details.ProcessName, details.ExecutablePath);
                return new PortProcessEntry(
                    row.Protocol,
                    row.LocalAddress,
                    row.Port,
                    row.State,
                    row.ProcessId,
                    details.ProcessName,
                    details.ExecutablePath,
                    details.IsProtected,
                    details.ProtectionReason,
                    profile.Key,
                    profile.Label);
            })
            .OrderBy(entry => entry.Port)
            .ThenBy(entry => entry.ProcessName)
            .ToList();
    }

    private static string RunNetstat(string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? "netstat failed."
                : error.Trim());
        }

        return output;
    }

    private static IEnumerable<NetstatRow> ParseTcp(string output)
    {
        foreach (var line in output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmedLine = line.Trim();
            if (!trimmedLine.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = SplitLine(trimmedLine);
            if (parts.Length < 5)
            {
                continue;
            }

            if (!TryParseEndpoint(parts[1], out var localAddress, out var localPort))
            {
                continue;
            }

            if (!TryParseEndpoint(parts[2], out _, out var remotePort))
            {
                continue;
            }

            if (remotePort != 0)
            {
                continue;
            }

            if (!int.TryParse(parts[4], out var processId))
            {
                continue;
            }

            yield return new NetstatRow("TCP", localAddress, localPort, parts[3], processId);
        }
    }

    private static IEnumerable<NetstatRow> ParseUdp(string output)
    {
        foreach (var line in output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmedLine = line.Trim();
            if (!trimmedLine.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = SplitLine(trimmedLine);
            if (parts.Length < 4)
            {
                continue;
            }

            if (!TryParseEndpoint(parts[1], out var localAddress, out var localPort))
            {
                continue;
            }

            if (!int.TryParse(parts[3], out var processId))
            {
                continue;
            }

            yield return new NetstatRow("UDP", localAddress, localPort, "Listening", processId);
        }
    }

    private static string[] SplitLine(string line)
    {
        return line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool TryParseEndpoint(string endpoint, out string address, out int port)
    {
        address = string.Empty;
        port = 0;

        if (string.Equals(endpoint, "*:*", StringComparison.Ordinal))
        {
            address = "*";
            return true;
        }

        if (endpoint.StartsWith("[", StringComparison.Ordinal))
        {
            var closingBracketIndex = endpoint.IndexOf(']');
            if (closingBracketIndex < 0 || closingBracketIndex + 2 > endpoint.Length)
            {
                return false;
            }

            address = endpoint[1..closingBracketIndex];
            return int.TryParse(endpoint[(closingBracketIndex + 2)..], out port);
        }

        var lastColonIndex = endpoint.LastIndexOf(':');
        if (lastColonIndex <= 0 || lastColonIndex + 1 >= endpoint.Length)
        {
            return false;
        }

        address = endpoint[..lastColonIndex];
        return int.TryParse(endpoint[(lastColonIndex + 1)..], out port);
    }

    private sealed record NetstatRow(
        string Protocol,
        string LocalAddress,
        int Port,
        string State,
        int ProcessId);
}
